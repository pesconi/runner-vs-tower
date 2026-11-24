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
    /// generated Gradle project here. Without the file, or with a file that has no Android app for this build's
    /// package (RT-9b.1: a file registered under an older application ID), the build still succeeds with a warning and
    /// measures nothing (the sink is not created at run time). A malformed file fails the build, and so does a
    /// missing or wrong-package file in a build signed with the upload key (that one goes to Google Play).
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
            // A build signed with the upload key goes to Google Play: there, no Firebase configuration is an error, not a
            // warning (RT-9b.1). Debug-signed builds (phone tests, rt4-check) go on without it.
            bool forPlay = PlayerSettings.Android.useCustomKeystore;
            if (config == null && forPlay)
            {
                throw new InvalidOperationException("[ClaybreakFirebase] A build signed with the upload key needs google-services.json ("
                    + DefaultPath + " or " + EnvironmentVariable + ").");
            }

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

            string json = File.ReadAllText(config);
            string package = PlayerSettings.GetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android);
            if (!HasAndroidApp(json, package) && forPlay)
            {
                throw new InvalidOperationException("[ClaybreakFirebase] " + config + " has no Android app for the package " + package
                    + ": a build signed with the upload key cannot leave Firebase out (docs/store/passo-a-passo-console.md, step 4b).");
            }

            if (!HasAndroidApp(json, package))
            {
                // RT-9b.1: after an application ID change the old file belongs to another app. The build goes on without
                // Firebase (nothing is measured) instead of failing; a malformed file still fails below.
                FirebaseAndroidConfig.RemoveFrom(unityLibraryPath);
                Debug.LogWarning("[ClaybreakFirebase] WARNING: " + config + " has no Android app for the package " + package
                    + ": this build carries NO Firebase configuration and measures nothing. Add an Android app with this package"
                    + " in the Firebase console, download its google-services.json to the same place and check it with"
                    + " Tools/rt8b-firebase-check.sh (docs/store/passo-a-passo-console.md).");
                return;
            }

            string xml = FirebaseAndroidConfig.ToValuesXml(json, package);
            FirebaseAndroidConfig.WriteInto(unityLibraryPath, xml);
            Debug.Log("[ClaybreakFirebase] Firebase configuration written into the Gradle project (values/"
                + FirebaseAndroidConfig.ResourceFileName + ").");
        }

        [Serializable]
        private sealed class ServicesClients
        {
            public ClientEntry[] client;
        }

        [Serializable]
        private sealed class ClientEntry
        {
            public ClientInfo client_info;
        }

        [Serializable]
        private sealed class ClientInfo
        {
            public AndroidInfo android_client_info;
        }

        [Serializable]
        private sealed class AndroidInfo
        {
            public string package_name;
        }

        /// <summary>
        /// Whether <paramref name="json"/> (a google-services.json) has an Android app for <paramref name="package"/>.
        /// Text that is not JSON answers true, so the kit's parser reports what is wrong with it.
        /// </summary>
        public static bool HasAndroidApp(string json, string package)
        {
            ServicesClients file;
            try
            {
                file = JsonUtility.FromJson<ServicesClients>(json);
            }
            catch (ArgumentException)
            {
                return true;
            }

            if (file?.client == null)
            {
                return true;
            }

            foreach (ClientEntry entry in file.client)
            {
                if (entry?.client_info?.android_client_info?.package_name == package)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
