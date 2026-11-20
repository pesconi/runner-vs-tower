using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JungleDegree.StudioKit.Crowd;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The game's validator (RT-2): level data against the pool, the theme, the scene wiring, the
    /// preallocated pools, and the code rules of the prototype (no Instantiate/Destroy at run time,
    /// rule 2.2.1; no physics queries, rule 2.2.7).
    /// Batchmode: -executeMethod Claybreak.Editor.ClaybreakValidation.RunAndQuit
    /// </summary>
    public static class ClaybreakValidation
    {
        private const string RuntimeFolder = "Assets/Claybreak/Runtime";
        private static readonly Regex ForbiddenRuntimeCalls = new Regex(
            @"\b(Instantiate|Destroy|DestroyImmediate)\s*\(|new\s+GameObject\s*\(|\.AddComponent\s*<|\bPhysics\.",
            RegexOptions.Compiled);

        [MenuItem("Claybreak/Validate Prototype")]
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
            CheckData(failures);
            CheckRuntimeCode(failures);
            CheckScene(failures);
            return failures;
        }

        private static void Report(List<string> failures)
        {
            if (failures.Count == 0)
            {
                Debug.Log("PASS Claybreak validation: levels, theme, scene, pools and runtime code rules.");
            }
            else
            {
                Debug.LogError("FAIL Claybreak validation:\n" + string.Join("\n", failures));
            }
        }

        private static void CheckData(List<string> failures)
        {
            var level = AssetDatabase.LoadAssetAtPath<PrototypeLevel>(PrototypeSceneBuilder.LevelPath);
            var stress = AssetDatabase.LoadAssetAtPath<PrototypeLevel>(PrototypeSceneBuilder.StressLevelPath);
            var theme = AssetDatabase.LoadAssetAtPath<ClaybreakTheme>(PrototypeSceneBuilder.ThemePath);
            if (level == null || stress == null || theme == null)
            {
                failures.Add("missing level, device-gate level or theme (run Claybreak > Build Prototype Scene)");
                return;
            }

            CheckLevel(level, failures);
            CheckLevel(stress, failures);
            if (level.IsDeviceGateLevel)
            {
                failures.Add($"{level.name}: the played level must not auto-launch or recycle units");
            }

            if (level.RunGates.Count < 3 || level.RunGates.Count > RunGateCourse.MaximumPairs)
            {
                failures.Add($"{level.name}: RT-2 asks for 3-{RunGateCourse.MaximumPairs} run gate pairs, has {level.RunGates.Count}");
            }

            if (!stress.IsDeviceGateLevel || stress.StartCount != PrototypeSceneBuilder.PoolSize
                || stress.MaximumUnitsInFlight != PrototypeSceneBuilder.PoolSize)
            {
                failures.Add($"{stress.name}: the device-gate level must auto-launch with {PrototypeSceneBuilder.PoolSize} units in flight");
            }

            if (!theme.TryValidate(out string error))
            {
                failures.Add("theme: " + error);
            }
        }

        private static void CheckLevel(PrototypeLevel level, List<string> failures)
        {
            if (!level.TryValidate(PrototypeSceneBuilder.PoolSize, out string error))
            {
                failures.Add($"{level.name}: {error}");
            }

            if (level.MaximumProjectiles > PrototypeSceneBuilder.ProjectilePoolSize)
            {
                failures.Add($"{level.name}: {level.MaximumProjectiles} projectiles exceed the scene pool of {PrototypeSceneBuilder.ProjectilePoolSize}");
            }
        }

        private static void CheckRuntimeCode(List<string> failures)
        {
            foreach (string file in Directory.GetFiles(RuntimeFolder, "*.cs", SearchOption.AllDirectories))
            {
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string code = lines[i].Trim();
                    if (code.StartsWith("//") || code.StartsWith("///"))
                    {
                        continue;
                    }

                    if (ForbiddenRuntimeCalls.IsMatch(code))
                    {
                        failures.Add($"{file.Replace('\\', '/')}:{i + 1}: runtime code creates/destroys objects or queries physics: {code}");
                    }
                }
            }
        }

        private static void CheckScene(List<string> failures)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length != 1 || scenes[0].path != PrototypeSceneBuilder.ScenePath || !scenes[0].enabled)
            {
                failures.Add("build settings must list only " + PrototypeSceneBuilder.ScenePath);
            }

            if (!File.Exists(PrototypeSceneBuilder.ScenePath))
            {
                failures.Add("missing scene " + PrototypeSceneBuilder.ScenePath);
                return;
            }

            var previous = EditorSceneManager.GetActiveScene().path;
            var scene = EditorSceneManager.OpenScene(PrototypeSceneBuilder.ScenePath, OpenSceneMode.Single);
            var flow = Object.FindFirstObjectByType<ClaybreakFlow>();
            if (flow == null)
            {
                failures.Add("the scene has no ClaybreakFlow");
                return;
            }

            var so = new SerializedObject(flow);
            SerializedProperty property = so.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
                {
                    failures.Add("ClaybreakFlow." + property.name + " is not assigned");
                }
            }

            // The kit's leader is a CharacterController with gravity: it needs a collider to stand on.
            var track = so.FindProperty("track").objectReferenceValue as Renderer;
            var leader = Object.FindFirstObjectByType<PlayerController>();
            if (track == null || !track.TryGetComponent(out Collider ground) || !ground.enabled)
            {
                failures.Add("the track needs an enabled collider (the leader would fall through it)");
            }
            else if (leader == null || !ground.bounds.Contains(new Vector3(leader.transform.position.x,
                         ground.bounds.max.y - 0.01f, leader.transform.position.z)))
            {
                failures.Add("the leader does not start above the track collider");
            }

            var pool = Object.FindFirstObjectByType<RunnerPool>(FindObjectsInactive.Include);
            if (pool == null || pool.Capacity != PrototypeSceneBuilder.PoolSize)
            {
                failures.Add($"the scene needs a RunnerPool with {PrototypeSceneBuilder.PoolSize} preallocated units");
            }

            var cannon = Object.FindFirstObjectByType<CannonDefense>();
            if (cannon == null || cannon.PoolSize != PrototypeSceneBuilder.ProjectilePoolSize)
            {
                failures.Add($"the cannon needs {PrototypeSceneBuilder.ProjectilePoolSize} preallocated projectiles");
            }

            var gates = Object.FindFirstObjectByType<RunGateCourse>();
            if (gates == null || gates.Capacity != RunGateCourse.MaximumPairs)
            {
                failures.Add($"the run needs {RunGateCourse.MaximumPairs} preallocated gate pairs");
            }

            if (scene.isDirty)
            {
                failures.Add("opening the scene changed it (rebuild the scene)");
            }

            if (!string.IsNullOrEmpty(previous) && previous != scene.path)
            {
                EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
            }
        }
    }
}
