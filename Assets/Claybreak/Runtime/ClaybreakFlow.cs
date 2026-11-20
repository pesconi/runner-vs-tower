using System;
using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Crowd;
using UnityEngine;

namespace Claybreak
{
    /// <summary>The game's own flow states (the kit only sees them as <see cref="RunPhase"/>).</summary>
    public enum FlowState
    {
        Menu,
        Run,
        Assault,
        Won,
        Lost
    }

    /// <summary>
    /// Flow of the RT-4 vertical slice: home menu, the short run through the math gates (and the Brute
    /// crates), the assault at the firing line (L1: drag to aim, hold to stream units; Brutes go out
    /// every third launch), and the result, which saves the progress and offers NEXT / AGAIN in one tap.
    /// Owns the run phase the kit reads: Menu is Menu, Run and Assault are Playing. In the assault the
    /// leader stays at the firing line (course speed 0) and its sideways drag picks the lane; the army is
    /// the leader's followers, launched one by one from the back. The whole world lives in one scene:
    /// changing level re-places preallocated objects, never creates them.
    /// </summary>
    public sealed class ClaybreakFlow : RunPhaseSource
    {
        private const string RunHint = "DRAG TO PICK A GATE";
        private const string AssaultHint = "DRAG TO AIM - HOLD TO LAUNCH";
        private const int BruteEvery = 3;
        private const float WonResultDelay = 1.5f;
        private const float LostResultDelay = 0.8f;

        [SerializeField] private ClaybreakWorld world;
        [Tooltip("Device-gate level (300 units in flight, every defense at its limit, the heaviest tower). Development and measurement builds pick it with `--ez stress true`.")]
        [SerializeField] private ClaybreakLevel stressLevel;
        [SerializeField] private bool useStressLevelInEditor;
        [SerializeField] private ClaybreakTheme theme;
        [SerializeField] private Renderer track;
        [SerializeField] private PlayerController player;
        [SerializeField] private RunnerCrowd crowd;
        [SerializeField] private RunnerPool pool;
        [SerializeField] private CameraFollow runCamera;
        [SerializeField] private AssaultCamera assaultCamera;
        [SerializeField] private RunGateCourse gates;
        [SerializeField] private AssaultField field;
        [SerializeField] private AssaultTower tower;
        [SerializeField] private CannonDefense cannon;
        [SerializeField] private ClaybreakHud hud;
        [SerializeField] private ClaybreakScreens screens;
        [SerializeField] private ClaybreakSfx sfx;
        [SerializeField] private Transform firingLine;
        [SerializeField] private Transform laneMarker;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private FrameRateGovernor frameRateGovernor;

        private Progression progression;
        private LevelOutcome lastOutcome;
        private string lastDetail;
        private ClaybreakLevel activeLevel;
        private FlowState state = FlowState.Menu;
        private bool deviceGate;
        private bool resultShown;
        private float launchCredit;
        private int autoLane;
        private int launches;
        private int brutes;
        private int armyAtLine;
        private float holdTime;
        private float stateTime;
        private float levelTime;
#if DEVELOPMENT_BUILD || RELEASE_MEASUREMENT
        private float nextDevLog;
#endif
        private bool configured;

#if UNITY_EDITOR
        /// <summary>Editor only (PlayMode tests): the next flow to start runs the device-gate level.</summary>
        public static bool EditorUsesDeviceGateLevel { get; set; }

        /// <summary>Editor only (PlayMode tests): the next flow presses PLAY by itself.</summary>
        public static bool EditorStartsWithoutTap { get; set; }

        /// <summary>Editor only (PlayMode tests): the save is not read nor written.</summary>
        public static bool EditorUsesFreshSave { get; set; }
#endif

        public FlowState State => state;
        public AssaultField Field => field;
        public CannonDefense Cannon => cannon;
        public AssaultTower Tower => tower;
        public ClaybreakLevel ActiveLevel => activeLevel;
        public Progression Progress => progression;
        public int Brutes => brutes;

        public override RunPhase CurrentPhase
        {
            get
            {
                switch (state)
                {
                    case FlowState.Menu: return RunPhase.Menu;
                    case FlowState.Won: return RunPhase.Won;
                    case FlowState.Lost: return RunPhase.Lost;
                    default: return RunPhase.Playing;
                }
            }
        }

        private void Start()
        {
            deviceGate = PickDeviceGate();
            if (!Configure(out string error))
            {
                Debug.LogError("[Claybreak] The game cannot start: " + error, this);
                enabled = false;
                return;
            }

            configured = true;
            Debug.Log($"[Claybreak] Ready: {world.WorldName}, {world.LevelCount} levels, next {progression.CurrentLevel.name}, "
                + $"coins {progression.Save.coins}, upgrade step {progression.Save.startingArmyStep}, "
                + $"brute {(progression.Save.bruteUnlocked ? "unlocked" : "locked")}, pool {pool.Capacity}"
                + (deviceGate ? ", DEVICE GATE" : string.Empty) + ".", this);
            if (deviceGate)
            {
                StartLevel(stressLevel);
            }
            else
            {
                ShowMenu();
            }
        }

        private bool Configure(out string error)
        {
            if (world == null || stressLevel == null || theme == null || track == null || player == null || crowd == null
                || pool == null || runCamera == null || assaultCamera == null || gates == null || field == null
                || tower == null || cannon == null || hud == null || screens == null || sfx == null
                || firingLine == null || laneMarker == null || gameAudio == null || frameRateGovernor == null)
            {
                error = "a scene reference is missing (rebuild the scene with Claybreak > Build Scene).";
                return false;
            }

            if (!world.TryValidate(pool.Capacity, out error) || !stressLevel.TryValidate(pool.Capacity, out error)
                || !theme.TryValidate(out error) || !tower.TryValidate(out error))
            {
                return false;
            }

            progression = new Progression(world, LoadSave());
            CharacterController leader = player.GetComponent<CharacterController>();
            if (!player.TryApplyColorLook(theme.Roster, ClaybreakTheme.CrowdColorId, theme.CrowdColor)
                || !crowd.ApplyColorLook(theme.Roster, ClaybreakTheme.CrowdColorId, theme.CrowdColor)
                || !field.Initialize(track, theme, world, leader, Vector3.one * 0.5f * player.transform.lossyScale.x))
            {
                error = "the crowd look or the assault field did not configure (see the log above).";
                return false;
            }

            RunnerDeathEffects.Configure(theme.DeathEffect, track);
            gameAudio.Configure(this, theme.Audio);
            sfx.Configure(theme.Audio);
            frameRateGovernor.Configure(this);

            gates.GateCrossed += OnGateCrossed;
            gates.CratePicked += OnCratePicked;
            tower.Destroyed += OnTowerDestroyed;
            tower.HealthChanged += hud.SetTower;
            tower.FloorFallen += OnFloorFallen;
            crowd.CountChanged += count => hud.SetArmy(count - 1);
            screens.PlayPressed += () => StartLevel(progression.CurrentLevel);
            screens.UpgradePressed += OnUpgrade;
            screens.PrimaryPressed += OnPrimary;
            screens.HomePressed += ShowMenu;
            error = null;
            return true;
        }

        private bool PickDeviceGate()
        {
#if UNITY_EDITOR
            return EditorUsesDeviceGateLevel || useStressLevelInEditor;
#elif (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID
            return ReadStressExtra();
#else
            return false;
#endif
        }

#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
        // Development builds only: `adb shell am start ... --ez stress true` runs the device-gate level.
        private static bool ReadStressExtra()
        {
            try
            {
                using AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                return intent.Call<bool>("getBooleanExtra", "stress", false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Claybreak] Could not read the stress extra: " + exception.Message);
                return false;
            }
        }
#endif

        private SaveData LoadSave()
        {
#if UNITY_EDITOR
            if (EditorUsesFreshSave)
            {
                return new SaveData();
            }
#endif
            return deviceGate ? new SaveData() : SaveStore.Load();
        }

        private void WriteSave()
        {
#if UNITY_EDITOR
            if (EditorUsesFreshSave)
            {
                return;
            }
#endif
            if (!deviceGate)
            {
                SaveStore.Save(progression.Save);
            }
        }

        // ---------- screens ----------

        private void ShowMenu()
        {
            PrepareLevel(progression.CurrentLevel);
            hud.Show(false);
            screens.ShowHome(world, progression);
            Enter(FlowState.Menu);
        }

        private void OnUpgrade()
        {
            if (state == FlowState.Menu && progression.TryBuyUpgrade())
            {
                GameAudio.Play(GameSound.AdReward);
                WriteSave();
                Debug.Log($"[Claybreak] Upgrade bought: step {progression.Save.startingArmyStep}, +{progression.StartingArmyBonus} units, coins {progression.Save.coins}.", this);
                PrepareLevel(progression.CurrentLevel);
                screens.ShowHome(world, progression);
            }
        }

        private void OnPrimary()
        {
            if (state == FlowState.Won || state == FlowState.Lost)
            {
                // After a win the progress already points at the next level; after a loss, the same one.
                StartLevel(progression.CurrentLevel);
            }
        }

        // ---------- level ----------

        // Places the level (tower, defenses, gates, army) behind the menu, so the home screen shows it.
        private void PrepareLevel(ClaybreakLevel level)
        {
            activeLevel = level;
            bool played = !level.IsDeviceGateLevel;
            int towerHealth = played ? progression.ScaleHealth(level.TowerHealth) : level.TowerHealth;
            int wallHealth = played ? progression.ScaleHealth(level.WallHealth) : level.WallHealth;
            field.ResetField();
            field.Configure(level, track, wallHealth, level.DamagePerUnit);
            tower.ResetTower(level.Tower, towerHealth, level.Floors);
            if (!cannon.Configure(level, field, tower.Muzzle)
                || !gates.Configure(level, track, played && progression.Save.bruteUnlocked))
            {
                Debug.LogError("[Claybreak] The cannon or the run gates did not configure for " + level.name, this);
            }

            RunnerDeathEffects.ClearAll();
            player.ResetForNewRun();
            player.SetCourseSpeedMultiplier(0f);
            int army = level.StartCount + (played ? progression.StartingArmyBonus : 0);
            crowd.SetCount(Mathf.Min(army, crowd.MaximumCount - 1) + 1);
            crowd.SnapFollowersToFormation();
            brutes = 0;
            launches = 0;
            holdTime = 0f;
            assaultCamera.Stop();
            runCamera.enabled = true;
            Vector3 line = firingLine.position;
            firingLine.position = new Vector3(line.x, line.y, level.FiringLineZ);
            Vector3 marker = laneMarker.position;
            laneMarker.position = new Vector3(marker.x, marker.y, level.FiringLineZ + laneMarker.localScale.z * 0.5f);
            laneMarker.gameObject.SetActive(false);
            hud.SetLevel(level.Title);
            hud.SetCoins(progression.Save.coins);
            hud.SetArmy(crowd.CurrentCount - 1);
            hud.SetBrutes(0, false);
            hud.ShowTower(false, towerHealth);
            hud.ClearBanner();
            levelTime = 0f;
        }

        private void StartLevel(ClaybreakLevel level)
        {
            PrepareLevel(level);
            screens.HideAll();
            hud.Show(true);
            player.SetCourseSpeedMultiplier(level.RunSpeed);
            Debug.Log($"[Claybreak] Level '{level.name}' starts: army {crowd.CurrentCount - 1}, {level.RunGates.Count} gate pairs, "
                + $"tower {level.Tower} {tower.MaximumHealth} in {level.Floors} floor(s), projectile limit {level.MaximumProjectiles}, "
                + $"sweeper {(level.SweeperEnabled ? "on" : "off")}, wall {(level.WallEnabled ? "on" : "off")}.", this);
            Enter(FlowState.Run);
        }

        private void Update()
        {
            if (!configured)
            {
                return;
            }

            stateTime += Time.deltaTime;
            if (state == FlowState.Run || state == FlowState.Assault)
            {
                levelTime += Time.deltaTime;
            }

            switch (state)
            {
                case FlowState.Menu:
                    if (StartsWithoutTap && stateTime > 0.5f)
                    {
                        StartLevel(progression.CurrentLevel);
                    }

                    break;
                case FlowState.Run:
                    if (stateTime > 2.5f && progression.LevelIndex == 0)
                    {
                        hud.SetHint(null);
                    }

                    gates.Track(player.transform.position);
                    if (state == FlowState.Run && player.transform.position.z >= activeLevel.FiringLineZ)
                    {
                        Enter(FlowState.Assault);
                    }

                    break;
                case FlowState.Assault:
                    UpdateAssault();
                    break;
                case FlowState.Won:
                case FlowState.Lost:
                    if (!resultShown && stateTime >= (state == FlowState.Won ? WonResultDelay : LostResultDelay))
                    {
                        resultShown = true;
                        if (deviceGate)
                        {
                            StartLevel(stressLevel);
                        }
                        else
                        {
                            hud.Show(false);
                            screens.ShowResult(lastOutcome, lastDetail, progression.CurrentLevel.IsBoss);
                        }
                    }

                    break;
            }

            LogDeviceGateSample();
        }

        private void UpdateAssault()
        {
            int lane = field.GetNearestLane(player.transform.position.x);
            Vector3 marker = laneMarker.position;
            laneMarker.position = new Vector3(field.GetLaneX(lane), marker.y, marker.z);

            if (PointerInput.IsHeld || activeLevel.AutoLaunch)
            {
                holdTime += Time.deltaTime;
                if (holdTime > 1.5f)
                {
                    hud.SetHint(null);
                }

                launchCredit += activeLevel.LaunchRate * Time.deltaTime;
                while (launchCredit >= 1f && LaunchOne(activeLevel.AutoLaunch ? autoLane++ % ClaybreakLevel.LaneCount : lane))
                {
                    launchCredit -= 1f;
                }
            }

            launchCredit = Mathf.Min(launchCredit, 1f);
            if (tower.IsStanding && crowd.CurrentCount <= 1 && field.InFlight == 0)
            {
                Enter(FlowState.Lost);
            }
        }

        private bool LaunchOne(int lane)
        {
            if (field.InFlight >= field.Capacity || !crowd.TryGetLastFollower(out RunnerUnit follower))
            {
                return false;
            }

            Vector3 from = follower.transform.position;
            crowd.RemoveHitFollower(follower);
            bool brute = brutes > 0 && launches % BruteEvery == 0;
            if (brute)
            {
                brutes--;
                hud.SetBrutes(brutes, true);
            }

            launches++;
            return field.TryLaunch(lane, new Vector3(from.x, from.y, Mathf.Max(from.z, activeLevel.FiringLineZ)), brute);
        }

        private void OnGateCrossed(GateSpec gate)
        {
            int army = GateMath.Apply(gate, crowd.CurrentCount - 1, crowd.MaximumCount - 1);
            if (army == 0)
            {
                crowd.SetCount(1);
                brutes = 0;
                Enter(FlowState.Lost);
                return;
            }

            int before = crowd.CurrentCount - 1;
            crowd.SetCount(army + 1, army > before);
            if (army < before)
            {
                player.Trip();
                RunnerDeathEffects.Play(player.transform.position, Mathf.Min(before - army, 6));
            }

            // Brutes are part of the army: a gate that shrinks it below them takes Brutes too.
            if (brutes > army)
            {
                brutes = army;
                hud.SetBrutes(brutes, true);
            }
        }

        private void OnCratePicked(int units)
        {
            int army = crowd.CurrentCount - 1;
            int added = Mathf.Min(units, crowd.MaximumCount - 1 - army);
            if (added <= 0)
            {
                return;
            }

            crowd.SetCount(army + added + 1, true);
            brutes += added;
            hud.SetBrutes(brutes, true);
            hud.Banner("+" + added + " BRUTE");
        }

        private void OnFloorFallen(int floorsLeft)
        {
            if (activeLevel.ShiftGatesOnFloor)
            {
                field.ShiftGates();
                hud.Banner("FLOOR DOWN! GATES SHIFTED");
                ClaybreakSfx.Play(ClaybreakSfx.Cue.Boss);
            }
            else
            {
                hud.Banner("FLOOR DOWN!");
            }
        }

        private void OnTowerDestroyed()
        {
            if (state == FlowState.Assault)
            {
                Enter(FlowState.Won);
            }
        }

        private void Enter(FlowState next)
        {
            state = next;
            stateTime = 0f;
            switch (next)
            {
                case FlowState.Menu:
                    hud.SetHint(null);
                    break;
                case FlowState.Run:
                    hud.SetHint(progression.LevelIndex == 0 && !deviceGate ? RunHint : null);
                    break;
                case FlowState.Assault:
                    BeginAssault();
                    break;
                case FlowState.Won:
                case FlowState.Lost:
                    EndLevel(next == FlowState.Won);
                    break;
            }

            RaisePhaseChanged(CurrentPhase);
        }

        private static bool StartsWithoutTap
        {
            get
            {
#if UNITY_EDITOR
                return EditorStartsWithoutTap;
#else
                return false;
#endif
            }
        }

        private void BeginAssault()
        {
            armyAtLine = crowd.CurrentCount - 1;
            Debug.Log($"[Claybreak] Run reached the firing line in {levelTime:0.0} s: army {armyAtLine} ({brutes} brutes), "
                + $"leader at y={player.transform.position.y:0.00} (track top {track.bounds.max.y:0.00}).", this);
            player.SetCourseSpeedMultiplier(0f);
            launchCredit = 0f;
            holdTime = 0f;
            GameAudio.Play(GameSound.Finish);
            runCamera.enabled = false;
            Vector3 center = new Vector3(track.bounds.center.x, track.bounds.max.y, activeLevel.FiringLineZ);
            assaultCamera.Begin(center, new Vector3(center.x, center.y, activeLevel.TowerZ), activeLevel.IsBoss);
            laneMarker.gameObject.SetActive(true);
            field.Running = true;
            cannon.Armed = activeLevel.CannonEnabled;
            hud.ShowTower(true, tower.MaximumHealth);
            hud.SetTower(tower.Health);
            hud.SetHint(deviceGate ? null : AssaultHint);
            if (activeLevel.IsBoss && !deviceGate)
            {
                hud.Banner("BOSS! EACH FLOOR SHIFTS THE GATES");
                ClaybreakSfx.Play(ClaybreakSfx.Cue.Boss);
            }
        }

        private void EndLevel(bool won)
        {
            resultShown = false;
            player.SetCourseSpeedMultiplier(0f);
            cannon.Armed = false;
            laneMarker.gameObject.SetActive(false);
            hud.SetHint(null);
            int armyLeft = crowd.CurrentCount - 1 + field.InFlight;
            Debug.Log($"[Claybreak] Level {activeLevel.name} {(won ? "WON" : "LOST")} in {levelTime:0.0} s; army at line {armyAtLine}, "
                + $"left {armyLeft}, tower {tower.Health}/{tower.MaximumHealth}, hits {field.TowerHits}, peak in flight {field.PeakInFlight}, "
                + $"peak projectiles {cannon.PeakActive}/{activeLevel.MaximumProjectiles}.", this);
            if (deviceGate)
            {
                return;
            }

            LevelOutcome outcome = progression.Complete(won, armyAtLine, armyLeft);
            WriteSave();
            hud.SetCoins(progression.Save.coins);
            lastOutcome = outcome;
            lastDetail = won ? armyLeft + " left standing" : "the tower holds";
            Debug.Log($"[Claybreak] Result: {outcome.Stars} stars, +{outcome.Coins} coins (total {progression.Save.coins}), "
                + $"next {progression.CurrentLevel.name}, loop {progression.Save.loop}"
                + (outcome.UnlockedBrute ? ", BRUTE UNLOCKED" : string.Empty)
                + (outcome.ClearedWorld ? ", WORLD CLEARED" : string.Empty) + ".", this);
        }

        // Development builds: one line every 2 s with what the device gate needs on record (units in
        // flight and projectiles in the air), next to the kit's frame timing probe output.
        private void LogDeviceGateSample()
        {
#if DEVELOPMENT_BUILD || RELEASE_MEASUREMENT
            if (Time.unscaledTime < nextDevLog)
            {
                return;
            }

            nextDevLog = Time.unscaledTime + 2f;
            if (state == FlowState.Run)
            {
                Vector3 leader = player.transform.position;
                Debug.Log($"[ClaybreakGate] t={levelTime:0.0} run army={crowd.CurrentCount - 1} leader z={leader.z:0.0} y={leader.y:0.00}");
            }
            else if (state == FlowState.Assault)
            {
                Debug.Log($"[ClaybreakGate] t={levelTime:0.0} tower={activeLevel.Tower} inFlight={field.InFlight} projectiles={cannon.ActiveCount}/"
                    + $"{activeLevel.MaximumProjectiles} reserve={crowd.CurrentCount - 1} towerHealth={tower.Health}");
            }
#endif
        }
    }
}
