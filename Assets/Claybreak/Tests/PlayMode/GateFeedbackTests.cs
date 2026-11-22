using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.9: the gates and their feedback in the game scene. The run gates and the assault row carry TextMeshPro labels
    /// in Baloo 2 with the side's outline preset; crossing a run gate floats its number (one of the four pooled labels,
    /// gone after a second) and pops the army counter; nothing is created in play. The capture (GPU only,
    /// <c>-testFilter Claybreak.Tests.GateFeedbackTests -gateCaptureDir &lt;folder&gt;</c>) records the gates, the float, the
    /// assault row and the glyphs (+ − × ÷ and Portuguese accents, in TextMeshPro and in the UI.Text screens).
    /// </summary>
    public sealed class GateFeedbackTests
    {
        private const string FolderArgument = "-gateCaptureDir";
        private const int Width = 540;
        private const int Height = 1177;
        private const string Glyphs = "+10 −30 ×2 ÷2";
        private const string Accents = "AÇÃO ÁÉÍÓÚ ÂÊÔ ÃÕ àçãéêõ";

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = false;
            ClaybreakFlow.EditorUsesFreshSave = false;
            ClaybreakFlow.EditorUsesDeviceGateLevel = false;
#endif
        }

        [UnityTest]
        public IEnumerator CrossingAGateFloatsItsNumberAndPopsTheCounter()
        {
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            yield return WaitFor(() => flow.State == FlowState.Run, 5f);

            // Labels: Baloo 2 SDF, the level's gate text, the outline preset of the side.
            RunGatePair first = flow.ActiveLevel.RunGates[0];
            AssertLabel(flow.Gates.LabelOf(0, false), first.left);
            AssertLabel(flow.Gates.LabelOf(0, true), first.right);
            for (int lane = 0; lane < ClaybreakLevel.LaneCount; lane++)
            {
                var label = GameObject.Find("/AssaultField/AssaultGate" + (lane + 1) + "/Label").GetComponent<TMP_Text>();
                AssertLabel(label, flow.Field.GateIn(lane));
            }

            FloatingNumbers floating = flow.Hud.Floating;
            Assert.AreEqual(4, floating.Capacity, "the float pool");
            Assert.AreEqual(0, floating.Showing);
            int objectsBefore = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

            // Level 1 untouched: the crowd runs down the middle and takes the right-hand gate.
            int army = flow.Crowd.Army;
            float deadline = Time.realtimeSinceStartup + 30f;
            while (flow.Crowd.Army == army && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.AreNotEqual(army, flow.Crowd.Army, "the crowd never crossed the first gate");
            Assert.AreEqual(1, floating.Showing, "the gate number did not float");
            TMP_Text number = FindShowing(floating);
            Assert.AreEqual(first.right.Label, number.text);
            Color expected = first.right.IsGood ? new Color(0.184f, 0.749f, 0.525f) : new Color(1f, 0.302f, 0.427f);
            Assert.AreEqual(expected.r, number.color.r, 0.01f);
            Assert.AreEqual(expected.g, number.color.g, 0.01f);

            float peak = 1f;
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                peak = Mathf.Max(peak, flow.Hud.ArmyScale);
                yield return null;
            }

            Assert.Greater(peak, 1.1f, "the counter did not pop");
            Assert.LessOrEqual(peak, ClaybreakHud.PopScale + 0.001f);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual(0, floating.Showing, "the number did not go away after a second");
            Assert.AreEqual(1f, flow.Hud.ArmyScale, 0.001f, "the counter stayed big");
            Assert.AreEqual(objectsBefore,
                UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                "objects were created in play");
        }

        [UnityTest]
        public IEnumerator CaptureTheGatesAndTheirFeedback()
        {
            string folder = Argument(FolderArgument);
            if (folder == null || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                Assert.Ignore($"capture only with a GPU and {FolderArgument} <folder>");
            }

            Directory.CreateDirectory(folder);
#if UNITY_EDITOR
            ClaybreakFlow.EditorStartsWithoutTap = true;
#endif
            yield return ClaybreakSceneTests.LoadScene(false);
            var flow = UnityEngine.Object.FindFirstObjectByType<ClaybreakFlow>();
            Assert.IsTrue(flow.enabled, "the flow disabled itself (see the log)");
            Camera view = flow.Crowd.CameraRig.GetComponent<Camera>();
            var canvas = GameObject.Find("/Ui").GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = view;
            canvas.planeDistance = 1f;
            using var shot = new ShapeSkinCaptureTests.Shot(view, Width, Height);
            using var sheet = new ShapeSkinCaptureTests.Sheet(4, 1, Width, Height);
            yield return WaitFor(() => flow.State == FlowState.Run, 5f);

            // 1: the first pair ahead (panels, posts, outlined labels).
            float firstZ = flow.ActiveLevel.RunGates[0].z;
            yield return WaitFor(() => flow.Crowd.Position.z > firstZ - 9f, 20f);
            yield return shot.Take();
            sheet.Put(0, 0, shot.Panel);

            // 2: just after crossing: the floating number and the counter's pop.
            int army = flow.Crowd.Army;
            yield return WaitFor(() => flow.Crowd.Army != army, 20f);
            yield return new WaitForSeconds(0.12f);
            yield return shot.Take();
            sheet.Put(1, 0, shot.Panel);
            Debug.Log($"[GateCapture] float army={flow.Crowd.Army} showing={flow.Hud.Floating.Showing} scale={flow.Hud.ArmyScale:0.00}");

            // 3: the glyphs, in TextMeshPro (the counter and a float) and in the UI.Text screens (the banner).
            var counter = GameObject.Find("/Ui/Hud/Army").GetComponent<TMP_Text>();
            counter.text = Glyphs;
            flow.Hud.FloatGate(new GateSpec(GateOperation.Divide, 2));
            flow.Hud.Banner(Accents + "\n×2  ÷2  −30  +10");
            yield return new WaitForSeconds(0.2f);
            yield return shot.Take();
            sheet.Put(2, 0, shot.Panel);

            // 4: the assault row (labels on the lane gates).
            yield return WaitFor(() => flow.State == FlowState.Assault, 40f);
            yield return new WaitForSeconds(1.2f);
            yield return shot.Take();
            sheet.Put(3, 0, shot.Panel);

            sheet.Save(Path.Combine(folder, "portoes-oppo.png"));
        }

        private static void AssertLabel(TMP_Text label, GateSpec spec)
        {
            // A fresh save has no gate-value upgrade: labels show the level's values.
            GateSpec gate = spec;
            Assert.AreEqual(gate.Label, label.text);
            StringAssert.StartsWith("Baloo2-ExtraBold", label.font.name);
            Assert.AreEqual(AtlasPopulationMode.Static, label.font.atlasPopulationMode);
            StringAssert.Contains(gate.IsGood ? "Gate Good" : "Gate Bad", label.fontSharedMaterial.name);
        }

        private static TMP_Text FindShowing(FloatingNumbers floating)
        {
            foreach (TMP_Text label in floating.GetComponentsInChildren<TMP_Text>(false))
            {
                return label;
            }

            return null;
        }

        private static IEnumerator WaitFor(Func<bool> condition, float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            yield return null;
        }

        private static string Argument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (arguments[i] == name)
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }
    }
}
