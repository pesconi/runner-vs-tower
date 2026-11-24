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
    /// D6 rewarded + interstitial OFF + no in-app purchase, D4 + D21 minimum measurement through Firebase Analytics in
    /// its minimal setup), the ad
    /// units (the committed assets hold only Google's sample app and no unit: the studio's IDs live outside the
    /// repository and enter only the game build, D35; when that file is present it must be a production pair), the
    /// scene wiring (both ad services on the game's units, no forced test ads, the gateway complete), the
    /// packages (no billing, no other analytics SDK), the Firebase module (one pinned dependency, collection off and
    /// no advertising ID in its manifest, the configuration file outside the repository) and the privacy documents.
    /// Batchmode: -executeMethod Claybreak.Editor.MonetizationValidation.RunAndQuit (also part of ClaybreakValidation).
    /// </summary>
    public static class MonetizationValidation
    {

        public const string PrivacyPolicyPath = "docs/politica-de-privacidade.md";
        public const string DataSafetyPath = "docs/data-safety.md";
        private const string ManifestPath = "Packages/manifest.json";
        private const string AdsSettingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
        private const string MainGradleTemplatePath = "Assets/Plugins/Android/mainTemplate.gradle";
        private const string ConfigFileName = "google-services.json";

        /// <summary>Manifest flags of the minimal Firebase setup (D21 = A); each must be present and "false".</summary>
        public static readonly string[] FirebaseFlagsOff =
        {
            "firebase_analytics_collection_enabled", "google_analytics_adid_collection_enabled",
            "google_analytics_ssaid_collection_enabled", "google_analytics_default_allow_ad_personalization_signals",
            "google_analytics_default_allow_analytics_storage", "google_analytics_default_allow_ad_storage",
            "google_analytics_default_allow_ad_user_data", "google_analytics_default_allow_ad_personalization",
            "google_analytics_automatic_screen_reporting_enabled"
        };

        // Packages that would add billing or another analytics SDK (D6: no IAP; D21: Firebase Analytics only, through
        // the native module below, never the heavier Unity Firebase packages).
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
            Check(monetization.PrivacyPolicyUrl.Length > 0,
                "privacy: a production build needs the public privacy policy link (MonetizationContent.PrivacyPolicyUrl)", failures);
            CheckPackages(failures);
            CheckFirebaseModule(failures);
            CheckDocuments(failures);
            CheckScene(monetization, failures);
            return failures;
        }

        private static void Report(List<string> failures)
        {
            if (failures.Count == 0)
            {
                Debug.Log("PASS Claybreak monetization validation: decisions D4/D5/D6/D21/D35, no real AdMob ID in the repository, scene wiring, packages, "
                    + "minimal Firebase module and privacy documents.");
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

            // D35: the committed assets never hold the studio's IDs (a build that stopped half way could leave them).
            Check(units.AndroidRewardedUnitId.Length == 0 && units.AndroidInterstitialUnitId.Length == 0,
                $"ads: {MonetizationContent.AdUnitsPath} holds an ad unit; the real IDs stay outside the repository (D35): restore it with git",
                failures);
            Check(ReadAppId() == ProjectConfiguration.SampleAdMobAppId,
                $"ads: {AdsSettingsPath} must keep Google's sample app id; the real one stays outside the repository (D35): restore it with git",
                failures);

            string privateIds = ClaybreakAdMob.ConfigPath();
            if (File.Exists(privateIds))
            {
                try
                {
                    ClaybreakAdMob.Load(privateIds);
                }
                catch (System.Exception exception) when (exception is System.InvalidOperationException || exception is IOException
                    || exception is System.UnauthorizedAccessException)
                {
                    failures.Add("ads: " + exception.Message);
                }
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
                Check(!manifest.Contains("\"" + package), $"packages: '{package}' would add billing or another analytics SDK (D6/D21)", failures);
            }

            foreach (string file in Directory.GetFiles("Assets", "*.xml", SearchOption.AllDirectories))
            {
                if (file.Replace('\\', '/').Contains("/Plugins/Android/"))
                {
                    Check(!File.ReadAllText(file).Contains("BILLING"), $"android: {file} asks for the billing permission (D6)", failures);
                }
            }
        }

        private static void CheckFirebaseModule(List<string> failures)
        {
            string gradlePath = ClaybreakFirebase.ModulePath + "/build.gradle";
            string manifestPath = ClaybreakFirebase.ModulePath + "/AndroidManifest.xml";
            if (!File.Exists(gradlePath) || !File.Exists(manifestPath))
            {
                failures.Add("firebase: missing " + ClaybreakFirebase.ModulePath + " (build.gradle and AndroidManifest.xml)");
                return;
            }

            var dependencies = new List<string>();
            foreach (string line in File.ReadAllLines(gradlePath))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("implementation") || trimmed.StartsWith("api") || trimmed.StartsWith("runtimeOnly"))
                {
                    dependencies.Add(trimmed);
                }
            }

            Check(dependencies.Count == 1
                && dependencies[0] == "implementation 'com.google.firebase:firebase-analytics:" + ClaybreakFirebase.FirebaseAnalyticsVersion + "'",
                "firebase: the module must depend only on firebase-analytics " + ClaybreakFirebase.FirebaseAnalyticsVersion
                + " (pinned, no other Firebase product)", failures);
            string template = File.Exists(MainGradleTemplatePath) ? File.ReadAllText(MainGradleTemplatePath) : string.Empty;
            Check(!template.Contains("com.google.firebase"), "firebase: no Firebase dependency outside the module (mainTemplate.gradle)", failures);

            string manifest = File.ReadAllText(manifestPath);
            foreach (string flag in FirebaseFlagsOff)
            {
                Check(manifest.Contains("android:name=\"" + flag + "\" android:value=\"false\""),
                    $"firebase: the manifest must set {flag} to false (D21 minimal setup)", failures);
            }

            Check(!manifest.Contains("collection_deactivated"), "firebase: collection must be switchable by consent, not deactivated", failures);

            foreach (string folder in new[] { "Assets", "ProjectSettings", "Packages" })
            {
                foreach (string file in Directory.GetFiles(folder, ConfigFileName, SearchOption.AllDirectories))
                {
                    failures.Add($"firebase: {file} must not be in the repository (keep it in {ClaybreakFirebase.DefaultPath})");
                }
            }

            string ignore = File.Exists(".gitignore") ? File.ReadAllText(".gitignore") : string.Empty;
            Check(ignore.Contains(ConfigFileName), "firebase: .gitignore must refuse " + ConfigFileName, failures);
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

                foreach (string must in new[] { "AdMob", "UMP", "analytics", "Firebase" })
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
