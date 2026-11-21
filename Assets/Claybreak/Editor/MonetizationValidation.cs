using System.Collections.Generic;
using System.IO;
using JungleDegree.StudioKit.Ads;
using JungleDegree.StudioKit.Analytics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The monetization validator (RT-7 acceptance): the monetization asset and its decisions (D5 daily reward,
    /// D6 rewarded + interstitial OFF + no in-app purchase, D4 no analytics SDK before the tool is chosen), the ad
    /// units (only Google's test units until RT-9; a production unit must be well formed and come with a real app
    /// ID), the scene wiring (both ad services on the game's units, no forced test ads, the gateway complete), the
    /// manifest (no billing, no analytics SDK) and the privacy documents the policy needs.
    /// Batchmode: -executeMethod Claybreak.Editor.MonetizationValidation.RunAndQuit (also part of ClaybreakValidation).
    /// </summary>
    public static class MonetizationValidation
    {
        /// <summary>
        /// False until the user hands over the AdMob account units (RT-9, decision D7): until then the unit fields
        /// stay empty and every build shows Google's test ads. RT-9 sets this to true with the real IDs.
        /// </summary>
        public const bool ProductionUnitsAllowed = false;

        public const string PrivacyPolicyPath = "docs/politica-de-privacidade.md";
        public const string DataSafetyPath = "docs/data-safety.md";
        private const string ManifestPath = "Packages/manifest.json";
        private const string AdsSettingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        // Packages that would add billing or an analytics SDK (D6: no IAP; D4: nothing installed before the ficha).
        private static readonly string[] ForbiddenPackages =
        {
            "com.unity.purchasing", "com.android.billingclient", "com.google.firebase", "com.gameanalytics",
            "com.unity.services.analytics", "com.unity.analytics"
        };

        [MenuItem("Claybreak/Validate Monetization")]
        public static void ValidateFromMenu()
        {
            Report(Run());
        }

        public static void RunAndQuit()
        {
            List<string> failures = Run();
            Report(failures);
            EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
        }

        public static List<string> Run()
        {
            var failures = new List<string>();
            var monetization = AssetDatabase.LoadAssetAtPath<ClaybreakMonetization>(MonetizationContent.MonetizationPath);
            if (monetization == null)
            {
                failures.Add("monetization: missing " + MonetizationContent.MonetizationPath + " (rebuild the scene)");
                return failures;
            }

            CheckDecisions(monetization, failures);
            CheckUnits(monetization.AdUnits, failures);
            Check(!ProductionUnitsAllowed || monetization.PrivacyPolicyUrl.Length > 0,
                "privacy: a production build needs the public privacy policy link (MonetizationContent.PrivacyPolicyUrl)", failures);
            CheckPackages(failures);
            CheckDocuments(failures);
            CheckScene(monetization, failures);
            return failures;
        }

        private static void Report(List<string> failures)
        {
            if (failures.Count == 0)
            {
                Debug.Log("PASS Claybreak monetization validation: decisions D4/D5/D6, test ad units, scene wiring, packages and privacy documents.");
            }
            else
            {
                Debug.LogError("FAIL Claybreak monetization validation:\n" + string.Join("\n", failures));
            }
        }

        private static void CheckDecisions(ClaybreakMonetization monetization, List<string> failures)
        {
            if (!monetization.TryValidate(out string error))
            {
                failures.Add("monetization: " + error);
            }

            Check(!monetization.InterstitialOn && !MonetizationContent.InterstitialOn,
                "monetization: the interstitial must stay OFF (D6) until the user turns it on", failures);
            Check(monetization.DoubleCoins && monetization.ContinueOnce,
                "monetization: the two rewarded uses of GDD 7.2 (double coins, continue once) are expected on", failures);
            Check(monetization.InterstitialPacing != null && monetization.InterstitialPacing.LevelsBeforeFirst >= 2
                && monetization.InterstitialPacing.SecondsBeforeFirst >= 60f,
                "monetization: no interstitial in the first minute nor after the first level (GDD 7.1)", failures);
            Check(PlayerMetrics.Events.Count == 4,
                "metrics: only session start, level start/end and the rewarded ad are measured (D4)", failures);
        }

        private static void CheckUnits(AdUnitSettings units, List<string> failures)
        {
            if (units == null)
            {
                failures.Add("ads: the monetization asset has no ad unit settings");
                return;
            }

            string appId = ReadAppId();
            bool production = false;
            foreach (string unit in new[] { units.AndroidRewardedUnitId, units.AndroidInterstitialUnitId })
            {
                if (string.IsNullOrEmpty(unit))
                {
                    continue;
                }

                production = true;
                Check(ProductionUnitsAllowed, $"ads: production unit '{unit}' set before RT-9 (D7: only Google test units in the repo now)", failures);
                Check(AdUnitSettings.IsWellFormedUnitId(unit) && !AdUnitSettings.IsGoogleTestUnit(unit),
                    $"ads: '{unit}' is not a production ad unit id", failures);
                Check(appId != null && unit.Split('/')[0] == appId.Split('~')[0],
                    $"ads: unit '{unit}' does not belong to the app id '{appId}'", failures);
            }

            if (!production)
            {
                Check(appId == ProjectConfiguration.SampleAdMobAppId,
                    $"ads: with test units the app id must be Google's sample app id (found '{appId}')", failures);
            }
        }

        private static string ReadAppId()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AdsSettingsPath);
            if (settings == null)
            {
                return null;
            }

            SerializedProperty appId = new SerializedObject(settings).FindProperty("adMobAndroidAppId");
            return appId?.stringValue;
        }

        private static void CheckPackages(List<string> failures)
        {
            string manifest = File.Exists(ManifestPath) ? File.ReadAllText(ManifestPath) : string.Empty;
            foreach (string package in ForbiddenPackages)
            {
                Check(!manifest.Contains("\"" + package), $"packages: '{package}' would add billing or an analytics SDK (D4/D6)", failures);
            }

            foreach (string file in Directory.GetFiles("Assets", "*.xml", SearchOption.AllDirectories))
            {
                if (file.Replace('\\', '/').Contains("/Plugins/Android/"))
                {
                    Check(!File.ReadAllText(file).Contains("BILLING"), $"android: {file} asks for the billing permission (D6)", failures);
                }
            }
        }

        private static void CheckDocuments(List<string> failures)
        {
            foreach (string path in new[] { PrivacyPolicyPath, DataSafetyPath })
            {
                string text = File.Exists(path) ? File.ReadAllText(path) : null;
                if (text == null)
                {
                    failures.Add("docs: missing " + path);
                    continue;
                }

                foreach (string must in new[] { "AdMob", "UMP", "analytics" })
                {
                    Check(text.Contains(must), $"docs: {path} does not mention {must}", failures);
                }
            }
        }

        private static void CheckScene(ClaybreakMonetization monetization, List<string> failures)
        {
            string previous = EditorSceneManager.GetActiveScene().path;
            if (!File.Exists(ClaybreakSceneBuilder.ScenePath))
            {
                failures.Add("scene: missing " + ClaybreakSceneBuilder.ScenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(ClaybreakSceneBuilder.ScenePath, OpenSceneMode.Single);
            var ads = Object.FindFirstObjectByType<ClaybreakAds>(FindObjectsInactive.Include);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>(FindObjectsInactive.Include);
            RewardedAdService[] rewarded = Object.FindObjectsByType<RewardedAdService>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            InterstitialAdService[] interstitial = Object.FindObjectsByType<InterstitialAdService>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Check(ads != null && ads.HasParts, "scene: ClaybreakAds needs the monetization asset and both ad services", failures);
            Check(rewarded.Length == 1 && interstitial.Length == 1, "scene: exactly one rewarded and one interstitial service", failures);
            if (flow != null)
            {
                var so = new SerializedObject(flow);
                Check(so.FindProperty("monetization").objectReferenceValue == monetization && so.FindProperty("ads").objectReferenceValue == ads,
                    "scene: the flow must use the monetization asset and the scene's ads", failures);
            }

            foreach (Object service in new Object[] { rewarded.Length > 0 ? rewarded[0] : null, interstitial.Length > 0 ? interstitial[0] : null })
            {
                if (service == null)
                {
                    continue;
                }

                var so = new SerializedObject(service);
                Check(so.FindProperty("units").objectReferenceValue == monetization.AdUnits,
                    $"scene: {service.GetType().Name} must read the game's ad units", failures);
                Check(!so.FindProperty("forceTestAds").boolValue, $"scene: {service.GetType().Name} forces test ads", failures);
            }

            if (!string.IsNullOrEmpty(previous) && previous != scene.path)
            {
                EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
            }
        }

        private static void Check(bool ok, string failure, List<string> failures)
        {
            if (!ok)
            {
                failures.Add(failure);
            }
        }
    }
}
