using System;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>Which list the shop shows.</summary>
    public enum ShopTab
    {
        Upgrades,
        Units,
        Skins,
        /// <summary>RT-6t.20 (D31 A): the crowd shapes, bought and worn here.</summary>
        Shapes
    }

    /// <summary>
    /// The menu screens (RT-5): home (world progress, stars, PLAY, SHOP, WORLDS, SETTINGS), the settings page
    /// (RT-8: the kit's sound / music / vibration switches, privacy policy, privacy options), the shop (upgrades, special units and their loadout,
    /// crowd skins, crowd shapes since RT-6t.20; coins only, no real money, D6), the world list and the result (stars, coins, what was
    /// unlocked, one tap to go on: GDD 3, never more than 2 taps to retry). Built by the scene builder with
    /// one list row per catalog entry; this class only fills it. Every text here is set on a tap, never per frame.
    /// </summary>
    public sealed class ClaybreakScreens : MonoBehaviour
    {
        [Header("Home")]
        [SerializeField] private GameObject home;
        [SerializeField] private TMP_Text homeCoins;
        [SerializeField] private TMP_Text worldText;
        [SerializeField] private TMP_Text starsText;
        [SerializeField] private TMP_Text loadoutText;
        [SerializeField] private Image[] levelDots = new Image[0];
        [Tooltip("RT-6t.16: the level meter's caption (LEVEL x/n) over the dots.")]
        [SerializeField] private TMP_Text levelMeterText;
        [SerializeField] private Button playButton;
        [SerializeField] private TMP_Text playLabel;
        [SerializeField] private Button shopButton;
        [SerializeField] private GameObject shopBadge;
        [SerializeField] private Button worldsButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private TMP_Text soundLabel;
        [SerializeField] private Button musicButton;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private Button vibrationButton;
        [SerializeField] private TMP_Text vibrationLabel;
        [SerializeField] private Button dailyButton;
        [SerializeField] private TMP_Text dailyLabel;
        [SerializeField] private Button privacyButton;
        [Header("Settings (RT-8)")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settings;
        [SerializeField] private Button policyButton;
        [SerializeField] private TMP_Text settingsNote;
        [SerializeField] private Button settingsBack;

        [Header("Shop")]
        [SerializeField] private GameObject shop;
        [SerializeField] private TMP_Text shopCoins;
        [SerializeField] private Button[] shopTabs = new Button[ShopTabCount];
        [SerializeField] private RectTransform shopContent;
        [Tooltip("RT-6t.17: the fade at the list's foot, shown only when the rows overflow the list.")]
        [SerializeField] private GameObject shopFade;
        [SerializeField] private ClaybreakListRow[] shopRows = new ClaybreakListRow[0];
        [SerializeField] private Button shopBack;

        [Header("Worlds")]
        [SerializeField] private GameObject worlds;
        [SerializeField] private TMP_Text worldsStars;
        [SerializeField] private RectTransform worldsContent;
        [SerializeField] private GameObject worldsFade;
        [SerializeField] private ClaybreakListRow[] worldRows = new ClaybreakListRow[0];
        [SerializeField] private TMP_Text worldsFooter;
        [SerializeField] private Button worldsBack;

        [Header("Result")]
        [SerializeField] private GameObject result;
        [SerializeField] private TMP_Text resultTitle;
        [SerializeField] private Image[] stars = new Image[3];
        [SerializeField] private TMP_Text resultDetail;
        [SerializeField] private TMP_Text resultReward;
        [SerializeField] private TMP_Text resultUnlock;
        [SerializeField] private Button primaryButton;
        [SerializeField] private TMP_Text primaryLabel;
        [SerializeField] private Button homeButton;
        [Tooltip("Optional rewarded offer under the result card (gain written on it; hidden when no ad is loaded).")]
        [SerializeField] private Button resultAdButton;
        [SerializeField] private TMP_Text resultAdLabel;

        [Header("Continue offer (rewarded, once per level, with a clear NO THANKS)")]
        [SerializeField] private GameObject continueOffer;
        [SerializeField] private TMP_Text continueText;
        [SerializeField] private Button continueAdButton;
        [SerializeField] private TMP_Text continueAdLabel;
        [SerializeField] private Button continueDecline;

        [Header("Layout and colors")]
        [Tooltip("Vertical distance between two list rows (the builder places row i at -i * rowStep).")]
        [SerializeField, Min(1f)] private float rowStep = 230f;
        [SerializeField] private Color dotDone = UiColors.Primary;
        [SerializeField] private Color dotNext = UiColors.Pink;
        [SerializeField] private Color dotLocked = UiColors.WithAlpha(UiColors.Ink, 0.2f);
        [SerializeField] private Color dotBoss = UiColors.Coral;
        [Tooltip("RT-6t.17: the boss level's mark on the level meter (a crown instead of a dot).")]
        [SerializeField] private Sprite bossIcon;
        [SerializeField] private Sprite dotIcon;
        [SerializeField] private Color starOn = UiColors.Gold;
        [SerializeField] private Color starOff = UiColors.WithAlpha(UiColors.Ink, 0.15f);
        [SerializeField] private Color buttonEnabled = UiColors.Primary;
        [SerializeField] private Color buttonOwned = UiColors.Mint;
        [SerializeField] private Color buttonDisabled = UiColors.Unavailable;
        [SerializeField] private Color tabOn = UiColors.Coral;
        [SerializeField] private Color tabOff = UiColors.Lilac;

        /// <summary>One tab per <see cref="ShopTab"/>.</summary>
        public const int ShopTabCount = 4;

        private ShopTab tab;
        private Progression shownProgression;
        private string privacyPolicyUrl;

        public event Action PlayPressed;
        public event Action ShopPressed;
        public event Action WorldsPressed;
        public event Action BackPressed;
        public event Action<ShopTab, int> ShopItemPressed;
        public event Action<int> WorldPressed;
        public event Action PrimaryPressed;
        public event Action HomePressed;
        public event Action DailyPressed;
        public event Action PrivacyPressed;
        public event Action ResultAdPressed;
        public event Action ContinueAdPressed;
        public event Action ContinueDeclined;

        public ShopTab Tab => tab;
        public int ShopRowCapacity => shopRows == null ? 0 : shopRows.Length;
        public int WorldRowCapacity => worldRows == null ? 0 : worldRows.Length;
        public int DotCapacity => levelDots == null ? 0 : levelDots.Length;
        public bool ShopShown => shop != null && shop.activeSelf;
        public bool SettingsShown => settings != null && settings.activeSelf;
        public bool WorldsShown => worlds != null && worlds.activeSelf;
        public bool ResultShown => result != null && result.activeSelf;
        public bool ContinueShown => continueOffer != null && continueOffer.activeSelf;
        public bool ResultAdShown => resultAdButton != null && resultAdButton.gameObject.activeSelf;
        public bool DailyShown => dailyButton != null && dailyButton.gameObject.activeSelf;

        public bool HasParts => home != null && homeCoins != null && worldText != null && starsText != null
            && loadoutText != null && levelDots != null && levelDots.Length > 0 && levelMeterText != null && playButton != null && playLabel != null
            && shopButton != null && shopBadge != null && worldsButton != null
            && soundButton != null && soundLabel != null
            && musicButton != null && musicLabel != null && vibrationButton != null && vibrationLabel != null
            && shop != null && shopCoins != null && shopTabs != null && shopTabs.Length == ShopTabCount && Array.TrueForAll(shopTabs, b => b != null)
            && shopContent != null && shopFade != null && worldsFade != null && shopRows != null && Array.TrueForAll(shopRows, r => r != null && r.HasParts) && shopBack != null
            && worlds != null && worldsStars != null && worldsContent != null && worldRows != null
            && Array.TrueForAll(worldRows, r => r != null && r.HasParts) && worldsFooter != null && worldsBack != null
            && result != null && resultTitle != null && stars != null && stars.Length == 3 && resultDetail != null
            && resultReward != null && resultUnlock != null && primaryButton != null && primaryLabel != null
            && homeButton != null && dailyButton != null && dailyLabel != null && privacyButton != null
            && settingsButton != null && settings != null && policyButton != null && settingsNote != null && settingsBack != null
            && resultAdButton != null && resultAdLabel != null && continueOffer != null && continueText != null
            && continueAdButton != null && continueAdLabel != null && continueDecline != null;

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("ClaybreakScreens is missing a part (rebuild the scene).", this);
                enabled = false;
                return;
            }

            playButton.onClick.AddListener(() => Raise(PlayPressed));
            shopButton.onClick.AddListener(() => Raise(ShopPressed));
            worldsButton.onClick.AddListener(() => Raise(WorldsPressed));
            shopBack.onClick.AddListener(Back);
            worldsBack.onClick.AddListener(Back);
            primaryButton.onClick.AddListener(() => Raise(PrimaryPressed));
            homeButton.onClick.AddListener(() => Raise(HomePressed));
            dailyButton.onClick.AddListener(() => Raise(DailyPressed));
            privacyButton.onClick.AddListener(() => Raise(PrivacyPressed));
            settingsButton.onClick.AddListener(() =>
            {
                GameAudio.Play(GameSound.ButtonClick);
                ShowSettings();
            });
            settingsBack.onClick.AddListener(Back);
            policyButton.onClick.AddListener(OpenPrivacyPolicy);
            resultAdButton.onClick.AddListener(() => Raise(ResultAdPressed));
            continueAdButton.onClick.AddListener(() => Raise(ContinueAdPressed));
            continueDecline.onClick.AddListener(() => Raise(ContinueDeclined));
            for (int i = 0; i < shopTabs.Length; i++)
            {
                var pick = (ShopTab)i;
                shopTabs[i].onClick.AddListener(() => PickTab(pick));
            }

            for (int i = 0; i < shopRows.Length; i++)
            {
                int index = i;
                shopRows[i].Button.onClick.AddListener(() =>
                {
                    GameAudio.Play(GameSound.ButtonClick);
                    ShopItemPressed?.Invoke(tab, index);
                });
            }

            for (int i = 0; i < worldRows.Length; i++)
            {
                int index = i;
                worldRows[i].Button.onClick.AddListener(() =>
                {
                    GameAudio.Play(GameSound.ButtonClick);
                    WorldPressed?.Invoke(index);
                });
            }

            soundButton.onClick.AddListener(() =>
            {
                GameAudio.SetSoundMuted(!GameAudio.SoundMuted);
                GameAudio.Play(GameSound.ButtonClick);
                RefreshSwitches();
            });
            musicButton.onClick.AddListener(() =>
            {
                GameAudio.SetMusicMuted(!GameAudio.MusicMuted);
                GameAudio.Play(GameSound.ButtonClick);
                RefreshSwitches();
            });
            vibrationButton.onClick.AddListener(() =>
            {
                GameHaptics.SetEnabled(!GameHaptics.Enabled);
                GameAudio.Play(GameSound.ButtonClick);
                RefreshSwitches();
            });
        }

        private void Back()
        {
            GameAudio.Play(GameSound.ButtonBack);
            BackPressed?.Invoke();
        }

        private static void Raise(Action action)
        {
            GameAudio.Play(GameSound.ButtonClick);
            action?.Invoke();
        }

        // A tab change asks the flow for nothing: it refills from the progression the shop was opened with.
        private void PickTab(ShopTab pick)
        {
            GameAudio.Play(GameSound.ButtonClick);
            tab = pick;
            shopContent.anchoredPosition = new Vector2(shopContent.anchoredPosition.x, 0f);
            if (shownProgression != null)
            {
                ShowShop(shownProgression);
            }
        }

        public void HideAll()
        {
            home.SetActive(false);
            settings.SetActive(false);
            shop.SetActive(false);
            worlds.SetActive(false);
            result.SetActive(false);
            continueOffer.SetActive(false);
        }

        // ---------- home ----------

        /// <summary>Fills and shows the home screen from the current progress.</summary>
        public void ShowHome(Progression progression)
        {
            HideAll();
            home.SetActive(true);
            ClaybreakWorld world = progression.World;
            WorldSave save = progression.WorldProgress;
            homeCoins.text = UiIcons.Coins(progression.Save.coins);
            string loop = save.loop > 0 ? $"  ·  TOUGHER x{progression.HealthMultiplier:0.##}" : string.Empty;
            worldText.text = $"{world.WorldName}  ·  {progression.CurrentLevel.Title}{loop}";
            starsText.text = $"{UiIcons.Star} {progression.WorldStars(progression.WorldIndex)} / {progression.MaximumWorldStars(progression.WorldIndex)}";
            int levelCount = world.LevelCount;
            levelMeterText.text = LevelMeterLine(save.nextLevel, levelCount);
            for (int i = 0; i < levelDots.Length; i++)
            {
                bool used = i < levelCount;
                levelDots[i].gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                // RT-6t.16: the world's dots spread evenly over the meter's row, whatever its width and level count.
                RectTransform dot = levelDots[i].rectTransform;
                float x = (i + 0.5f) / levelCount;
                dot.anchorMin = new Vector2(x, 0.5f);
                dot.anchorMax = new Vector2(x, 0.5f);

                // RT-6t.17: the boss level wears a crown (its color still says beaten, next or ahead).
                bool boss = i == levelCount - 1;
                Color color = i == save.nextLevel ? dotNext
                    : progression.StarsAt(i) > 0 ? dotDone
                    : boss ? dotBoss : dotLocked;
                levelDots[i].sprite = boss && bossIcon != null ? bossIcon : dotIcon != null ? dotIcon : levelDots[i].sprite;
                levelDots[i].color = color;
                float size = boss ? 1.7f : i == save.nextLevel ? 1.35f : 1f;
                levelDots[i].rectTransform.localScale = new Vector3(size, size, 1f);
            }

            playLabel.text = progression.CurrentLevel.IsBoss ? "FIGHT BOSS" : "PLAY";
            shopBadge.SetActive(progression.CanBuyAnything);
            RefreshSwitches();
        }

        /// <summary>The daily gift seal's caption.</summary>
        public static string DailyLine(int coins) => $"GIFT  +{UiIcons.Coins(coins)}";

        /// <summary>The level meter's caption: the level to play now (1-based, capped at the count) of the world's count.</summary>
        public static string LevelMeterLine(int nextLevel, int levelCount)
        {
            int count = Mathf.Max(1, levelCount);
            return $"LEVEL {Mathf.Clamp(nextLevel + 1, 1, count)}/{count}";
        }

        /// <summary>The daily reward button: shown with its coins when a claim is due, hidden otherwise.</summary>
        public void SetDaily(int coins)
        {
            dailyButton.gameObject.SetActive(coins > 0);
            dailyLabel.text = coins > 0 ? DailyLine(coins) : string.Empty;
        }

        // ---------- settings (RT-8) ----------

        /// <summary>The settings page: sound, music and vibration switches, privacy policy and privacy options.</summary>
        public void ShowSettings()
        {
            HideAll();
            settings.SetActive(true);
            settingsNote.text = string.Empty;
            RefreshSwitches();
        }

        /// <summary>The public privacy policy page (https); empty until the store release publishes it.</summary>
        public void SetPrivacyPolicyUrl(string url)
        {
            privacyPolicyUrl = url;
        }

        private void OpenPrivacyPolicy()
        {
            GameAudio.Play(GameSound.ButtonClick);
            if (!ClaybreakMonetization.IsHttpsPage(privacyPolicyUrl))
            {
                settingsNote.text = "THE PRIVACY POLICY LINK COMES WITH THE STORE RELEASE";
                return;
            }

            Application.OpenURL(privacyPolicyUrl);
        }

        /// <summary>The privacy options entry point (settings page), shown only where UMP requires it.</summary>
        public void SetPrivacyVisible(bool visible)
        {
            privacyButton.gameObject.SetActive(visible);
        }

        private static string LoadoutLine(Progression progression)
        {
            ClaybreakCatalog catalog = progression.Catalog;
            string units = string.Empty;
            foreach (string id in progression.Save.loadout)
            {
                int unit = catalog.UnitIndexOf(id);
                if (unit >= 0)
                {
                    units += (units.Length > 0 ? " + " : string.Empty) + catalog.Units[unit].title;
                }
            }

            RunLoadout loadout = progression.Loadout;
            return (units.Length > 0 ? "ELITES: " + units : "NO ELITES YET")
                + $"  ·  ARMY +{loadout.StartingArmy}  ·  DAMAGE +{Mathf.RoundToInt((loadout.DamageMultiplier - 1f) * 100f)}%";
        }

        // ---------- shop ----------

        /// <summary>Fills and shows the shop on its current tab.</summary>
        public void ShowShop(Progression progression)
        {
            HideAll();
            shop.SetActive(true);
            shownProgression = progression;
            shopCoins.text = UiIcons.Coins(progression.Save.coins);
            // RT-6t.17: what the army takes into a run, over the list (it left the home screen).
            loadoutText.text = LoadoutLine(progression);
            for (int i = 0; i < shopTabs.Length; i++)
            {
                Color color = i == (int)tab ? tabOn : tabOff;
                shopTabs[i].image.color = color;
                Shadow slab = shopTabs[i].image.GetComponent<Shadow>();
                if (slab != null)
                {
                    slab.effectColor = ClayUi.SlabColor(color);
                }
            }

            ClaybreakCatalog catalog = progression.Catalog;
            int count = ShopCount(catalog, tab);
            for (int i = 0; i < shopRows.Length; i++)
            {
                if (i >= count)
                {
                    shopRows[i].Hide();
                    continue;
                }

                switch (tab)
                {
                    case ShopTab.Upgrades:
                        FillUpgrade(shopRows[i], progression, i);
                        break;
                    case ShopTab.Units:
                        FillUnit(shopRows[i], progression, i);
                        break;
                    case ShopTab.Skins:
                        FillSkin(shopRows[i], progression, i);
                        break;
                    default:
                        FillShape(shopRows[i], progression, i);
                        break;
                }
            }

            Fit(shopContent, shopFade, Mathf.Min(count, shopRows.Length));
        }

        private void FillUpgrade(ClaybreakListRow row, Progression progression, int track)
        {
            UpgradeTrack data = progression.Catalog.Upgrades[track];
            int level = progression.UpgradeLevel(track);
            int steps = data.steps.Length;
            string now = Effect(data.kind, progression.UpgradeValue(track));
            if (progression.IsUpgradeMaxed(track))
            {
                row.Show($"{data.title}  {level}/{steps}", now + "  ·  MAX", "MAX", false, buttonOwned);
                return;
            }

            string next = Effect(data.kind, progression.UpgradeValue(track, level + 1));
            bool canBuy = progression.CanBuyUpgrade(track);
            row.Show($"{data.title}  {level}/{steps}", $"Now {now}\nNext {next}", UiIcons.Coins(progression.NextUpgradeCost(track)),
                canBuy, canBuy ? buttonEnabled : buttonDisabled);
        }

        /// <summary>What an upgrade bonus means, in the words of its kind.</summary>
        public static string Effect(UpgradeKind kind, int value)
        {
            switch (kind)
            {
                case UpgradeKind.StartingArmy: return $"+{value} units at the start";
                case UpgradeKind.Damage: return $"+{value}% damage";
                case UpgradeKind.GateBonus: return $"+{value} on every + gate";
                default: return $"+{value}% launch speed";
            }
        }

        private void FillUnit(ClaybreakListRow row, Progression progression, int unit)
        {
            UnitType data = progression.Catalog.Units[unit];
            string stats = $"{data.description}\nHits x{data.damage}  ·  Armor {data.armor}  ·  Speed x{data.speed:0.#}";
            if (progression.IsUnitOwned(unit))
            {
                bool equipped = progression.IsUnitEquipped(unit);
                bool slotFree = progression.EquippedCount < progression.Catalog.LoadoutSlots;
                row.Show(data.title, stats, equipped ? "ON" : "EQUIP", equipped || slotFree,
                    equipped ? buttonOwned : slotFree ? buttonEnabled : buttonDisabled);
            }
            else if (data.price > 0)
            {
                string hint = progression.UnlockHint(unit);
                bool canBuy = progression.CanBuyUnit(unit);
                row.Show(data.title, hint == null ? stats : stats + $"\nFree: beat {hint}", UiIcons.Coins(data.price), canBuy,
                    canBuy ? buttonEnabled : buttonDisabled);
            }
            else
            {
                row.Show(data.title, stats + $"\nBeat {progression.UnlockHint(unit)}", "LOCKED", false, buttonDisabled);
            }

            row.ShowSwatch(data.color);
        }

        private void FillSkin(ClaybreakListRow row, Progression progression, int skin)
        {
            CrowdSkin data = progression.Catalog.Skins[skin];
            string owned = skin == 0 ? "Each world's own color" : "Owned";
            if (progression.IsSkinWorn(skin))
            {
                row.Show(data.title, skin == 0 ? owned : "Your army wears it", "WORN", false, buttonOwned);
            }
            else if (progression.IsSkinOwned(skin))
            {
                row.Show(data.title, owned, "WEAR", true, buttonEnabled);
            }
            else
            {
                bool canBuy = progression.CanBuySkin(skin);
                row.Show(data.title, "One color for the army in every world", UiIcons.Coins(data.price), canBuy, canBuy ? buttonEnabled : buttonDisabled);
            }

            // The free default skin has no color of its own (RT-6t.6a): it wears each world's palette color.
            row.ShowSwatch(skin == 0 ? progression.WorldCrowdColor(progression.WorldIndex) : data.color);
        }

        /// <summary>How many rows the shop shows on <paramref name="list"/>.</summary>
        public static int ShopCount(ClaybreakCatalog catalog, ShopTab list)
        {
            switch (list)
            {
                case ShopTab.Upgrades: return catalog.Upgrades.Count;
                case ShopTab.Units: return catalog.Units.Count;
                case ShopTab.Skins: return catalog.Skins.Count;
                default: return catalog.Shapes.Count;
            }
        }

        // RT-6t.20 (D31 A): a shape row shows the shape's figure in the army's color of the world shown (the body layer takes
        // the tint, the eyes, beak and outline keep their own colors), its price, then WEAR / WORN.
        private void FillShape(ClaybreakListRow row, Progression progression, int shape)
        {
            CrowdShapeOption data = progression.Catalog.Shapes[shape];
            string line = shape == 0 ? "Free  ·  the classic army" : "A new look for the whole army";
            if (progression.IsShapeWorn(shape))
            {
                row.Show(data.title, shape == 0 ? line : "Your army wears it", "WORN", false, buttonOwned);
            }
            else if (progression.IsShapeOwned(shape))
            {
                row.Show(data.title, shape == 0 ? line : "Owned", "WEAR", true, buttonEnabled);
            }
            else
            {
                bool canBuy = progression.CanBuyShape(shape);
                row.Show(data.title, line, UiIcons.Coins(data.price), canBuy, canBuy ? buttonEnabled : buttonDisabled);
            }

            row.ShowFigure(data.figureBody, data.figureDetail, progression.CrowdColorIn(progression.WorldIndex));
        }

        // ---------- worlds ----------

        /// <summary>Fills and shows the world list (one row per catalog world).</summary>
        public void ShowWorlds(Progression progression)
        {
            HideAll();
            worlds.SetActive(true);
            ClaybreakCatalog catalog = progression.Catalog;
            worldsStars.text = $"{UiIcons.Star} {progression.TotalStars} / {catalog.LevelCount * 3}";
            int count = Mathf.Min(catalog.Worlds.Count, worldRows.Length);
            for (int i = 0; i < worldRows.Length; i++)
            {
                if (i >= count)
                {
                    worldRows[i].Hide();
                    continue;
                }

                WorldEntry entry = catalog.Worlds[i];
                ClaybreakWorld world = entry.world;
                WorldSave save = progression.Save.worlds[i];
                string title = $"{world.WorldName}   {UiIcons.Star} {progression.WorldStars(i)}/{progression.MaximumWorldStars(i)}";
                if (!progression.IsWorldUnlocked(i))
                {
                    string boss = catalog.Worlds[i - 1].world.WorldName;
                    string need = entry.starsToUnlock > 0 ? $" and {UiIcons.Star} {entry.starsToUnlock}" : string.Empty;
                    worldRows[i].Show(title, $"Locked: beat the {boss} boss{need}", "LOCKED", false, buttonDisabled);
                    continue;
                }

                string state = save.loop > 0 ? $"Cleared x{save.loop}  ·  next {world.Levels[save.nextLevel].Title}"
                    : $"{world.Levels[save.nextLevel].Title} of {world.LevelCount}";
                bool current = i == progression.WorldIndex;
                worldRows[i].Show(title, state, current ? "HERE" : "GO", !current, current ? buttonOwned : buttonEnabled);
            }

            worldsFooter.text = catalog.WorldsFooter;
            worldsFooter.gameObject.SetActive(!string.IsNullOrEmpty(catalog.WorldsFooter));
            Fit(worldsContent, worldsFade, count);
        }

        // ---------- result ----------

        public void ShowResult(LevelOutcome outcome, string detail, bool nextIsBoss)
        {
            HideAll();
            result.SetActive(true);
            resultTitle.text = outcome.Won ? (outcome.ClearedWorld ? "BOSS DOWN!" : "TOWER DOWN!") : "ARMY LOST";
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(outcome.Won);
                stars[i].color = i < outcome.Stars ? starOn : starOff;
            }

            resultDetail.text = detail;
            resultReward.text = "+" + UiIcons.Coins(outcome.Coins);
            string unlock = outcome.UnlockedUnit != null ? $"NEW ELITE: {outcome.UnlockedUnit.title}\nGrab its crates on the run."
                : outcome.OpenedWorld != null ? $"NEW WORLD: {outcome.OpenedWorld.WorldName}"
                : outcome.ClearedWorld ? "WORLD CLEARED\nThe towers come back tougher."
                : string.Empty;
            resultUnlock.text = unlock;
            resultUnlock.gameObject.SetActive(unlock.Length > 0);
            primaryLabel.text = outcome.Won ? (nextIsBoss ? "BOSS" : "NEXT") : "AGAIN";
            SetResultAd(null);
        }

        /// <summary>The coins line of the result (after a rewarded bonus it shows the doubled amount).</summary>
        public void SetResultReward(string text)
        {
            resultReward.text = text;
        }

        /// <summary>The rewarded offer under the result card; null hides it.</summary>
        public void SetResultAd(string caption)
        {
            resultAdButton.gameObject.SetActive(caption != null);
            resultAdLabel.text = caption ?? string.Empty;
        }

        // ---------- continue offer ----------

        /// <summary>Shows the continue offer: what the ad gives, and a clear way to say no.</summary>
        public void ShowContinue(int units)
        {
            HideAll();
            continueOffer.SetActive(true);
            continueText.text = $"Watch a short ad to send\n{units} more units at the tower?";
            continueAdLabel.text = $"WATCH AD  +{units}";
        }

        // Rows sit at fixed steps from the top of the scroll content; the content is as tall as the rows shown
        // (the scroll position is kept, so buying a row low in the list does not jump back to the top). The fade at the
        // list's foot says there is more below: shown only when the rows are taller than the list.
        private void Fit(RectTransform content, GameObject fade, int rows)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, rows * rowStep);
            var viewport = (RectTransform)content.parent;
            fade.SetActive(rows * rowStep > viewport.rect.height + 1f);
        }

        private void RefreshSwitches()
        {
            soundLabel.text = GameAudio.SoundMuted ? "SOUND OFF" : "SOUND ON";
            musicLabel.text = GameAudio.MusicMuted ? "MUSIC OFF" : "MUSIC ON";
            vibrationLabel.text = GameHaptics.Enabled ? "VIBRATION ON" : "VIBRATION OFF";
        }
    }
}
