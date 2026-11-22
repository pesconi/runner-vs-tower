using System;
using JungleDegree.StudioKit.Analytics;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
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
        Lost,
        /// <summary>The army fell in the assault and a rewarded continue is offered (RT-7, once per level).</summary>
        ContinueOffer
    }

    /// <summary>
    /// Flow of the game: home menu (with the shop and the world list, RT-5), the short run through the math
    /// gates (and the special-unit crates of the loadout), the assault at the firing line (L1: drag to aim,
    /// hold to stream units; a special goes out every few launches), and the result, which saves the
    /// progress and offers NEXT / AGAIN in one tap. Upgrades, loadout and skin come from
    /// <see cref="Progression"/> when a level is prepared; the device gate runs the maximum loadout.
    /// Owns the run phase the kit reads: Menu is Menu, Run and Assault are Playing. RT-6t.4: the army on the track is
    /// the instanced crowd (<see cref="RunCrowd"/>, no object per unit, no leader); its formation center crosses the
    /// gates and crates. In the assault the crowd stays at the firing line (speed 0) and its sideways drag picks the
    /// lane; units are launched one by one from its outer ring into the assault field (still the kit's runner pool
    /// until RT-6t.7). The whole world lives in one scene: changing level re-places preallocated objects, never
    /// creates them.
    /// RT-7: the rewarded uses of GDD 7.2 (double the coins on the result, continue once per level after the army
    /// falls in the assault; both optional, gain written on the button, a clear NO THANKS), the interstitial
    /// between levels (off by configuration, D6), the daily reward on the home screen (D5), the privacy options
    /// entry point (UMP) and the minimum measurement events (D4, no SDK yet).
    /// </summary>
    // After the crowd moves (RunCrowd, -50) and before the swarm schedules its frame (0): counts and restarts made here are
    // drawn this same frame.
    [DefaultExecutionOrder(-40)]
    public sealed class ClaybreakFlow : RunPhaseSource
    {
        private const string RunHint = "DRAG TO PICK A GATE";
        private const string AssaultHint = "DRAG TO AIM - HOLD TO LAUNCH";
        // The device gate streams specials for as long as it runs (never written anywhere).
        private const int DeviceGateSpecials = 1_000_000;
        private const float WonResultDelay = 1.5f;
        private const float LostResultDelay = 0.8f;
        // Boss rule "overheat": each fallen floor makes the cannon and the sweeper this much quicker.
        private const float OverheatFactor = 0.75f;
        // The device-gate run loops back to the start this far before the tower (the scenery and the tower stay ahead).
        private const float RunGateTowerGap = 10f;

        [SerializeField] private ClaybreakCatalog catalog;
        [Tooltip("Device-gate level per catalog world (300 units in flight, every defense at its limit, the world's heaviest tower, its scenery). Development and measurement builds pick it with `--ez stress true [--ei world N]` (N from 1); `--ez run true` measures its run instead (the crowd at its cap, looping, RT-6t.4).")]
        [SerializeField] private ClaybreakLevel[] stressLevels = new ClaybreakLevel[0];
        [SerializeField] private bool useStressLevelInEditor;
        [SerializeField, Min(1)] private int stressWorldInEditor = 1;
        [SerializeField] private WorldScenery scenery;
        [SerializeField] private ClaybreakTheme theme;
        [SerializeField] private Renderer track;
        [SerializeField] private RunCrowd crowd;
        [Tooltip("The camera rig's fixed shot of the assault (RT-6t.7): lanes, defenses and tower from behind the firing line.")]
        [SerializeField] private SwarmCameraShot assaultShot;
        [Tooltip("The assault shot of a boss level (higher and farther back: the boss tower is taller).")]
        [SerializeField] private SwarmCameraShot bossAssaultShot;
        [SerializeField] private RunGateCourse gates;
        [SerializeField] private AssaultField field;
        [SerializeField] private AssaultTower tower;
        [SerializeField] private CannonDefense cannon;
        [SerializeField] private ClaybreakHud hud;
        [SerializeField] private ClaybreakScreens screens;
        [SerializeField] private Transform firingLine;
        [SerializeField] private Transform laneMarker;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private FrameRateGovernor frameRateGovernor;
        [SerializeField] private ClaybreakMonetization monetization;
        [SerializeField] private ClaybreakAds ads;

        private IAdGateway adGateway;
        private bool continueUsed;
        private int continueUnits;
        private bool continueAdPending;
        private bool resultBonusTaken;
        private bool resultAdOffered;
        private bool leavingResult;

        private Progression progression;
        private LevelOutcome lastOutcome;
        private string lastDetail;
        private ClaybreakLevel activeLevel;
        private FlowState state = FlowState.Menu;
        private bool deviceGate;
        private bool deviceGateRun;
        private int deviceGateWorld;
        private bool resultShown;
        private float launchCredit;
        private int autoLane;
        private int launches;
        private readonly UnitType[] loadout = new UnitType[ClaybreakCatalog.MaximumLoadoutSlots];
        private readonly int[] specials = new int[ClaybreakCatalog.MaximumLoadoutSlots];
        private int loadoutCount;
        private int nextSpecialSlot;
        private RunLoadout runLoadout = RunLoadout.None;
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

        /// <summary>Editor only (PlayMode tests): the device-gate level measures its run (the crowd at its cap, looping).</summary>
        public static bool EditorUsesDeviceGateRun { get; set; }

        /// <summary>Editor only (PlayMode tests): the world (from 1) whose device-gate level runs.</summary>
        public static int EditorDeviceGateWorld { get; set; } = 1;

        /// <summary>Editor only (PlayMode tests): the next flow presses PLAY by itself.</summary>
        public static bool EditorStartsWithoutTap { get; set; }

        /// <summary>Editor only (PlayMode tests): the save is not read nor written.</summary>
        public static bool EditorUsesFreshSave { get; set; }

        /// <summary>Editor only (PlayMode tests): stands in for the scene's ads (the SDK has no ads in the Editor).</summary>
        public static IAdGateway EditorAds { get; set; }
#endif

        public FlowState State => state;
        public AssaultField Field => field;
        public RunCrowd Crowd => crowd;
        public RunGateCourse Gates => gates;
        public ClaybreakHud Hud => hud;

        /// <summary>True when the device gate measures the run (the crowd loops at its cap and never reaches the assault).</summary>
        public bool MeasuresRun => deviceGate && deviceGateRun;
        public CannonDefense Cannon => cannon;
        public AssaultTower Tower => tower;
        public ClaybreakLevel ActiveLevel => activeLevel;
        public Progression Progress => progression;
        public ClaybreakScreens Screens => screens;
        public WorldScenery Scenery => scenery;
        private ClaybreakLevel StressLevel => stressLevels[deviceGateWorld];
        public RunLoadout ActiveLoadout => runLoadout;
        public int LoadoutCount => loadoutCount;
        public LevelOutcome LastOutcome => lastOutcome;

        /// <summary>Special units still in the army (all loadout slots).</summary>
        public int Specials
        {
            get
            {
                int total = 0;
                for (int i = 0; i < loadoutCount; i++)
                {
                    total += specials[i];
                }

                return total;
            }
        }

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
            ApplyMeasurementExtras();
            deviceGate = PickDeviceGate(out deviceGateWorld, out deviceGateRun);
            if (!Configure(out string error))
            {
                Debug.LogError("[Claybreak] The game cannot start: " + error, this);
                enabled = false;
                return;
            }

            configured = true;
            RunLoadout ready = progression.Loadout;
            Debug.Log($"[Claybreak] Ready: {catalog.Worlds.Count} world(s), {progression.World.WorldName}, next {progression.CurrentLevel.name}, "
                + $"coins {progression.Save.coins}, stars {progression.TotalStars}, army +{ready.StartingArmy}, damage x{ready.DamageMultiplier:0.##}, "
                + $"gates +{ready.GateBonus}, launch x{ready.LaunchRateMultiplier:0.##}, loadout [{string.Join(", ", progression.Save.loadout)}], "
                + $"skin {progression.Skin.id}, crowd cap {crowd.MaximumArmy} (shape {progression.Shape.id}), units in flight {field.FlightCapacity}"
                + (deviceGate ? $", DEVICE GATE world {deviceGateWorld + 1} {(deviceGateRun ? "RUN" : "assault")} (maximum loadout)" : string.Empty) + ".", this);
            if (deviceGate)
            {
#if DEVELOPMENT_BUILD || RELEASE_MEASUREMENT
                // RT-8: the gate is measured with sound, music and vibration on, whatever the phone had saved.
                GameAudio.SetSoundMuted(false);
                GameAudio.SetMusicMuted(false);
                GameHaptics.SetEnabled(true);
#endif
                StartLevel(StressLevel);
            }
            else
            {
                StartSession();
                ShowMenu();
            }
        }

        // First launch day (D1/D7) and the session event; it waits for the consent answer before leaving the device (D4, D21).
        private void StartSession()
        {
            ClaybreakMetrics.Install();
            int today = DailyReward.Today();
            progression.RecordFirstDay(today);
            WriteSave();
            PlayerMetrics.SessionStarted(DailyReward.DaysSinceInstall(progression.Save, today));
        }

        private bool Configure(out string error)
        {
            if (catalog == null || stressLevels == null || scenery == null || theme == null || track == null || crowd == null
                || assaultShot == null || bossAssaultShot == null || gates == null || field == null
                || tower == null || cannon == null || hud == null || screens == null
                || firingLine == null || laneMarker == null || gameAudio == null || frameRateGovernor == null
                || monetization == null || ads == null)
            {
                error = "a scene reference is missing (rebuild the scene with Claybreak > Build Scene).";
                return false;
            }

            if (!crowd.TryValidate(out error) || !theme.TryValidate(out error))
            {
                return false;
            }

            if (!field.Initialize(track, theme))
            {
                error = "the assault field did not configure (see the log above).";
                return false;
            }

            if (!catalog.TryValidate(crowd.MaximumArmy, field.FlightCapacity, out error) || !tower.TryValidate(out error)
                || !scenery.TryValidate(catalog.Worlds.Count, out error) || !monetization.TryValidate(out error))
            {
                return false;
            }

            if (stressLevels.Length != catalog.Worlds.Count)
            {
                error = $"one device-gate level per world is expected ({stressLevels.Length} for {catalog.Worlds.Count}).";
                return false;
            }

            foreach (ClaybreakLevel stress in stressLevels)
            {
                if (stress == null || !stress.TryValidate(crowd.MaximumArmy, field.FlightCapacity, out error))
                {
                    error = "device-gate level: " + (error ?? "missing");
                    return false;
                }
            }

            deviceGateWorld = Mathf.Clamp(deviceGateWorld, 0, stressLevels.Length - 1);

            progression = new Progression(catalog, LoadSave());
            UseShape(progression.Shape.mesh);
            DressArmy(progression.WorldIndex);
            gameAudio.Configure(this, theme.Audio);
            ClaybreakSfx.Configure(theme.Audio);
            frameRateGovernor.Configure(this);

            gates.GateCrossed += OnGateCrossed;
            gates.CratePicked += OnCratePicked;
            tower.Destroyed += OnTowerDestroyed;
            tower.HealthChanged += hud.SetTower;
            tower.FloorFallen += OnFloorFallen;
            tower.BlockDropped += OnBlockDropped;
            field.Shield.Fallen += OnShieldFallen;
            crowd.ArmyChanged += hud.SetArmy;
            screens.PlayPressed += () => StartLevel(progression.CurrentLevel);
            screens.ShopPressed += () => screens.ShowShop(progression);
            screens.WorldsPressed += () => screens.ShowWorlds(progression);
            screens.ShapePressed += OnShape;
            screens.BackPressed += ShowMenu;
            screens.ShopItemPressed += OnShopItem;
            screens.WorldPressed += OnWorld;
            screens.PrimaryPressed += OnPrimary;
            screens.HomePressed += ShowMenu;
            screens.DailyPressed += OnDaily;
            screens.PrivacyPressed += () => adGateway.ShowPrivacyOptions();
            screens.SetPrivacyPolicyUrl(monetization.PrivacyPolicyUrl);
            screens.ResultAdPressed += OnResultAd;
            screens.ContinueAdPressed += OnContinueAd;
            screens.ContinueDeclined += OnContinueDeclined;
            adGateway = ads;
#if UNITY_EDITOR
            adGateway = EditorAds ?? adGateway;
#endif
            adGateway.Changed += OnAdsChanged;
            error = null;
            return true;
        }

        private bool PickDeviceGate(out int world, out bool run)
        {
#if UNITY_EDITOR
            world = (EditorUsesDeviceGateLevel ? EditorDeviceGateWorld : stressWorldInEditor) - 1;
            run = EditorUsesDeviceGateLevel && EditorUsesDeviceGateRun;
            return EditorUsesDeviceGateLevel || useStressLevelInEditor;
#elif (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID
            return ReadStressExtra(out world, out run);
#else
            world = 0;
            run = false;
            return false;
#endif
        }

        // Development and measurement builds: `--ei fps N` lifts the frame rate cap and attaches the kit's frame timing
        // probe ([DevTiming] CPU/GPU per window); `--ei profileAt S [--ei profileSeconds N]` records a profiler log. The
        // kit's per-unit crowd did this until RT-6t.4; the game owns it now. Release builds compile it to nothing.
        private void ApplyMeasurementExtras()
        {
#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                int frameRate = intent.Call<int>("getIntExtra", "fps", 0);
                if (frameRate <= 0)
                {
                    return;
                }

                FrameRateGovernor.ExternallyControlled = true;
                Application.targetFrameRate = frameRate;
                JungleDegree.StudioKit.Crowd.DevFrameTimingProbe probe = gameObject.AddComponent<JungleDegree.StudioKit.Crowd.DevFrameTimingProbe>(); // load-time: measurement probe, once per scene (rule 2.2.1)
                int profileAt = intent.Call<int>("getIntExtra", "profileAt", -1);
                if (profileAt >= 0)
                {
                    probe.ConfigureProfileCapture(profileAt, Mathf.Max(1, intent.Call<int>("getIntExtra", "profileSeconds", 5)));
                }

                Debug.Log($"[Claybreak] Measurement: target frame rate {frameRate}, frame timing probe on.", this);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Claybreak] Could not read the measurement extras: " + exception.Message);
            }
#endif
        }

#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
        // Development builds only: `adb shell am start ... --ez stress true [--ei world N] [--ez run true]` runs the
        // device-gate level of world N (from 1; default 1), its assault or (run) its run.
        private static bool ReadStressExtra(out int world, out bool run)
        {
            world = 0;
            run = false;
            try
            {
                using AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                world = intent.Call<int>("getIntExtra", "world", 1) - 1;
                run = intent.Call<bool>("getBooleanExtra", "run", false);
                return intent.Call<bool>("getBooleanExtra", "stress", false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Claybreak] Could not read the stress extra: " + exception.Message);
                return false;
            }
        }
#endif

        // The device gate always runs the maximum loadout (RT-5) and never reads nor writes the player's save.
        private SaveData LoadSave()
        {
            if (deviceGate)
            {
                return Progression.MaximumLoadout(catalog);
            }
#if UNITY_EDITOR
            if (EditorUsesFreshSave)
            {
                return new SaveData();
            }
#endif
            return SaveStore.Load();
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
            screens.ShowHome(progression);
            int today = DailyReward.Today();
            screens.SetDaily(!deviceGate && progression.CanClaimDaily(today)
                ? DailyReward.NextAmount(progression.Save, monetization.DailyRewards) : 0);
            screens.SetPrivacyVisible(adGateway.PrivacyOptionsRequired);
            Enter(FlowState.Menu);
        }

        // RT-6t.6b (D26 A): the home screen's shape button walks through the free crowd shapes; the crowd waiting behind the
        // menu changes at once (a mesh reference swap, nothing created).
        private void OnShape()
        {
            if (state != FlowState.Menu || deviceGate || !progression.TrySelectNextShape())
            {
                return;
            }

            UseShape(progression.Shape.mesh);
            WriteSave();
            Debug.Log($"[Claybreak] Crowd shape: {progression.Shape.id}.", this);
            screens.ShowShape(progression);
        }

        private void OnDaily()
        {
            if (state != FlowState.Menu || deviceGate
                || !progression.TryClaimDaily(DailyReward.Today(), monetization.DailyRewards, out int coins))
            {
                return;
            }

            GameAudio.Play(GameSound.AdReward);
            WriteSave();
            Debug.Log($"[Claybreak] Daily reward: +{coins} coins (claim {progression.Save.dailyClaims}), total {progression.Save.coins}.", this);
            ShowMenu();
        }

        // ---------- rewarded and interstitial (RT-7) ----------

        private void OnAdsChanged()
        {
            if (state == FlowState.Menu)
            {
                screens.SetPrivacyVisible(adGateway.PrivacyOptionsRequired);
            }

            RefreshResultAd();
        }

        private bool OnResult => (state == FlowState.Won || state == FlowState.Lost) && resultShown;

        // Double the coins of the level: offered only on a result that paid coins, once, and only when an ad is loaded.
        private void RefreshResultAd()
        {
            if (!OnResult)
            {
                return;
            }

            bool offer = !deviceGate && monetization.DoubleCoins && !resultBonusTaken && !leavingResult && lastOutcome.Coins > 0
                && adGateway.RewardedReady;
            screens.SetResultAd(offer ? $"WATCH AD:  x2 COINS  +{lastOutcome.Coins} ¢" : null);
            if (offer && !resultAdOffered)
            {
                resultAdOffered = true;
                PlayerMetrics.RewardedAd(ClaybreakMetrics.DoubleCoinsPlacement, PlayerMetrics.Offered);
            }
        }

        private void OnResultAd()
        {
            if (!OnResult || resultBonusTaken || leavingResult)
            {
                return;
            }

            resultBonusTaken = true;
            screens.SetResultAd(null);
            PlayerMetrics.RewardedAd(ClaybreakMetrics.DoubleCoinsPlacement, PlayerMetrics.Accepted);
            ShowRewarded(earned =>
            {
                PlayerMetrics.RewardedAd(ClaybreakMetrics.DoubleCoinsPlacement, earned ? PlayerMetrics.Earned : PlayerMetrics.NotEarned);
                if (!earned)
                {
                    resultBonusTaken = false;
                    RefreshResultAd();
                    return;
                }

                progression.GrantBonusCoins(lastOutcome.Coins);
                WriteSave();
                hud.SetCoins(progression.Save.coins);
                screens.SetResultReward($"+{lastOutcome.Coins * 2} ¢  (x2)");
                GameAudio.Play(GameSound.AdReward);
                Debug.Log($"[Claybreak] Rewarded: coins doubled (+{lastOutcome.Coins}), total {progression.Save.coins}.", this);
            });
        }

        // Continue: the army fell in the assault; offered once per level, only with an ad loaded and a real army.
        private bool TryOfferContinue()
        {
            if (deviceGate || continueUsed || !monetization.ContinueOnce || armyAtLine < monetization.ContinueMinimumArmy
                || !adGateway.RewardedReady)
            {
                return false;
            }

            continueUsed = true;
            continueUnits = Mathf.Min(monetization.ContinueArmy(armyAtLine), crowd.MaximumArmy);
            Enter(FlowState.ContinueOffer);
            return true;
        }

        private void OnContinueAd()
        {
            // One ad per offer: a second tap before the ad covers the screen must not end the offer while the
            // first ad plays (its reward would then be lost).
            if (state != FlowState.ContinueOffer || continueAdPending)
            {
                return;
            }

            continueAdPending = true;
            PlayerMetrics.RewardedAd(ClaybreakMetrics.ContinuePlacement, PlayerMetrics.Accepted);
            ShowRewarded(earned =>
            {
                continueAdPending = false;
                if (state != FlowState.ContinueOffer)
                {
                    return;
                }

                PlayerMetrics.RewardedAd(ClaybreakMetrics.ContinuePlacement, earned ? PlayerMetrics.Earned : PlayerMetrics.NotEarned);
                if (earned)
                {
                    ResumeAssault();
                }
                else
                {
                    Enter(FlowState.Lost);
                }
            });
        }

        private void OnContinueDeclined()
        {
            if (state == FlowState.ContinueOffer && !continueAdPending)
            {
                Enter(FlowState.Lost);
            }
        }

        // Back to the firing line with the units of the continue; the tower keeps its damage, the stars still count
        // the army that first reached the line.
        private void ResumeAssault()
        {
            screens.HideAll();
            hud.Show(true);
            crowd.SetArmy(continueUnits);
            crowd.Steering = true;
            state = FlowState.Assault;
            stateTime = 0f;
            launchCredit = 0f;
            holdTime = 0f;
            field.Running = true;
            cannon.Armed = activeLevel.CannonEnabled;
            laneMarker.gameObject.SetActive(true);
            hud.Banner("BACK IN THE FIGHT!");
            Debug.Log($"[Claybreak] Continue: +{continueUnits} units at the firing line.", this);
            RaisePhaseChanged(CurrentPhase);
        }

        private void ShowRewarded(Action<bool> earned)
        {
            gameAudio.SetExternalPause(true);
            adGateway.ShowRewarded(result =>
            {
                gameAudio.SetExternalPause(false);
                earned(result);
            });
        }

        // Shop rows: buy, equip or wear (ShopTab says which list the index belongs to).
        private void OnShopItem(ShopTab tab, int index)
        {
            if (state != FlowState.Menu)
            {
                return;
            }

            bool changed;
            bool bought;
            switch (tab)
            {
                case ShopTab.Upgrades:
                    changed = bought = progression.TryBuyUpgrade(index);
                    break;
                case ShopTab.Units:
                    bought = progression.TryBuyUnit(index);
                    changed = bought || progression.TryToggleUnit(index);
                    break;
                default:
                    bought = progression.TryBuySkin(index);
                    // The level prepared below dresses the army in the new color.
                    changed = bought || progression.TryWearSkin(index);
                    break;
            }

            if (!changed)
            {
                return;
            }

            GameAudio.Play(bought ? GameSound.Coin : GameSound.ButtonClick);
            WriteSave();
            Debug.Log($"[Claybreak] Shop: {tab} #{index} {(bought ? "bought" : "toggled")}; coins {progression.Save.coins}, "
                + $"loadout [{string.Join(", ", progression.Save.loadout)}], skin {progression.Save.skin}.", this);
            PrepareLevel(progression.CurrentLevel);
            screens.ShowShop(progression);
        }

        private void OnWorld(int index)
        {
            if (state == FlowState.Menu && progression.TrySelectWorld(index))
            {
                WriteSave();
                ShowMenu();
            }
        }

        // The army's color in the world shown (RT-6t.6a: the default skin wears the world's palette color, any other skin
        // its own): the run crowd wears it as its tint and the assault's plain units as theirs (RT-6t.7, per instance).
        // The leader wears the crowd's color and a crown in the world's UI accent (RT-6t.13).
        private void DressArmy(int world)
        {
            Color color = progression.CrowdColorIn(world);
            crowd.Wear(color);
            field.Wear(color);
            ToonPalette palette = catalog.Worlds[world].world.Look.palette;
            if (palette != null)
            {
                crowd.CrownLeader(palette.UiAccent);
            }
        }

        // The crowd shape picked in the menu (RT-6t.6b), in the run and in the assault alike (RT-6t.7).
        private void UseShape(Mesh shape)
        {
            crowd.UseShape(shape);
            field.UseShape(shape);
        }

        // A falling tower floor throws units off where each block stood (the crowd's ballistic death).
        private void OnBlockDropped(Vector3 position)
        {
            field.ThrowOff(position, AssaultTower.UnitsPerBlock);
        }

        private void OnPrimary()
        {
            if (!OnResult || leavingResult)
            {
                return;
            }

            // After a win the progress already points at the next level; after a loss, the same one. Between two
            // levels after a win the interstitial may show (off by configuration, D6; never right after a
            // rewarded ad the player chose to watch on this result).
            if (!lastOutcome.Won || resultBonusTaken)
            {
                StartLevel(progression.CurrentLevel);
                return;
            }

            leavingResult = true;
            screens.SetResultAd(null);
            gameAudio.SetExternalPause(true);
            adGateway.ShowInterstitialIfDue(() =>
            {
                gameAudio.SetExternalPause(false);
                StartLevel(progression.CurrentLevel);
            });
        }

        // ---------- level ----------

        // Places the level (tower, defenses, gates, army) behind the menu, so the home screen shows it.
        private void PrepareLevel(ClaybreakLevel level)
        {
            activeLevel = level;
            bool played = !level.IsDeviceGateLevel;
            runLoadout = progression.Loadout;
            loadoutCount = progression.FillLoadout(loadout);
            int world = played ? progression.WorldIndex : deviceGateWorld;
            field.ConfigureLoadout(loadout, progression.CrowdColorIn(world), runLoadout.DamageMultiplier);
            int towerHealth = played ? progression.ScaleHealth(level.TowerHealth) : level.TowerHealth;
            int wallHealth = played ? progression.ScaleHealth(level.WallHealth) : level.WallHealth;
            int shieldHealth = played ? progression.ScaleHealth(level.ShieldHealth) : level.ShieldHealth;
            scenery.Apply(world, catalog.Worlds[world].world.Look);
            field.ResetField();
            DressArmy(world);
            field.Configure(level, track, wallHealth, shieldHealth, level.DamagePerUnit);
            tower.ResetTower(level.Tower, towerHealth, level.Floors);
            if (!cannon.Configure(level, field, tower.Muzzle)
                || !gates.Configure(level, track, played && loadoutCount > 0, runLoadout.GateBonus))
            {
                Debug.LogError("[Claybreak] The cannon or the run gates did not configure for " + level.name, this);
            }

            crowd.FollowWithCamera();
            crowd.ResetRun(level.StartCount + (played ? runLoadout.StartingArmy : 0));
            for (int i = 0; i < specials.Length; i++)
            {
                specials[i] = played || i >= loadoutCount ? 0 : DeviceGateSpecials;
            }

            nextSpecialSlot = 0;
            launches = 0;
            holdTime = 0f;
            Vector3 line = firingLine.position;
            firingLine.position = new Vector3(line.x, line.y, level.FiringLineZ);
            Vector3 marker = laneMarker.position;
            laneMarker.position = new Vector3(marker.x, marker.y, level.FiringLineZ + laneMarker.localScale.z * 0.5f);
            laneMarker.gameObject.SetActive(false);
            hud.SetLevel(level.Title);
            hud.SetCoins(progression.Save.coins);
            hud.SetArmy(crowd.Army);
            hud.SetSpecials(0, false);
            hud.ShowTower(false, towerHealth);
            hud.ClearBanner();
            levelTime = 0f;
        }

        private void StartLevel(ClaybreakLevel level)
        {
            PrepareLevel(level);
            continueUsed = false;
            continueAdPending = false;
            leavingResult = false;
            screens.HideAll();
            hud.Show(true);
            if (!deviceGate)
            {
                PlayerMetrics.LevelStarted(ClaybreakMetrics.LevelId(progression.World.WorldId, progression.LevelIndex),
                    progression.WorldIndex + 1, progression.LevelIndex + 1);
            }

            crowd.Speed = level.RunSpeed;
            crowd.Steering = true;
            Debug.Log($"[Claybreak] Level '{level.name}' starts: army {crowd.Army}, {level.RunGates.Count} gate pairs, "
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

                    Vector3 center = crowd.Position;
                    if (MeasuresRun)
                    {
                        // The device gate keeps the crowd at its cap on the track: back to the start before the tower.
                        if (center.z >= activeLevel.TowerZ - RunGateTowerGap)
                        {
                            crowd.RestartRun();
                        }

                        break;
                    }

                    gates.Track(center);
                    if (state == FlowState.Run && center.z >= activeLevel.FiringLineZ)
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
                            StartLevel(StressLevel);
                        }
                        else
                        {
                            hud.Show(false);
                            screens.ShowResult(lastOutcome, lastDetail, progression.CurrentLevel.IsBoss);
                            resultBonusTaken = false;
                            resultAdOffered = false;
                            RefreshResultAd();
                        }
                    }

                    break;
            }

            LogDeviceGateSample();
        }

        private void UpdateAssault()
        {
            int lane = field.GetNearestLane(crowd.Position.x);
            Vector3 marker = laneMarker.position;
            laneMarker.position = new Vector3(field.GetLaneX(lane), marker.y, marker.z);

            if (PointerInput.IsHeld || activeLevel.AutoLaunch)
            {
                holdTime += Time.deltaTime;
                if (holdTime > 1.5f)
                {
                    hud.SetHint(null);
                }

                launchCredit += activeLevel.LaunchRate * runLoadout.LaunchRateMultiplier * Time.deltaTime;
                while (launchCredit >= 1f && LaunchOne(activeLevel.AutoLaunch ? autoLane++ % ClaybreakLevel.LaneCount : lane))
                {
                    launchCredit -= 1f;
                }
            }

            launchCredit = Mathf.Min(launchCredit, 1f);
            if (tower.IsStanding && crowd.Army == 0 && field.InFlight == 0 && !TryOfferContinue())
            {
                Enter(FlowState.Lost);
            }
        }

        private bool LaunchOne(int lane)
        {
            if (field.InFlight >= field.Capacity || !crowd.TryTakeOuterUnit(out Vector3 from))
            {
                return false;
            }

            int slot = launches % catalog.SpecialEvery == 0 ? TakeSpecial() : -1;
            launches++;
            return field.TryLaunch(lane, new Vector3(from.x, from.y, Mathf.Max(from.z, activeLevel.FiringLineZ)), slot);
        }

        // Round robin over the loadout slots that still hold specials; -1 when none is left.
        private int TakeSpecial()
        {
            for (int tried = 0; tried < loadoutCount; tried++)
            {
                int slot = nextSpecialSlot;
                nextSpecialSlot = (nextSpecialSlot + 1) % loadoutCount;
                if (specials[slot] > 0)
                {
                    specials[slot]--;
                    hud.SetSpecials(Specials, true);
                    return slot;
                }
            }

            return -1;
        }

        private void OnGateCrossed(GateSpec gate)
        {
            int before = crowd.Army;
            int army = GateMath.Apply(gate, before, crowd.MaximumArmy);
            hud.FloatGate(gate);
            // A bad gate throws the outer ring out in the ballistic death and shakes the camera.
            crowd.SetArmy(army, SwarmRemoval.Die);
            if (army < before)
            {
                crowd.Shake();
            }

            if (army == 0)
            {
                TrimSpecials(0);
                Enter(FlowState.Lost);
                return;
            }

            // Specials are part of the army: a gate that shrinks it below them takes specials too.
            if (Specials > army)
            {
                TrimSpecials(army);
            }
        }

        // Takes specials away (the largest slot first) until at most <paramref name="army"/> are left.
        private void TrimSpecials(int army)
        {
            int total = Specials;
            while (total > army)
            {
                int largest = 0;
                for (int i = 1; i < loadoutCount; i++)
                {
                    largest = specials[i] > specials[largest] ? i : largest;
                }

                specials[largest]--;
                total--;
            }

            hud.SetSpecials(total, loadoutCount > 0);
        }

        // A crate holds specials of one loadout slot: crates alternate between the equipped types.
        private void OnCratePicked(int crate, int units)
        {
            int army = crowd.Army;
            int added = Mathf.Min(units, crowd.MaximumArmy - army);
            if (added <= 0 || loadoutCount == 0)
            {
                return;
            }

            int slot = crate % loadoutCount;
            crowd.SetArmy(army + added);
            specials[slot] += added;
            hud.SetSpecials(Specials, true);
            hud.Banner("+" + added + " " + loadout[slot].title);
        }

        // The boss rule of the level (GDD 4.4), one per world; a plain tower just loses the floor.
        private void OnFloorFallen(int floorsLeft)
        {
            switch (activeLevel.BossRule)
            {
                case BossRule.ShiftGates:
                    field.ShiftGates();
                    break;
                case BossRule.ShieldReturns:
                    field.RaiseShield();
                    break;
                case BossRule.Overheat:
                    cannon.Overheat(OverheatFactor);
                    field.SpeedUpSweeper(OverheatFactor);
                    break;
                case BossRule.TurnGate:
                    field.TurnBestGate();
                    break;
                default:
                    hud.Banner("FLOOR DOWN!");
                    return;
            }

            hud.Banner(BossRules.FloorBanner(activeLevel.BossRule));
            ClaybreakSfx.Play(ClaybreakSfx.Cue.Boss);
        }

        private void OnShieldFallen()
        {
            if (state == FlowState.Assault)
            {
                hud.Banner("SHIELD DOWN!");
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
                case FlowState.ContinueOffer:
                    crowd.Steering = false;
                    cannon.Armed = false;
                    laneMarker.gameObject.SetActive(false);
                    hud.SetHint(null);
                    hud.Show(false);
                    screens.ShowContinue(continueUnits);
                    PlayerMetrics.RewardedAd(ClaybreakMetrics.ContinuePlacement, PlayerMetrics.Offered);
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
            armyAtLine = crowd.Army;
            Vector3 center = crowd.Position;
            Debug.Log($"[Claybreak] Run reached the firing line in {levelTime:0.0} s: army {armyAtLine} ({Specials} specials), "
                + $"leader x={center.x:0.00} z={center.z:0.0} (track top {track.bounds.max.y:0.00}).", this);
            crowd.Speed = 0f;
            launchCredit = 0f;
            holdTime = 0f;
            GameAudio.Play(GameSound.Finish);
            Vector3 line = new Vector3(track.bounds.center.x, track.bounds.max.y, activeLevel.FiringLineZ);
            crowd.FrameWithCamera(activeLevel.IsBoss ? bossAssaultShot : assaultShot, line, new Vector3(line.x, line.y, activeLevel.TowerZ));
            laneMarker.gameObject.SetActive(true);
            field.Running = true;
            cannon.Armed = activeLevel.CannonEnabled;
            hud.ShowTower(true, tower.MaximumHealth);
            hud.SetTower(tower.Health);
            hud.SetHint(deviceGate ? null : AssaultHint);
            if (activeLevel.IsBoss && !deviceGate)
            {
                hud.Banner(BossRules.Intro(activeLevel.BossRule));
                ClaybreakSfx.Play(ClaybreakSfx.Cue.Boss);
            }
        }

        private void EndLevel(bool won)
        {
            resultShown = false;
            crowd.Speed = 0f;
            crowd.Steering = false;
            cannon.Armed = false;
            laneMarker.gameObject.SetActive(false);
            hud.SetHint(null);
            int armyLeft = crowd.Army + field.InFlight;
            Debug.Log($"[Claybreak] Level {activeLevel.name} {(won ? "WON" : "LOST")} in {levelTime:0.0} s; army at line {armyAtLine}, "
                + $"left {armyLeft}, tower {tower.Health}/{tower.MaximumHealth}, hits {field.TowerHits}, peak in flight {field.PeakInFlight}, "
                + $"peak projectiles {cannon.PeakActive}/{activeLevel.MaximumProjectiles}.", this);
            if (deviceGate)
            {
                return;
            }

            string levelId = ClaybreakMetrics.LevelId(progression.World.WorldId, progression.LevelIndex);
            int worldNumber = progression.WorldIndex + 1;
            int levelNumber = progression.LevelIndex + 1;
            LevelOutcome outcome = progression.Complete(won, armyAtLine, armyLeft);
            adGateway.LevelFinished();
            PlayerMetrics.LevelEnded(levelId, worldNumber, levelNumber, won, Mathf.RoundToInt(levelTime));
            WriteSave();
            hud.SetCoins(progression.Save.coins);
            lastOutcome = outcome;
            lastDetail = won ? armyLeft + " left standing" : "the tower holds";
            Debug.Log($"[Claybreak] Result: {outcome.Stars} stars, +{outcome.Coins} coins (total {progression.Save.coins}), "
                + $"next {progression.World.WorldId}/{progression.CurrentLevel.name}, loop {progression.WorldProgress.loop}, stars {progression.TotalStars}"
                + (outcome.UnlockedUnit != null ? ", UNIT UNLOCKED " + outcome.UnlockedUnit.id : string.Empty)
                + (outcome.ClearedWorld ? ", WORLD CLEARED" : string.Empty)
                + (outcome.OpenedWorld != null ? ", WORLD OPENED " + outcome.OpenedWorld.WorldId : string.Empty) + ".", this);
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
                Vector3 center = crowd.Position;
                Debug.Log($"[ClaybreakGate] t={levelTime:0.0} run army={crowd.Army} crowd z={center.z:0.0} x={center.x:0.00}");
            }
            else if (state == FlowState.Assault)
            {
                Debug.Log($"[ClaybreakGate] t={levelTime:0.0} tower={activeLevel.Tower} inFlight={field.InFlight} specials={field.SpecialsInFlight} dying={field.Flyers.DyingCount} "
                    + $"projectiles={cannon.ActiveCount}/{activeLevel.MaximumProjectiles} reserve={crowd.Army} towerHealth={tower.Health} "
                    + $"loadout={loadoutCount} damage=x{runLoadout.DamageMultiplier:0.##} launch=x{runLoadout.LaunchRateMultiplier:0.##}");
            }
#endif
        }
    }
}
