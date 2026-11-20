using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// Android APK of the prototype, outside the repository (debug key, nothing uploaded anywhere).
    /// Batchmode: -buildTarget Android -executeMethod Claybreak.Editor.ClaybreakBuild.BuildAndroidAndQuit
    ///   [-claybreakApk C:\temp\rt2\Claybreak.apk] [-claybreakMeasure]
    /// -claybreakMeasure adds RELEASE_MEASUREMENT: a release player that also reads the device-gate
    /// extras (`--ez stress true`, the kit's `--ei fps N`) and logs the kit's frame timing probe.
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
                string output = BatchRunner.GetArgument("-claybreakApk") ?? DefaultApk;
                Build(output, measure);
            });
        }

        public static void Build(string output, bool measure)
        {
            ProjectConfiguration.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { PrototypeSceneBuilder.ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None,
                extraScriptingDefines = measure ? new[] { "RELEASE_MEASUREMENT" } : Array.Empty<string>(),
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Android build " + report.summary.result);
            }

            Debug.Log($"[ClaybreakBuild] {(measure ? "Measurement" : "Prototype")} APK: {output} "
                + $"({new FileInfo(output).Length / (1024 * 1024)} MB, {report.summary.totalTime.TotalSeconds:0} s)");
        }
    }
}
