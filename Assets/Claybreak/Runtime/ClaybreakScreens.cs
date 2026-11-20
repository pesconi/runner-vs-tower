using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>Which list the shop shows.</summary>
    public enum ShopTab
    {
        Upgrades,
        Units,
        Skins
    }

    /// <summary>
    /// The menu screens (RT-5): home (world progress, stars, PLAY, SHOP, WORLDS and the sound / music /
    /// vibration switches, the kit's own preferences), the shop (upgrades, special units and their loadout,
    /// crowd skins; coins only, no real money, D6), the world list and the result (stars, coins, what was
    /// unlocked, one tap to go on: GDD 3, never more than 2 taps to retry). Built by the scene builder with
    /// one list row per catalog entry; this class only fills it. Every text here is set on a tap, never per frame.
    /// </summary>
    public sealed class ClaybreakScreens : MonoBehaviour
    {
        [Header("Home")]
        [SerializeField] private GameObject home;
        [SerializeField] private Text homeCoins;
        [SerializeField] private Text worldText;
        [SerializeField] private Text starsText;
        [SerializeField] private Text loadoutText;
        [SerializeField] private Image[] levelDots = new Image[0];
        [SerializeField] private Button playButton;
        [SerializeField] private Text playLabel;
        [SerializeField] private Button shopButton;
        [SerializeField] private GameObject shopBadge;
        [SerializeField] private Button worldsButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Text soundLabel;
        [SerializeField] private Button musicButton;
        [SerializeField] private Text musicLabel;
        [SerializeField] private Button vibrationButton;
        [SerializeField] private Text vibrationLabel;

        [Header("Shop")]
        [SerializeField] private GameObject shop;
        [SerializeField] private Text shopCoins;
        [SerializeField] private Button[] shopTabs = new Button[3];
        [SerializeField] private RectTransform shopContent;
        [SerializeField] private ClaybreakListRow[] shopRows = new ClaybreakListRow[0];
        [SerializeField] private Button shopBack;

        [Header("Worlds")]
        [SerializeField] private GameObject worlds;
        [SerializeField] private Text worldsStars;
        [SerializeField] private RectTransform worldsContent;
        [SerializeField] private ClaybreakListRow[] worldRows = new ClaybreakListRow[0];
        [SerializeField] private Text worldsFooter;
        [SerializeField] private Button worldsBack;

        [Header("Result")]
        [SerializeField] private GameObject result;
        [SerializeField] private Text resultTitle;
        [SerializeField] private Image[] stars = new Image[3];
        [SerializeField] private Text resultDetail;
        [SerializeField] private Text resultReward;
        [SerializeField] private Text resultUnlock;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private Button homeButton;

        [Header("Layout and colors")]
        [Tooltip("Vertical distance between two list rows (the builder places row i at -i * rowStep).")]
        [SerializeField, Min(1f)] private float rowStep = 230f;
        [SerializeField] private Color dotDone = new Color(0.95f, 0.55f, 0.18f);
        [SerializeField] private Color dotNext = Color.white;
        [SerializeField] private Color dotLocked = new Color(1f, 1f, 1f, 0.22f);
        [SerializeField] private Color dotBoss = new Color(0.86f, 0.2f, 0.16f);
        [SerializeField] private Color starOn = new Color(1f, 0.8f, 0.25f);
        [SerializeField] private Color starOff = new Color(1f, 1f, 1f, 0.18f);
        [SerializeField] private Color buttonEnabled = new Color(0.95f, 0.55f, 0.18f);
        [SerializeField] private Color buttonOwned = new Color(0.32f, 0.55f, 0.3f);
        [SerializeField] private Color buttonDisabled = new Color(0.4f, 0.36f, 0.36f);
        [SerializeField] private Color tabOn = new Color(0.95f, 0.55f, 0.18f);
        [SerializeField] private Color tabOff = new Color(0.3f, 0.26f, 0.26f);

        private ShopTab tab;
        private Progression shownProgression;

        public event Action PlayPressed;
        public event Action ShopPressed;
        public event Action WorldsPressed;
        public event Action BackPressed;
        public event Action<ShopTab, int> ShopItemPressed;
        public event Action<int> WorldPressed;
        public event Action PrimaryPressed;
        public event Action HomePressed;

        public ShopTab Tab => tab;
        public int ShopRowCapacity => shopRows == null ? 0 : shopRows.Length;
        public int WorldRowCapacity => worldRows == null ? 0 : worldRows.Length;
        public int DotCapacity => levelDots == null ? 0 : levelDots.Length;
        public bool ShopShown => shop != null && shop.activeSelf;
        public bool WorldsShown => worlds != null && worlds.activeSelf;

        public bool HasParts => home != null && homeCoins != null && worldText != null && starsText != null
            && loadoutText != null && levelDots != null && levelDots.Length > 0 && playButton != null && playLabel != null
            && shopButton != null && shopBadge != null && worldsButton != null && soundButton != null && soundLabel != null
            && musicButton != null && musicLabel != null && vibrationButton != null && vibrationLabel != null
            && shop != null && shopCoins != null && shopTabs != null && shopTabs.Length == 3 && Array.TrueForAll(shopTabs, b => b != null)
            && shopContent != null && shopRows != null && Array.TrueForAll(shopRows, r => r != null && r.HasParts) && shopBack != null
            && worlds != null && worldsStars != null && worldsContent != null && worldRows != null
            && Array.TrueForAll(worldRows, r => r != null && r.HasParts) && worldsFooter != null && worldsBack != null
            && result != null && resultTitle != null && stars != null && stars.Length == 3 && resultDetail != null
            && resultReward != null && resultUnlock != null && primaryButton != null && primaryLabel != null
            && homeButton != null;

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
            shopBack.onClick.AddListener(() => Raise(BackPressed));
            worldsBack.onClick.AddListener(() => Raise(BackPressed));
            primaryButton.onClick.AddListener(() => Raise(PrimaryPressed));
            homeButton.onClick.AddListener(() => Raise(HomePressed));
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
            shop.SetActive(false);
            worlds.SetActive(false);
            result.SetActive(false);
        }

        // ---------- home ----------

        /// <summary>Fills and shows the home screen from the current progress.</summary>
        public void ShowHome(Progression progression)
        {
            HideAll();
            home.SetActive(true);
            ClaybreakWorld world = progression.World;
            WorldSave save = progression.WorldProgress;
            homeCoins.text = NumberLabels.Plain(progression.Save.coins);
            string loop = save.loop > 0 ? $"  ·  TOUGHER x{progression.HealthMultiplier:0.##}" : string.Empty;
            worldText.text = $"{world.WorldName}  ·  {progression.CurrentLevel.Title}{loop}";
            starsText.text = $"★ {progression.WorldStars(progression.WorldIndex)} / {progression.MaximumWorldStars(progression.WorldIndex)}";
            int levelCount = world.LevelCount;
            for (int i = 0; i < levelDots.Length; i++)
            {
                bool used = i < levelCount;
                levelDots[i].gameObject.SetActive(used);
                if (!used)
                {
                    continue;
                }

                bool boss = i == levelCount - 1;
                Color color = i == save.nextLevel ? dotNext
                    : progression.StarsAt(i) > 0 ? dotDone
                    : boss ? dotBoss : dotLocked;
                levelDots[i].color = color;
                float size = i == save.nextLevel ? 1.3f : boss ? 1.15f : 1f;
                levelDots[i].rectTransform.localScale = new Vector3(size, size, 1f);
            }

            playLabel.text = progression.CurrentLevel.IsBoss ? "FIGHT BOSS" : "PLAY";
            loadoutText.text = LoadoutLine(progression);
            shopBadge.SetActive(progression.CanBuyAnything);
            RefreshSwitches();
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
                + $"\nARMY +{loadout.StartingArmy}  ·  DAMAGE +{Mathf.RoundToInt((loadout.DamageMultiplier - 1f) * 100f)}%";
        }

        // ---------- shop ----------

        /// <summary>Fills and shows the shop on its current tab.</summary>
        public void ShowShop(Progression progression)
        {
            HideAll();
            shop.SetActive(true);
            shownProgression = progression;
            shopCoins.text = NumberLabels.Plain(progression.Save.coins);
            for (int i = 0; i < shopTabs.Length; i++)
            {
                shopTabs[i].image.color = i == (int)tab ? tabOn : tabOff;
            }

            ClaybreakCatalog catalog = progression.Catalog;
            int count = tab == ShopTab.Upgrades ? catalog.Upgrades.Count : tab == ShopTab.Units ? catalog.Units.Count : catalog.Skins.Count;
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
                    default:
                        FillSkin(shopRows[i], progression, i);
                        break;
                }
            }

            Fit(shopContent, Mathf.Min(count, shopRows.Length));
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
            row.Show($"{data.title}  {level}/{steps}", $"Now {now}\nNext {next}", progression.NextUpgradeCost(track) + " ¢",
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
                row.Show(data.title, hint == null ? stats : stats + $"\nFree: beat {hint}", data.price + " ¢", canBuy,
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
            if (progression.IsSkinWorn(skin))
            {
                row.Show(data.title, "Your army wears it", "WORN", false, buttonOwned);
            }
            else if (progression.IsSkinOwned(skin))
            {
                row.Show(data.title, "Owned", "WEAR", true, buttonEnabled);
            }
            else
            {
                bool canBuy = progression.CanBuySkin(skin);
                row.Show(data.title, "Clay color for the whole army", data.price + " ¢", canBuy, canBuy ? buttonEnabled : buttonDisabled);
            }

            row.ShowSwatch(data.color);
        }

        // ---------- worlds ----------

        /// <summary>Fills and shows the world list (one row per catalog world).</summary>
        public void ShowWorlds(Progression progression)
        {
            HideAll();
            worlds.SetActive(true);
            ClaybreakCatalog catalog = progression.Catalog;
            worldsStars.text = $"★ {progression.TotalStars} / {catalog.LevelCount * 3}";
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
                string title = $"{world.WorldName}   ★ {progression.WorldStars(i)}/{progression.MaximumWorldStars(i)}";
                if (!progression.IsWorldUnlocked(i))
                {
                    string boss = catalog.Worlds[i - 1].world.WorldName;
                    string need = entry.starsToUnlock > 0 ? $" and ★ {entry.starsToUnlock}" : string.Empty;
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
            Fit(worldsContent, count);
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
            resultReward.text = "+" + outcome.Coins + " ¢";
            string unlock = outcome.UnlockedUnit != null ? $"NEW ELITE: {outcome.UnlockedUnit.title}\nGrab its crates on the run."
                : outcome.OpenedWorld != null ? $"NEW WORLD: {outcome.OpenedWorld.WorldName}"
                : outcome.ClearedWorld ? "WORLD CLEARED\nThe towers come back tougher."
                : string.Empty;
            resultUnlock.text = unlock;
            resultUnlock.gameObject.SetActive(unlock.Length > 0);
            primaryLabel.text = outcome.Won ? (nextIsBoss ? "BOSS" : "NEXT") : "AGAIN";
        }

        // Rows sit at fixed steps from the top of the scroll content; the content is as tall as the rows shown
        // (the scroll position is kept, so buying a row low in the list does not jump back to the top).
        private void Fit(RectTransform content, int rows)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, rows * rowStep);
        }

        private void RefreshSwitches()
        {
            soundLabel.text = GameAudio.SoundMuted ? "SOUND OFF" : "SOUND ON";
            musicLabel.text = GameAudio.MusicMuted ? "MUSIC OFF" : "MUSIC ON";
            vibrationLabel.text = GameHaptics.Enabled ? "VIBRATION ON" : "VIBRATION OFF";
        }
    }
}
