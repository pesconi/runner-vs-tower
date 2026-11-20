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
        Ready,
        Run,
        Assault,
        Won,
        Lost
    }

    /// <summary>
    /// Flow of the RT-2 prototype level: tap to start, the short run through the math gates, the
    /// assault at the firing line (L1: drag to aim, hold to stream units), and the result with an
    /// instant retry. Owns the run phase the kit reads: Ready is Menu, Run and Assault are Playing.
    /// In the assault the leader stays at the firing line (course speed 0) and its sideways drag
    /// picks the lane; the army is the leader's followers, launched one by one from the back.
    /// </summary>
    public sealed class ClaybreakFlow : RunPhaseSource
    {
        private const string ReadyHint = "TAP TO START";
        private const string AssaultHint = "DRAG TO AIM - HOLD TO LAUNCH";

        [SerializeField] private PrototypeLevel level;
        [Tooltip("Device-gate level (300 units in flight, projectiles at the limit). Development and measurement builds pick it with `--ez stress true`.")]
        [SerializeField] private PrototypeLevel stressLevel;
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
        [SerializeField] private PrototypeHud hud;
        [SerializeField] private Transform firingLine;
        [SerializeField] private Transform laneMarker;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private FrameRateGovernor frameRateGovernor;

        private PrototypeLevel activeLevel;
        private FlowState state = FlowState.Ready;
        private float launchCredit;
        private int autoLane;
        private float stateTime;
        private float levelTime;
#if DEVELOPMENT_BUILD || RELEASE_MEASUREMENT
        private float nextDevLog;
#endif
        private bool configured;

#if UNITY_EDITOR
        /// <summary>Editor only (PlayMode tests): the next flow to start runs the device-gate level.</summary>
        public static bool EditorUsesDeviceGateLevel { get; set; }
#endif

        public FlowState State => state;
        public AssaultField Field => field;
        public CannonDefense Cannon => cannon;
        public PrototypeLevel ActiveLevel => activeLevel;

        public override RunPhase CurrentPhase
        {
            get
            {
                switch (state)
                {
                    case FlowState.Ready: return RunPhase.Menu;
                    case FlowState.Won: return RunPhase.Won;
                    case FlowState.Lost: return RunPhase.Lost;
                    default: return RunPhase.Playing;
                }
            }
        }

        private void Start()
        {
            activeLevel = PickLevel();
            if (!Configure(out string error))
            {
                Debug.LogError("[Claybreak] The prototype level cannot start: " + error, this);
                enabled = false;
                return;
            }

            configured = true;
            Debug.Log($"[Claybreak] Level '{activeLevel.name}' ready: army {activeLevel.StartCount}, "
                + $"{activeLevel.RunGates.Count} gate pairs, tower {activeLevel.TowerHealth}, "
                + $"projectile limit {activeLevel.MaximumProjectiles}, pool {pool.Capacity}.", this);
            ResetLevel();
        }

        private bool Configure(out string error)
        {
            if (activeLevel == null || theme == null || track == null || player == null || crowd == null
                || pool == null || runCamera == null || assaultCamera == null || gates == null || field == null
                || tower == null || cannon == null || hud == null || firingLine == null || laneMarker == null
                || gameAudio == null || frameRateGovernor == null)
            {
                error = "a scene reference is missing (rebuild the scene with Claybreak > Build Prototype Scene).";
                return false;
            }

            if (!activeLevel.TryValidate(pool.Capacity, out error) || !theme.TryValidate(out error))
            {
                return false;
            }

            CharacterController leader = player.GetComponent<CharacterController>();
            if (!player.TryApplyColorLook(theme.Roster, ClaybreakTheme.CrowdColorId, theme.CrowdColor)
                || !crowd.ApplyColorLook(theme.Roster, ClaybreakTheme.CrowdColorId, theme.CrowdColor)
                || !field.Configure(activeLevel, track, theme, leader, Vector3.one * 0.5f * player.transform.lossyScale.x)
                || !cannon.Configure(activeLevel, field)
                || !gates.Configure(activeLevel, track))
            {
                error = "the crowd look, the assault field, the cannon or the gates did not configure (see the log above).";
                return false;
            }

            RunnerDeathEffects.Configure(theme.DeathEffect, track);
            gameAudio.Configure(this, theme.Audio);
            frameRateGovernor.Configure(this);
            Vector3 line = firingLine.position;
            firingLine.position = new Vector3(line.x, line.y, activeLevel.FiringLineZ);
            Vector3 marker = laneMarker.position;
            laneMarker.position = new Vector3(marker.x, marker.y, activeLevel.FiringLineZ + laneMarker.localScale.z * 0.5f);

            gates.GateCrossed += OnGateCrossed;
            tower.Destroyed += OnTowerDestroyed;
            tower.HealthChanged += health => hud.SetTower(health, true);
            crowd.CountChanged += count => hud.SetArmy(count - 1);
            hud.RetryPressed += OnRetry;
            error = null;
            return true;
        }

        private PrototypeLevel PickLevel()
        {
#if UNITY_EDITOR
            if (EditorUsesDeviceGateLevel || (useStressLevelInEditor && stressLevel != null))
            {
                return stressLevel != null ? stressLevel : level;
            }
#elif (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID
            if (stressLevel != null && ReadStressExtra())
            {
                return stressLevel;
            }
#endif
            return level;
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

        private void ResetLevel()
        {
            field.ResetField();
            cannon.ResetCannon();
            gates.ResetCourse();
            RunnerDeathEffects.ClearAll();
            player.ResetForNewRun();
            player.SetCourseSpeedMultiplier(activeLevel.RunSpeed);
            crowd.SetCount(activeLevel.StartCount + 1);
            crowd.SnapFollowersToFormation();
            assaultCamera.Stop();
            runCamera.enabled = true;
            laneMarker.gameObject.SetActive(false);
            hud.HideResult();
            hud.SetArmy(crowd.CurrentCount - 1);
            hud.SetTower(tower.Health, false);
            levelTime = 0f;
            Enter(FlowState.Ready);
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
                case FlowState.Ready:
                    if ((stateTime > 0.2f && PointerInput.WasPressedThisFrame)
                        || (activeLevel.AutoLaunch && stateTime > 1f))
                    {
                        Enter(FlowState.Run);
                    }

                    break;
                case FlowState.Run:
                    gates.Track(player.transform.position);
                    if (state == FlowState.Run && player.transform.position.z >= activeLevel.FiringLineZ)
                    {
                        Enter(FlowState.Assault);
                    }

                    break;
                case FlowState.Assault:
                    UpdateAssault();
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
                launchCredit += activeLevel.LaunchRate * Time.deltaTime;
                while (launchCredit >= 1f && LaunchOne(activeLevel.AutoLaunch ? autoLane++ % PrototypeLevel.LaneCount : lane))
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
            return field.TryLaunch(lane, new Vector3(from.x, from.y, Mathf.Max(from.z, activeLevel.FiringLineZ)));
        }

        private void OnGateCrossed(GateSpec gate)
        {
            int army = GateMath.Apply(gate, crowd.CurrentCount - 1, crowd.MaximumCount - 1);
            if (army == 0)
            {
                crowd.SetCount(1);
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
        }

        private void OnTowerDestroyed()
        {
            if (state == FlowState.Assault)
            {
                Enter(FlowState.Won);
            }
        }

        private void OnRetry()
        {
            if (state == FlowState.Won || state == FlowState.Lost)
            {
                GameAudio.Play(GameSound.ButtonClick);
                ResetLevel();
            }
        }

        private void Enter(FlowState next)
        {
            state = next;
            stateTime = 0f;
            switch (next)
            {
                case FlowState.Ready:
                    hud.SetHint(ReadyHint);
                    break;
                case FlowState.Run:
                    hud.SetHint(null);
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

        private void BeginAssault()
        {
            player.SetCourseSpeedMultiplier(0f);
            launchCredit = 0f;
            GameAudio.Play(GameSound.Finish);
            runCamera.enabled = false;
            Vector3 center = new Vector3(track.bounds.center.x, track.bounds.max.y, activeLevel.FiringLineZ);
            assaultCamera.Begin(center, new Vector3(center.x, center.y, activeLevel.TowerZ));
            laneMarker.gameObject.SetActive(true);
            field.Running = true;
            cannon.Armed = true;
            hud.SetTower(tower.Health, true);
            hud.SetHint(AssaultHint);
        }

        private void EndLevel(bool won)
        {
            player.SetCourseSpeedMultiplier(0f);
            cannon.Armed = false;
            laneMarker.gameObject.SetActive(false);
            hud.SetHint(null);
            int army = crowd.CurrentCount - 1 + field.InFlight;
            string detail = won ? army + " left standing" : "the tower holds";
            hud.ShowResult(won, detail);
            Debug.Log($"[Claybreak] Level {(won ? "WON" : "LOST")} in {levelTime:0.0} s; army left {army}, "
                + $"tower {tower.Health}/{tower.MaximumHealth}, peak in flight {field.PeakInFlight}, "
                + $"peak projectiles {cannon.PeakActive}/{activeLevel.MaximumProjectiles}.", this);
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
            if (state == FlowState.Assault)
            {
                Debug.Log($"[ClaybreakGate] t={levelTime:0.0} inFlight={field.InFlight} projectiles={cannon.ActiveCount}/"
                    + $"{activeLevel.MaximumProjectiles} reserve={crowd.CurrentCount - 1} tower={tower.Health}");
            }
#endif
        }
    }
}
