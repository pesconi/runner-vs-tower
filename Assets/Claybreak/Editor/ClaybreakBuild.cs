using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// Android release builds of the game, outside the repository (nothing uploaded anywhere).
    /// Batchmode: -buildTarget Android -executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit
    ///   [-claybreakApk C:\temp\rt2\Claybreak.apk] [-claybreakMeasure] [-claybreakAab C:\temp\rt8\Claybreak.aab]
    /// -claybreakMeasure adds RELEASE_MEASUREMENT: a release player that also reads the device-gate
    /// extras (`--ez stress true`, the kit's `--ei fps N`) and logs the kit's frame timing probe.
    /// -claybreakAab writes the Play bundle (AAB) instead of an APK.
    /// Signing (RT-8): every build is a release player. Without a key it is signed with Unity's debug key
    /// (fine for the phone, refused by the Play Store). The upload key never lives in the repository: set
    /// CLAYBREAK_KEYSTORE (path outside the repo), CLAYBREAK_KEYSTORE_PASS, CLAYBREAK_KEY_ALIAS and
    /// CLAYBREAK_KEY_PASS in the environment of the build; the values are used for this build only and the
    /// project settings are restored afterwards, so nothing about the key is saved in ProjectSettings.
    /// </summary>
    public static class ClaybreakBuild
    {
        public const string DefaultApk = @"C:\temp\rt2\Claybreak.apk";

        public static void BuildAndroidAndQuit()
        {
            BatchRunner.Run("ClaybreakBuild", () =>
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                {
                    throw new InvalidOperationException("Run with -buildTarget Android.");
                }

                bool measure = BatchRunner.HasFlag("-claybreakMeasure");
                string aab = BatchRunner.GetArgument("-claybreakAab");
                if (aab != null)
                {
                    Build(aab, measure, true);
                    return;
                }

                Build(BatchRunner.GetArgument("-claybreakApk") ?? DefaultApk, measure, false);
            });
        }

        public static void Build(string output, bool measure)
        {
            Build(output, measure, false);
        }

        public static void Build(string output, bool measure, bool bundle)
        {
            ProjectConfiguration.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            bool previousBundle = EditorUserBuildSettings.buildAppBundle;
            // The measurement player reads debug extras: it never carries the upload key.
            using (var signing = new UploadKeyScope(!measure))
            {
                try
                {
                    EditorUserBuildSettings.buildAppBundle = bundle;
                    BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                    {
                        scenes = new[] { ClaybreakSceneBuilder.ScenePath },
                        locationPathName = output,
                        target = BuildTarget.Android,
                        options = BuildOptions.None,
                        extraScriptingDefines = measure ? new[] { "RELEASE_MEASUREMENT" } : Array.Empty<string>(),
                    });
                    if (report.summary.result != BuildResult.Succeeded)
                    {
                        throw new InvalidOperationException("Android build " + report.summary.result);
                    }

                    long bytes = new FileInfo(output).Length;
                    Debug.Log($"[ClaybreakBuild] {(measure ? "Measurement" : "Game")} {(bundle ? "AAB" : "APK")}: {output} "
                        + $"({bytes / (1024f * 1024f):0.0} MB, {bytes} bytes, {report.summary.totalTime.TotalSeconds:0} s), "
                        + $"version {PlayerSettings.bundleVersion} ({PlayerSettings.Android.bundleVersionCode}), "
                        + $"signed with {(signing.UsesUploadKey ? "the upload key from the environment" : "the debug key")}.");
                }
                finally
                {
                    EditorUserBuildSettings.buildAppBundle = previousBundle;
                }
            }
        }

        /// <summary>True when a folder above the file holds .git (this project, the kit next to it, any repository).</summary>
        public static bool InsideARepository(string file)
        {
            for (DirectoryInfo folder = new FileInfo(file).Directory; folder != null; folder = folder.Parent)
            {
                string git = Path.Combine(folder.FullName, ".git");
                if (Directory.Exists(git) || File.Exists(git))
                {
                    return true;
                }
            }

            return false;
        }

        // Applies the upload key from the environment for one build and restores the previous settings.
        private sealed class UploadKeyScope : IDisposable
        {
            private readonly bool previousCustom;
            private readonly string previousKeystore;
            private readonly string previousAlias;

            public bool UsesUploadKey { get; }

            public UploadKeyScope(bool allowUploadKey)
            {
                previousCustom = PlayerSettings.Android.useCustomKeystore;
                previousKeystore = PlayerSettings.Android.keystoreName;
                previousAlias = PlayerSettings.Android.keyaliasName;
                string keystore = allowUploadKey ? Environment.GetEnvironmentVariable("CLAYBREAK_KEYSTORE") : null;
                if (string.IsNullOrEmpty(keystore))
                {
                    PlayerSettings.Android.useCustomKeystore = false;
                    return;
                }

                // Everything is read and checked before any setting changes: a missing value leaves the project
                // as it was.
                string full = Path.GetFullPath(keystore);
                if (!File.Exists(full))
                {
                    throw new InvalidOperationException("CLAYBREAK_KEYSTORE does not point to a file.");
                }

                if (InsideARepository(full))
                {
                    throw new InvalidOperationException("The upload keystore must live outside any git repository.");
                }

                string keystorePass = Required("CLAYBREAK_KEYSTORE_PASS");
                string alias = Required("CLAYBREAK_KEY_ALIAS");
                string keyPass = Required("CLAYBREAK_KEY_PASS");
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = full;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = keyPass;
                UsesUploadKey = true;
            }

            public void Dispose()
            {
                PlayerSettings.Android.useCustomKeystore = previousCustom;
                PlayerSettings.Android.keystoreName = previousKeystore;
                PlayerSettings.Android.keyaliasName = previousAlias;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
            }

            private static string Required(string name)
            {
                string value = Environment.GetEnvironmentVariable(name);
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException(name + " is not set (the upload key needs it).");
                }

                return value;
            }
        }
    }
}
