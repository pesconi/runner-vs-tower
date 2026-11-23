using System;
using System.Collections;
using System.IO;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.18: the run obstacles in the game scene.
    /// <list type="bullet">
    /// <item>The obstacle lab (the flow's lab mode, <see cref="ClaybreakFlow.EditorUsesObstacleLab"/>): the crowd runs through
    /// the five kinds; each one kills units as the crowd passes (the clock is set so each is deadly when the crowd arrives:
    /// a straight run could pass the log or the hammer while they are up), the army loses exactly what the course killed,
    /// the kills fly out in the ballistic death, and no object is created while playing.</item>
    /// <item>The device-gate run (world 4, the most obstacles a level holds): the obstacles kill and throw, the crowd stays at
    /// its cap.</item>
    /// <item>With a GPU and <c>-obstacleCaptureDir &lt;folder&gt;</c> (batchmode without -nographics): a capture of each kind
    /// killing units (<c>obstaculo-&lt;kind&gt;.png</c> and the sheet <c>obstaculos-matando.png</c>) from the game's run camera,
    /// and one instanced call per kind on both draw routes.</item>
    /// </list>
    /// </summary>
    public sealed class ObstacleRunTests
    {
        private const string FolderArgument = "-obstacleCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const int Gap = 8;
        // The crowd is put back to this size after an obstacle has passed, so one obstacle cannot end the lab for the others.
        private const int RefillArmy = 200;

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesObstacleLab = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
            ClaybreakFlow.EditorUsesDeviceGateRun = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorDeviceGateWorld = 1;
#endif
            ToonInstanceRouting.Forced = null;
        }

        [UnityTest]
        public IEnumerator EveryKindKillsUnitsAsTheCrowdPasses()
        {
            int[] kills = null;
            int dyingSeen = 0;
            int objectsBefore = 0;
            yield return RunLab((flow, obstacle, killedNow) =>
            {
                dyingSeen = Mathf.Max(dyingSeen, flow.Obstacles.DyingCount);
                return null;
            }, flow => objectsBefore = Objects(), result => kills = result);

            Assert.AreEqual(objectsBefore, Objects(), "an object was created or destroyed while the obstacles killed");
            for (int i = 0; i < kills.Length; i++)
            {
                Assert.Greater(kills[i], 0, $"obstacle {i} killed nobody as the crowd passed");
            }

            Assert.Greater(dyingSeen, 0, "the killed units did not fly out in the ballistic death");
        }

        [UnityTest]
        public IEnumerator TheDeviceGateRunKeepsTheCrowdAtItsCapThroughTheObstacles()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesDeviceGateRun = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(true, 4);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.IsTrue(flow.MeasuresRun);
            RunObstacleCourse course = flow.Obstacles;
            Assert.AreEqual(ClaybreakLevel.MaximumObstacles, course.Count, "the gate runs the most obstacles a level holds");
            float deadline = Time.realtimeSinceStartup + 12f;
            while (Time.realtimeSinceStartup < deadline && course.Kills < 50)
            {
                Assert.AreEqual(flow.Crowd.MaximumArmy, flow.Crowd.Army, "the device gate's crowd left its cap");
                yield return null;
            }

            Assert.GreaterOrEqual(course.Kills, 50, "the gate's obstacles did not kill (nothing to measure)");
            Debug.Log($"[ObstacleRunTests] device gate run: kills {course.Kills}, dying {course.DyingCount}, calls {course.DrawCalls}");
        }

        [UnityTest]
        public IEnumerator CaptureEachKindKillingOnBothRoutes()
        {
            string folder = ScreenFitTests.Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
            var sheet = new Texture2D(ObstacleMotion.KindCount * (Width + Gap) - Gap, Height, TextureFormat.RGB24, false);
            var panel = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            Camera view = null;
            int[] kills = null;
            try
            {
                yield return RunLab((flow, obstacle, killedNow) => Capture(flow, obstacle, killedNow), flow =>
                {
                    view = flow.Crowd.CameraRig.GetComponent<Camera>();
                    view.targetTexture = target;
                }, result => kills = result);
                sheet.Apply();
                File.WriteAllBytes(Path.Combine(folder, "obstaculos-matando.png"), sheet.EncodeToPNG());
                Debug.Log($"[ObstacleCapture] kills per obstacle: {string.Join(", ", kills)}");
            }
            finally
            {
                if (view != null)
                {
                    view.targetTexture = null;
                }

                UnityEngine.Object.Destroy(target);
                UnityEngine.Object.Destroy(panel);
                UnityEngine.Object.Destroy(sheet);
            }

            // Called on the first frame an obstacle kills: a third of a second later the thrown units are high in the air.
            IEnumerator Capture(ClaybreakFlow flow, int obstacle, int killedNow)
            {
                if (killedNow == 0 || captured[obstacle])
                {
                    yield break;
                }

                captured[obstacle] = true;
                float until = Time.time + 0.35f;
                while (Time.time < until)
                {
                    yield return null;
                }

                RunObstacleCourse course = flow.Obstacles;
                ObstacleKind kind = course.PlacementOf(obstacle).kind;
                // One instanced call per kind on the route in use (buffer), then on the OpenGL ES fallback (blocks).
                Assert.AreEqual(ToonInstanceRoute.Buffer, course.Route, "the editor draws on the buffer route");
                Assert.AreEqual(course.KindsDrawn, course.DrawCalls, "one call per kind (buffer route)");
                Assert.AreEqual(5, course.KindsDrawn, "every kind of the lab is drawn");
                yield return null;
                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                panel.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                panel.Apply();
                RenderTexture.active = previous;
                sheet.SetPixels(obstacle * (Width + Gap), 0, Width, Height, panel.GetPixels());
                File.WriteAllBytes(Path.Combine(ScreenFitTests.Argument(FolderArgument), $"obstaculo-{Name(kind)}.png"), panel.EncodeToPNG());
                Debug.Log($"[ObstacleCapture] {kind}: dying {course.DyingCount}, army {flow.Crowd.Army}, calls {course.DrawCalls}");
                if (obstacle == 0)
                {
                    ToonInstanceRouting.Forced = ToonInstanceRoute.Chunks;
                    yield return null;
                    yield return null;
                    Assert.AreEqual(ToonInstanceRoute.Chunks, course.Route, "the course did not follow the forced route");
                    Assert.AreEqual(course.KindsDrawn, course.DrawCalls, "one call per kind (blocks route)");
                    ToonInstanceRouting.Forced = ToonInstanceRoute.Buffer;
                    yield return null;
                    yield return null;
                }
            }
        }

        private readonly bool[] captured = new bool[ClaybreakLevel.MaximumObstacles];

        /// <summary>
        /// Plays the obstacle lab to the firing line. Each obstacle is made deadly as the crowd arrives; <paramref name="onFrame"/>
        /// runs every frame an obstacle is the one the crowd is passing (with the units it killed that frame); the army is put
        /// back to <see cref="RefillArmy"/> once each obstacle is behind. Hands the kills per obstacle to <paramref name="done"/>.
        /// </summary>
        private static IEnumerator RunLab(Func<ClaybreakFlow, int, int, IEnumerator> onFrame, Action<ClaybreakFlow> started, Action<int[]> done)
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorUsesObstacleLab = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Assert.IsTrue(flow.PlaysObstacleLab, "the lab mode did not start");
            Assert.AreEqual(FlowState.Run, flow.State, "the lab starts running at once");
            RunObstacleCourse course = flow.Obstacles;
            RunCrowd crowd = flow.Crowd;
            Assert.AreEqual(ObstacleMotion.KindCount, course.Count, "the lab places every kind");
            started(flow);
            var kills = new int[course.Count];
            int current = -1;
            int lastKills = course.Kills;
            int lastArmy = crowd.Army;
            float deadline = Time.realtimeSinceStartup + 60f;
            while (Time.realtimeSinceStartup < deadline && flow.State == FlowState.Run)
            {
                float leader = crowd.Position.z;
                int next = current + 1;
                if (next < course.Count)
                {
                    ObstacleMotion.Reach(course.PlacementOf(next), out float nextNear, out _);
                    if (leader >= nextNear - 1f)
                    {
                        current = next;
                        MakeDeadly(course, current);
                    }
                }

                int killedNow = course.Kills - lastKills;
                lastKills = course.Kills;
                if (current >= 0)
                {
                    kills[current] += killedNow;
                    if (killedNow > 0)
                    {
                        Assert.AreEqual(lastArmy - killedNow, crowd.Army, "the army did not lose exactly the units the obstacle killed");
                    }

                    IEnumerator extra = onFrame(flow, current, killedNow);
                    if (extra != null)
                    {
                        yield return extra;
                    }

                    ObstacleMotion.Reach(course.PlacementOf(current), out _, out float far);
                    if (leader > far + 12f && crowd.Army < RefillArmy)
                    {
                        crowd.SetArmy(RefillArmy);
                    }
                }

                lastArmy = crowd.Army;
                lastKills = course.Kills;
                yield return null;
            }

            Assert.AreEqual(FlowState.Assault, flow.State, "the crowd did not reach the firing line (lost to the obstacles?)");
            Debug.Log($"[ObstacleRunTests] lab kills per obstacle: {string.Join(", ", kills)} (total {course.Kills})");
            done(kills);
        }

        // Sets the course clock so obstacle `index` is deadly while the crowd passes it.
        private static void MakeDeadly(RunObstacleCourse course, int index)
        {
            ObstaclePlacement obstacle = course.PlacementOf(index);
            float target;
            switch (obstacle.kind)
            {
                case ObstacleKind.SwingingLog: target = 0.38f; break; // about to swing down through the middle
                case ObstacleKind.Hammer: target = 0.80f; break;      // about to slam
                case ObstacleKind.Roller: target = 0.10f; break;      // just up, rolling at the crowd
                default: return;                                      // the saw and the hole always kill
            }

            float cycle = target - obstacle.phase;
            cycle -= Mathf.Floor(cycle);
            course.SetClock(cycle * ObstacleMotion.Period(obstacle));
        }

        private static int Objects() =>
            UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

        private static string Name(ObstacleKind kind)
        {
            switch (kind)
            {
                case ObstacleKind.SwingingLog: return "tronco";
                case ObstacleKind.Saw: return "serra";
                case ObstacleKind.Hammer: return "martelo";
                case ObstacleKind.Hole: return "buraco";
                default: return "rolo";
            }
        }
    }
}
