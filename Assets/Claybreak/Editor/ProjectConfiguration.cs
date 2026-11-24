using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// This app's Android identity and player settings (each game owns its own, reuso-tecnico 5.3).
    /// No signing data here: release signing stays outside the repository; prototype builds use the
    /// debug key.
    /// </summary>
    public static class ProjectConfiguration
    {
        public const string ApplicationId = "com.jungledegree.runnervstower";
        /// <summary>The name the player sees (launcher, home title, credits): D29.</summary>
        public const string GameName = "Stampeep";
        private const string AdsSettingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
        // Google's public sample AdMob app ID (not an account of the studio). The ads SDK the kit depends on
        // stops the app at launch without an app ID; RT-7 shows Google test ads with it; the real app ID comes with RT-9 (D7).
        public const string SampleAdMobAppId = "ca-app-pub-3940256099942544~3347511713";
        /// <summary>
        /// RT-9a (T-RT9a-4): the launcher icon, the same picture as the store icon (a frame the game drew, made by
        /// Tools/store/make_store_art.py), instead of Unity's default icon.
        /// </summary>
        public const string AppIconPath = "Assets/Claybreak/Content/Icon/AppIcon.png";

        public static void Apply()
        {
            PlayerSettings.companyName = "Jungle Degree";
            // Display name only (D29: "Stampeep", replacing the working name "Clay Siege" of D18). The application ID,
            // the save key and the code namespaces stay as they are, so installs and saves are untouched (T-RT4-6).
            PlayerSettings.productName = GameName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationId);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            // The kit's frame timing probe (measurement builds) reads FrameTimingManager.
            PlayerSettings.enableFrameTimingStats = true;
            ApplyAdsSampleAppId();
            DisableEngineHardwareStatistics();
            ApplyLauncherIcon();
        }

        // Both layers of every adaptive icon slot get the game's icon: the picture
        // is full-bleed and keeps the crowd and the gate in the middle, inside the adaptive icon's safe circle.
        private static void ApplyLauncherIcon()
        {
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AppIconPath);
            if (icon == null)
            {
                throw new InvalidOperationException($"The launcher icon {AppIconPath} is missing (Tools/store/make_store_art.py).");
            }

            // Unity 6 keeps only the adaptive kind (legacy and round are obsolete and generated from it).
            PlatformIconKind kind = UnityEditor.Android.AndroidPlatformIconKind.Adaptive;
            PlatformIcon[] slots = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (PlatformIcon slot in slots)
            {
                for (int layer = 0; layer < slot.maxLayerCount; layer++)
                {
                    slot.SetTexture(icon, layer);
                }
            }

            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, slots);
        }

        // RT-7 (privacy): the player does not send Unity its hardware statistics (Player setting
        // "submitAnalytics"), so the only network SDK of the build is the ads SDK the policy declares.
        private static void DisableEngineHardwareStatistics()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(assets[0]);
            SerializedProperty submit = so.FindProperty("submitAnalytics");
            if (submit != null && submit.boolValue)
            {
                submit.boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ApplyAdsSampleAppId()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AdsSettingsPath);
            if (settings == null)
            {
                // The plug-in creates its settings only from its Inspector; batchmode builds need them.
                Type type = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                    .FirstOrDefault(t => t.FullName == "GoogleMobileAds.Editor.GoogleMobileAdsSettings");
                if (type == null)
                {
                    throw new InvalidOperationException("The Google Mobile Ads editor plug-in (kit dependency) is missing.");
                }

                ClaybreakSceneBuilder.EnsureFolder("Assets/GoogleMobileAds/Resources");
                settings = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(settings, AdsSettingsPath);
            }

            var so = new SerializedObject(settings);
            SerializedProperty appId = so.FindProperty("adMobAndroidAppId");
            if (appId != null && string.IsNullOrEmpty(appId.stringValue))
            {
                appId.stringValue = SampleAdMobAppId;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
            }
        }
    }
}
