using JungleDegree.StudioKit.Ads;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// Writes the monetization asset (RT-7) on every scene build, like the catalog: the values below are the
    /// source of truth. The ad unit asset is created once and never overwritten, so the production units set in
    /// RT-9 (D7, by the user) survive rebuilds; until then it stays empty and every build uses Google's test units.
    /// </summary>
    public static class MonetizationContent
    {
        public const string AdUnitsPath = WorldContent.DataFolder + "/AdUnits.asset";
        public const string MonetizationPath = WorldContent.DataFolder + "/Monetization.asset";

        /// <summary>
        /// The interstitial switch (D6: ready but OFF for the first soft-launch cohort, until a clean retention
        /// reading). Turning it on is a user decision; the validator fails while it is on before that decision.
        /// </summary>
        public const bool InterstitialOn = false;

        // Interstitial limits [P]: after 3 levels and 3 minutes of session, then every 3 levels and 3 minutes
        // (D6: the final values come with the real eCPM; the switch above keeps them unused until then).
        public static InterstitialPacing Pacing => new InterstitialPacing(3, 3, 180f, 180f);

        // Daily cycle [P] (D5): about a level of coins at first, up to three on day 7 of the cycle; 365 coins
        // per minute of play on average (docs/rt6-economia.md), so the gift helps without replacing play.
        public static readonly int[] DailyRewards = { 50, 75, 100, 125, 150, 200, 300 };

        /// <summary>
        /// RT-8: public page of the privacy policy, opened from the settings page. Empty until RT-9 publishes
        /// docs/politica-de-privacidade.md at a public https address (the settings link then says it comes with
        /// the store release); the monetization validator refuses a production build without it.
        /// </summary>
        public const string PrivacyPolicyUrl = "";

        public static void Write()
        {
            var units = AssetDatabase.LoadAssetAtPath<AdUnitSettings>(AdUnitsPath);
            if (units == null)
            {
                units = ScriptableObject.CreateInstance<AdUnitSettings>();
                AssetDatabase.CreateAsset(units, AdUnitsPath);
            }

            var monetization = AssetDatabase.LoadAssetAtPath<ClaybreakMonetization>(MonetizationPath);
            if (monetization == null)
            {
                monetization = ScriptableObject.CreateInstance<ClaybreakMonetization>();
                AssetDatabase.CreateAsset(monetization, MonetizationPath);
            }

            monetization.Configure(units, true, true, 0.5f, 10, InterstitialOn, Pacing, (int[])DailyRewards.Clone());
            monetization.SetPrivacyPolicyUrl(PrivacyPolicyUrl);
            EditorUtility.SetDirty(monetization);
            AssetDatabase.SaveAssets();
        }
    }
}
