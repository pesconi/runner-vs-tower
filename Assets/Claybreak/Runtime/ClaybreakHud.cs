using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>
    /// In-level HUD (RT-4): level name, army and special-unit count, coins, the tower bar during the assault, a
    /// hint line and a short banner (floor down, boss rule). Texts change only when their value changes;
    /// the counters use cached strings (<see cref="NumberLabels"/>), so a frame never allocates.
    /// </summary>
    public sealed class ClaybreakHud : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text levelText;
        [SerializeField] private Text armyText;
        [SerializeField] private Text specialText;
        [SerializeField] private Text coinsText;
        [SerializeField] private GameObject towerBar;
        [SerializeField] private Image towerFill;
        [SerializeField] private Text towerText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text bannerText;
        [SerializeField, Min(0.5f)] private float bannerSeconds = 1.8f;

        private int shownArmy = -1;
        private int shownSpecials = -1;
        private int shownTower = -1;
        private int towerMaximum = 1;
        private float bannerTime;

        public bool HasParts => root != null && levelText != null && armyText != null && specialText != null
            && coinsText != null && towerBar != null && towerFill != null && towerText != null && hintText != null
            && bannerText != null;

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("ClaybreakHud is missing a part (rebuild the scene).", this);
                enabled = false;
            }
        }

        public void Show(bool visible)
        {
            root.SetActive(visible);
        }

        public void SetLevel(string title)
        {
            levelText.text = title;
        }

        public void SetCoins(int coins)
        {
            coinsText.text = NumberLabels.Plain(coins);
        }

        public void SetArmy(int army)
        {
            if (army != shownArmy)
            {
                shownArmy = army;
                armyText.text = NumberLabels.Army(army);
            }
        }

        /// <summary>Special units left in the army (all loadout types together).</summary>
        public void SetSpecials(int count, bool visible)
        {
            specialText.gameObject.SetActive(visible);
            if (visible && count != shownSpecials)
            {
                shownSpecials = count;
                specialText.text = NumberLabels.Specials(count);
            }
        }

        public void ShowTower(bool visible, int maximum)
        {
            towerBar.SetActive(visible);
            towerMaximum = Mathf.Max(1, maximum);
            shownTower = -1;
        }

        public void SetTower(int health)
        {
            if (health == shownTower)
            {
                return;
            }

            shownTower = health;
            towerText.text = NumberLabels.Plain(health);
            towerFill.fillAmount = (float)health / towerMaximum;
        }

        public void SetHint(string hint)
        {
            hintText.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            hintText.text = hint;
        }

        public void Banner(string message)
        {
            bannerText.text = message;
            bannerText.gameObject.SetActive(true);
            bannerTime = bannerSeconds;
        }

        public void ClearBanner()
        {
            bannerTime = 0f;
            bannerText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (bannerTime <= 0f)
            {
                return;
            }

            bannerTime -= Time.deltaTime;
            float pop = 1f + 0.25f * Mathf.Clamp01((bannerTime - bannerSeconds + 0.2f) / 0.2f);
            bannerText.rectTransform.localScale = new Vector3(pop, pop, 1f);
            if (bannerTime <= 0f)
            {
                bannerText.gameObject.SetActive(false);
            }
        }
    }
}
