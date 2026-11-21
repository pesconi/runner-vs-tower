using System;
using System.IO;
using JungleDegree.StudioKit.Editor;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// Firebase Analytics configuration at build time (D21 = A). The native SDK comes from
    /// <c>Assets/Plugins/Android/FirebaseAnalytics.androidlib</c> (pinned version, minimal manifest flags); the app's
    /// <c>google-services.json</c> stays outside every repository and is turned into string resources in the
    /// generated Gradle project here. Without the file the build still succeeds and measures nothing (the sink is
    /// not created at run time).
    /// <para>Where the file is read from: the environment variable <see cref="EnvironmentVariable"/>, or
    /// <see cref="DefaultPath"/> (<c>..\_privado\runner-vs-tower\google-services.json</c> beside the repositories).</para>
    /// </summary>
    public sealed class ClaybreakFirebase : IPostGenerateGradleAndroidProject
    {
        public const string EnvironmentVariable = "CLAYBREAK_GOOGLE_SERVICES_JSON";
        public const string ModulePath = "Assets/Plugins/Android/FirebaseAnalytics.androidlib";
        public const string FirebaseAnalyticsVersion = "24.0.0";

        public int callbackOrder => 0;

        /// <summary>The default place of the configuration: outside the repository, next to the games' folders.</summary>
        public static string DefaultPath => Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "_privado",
            "runner-vs-tower", "google-services.json"));

        /// <summary>The configuration file this build uses, or null when there is none.</summary>
        public static string ConfigPath()
        {
            string fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
            string path = string.IsNullOrEmpty(fromEnvironment) ? DefaultPath : Path.GetFullPath(fromEnvironment);
            return File.Exists(path) ? path : null;
        }

        public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
        {
            string config = ConfigPath();
            if (config == null)
            {
                FirebaseAndroidConfig.RemoveFrom(unityLibraryPath);
                Debug.Log("[ClaybreakFirebase] No google-services.json (" + DefaultPath + " or " + EnvironmentVariable
                    + "): this build carries the Firebase SDK but no configuration, so it measures nothing.");
                return;
            }

            if (ClaybreakBuild.InsideARepository(config))
            {
                throw new InvalidOperationException("google-services.json must live outside every git repository (D21).");
            }

            string xml = FirebaseAndroidConfig.ToValuesXml(File.ReadAllText(config),
                PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android));
            FirebaseAndroidConfig.WriteInto(unityLibraryPath, xml);
            Debug.Log("[ClaybreakFirebase] Firebase configuration written into the Gradle project (values/"
                + FirebaseAndroidConfig.ResourceFileName + ").");
        }
    }
}
