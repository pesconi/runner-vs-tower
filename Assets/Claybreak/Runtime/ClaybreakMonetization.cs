using JungleDegree.StudioKit.Ads;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// How the game makes money and brings players back (RT-7, decisions D5 and D6), as one asset written by
    /// <c>MonetizationContent</c>: the ad units (the kit's <see cref="AdUnitSettings"/>; empty until RT-9, so
    /// every build uses Google's test units), the two rewarded uses of GDD 7.2 (double the level coins, continue
    /// once per level), the interstitial between levels (ready but OFF: D6) with its limits, and the daily
    /// reward cycle (D5: local, no server, no penalty for missing a day). No in-app purchase (D6).
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/Monetization", fileName = "Monetization")]
    public sealed class ClaybreakMonetization : ScriptableObject
    {
        [SerializeField] private AdUnitSettings adUnits;

        [Header("Rewarded (GDD 7.2, always optional, gain shown before)")]
        [SerializeField] private bool doubleCoins = true;
        [SerializeField] private bool continueOnce = true;
        [Tooltip("Share of the army at the firing line that a continue brings back.")]
        [SerializeField, Range(0.1f, 1f)] private float continueArmyShare = 0.5f;
        [Tooltip("A continue is offered only when at least this many units reached the firing line.")]
        [SerializeField, Min(1)] private int continueMinimumArmy = 10;

        [Header("Interstitial (D6: ready but off for the first soft-launch cohort)")]
        [SerializeField] private bool interstitialOn;
        [SerializeField] private InterstitialPacing interstitialPacing = new InterstitialPacing();

        [Header("Daily reward (D5)")]
        [Tooltip("Coins per claim, in order; after the last the cycle starts again. Missing a day never resets it.")]
        [SerializeField] private int[] dailyRewards = { 50, 75, 100, 125, 150, 200, 300 };

        public AdUnitSettings AdUnits => adUnits;
        public bool DoubleCoins => doubleCoins;
        public bool ContinueOnce => continueOnce;
        public float ContinueArmyShare => continueArmyShare;
        public int ContinueMinimumArmy => continueMinimumArmy;
        public bool InterstitialOn => interstitialOn;
        public InterstitialPacing InterstitialPacing => interstitialPacing;
        public int[] DailyRewards => dailyRewards;

        /// <summary>Units a continue brings back for <paramref name="armyAtLine"/> units at the firing line.</summary>
        public int ContinueArmy(int armyAtLine)
        {
            return Mathf.Max(1, Mathf.RoundToInt(armyAtLine * continueArmyShare));
        }

        /// <summary>Writes the values (editor content builder and tests).</summary>
        public void Configure(AdUnitSettings units, bool doubleCoinsOn, bool continueOn, float armyShare, int minimumArmy,
            bool interstitial, InterstitialPacing pacing, int[] daily)
        {
            adUnits = units;
            doubleCoins = doubleCoinsOn;
            continueOnce = continueOn;
            continueArmyShare = armyShare;
            continueMinimumArmy = minimumArmy;
            interstitialOn = interstitial;
            interstitialPacing = pacing;
            dailyRewards = daily;
        }

        /// <summary>The rules the game relies on; the validator adds the release rules (test units only until RT-9).</summary>
        public bool TryValidate(out string error)
        {
            if (adUnits == null)
            {
                error = "the ad unit settings are missing.";
                return false;
            }

            if (continueArmyShare <= 0f || continueArmyShare > 1f || continueMinimumArmy < 1)
            {
                error = "the continue share or minimum army is out of range.";
                return false;
            }

            if (interstitialPacing == null || !interstitialPacing.IsValid)
            {
                error = "the interstitial limits are invalid.";
                return false;
            }

            if (dailyRewards == null || dailyRewards.Length == 0)
            {
                error = "the daily reward cycle is empty.";
                return false;
            }

            foreach (int coins in dailyRewards)
            {
                if (coins <= 0 || coins > DailyReward.MaximumClaim)
                {
                    error = $"a daily reward of {coins} coins is out of range.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
