using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Claybreak
{
    /// <summary>
    /// In-level HUD (RT-4): level name, army and special-unit count, coins, the tower bar during the assault, a
    /// hint line and a short banner (floor down, boss rule). Texts change only when their value changes;
    /// the counters use cached strings (<see cref="NumberLabels"/>), so a frame never allocates. RT-6t.9: the army
    /// counter is TextMeshPro (Baloo 2 with outline) and pops (scale 1.18 in 0.15 s and back) when it changes; the
    /// gate numbers float up from <see cref="FloatingNumbers"/>.
    /// </summary>
    public sealed class ClaybreakHud : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text levelText;
        [SerializeField] private TMP_Text armyText;
        [SerializeField] private Text specialText;
        [SerializeField] private Text coinsText;
        [SerializeField] private GameObject towerBar;
        [SerializeField] private Image towerFill;
        [SerializeField] private Text towerText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text bannerText;
        [SerializeField] private FloatingNumbers floatingNumbers;
        [SerializeField, Min(0.5f)] private float bannerSeconds = 1.8f;

        /// <summary>The counter's pop, after the prototype's <c>.count.pop</c> (scale 1.18, CSS transition 0.15 s).</summary>
        public const float PopScale = 1.18f;
        public const float PopHalfSeconds = 0.15f;

        private int shownArmy = -1;
        private int shownSpecials = -1;
        private int shownTower = -1;
        private int towerMaximum = 1;
        private float bannerTime;
        private float popAge = float.MaxValue;

        public bool HasParts => root != null && levelText != null && armyText != null && specialText != null
            && coinsText != null && towerBar != null && towerFill != null && towerText != null && hintText != null
            && bannerText != null && floatingNumbers != null && floatingNumbers.HasParts;

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
            floatingNumbers.Clear();
            // Update stops while hidden: a pop in flight (or one started by the level setup) would stay frozen.
            popAge = float.MaxValue;
            armyText.rectTransform.localScale = Vector3.one;
        }

        /// <summary>The army counter's current scale (tests and captures).</summary>
        public float ArmyScale => armyText.rectTransform.localScale.x;

        public FloatingNumbers Floating => floatingNumbers;

        /// <summary>Floats the gate the crowd just crossed above the crowd.</summary>
        public void FloatGate(GateSpec gate)
        {
            floatingNumbers.Show(gate);
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
                bool first = shownArmy < 0;
                shownArmy = army;
                armyText.text = NumberLabels.Army(army);
                // A pop already running is not restarted: in the assault the count changes every few frames.
                if (!first && popAge >= 2f * PopHalfSeconds)
                {
                    popAge = 0f;
                }
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

        /// <summary>Counter scale <paramref name="age"/> seconds after a pop: up to <see cref="PopScale"/> and back, eased out.</summary>
        public static float CounterPop(float age)
        {
            if (age < 0f || age >= 2f * PopHalfSeconds)
            {
                return 1f;
            }

            float u = age < PopHalfSeconds ? age / PopHalfSeconds : 2f - age / PopHalfSeconds;
            float eased = 1f - (1f - u) * (1f - u);
            return 1f + (PopScale - 1f) * eased;
        }

        private void Update()
        {
            if (popAge < 2f * PopHalfSeconds)
            {
                popAge += Time.deltaTime;
                float counter = CounterPop(popAge);
                armyText.rectTransform.localScale = new Vector3(counter, counter, 1f);
            }

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
