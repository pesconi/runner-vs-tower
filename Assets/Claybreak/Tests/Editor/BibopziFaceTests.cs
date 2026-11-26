using System.Collections.Generic;
using Claybreak.Editor;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.4b (direcao-visual-v3.md 3.2, 3.3, D36): the guide's event → expression table and the director that plays it,
    /// the face atlas (the script's output, 8 cells with empty margins, every pair still different at 40 px), and the
    /// Bibopzi's motion: squash at the landing and stretch on the rise with the volume kept, the spring trailing, the hands
    /// opening and the wind-up key turning about its shaft.
    /// </summary>
    public sealed class BibopziFaceTests
    {
        private static FaceRules Rules => StyleGuideContent.Faces();

        [Test]
        public void TheGuidesTableCoversEveryEventAndEveryCell()
        {
            Assert.IsNull(Rules.Validate());
            StyleGuide guide = StyleGuideContent.Create();
            Assert.IsNull(guide.Faces.Validate(), "the guide carries the table");
            Assert.AreEqual(Rules.rules.Length, guide.Faces.rules.Length);
            Object.DestroyImmediate(guide);
            Assert.AreEqual(FaceExpression.Scared, Rules.Of(FaceEvent.BadGate).expression, "a bad gate scares (docs 3.2)");
            Assert.AreEqual(FaceExpression.Happy, Rules.Of(FaceEvent.GoodGate).expression);
            Assert.AreEqual(FaceExpression.Ouch, Rules.Of(FaceEvent.Launch).expression, "\"ui!\" in the assault");
            Assert.AreEqual(1f, Rules.Of(FaceEvent.BadGate).handsOpen, "hands wide open in a fright");
            Assert.AreEqual(1f, Rules.Of(FaceEvent.Victory).handsOpen, "and in victory");
        }

        [Test]
        public void AnIncompleteTableIsRefused()
        {
            FaceRules rules = Rules;
            rules.rules = System.Array.FindAll(rules.rules, r => r.trigger != FaceEvent.Defeat);
            StringAssert.Contains("Defeat", rules.Validate());
            rules = Rules;
            rules.rules[1].expression = FaceExpression.Neutral; // the blink cell is never shown
            StringAssert.Contains("Blink", rules.Validate());
            rules = Rules;
            rules.rules[2].seconds = 0f; // a good gate holding as a base
            Assert.IsNull(rules.Validate(), "any event may be a base");
            rules.rules[0].seconds = 1f;
            Assert.IsNotNull(rules.Validate(), "the run face must hold");
            Assert.Throws<System.ArgumentException>(() => new FaceDirector(rules));
        }

        [Test]
        public void ReactionsShowForTheirTimeAndByPriority()
        {
            var director = new FaceDirector(Rules);
            Assert.AreEqual(FaceExpression.Neutral, director.Current(0.5f).expression);
            Assert.IsTrue(director.Trigger(FaceEvent.GoodGate, 1f));
            Assert.AreEqual(FaceExpression.Happy, director.Current(1.1f).expression);
            Assert.IsTrue(director.Trigger(FaceEvent.BadGate, 1.2f), "a fright wins over joy");
            Assert.AreEqual(FaceExpression.Scared, director.Current(1.3f).expression);
            Assert.IsFalse(director.Trigger(FaceEvent.GoodGate, 1.4f), "joy waits until the fright is over");
            Assert.AreEqual(FaceExpression.Scared, director.Current(1.9f).expression);
            Assert.AreEqual(FaceExpression.Neutral, director.Current(1.2f + Rules.Of(FaceEvent.BadGate).seconds + 0.01f).expression,
                "back to the base after its time");

            director.Trigger(FaceEvent.Assault, 5f);
            Assert.AreEqual(FaceExpression.Effort, director.Current(5.1f).expression);
            director.Trigger(FaceEvent.ObstacleHit, 5.2f);
            Assert.AreEqual(FaceExpression.Scared, director.Current(5.3f).expression);
            director.Trigger(FaceEvent.Victory, 5.4f);
            Assert.AreEqual(FaceExpression.Victory, director.Current(5.4f).expression, "the end of the level cuts the reaction");
            Assert.IsFalse(director.Trigger(FaceEvent.BadGate, 5.5f), "nothing reacts over the victory");
            Assert.AreEqual(FaceExpression.Victory, director.Current(60f).expression, "held");
            Assert.AreEqual((int)FaceExpression.Victory, director.Flyers().Cell, "the units in flight celebrate too");

            director.Reset(70f);
            Assert.AreEqual(FaceExpression.Neutral, director.Current(70.1f).expression);
            Assert.AreEqual((int)FaceExpression.Ouch, director.Flyers().Cell, "\"ui!\" in flight while playing");
        }

        [Test]
        public void TheRunFaceBlinksBrieflyAndTheOthersDoNot()
        {
            FaceRules rules = Rules;
            var director = new FaceDirector(rules);
            director.Reset(10f);
            float blink = rules.Of(FaceEvent.Blink).seconds;
            int blinking = 0;
            int samples = 0;
            for (float t = 10f; t < 10f + rules.blinkEvery * 3f; t += 0.01f)
            {
                samples++;
                if (director.Current(t).expression == FaceExpression.Blink)
                {
                    blinking++;
                }
            }

            Assert.AreEqual(blink / rules.blinkEvery, blinking / (float)samples, 0.006f, "one short blink per interval");
            director.Trigger(FaceEvent.Assault, 20f);
            for (float t = 20f; t < 20f + rules.blinkEvery * 2f; t += 0.01f)
            {
                Assert.AreEqual(FaceExpression.Effort, director.Current(t).expression, "effort does not blink");
            }
        }

        [Test]
        public void TheCrowdExpressionCarriesTheThrownFace()
        {
            var director = new FaceDirector(Rules);
            director.Trigger(FaceEvent.BadGate, 1f);
            SwarmExpression crowd = director.Crowd(1.1f);
            Assert.AreEqual((int)FaceExpression.Scared, crowd.Cell);
            Assert.AreEqual(1f, crowd.HandsOpen);
            Assert.AreEqual((int)FaceExpression.Ouch, crowd.DyingCell, "the ones thrown out say \"ui!\"");
        }

        [Test]
        public void TheAtlasIsTheScriptsOutputWithItsCellsAndMargins()
        {
            byte[] png = System.IO.File.ReadAllBytes(BibopziFaceArt.AtlasPath);
            Assert.AreEqual(BibopziFaceArt.ScriptHash(), BibopziFaceArt.PngText(png, "FACE_SCRIPT_HASH"),
                "run python -X utf8 Tools/art/bibopzi_faces.py");
            Color32[] pixels = BibopziFaceArt.ReadPixels(png, out int width, out int height);
            CollectionAssert.IsEmpty(BibopziFaceArt.PixelProblems(pixels, width, height));
            Assert.AreEqual(FaceRules.Cells, FaceRules.AtlasColumns * FaceRules.AtlasRows);
            Assert.LessOrEqual(BibopziFaceArt.Inset, BibopziFaceArt.DrawnMargin, "the shader never samples the drawing's edge band");
            Assert.IsNull(BibopziFaceArt.Atlas(Texture2D.whiteTexture).Validate(), "the kit takes the grid");
        }

        // D36: the eyes read at 40 px. At that size (box filter over the cell, on the body's pink) every pair of expressions
        // still differs in a clear share of its pixels, and every face has dark features against the body.
        [Test]
        public void EveryPairOfFacesStillDiffersAt40Pixels()
        {
            Color32[] pixels = BibopziFaceArt.ReadPixels(out _, out _);
            const int small = 40;
            var cells = new float[FaceRules.Cells][];
            for (int cell = 0; cell < FaceRules.Cells; cell++)
            {
                cells[cell] = Downsample(pixels, cell, small, small * 2, out float darkShare);
                Assert.Greater(darkShare, 0.02f, $"{(FaceExpression)cell} has dark features at {small} px");
            }

            for (int a = 0; a < FaceRules.Cells; a++)
            {
                for (int b = a + 1; b < FaceRules.Cells; b++)
                {
                    int differing = 0;
                    for (int i = 0; i < cells[a].Length; i++)
                    {
                        if (Mathf.Abs(cells[a][i] - cells[b][i]) > 0.25f)
                        {
                            differing++;
                        }
                    }

                    Assert.Greater(differing / (float)cells[a].Length, 0.025f,
                        $"{(FaceExpression)a} and {(FaceExpression)b} look alike at {small} px");
                }
            }
        }

        [Test]
        public void TheBibopziHopSquashesOnLandingAndStretchesRising()
        {
            CrowdShapeOptionAssert(out SwarmMotionSettings hop);
            Assert.AreEqual(WorldContent.BibopziLandSquash, hop.LandSquash);
            Assert.AreEqual(WorldContent.BibopziRiseStretch, hop.RiseStretch);
            float cycle = math.PI / hop.Frequency;
            Assert.AreEqual(hop.SquashBase - hop.LandSquash, SwarmMotion.ScaleY(cycle * 1e-4f, 0f, hop), 1e-3f, "squashed at the landing");
            Assert.Greater(SwarmMotion.ScaleY(cycle * 0.3f, 0f, hop), SwarmMotion.ScaleY(cycle * 0.7f, 0f, hop), "stretched on the rise");
            for (int i = 0; i < 64; i++)
            {
                float sy = SwarmMotion.ScaleY(cycle * i / 64f, 0f, hop);
                float sxz = SwarmMotion.SquashXZ(sy);
                Assert.AreEqual(1f, sy * sxz * sxz, 1e-4f, "volume preserved");
            }
        }

        [Test]
        public void TheSpringTrailsTheHandsOpenAndTheKeyTurns()
        {
            ToonVinylMotion motion = BibopziFaceArt.Motion();
            Assert.IsNull(motion.Validate());
            SimpleMeshData data = SimpleMeshData.Read(UnitShapeAssets.Named("Bibopzi").DataPath);
            Assert.AreEqual(data.KeyPivot, motion.KeyPivot, "the key turns about the data's pivot");
            var tip = new float3(0f, 0.76f, 0f);
            var springMark = new float4(1f, 0f, 0f, 0f);
            float speed = WorldContent.Shapes[0].Motion.Frequency;

            // Still unit: straight spring.
            float3 n = new float3(0f, 1f, 0f);
            Assert.AreEqual(0f, math.distance(tip, ToonVertexMotion.Deform(tip, ref n, springMark, new float4(0f, 0.6f, 0f, 0f), motion)), 1e-6f);

            // Rising: the tip trails down and back, never farther than the bend.
            n = new float3(0f, 1f, 0f);
            float3 rising = ToonVertexMotion.Deform(tip, ref n, springMark, new float4(0f, 0.6f, speed, 0f), motion);
            Assert.Less(rising.y, tip.y, "trails down while rising");
            Assert.Less(rising.z, tip.z, "and back");
            Assert.LessOrEqual(math.distance(rising, tip), math.length((float3)motion.SpringBend) + 1e-5f);

            // Hands: wide open moves the right hand out (+X) and up.
            var hand = new float3(0.25f, 0.25f, 0f);
            n = new float3(1f, 0f, 0f);
            float3 closed = ToonVertexMotion.Deform(hand, ref n, new float4(0f, 1f, 0f, 0f), new float4(0f, 0f, speed, 0f), motion);
            n = new float3(1f, 0f, 0f);
            float3 open = ToonVertexMotion.Deform(hand, ref n, new float4(0f, 1f, 0f, 0f), new float4(0f, 0f, speed, 1f), motion);
            Assert.AreEqual(motion.HandOpen.x, open.x - closed.x, 1e-5f);
            Assert.AreEqual(motion.HandOpen.y, open.y - closed.y, 1e-5f);

            // Key: a point of the bow keeps its distance to the shaft and turns through a whole turn per two hops.
            float3 pivot = data.KeyPivot;
            float3 bow = pivot + new float3(0.07f, 0.01f, -0.01f);
            n = new float3(0f, 0f, -1f);
            float3 quarter = ToonVertexMotion.Deform(bow, ref n, new float4(0f, 0f, 1f, 0f), new float4(0f, math.PI * 0.5f, speed, 0f), motion);
            Assert.AreEqual(math.length((bow - pivot).xy), math.length((quarter - pivot).xy), 1e-5f, "turns about the shaft");
            Assert.AreEqual(bow.z, quarter.z, 1e-5f, "in its plane");
            Assert.Greater(math.distance(bow, quarter), 0.05f, "a quarter turn moves the bow");
            n = new float3(0f, 0f, -1f);
            float3 whole = ToonVertexMotion.Deform(bow, ref n, new float4(0f, 0f, 1f, 0f), new float4(0f, 2f * math.PI - 1e-4f, speed, 0f), motion);
            Assert.AreEqual(0f, math.distance(bow, whole), 1e-3f, "whole turns: no jump when the phase wraps");
        }

        private static void CrowdShapeOptionAssert(out SwarmMotionSettings hop)
        {
            WorldContent.ShapeDefinition bibopzi = WorldContent.Shapes[0];
            Assert.AreEqual(SaveMigration.DefaultShape, bibopzi.Id);
            hop = bibopzi.Motion;
        }

        // The cell over the body's pink, box-filtered to width × height (the cell is twice as tall as wide), as luminance;
        // darkShare = share of clearly dark pixels.
        private static float[] Downsample(Color32[] pixels, int cell, int width, int height, out float darkShare)
        {
            var body = new Vector3(1f, 0.494f, 0.714f);
            var sum = new Vector3[width * height];
            var count = new int[width * height];
            BibopziFaceArt.ForEachPixel(cell, (x, y) =>
            {
                Color32 c = pixels[BibopziFaceArt.Index(cell, x, y)];
                float a = c.a / 255f;
                var rgb = new Vector3(c.r, c.g, c.b) / 255f * a + body * (1f - a);
                int i = y * height / BibopziFaceArt.CellHeight * width + x * width / BibopziFaceArt.CellWidth;
                sum[i] += rgb;
                count[i]++;
            });
            var luminance = new float[width * height];
            int dark = 0;
            for (int i = 0; i < luminance.Length; i++)
            {
                Vector3 rgb = sum[i] / count[i];
                luminance[i] = 0.2126f * rgb.x + 0.7152f * rgb.y + 0.0722f * rgb.z;
                if (luminance[i] < 0.3f)
                {
                    dark++;
                }
            }

            darkShare = dark / (float)luminance.Length;
            return luminance;
        }
    }
}
