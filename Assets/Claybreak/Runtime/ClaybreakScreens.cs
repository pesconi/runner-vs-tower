using System;
using JungleDegree.StudioKit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>
    /// The menu (home) and the result screen (RT-4). Home: world progress, PLAY, the starting-army upgrade,
    /// the special unit card and the sound / music / vibration switches (the kit's own preferences).
    /// Result: stars, coins, what was unlocked, and one tap to go on (NEXT after a win, AGAIN after a loss;
    /// GDD 3: never more than 2 taps to retry). Built by the scene builder; this class only fills it.
    /// </summary>
    public sealed class ClaybreakScreens : MonoBehaviour
    {
        [Header("Home")]
        [SerializeField] private GameObject home;
        [SerializeField] private Text homeCoins;
        [SerializeField] private Text worldText;
        [SerializeField] private Image[] levelDots = new Image[0];
        [SerializeField] private Button playButton;
        [SerializeField] private Text playLabel;
        [SerializeField] private Text upgradeTitle;
        [SerializeField] private Text upgradeDetail;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Text upgradeButtonLabel;
        [SerializeField] private Text unitTitle;
        [SerializeField] private Text unitDetail;
        [SerializeField] private Button soundButton;
        [SerializeField] private Text soundLabel;
        [SerializeField] private Button musicButton;
        [SerializeField] private Text musicLabel;
        [SerializeField] private Button vibrationButton;
        [SerializeField] private Text vibrationLabel;

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

        [Header("Colors")]
        [SerializeField] private Color dotDone = new Color(0.95f, 0.55f, 0.18f);
        [SerializeField] private Color dotNext = Color.white;
        [SerializeField] private Color dotLocked = new Color(1f, 1f, 1f, 0.22f);
        [SerializeField] private Color dotBoss = new Color(0.86f, 0.2f, 0.16f);
        [SerializeField] private Color starOn = new Color(1f, 0.8f, 0.25f);
        [SerializeField] private Color starOff = new Color(1f, 1f, 1f, 0.18f);
        [SerializeField] private Color buttonEnabled = new Color(0.95f, 0.55f, 0.18f);
        [SerializeField] private Color buttonDisabled = new Color(0.4f, 0.36f, 0.36f);

        public event Action PlayPressed;
        public event Action UpgradePressed;
        public event Action PrimaryPressed;
        public event Action HomePressed;

        public bool HasParts => home != null && homeCoins != null && worldText != null && levelDots != null
            && levelDots.Length >= 5 && playButton != null && playLabel != null && upgradeTitle != null
            && upgradeDetail != null && upgradeButton != null && upgradeButtonLabel != null && unitTitle != null
            && unitDetail != null && soundButton != null && soundLabel != null && musicButton != null
            && musicLabel != null && vibrationButton != null && vibrationLabel != null && result != null
            && resultTitle != null && stars != null && stars.Length == 3 && resultDetail != null
            && resultReward != null && resultUnlock != null && primaryButton != null && primaryLabel != null
            && homeButton != null;

        public int DotCapacity => levelDots == null ? 0 : levelDots.Length;

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("ClaybreakScreens is missing a part (rebuild the scene).", this);
                enabled = false;
                return;
            }

            playButton.onClick.AddListener(() => Raise(PlayPressed));
            upgradeButton.onClick.AddListener(() => Raise(UpgradePressed));
            primaryButton.onClick.AddListener(() => Raise(PrimaryPressed));
            homeButton.onClick.AddListener(() => Raise(HomePressed));
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

        public void HideAll()
        {
            home.SetActive(false);
            result.SetActive(false);
        }

        /// <summary>Fills and shows the home screen from the current progress.</summary>
        public void ShowHome(ClaybreakWorld world, Progression progression)
        {
            result.SetActive(false);
            home.SetActive(true);
            SaveData save = progression.Save;
            homeCoins.text = NumberLabels.Plain(save.coins);
            int levelCount = world.LevelCount;
            string loop = save.loop > 0 ? $"  ·  TOUGHER x{progression.HealthMultiplier:0.##}" : string.Empty;
            worldText.text = $"{world.WorldName}  ·  {progression.CurrentLevel.Title}{loop}";
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
            upgradeTitle.text = "STARTING ARMY";
            if (progression.UpgradeMaxed)
            {
                upgradeDetail.text = $"+{progression.StartingArmyBonus} units every run  ·  MAX";
                upgradeButtonLabel.text = "MAX";
            }
            else
            {
                upgradeDetail.text = $"Now +{progression.StartingArmyBonus}, next +{progression.NextUpgradeBonus} units per run";
                upgradeButtonLabel.text = progression.NextUpgradeCost + " ¢";
            }

            bool canBuy = progression.CanBuyUpgrade;
            upgradeButton.interactable = canBuy;
            upgradeButton.image.color = canBuy ? buttonEnabled : buttonDisabled;

            unitTitle.text = "BRUTE";
            unitDetail.text = save.bruteUnlocked
                ? $"Grab crates on the run.\nHits x{world.BruteDamage}, shrugs off {world.BruteArmor} blast."
                : $"Locked: beat {world.Levels[world.BruteUnlockLevel].Title}.";
            RefreshSwitches();
        }

        public void ShowResult(LevelOutcome outcome, string detail, bool nextIsBoss)
        {
            home.SetActive(false);
            result.SetActive(true);
            resultTitle.text = outcome.Won ? (outcome.ClearedWorld ? "BOSS DOWN!" : "TOWER DOWN!") : "ARMY LOST";
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].gameObject.SetActive(outcome.Won);
                stars[i].color = i < outcome.Stars ? starOn : starOff;
            }

            resultDetail.text = detail;
            resultReward.text = "+" + outcome.Coins + " ¢";
            string unlock = outcome.UnlockedBrute ? "NEW UNIT: BRUTE\nGrab its crates on the run."
                : outcome.ClearedWorld ? "WORLD CLEARED\nThe towers come back tougher."
                : string.Empty;
            resultUnlock.text = unlock;
            resultUnlock.gameObject.SetActive(unlock.Length > 0);
            primaryLabel.text = outcome.Won ? (nextIsBoss ? "BOSS" : "NEXT") : "AGAIN";
        }

        private void RefreshSwitches()
        {
            soundLabel.text = GameAudio.SoundMuted ? "SOUND OFF" : "SOUND ON";
            musicLabel.text = GameAudio.MusicMuted ? "MUSIC OFF" : "MUSIC ON";
            vibrationLabel.text = GameHaptics.Enabled ? "VIBRATION ON" : "VIBRATION OFF";
        }
    }
}
