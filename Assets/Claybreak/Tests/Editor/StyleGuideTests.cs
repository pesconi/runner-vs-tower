using System.Collections.Generic;
using System.IO;
using System.Linq;
using Claybreak.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.1: the style guide describes the game as it is and the validator both passes it and refuses what the acceptance names:
    /// a material, effect or clip without an entry, a mesh over the budget, and, once a class is marked v3, a color or shader outside
    /// the guide, a mesh outside the v3 budget or with an unbevelled edge, an effect over its limit and a clip off the loudness target.
    /// </summary>
    public sealed class StyleGuideTests
    {
        private StyleFacts gathered;

        [OneTimeSetUp]
        public void GatherTheGame()
        {
            gathered = StyleFacts.Gather();
        }

        // A copy of the game's facts a test may change.
        private StyleFacts Facts()
        {
            return new StyleFacts
            {
                Materials = new List<MaterialFact>(gathered.Materials),
                Meshes = new List<MeshFact>(gathered.Meshes),
                Colors = new List<ColorFact>(gathered.Colors),
                Clips = new List<ClipFact>(gathered.Clips),
                EffectCapacity = new Dictionary<StyleEffect, int>(gathered.EffectCapacity),
                UnclassifiedMeshes = new List<string>(gathered.UnclassifiedMeshes),
            };
        }

        private static void MakeV3(StyleGuide guide, StyleClass styleClass)
        {
            guide.ClassOf(styleClass).status = StyleStatus.V3;
        }

        // Marking a class v3 also puts its materials and meshes under the v3 rules (they are not migrated yet), so a test about one
        // rule looks only at the failures that name it.
        private static List<string> About(StyleGuide guide, StyleFacts facts, string word)
        {
            return StyleGuideValidation.Run(guide, facts).Where(f => f.Contains(word)).ToList();
        }

        private static void AssertFails(List<string> failures, string fragment)
        {
            Assert.IsTrue(failures.Any(f => f.Contains(fragment)), $"expected a failure with '{fragment}', got:\n{string.Join("\n", failures)}");
        }

        // ---------- the game today ----------

        [Test]
        public void TheGuideAssetExistsAndEveryClassIsLegacy()
        {
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            Assert.IsNotNull(guide, StyleGuide.AssetPath + " is missing (run Claybreak > Write Style Guide)");
            foreach (StyleClass styleClass in System.Enum.GetValues(typeof(StyleClass)))
            {
                Assert.AreEqual(StyleStatus.Legacy, guide.ClassOf(styleClass).status, $"{styleClass} is not legacy: RT-6v3.1 must not change the game");
            }
        }

        [Test]
        public void TheGameTodayPassesTheValidation()
        {
            Assert.IsEmpty(StyleGuideValidation.Run(), "the guide does not describe the game");
        }

        [Test]
        public void TheGuideAssetIsWhatTheContentWrites()
        {
            var asset = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            StyleGuide fresh = StyleGuideContent.Create();
            try
            {
                Assert.AreEqual(JsonUtility.ToJson(fresh), JsonUtility.ToJson(asset), "StyleGuide.asset is out of date with StyleGuideContent (rebuild the scene)");
            }
            finally
            {
                Object.DestroyImmediate(fresh);
            }
        }

        [Test]
        public void TheChecklistFileIsGeneratedFromTheGuide()
        {
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            Assert.IsTrue(File.Exists(StyleChecklist.Path), StyleChecklist.Path + " is missing (run Claybreak > Write Style Checklist)");
            Assert.AreEqual(StyleChecklist.Generate(guide).Replace("\r\n", "\n"), File.ReadAllText(StyleChecklist.Path).Replace("\r\n", "\n"),
                "the checklist is out of date with the guide");
        }

        [Test]
        public void TheChecklistHasALinePerRule()
        {
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            string text = StyleChecklist.Generate(guide);
            foreach (MaterialStyle material in guide.Materials)
            {
                StringAssert.Contains(material.material, text);
            }

            foreach (EffectStyle effect in guide.Effects)
            {
                StringAssert.Contains($"**{effect.effect}**", text);
            }

            foreach (SoundStyle sound in guide.Sounds)
            {
                StringAssert.Contains($"`{sound.clip}`", text);
            }

            foreach (WorldStyle world in guide.Worlds)
            {
                StringAssert.Contains($"### {world.name}", text);
            }

            foreach (StyleClass styleClass in System.Enum.GetValues(typeof(StyleClass)))
            {
                StringAssert.Contains($"**{styleClass}**", text);
            }
        }

        [Test]
        public void ThePaletteRolesAreThePaletteContent()
        {
            StyleGuide guide = StyleGuideContent.Create();
            try
            {
                PaletteDefinition[] palettes = PaletteContent.Palettes;
                Assert.AreEqual(palettes.Length, guide.Worlds.Length);
                for (int w = 0; w < palettes.Length; w++)
                {
                    Color[] colors = PaletteContent.Colors(palettes[w]);
                    for (int i = 0; i < colors.Length; i++)
                    {
                        Assert.AreEqual(colors[i], guide.Worlds[w].roles[i].levels.baseColor, $"world {w + 1} role {guide.Worlds[w].roles[i].role}");
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(guide);
            }
        }

        // ---------- always-on rules fail ----------

        [Test]
        public void AMaterialWithoutAnEntryFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            facts.Materials.Add(new MaterialFact { name = "Mystery", shader = "StudioKit/ToonLit" });
            AssertFails(StyleGuideValidation.Run(guide, facts), "material Mystery has no entry");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AnEntryForAMissingMaterialFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            facts.Materials.RemoveAll(m => m.name == "Stone");
            AssertFails(StyleGuideValidation.Run(guide, facts), "lists material Stone");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AMeshOverTheBudgetFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            facts.Meshes.Add(new MeshFact { name = "FatUnit", styleClass = StyleClass.Unit, triangles = guide.ClassOf(StyleClass.Unit).maxTriangles + 1, drawCalls = 1 });
            facts.Meshes.Add(new MeshFact { name = "BusyTower", styleClass = StyleClass.Tower, triangles = 100, drawCalls = guide.ClassOf(StyleClass.Tower).maxDrawCalls + 1 });
            List<string> failures = StyleGuideValidation.Run(guide, facts);
            AssertFails(failures, "Unit FatUnit has");
            AssertFails(failures, "Tower BusyTower takes");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AMeshAssetNoClassOwnsFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            facts.UnclassifiedMeshes.Add("Assets/Claybreak/Content/Generated/Stray.asset");
            AssertFails(StyleGuideValidation.Run(guide, facts), "belongs to no style class");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AnEffectWithoutAnEntryFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            guide.EditorAssign(guide.Classes, guide.Worlds, guide.SharedRoles, guide.Profiles, guide.Materials,
                guide.Effects.Where(e => e.effect != StyleEffect.TowerBreak).ToArray(), guide.Sounds, guide.ColorTolerance);
            AssertFails(StyleGuideValidation.Run(guide, Facts()), "effect TowerBreak has no entry");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AClipWithoutAnEntryOrALicenseFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            facts.Clips.Add(new ClipFact { name = "Stranger", peakDb = -1f, licensed = true });
            facts.Clips.Add(new ClipFact { name = "Coin", peakDb = -1f, licensed = false });
            List<string> failures = StyleGuideValidation.Run(guide, facts);
            AssertFails(failures, "clip Stranger has no entry");
            AssertFails(failures, "clip Coin is not listed");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void ALegacyMaterialOutsideItsProfileFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            int index = facts.Materials.FindIndex(m => m.name == "Stone");
            facts.Materials[index] = new MaterialFact { name = "Stone", shader = "Standard" };
            AssertFails(StyleGuideValidation.Run(guide, facts), "Stone uses shader 'Standard'");
            Object.DestroyImmediate(guide);
        }

        // ---------- the v3 rules are off until the class migrates, then fail ----------

        [Test]
        public void TheV3RulesStayOffForLegacyClasses()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            facts.Colors.Add(new ColorFact { world = 0, role = "structureLight", color = Color.red });
            facts.Meshes.Add(new MeshFact { name = "Boxy", styleClass = StyleClass.Tower, triangles = 12, drawCalls = 1, maxEdgeAngle = 90f });
            facts.EffectCapacity[StyleEffect.FloatingNumber] = 9999;
            facts.Clips[0] = new ClipFact { name = facts.Clips[0].name, peakDb = -30f, licensed = true };
            Assert.IsEmpty(StyleGuideValidation.Run(guide, facts));
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AColorOutsideTheGuideFailsInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MakeV3(guide, StyleClass.Tower);
            StyleFacts facts = Facts();
            Assert.IsEmpty(About(guide, facts, " color "), "the palette colors are the guide's base level");
            ColorFact near = facts.Colors.First(c => c.world == 0 && c.role == "structureLight");
            facts.Colors.Remove(near);
            facts.Colors.Add(new ColorFact { world = 0, role = "structureLight", color = Color.Lerp(near.color, Color.black, 0.01f) });
            Assert.IsEmpty(About(guide, facts, " color "), "a color a hair off is inside ΔE 3");
            facts.Colors.Add(new ColorFact { world = 0, role = "structureLight", color = Color.red });
            AssertFails(StyleGuideValidation.Run(guide, facts), "v3 Tower color world 1 structureLight");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AColorMatchingAnyOfTheThreeLevelsPassesInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MakeV3(guide, StyleClass.Tower);
            PaletteRole role = guide.Worlds[0].roles.First(r => r.role == "structureLight");
            role.levels = new ColorLevels(role.levels.baseColor, Color.blue, Color.green);
            StyleFacts facts = Facts();
            facts.Colors.RemoveAll(c => c.world == 0 && c.role == "structureLight");
            facts.Colors.Add(new ColorFact { world = 0, role = "structureLight", color = Color.green });
            Assert.IsEmpty(About(guide, facts, " color "));
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AShaderOutsideTheProfilesFailsInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MakeV3(guide, StyleClass.Obstacle);
            // Still on the legacy profile: a v3 class must follow a v3 one.
            AssertFails(StyleGuideValidation.Run(guide, Facts()), "ObstacleToon is in the v3 class Obstacle but follows the legacy profile");

            // On a v3 profile, but with a shader the profile does not accept (no sibling shader exists yet).
            MaterialStyle entry = guide.MaterialNamed("ObstacleToon");
            entry.profile = "matte-plastic";
            AssertFails(StyleGuideValidation.Run(guide, Facts()), "ObstacleToon uses shader 'StudioKit/ToonLit', outside the profile 'matte-plastic'");

            // Once the profile accepts the shader, it passes.
            guide.ProfileOf("matte-plastic").shaders = new[] { "StudioKit/ToonLit" };
            Assert.IsEmpty(About(guide, Facts(), "ObstacleToon"));
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AMeshOutsideTheV3BudgetOrWithoutABevelFailsInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MakeV3(guide, StyleClass.Unit);
            StyleFacts facts = Facts();
            facts.Meshes.RemoveAll(m => m.styleClass == StyleClass.Unit);
            facts.Meshes.Add(new MeshFact { name = "Low", styleClass = StyleClass.Unit, triangles = 490, drawCalls = 1, maxEdgeAngle = 20f });
            facts.Meshes.Add(new MeshFact { name = "Sharp", styleClass = StyleClass.Unit, triangles = 1000, drawCalls = 1, maxEdgeAngle = 90f });
            facts.Meshes.Add(new MeshFact { name = "Good", styleClass = StyleClass.Unit, triangles = 1000, drawCalls = 1, maxEdgeAngle = 25f });
            List<string> failures = StyleGuideValidation.Run(guide, facts);
            AssertFails(failures, "v3 Unit Low has 490 triangles");
            AssertFails(failures, "v3 Unit Sharp has an unbevelled edge");
            Assert.IsFalse(failures.Any(f => f.Contains("Good")), string.Join("\n", failures));
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AnEffectOverItsLimitFailsInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MakeV3(guide, StyleClass.Effect);
            StyleFacts facts = Facts();
            facts.EffectCapacity[StyleEffect.UnitDeath] = guide.EffectOf(StyleEffect.UnitDeath).maxActive + 1;
            AssertFails(StyleGuideValidation.Run(guide, facts), "v3 effect UnitDeath preallocates");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AClipOffTheLoudnessTargetFailsInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MakeV3(guide, StyleClass.Sound);
            StyleFacts facts = Facts();
            Assert.IsEmpty(StyleGuideValidation.Run(guide, facts), "the clips are at the guide's peaks");
            ClipFact first = facts.Clips[0];
            facts.Clips[0] = new ClipFact { name = first.name, peakDb = -12f, licensed = true };
            AssertFails(StyleGuideValidation.Run(guide, facts), $"v3 clip {first.name} peaks at -12");
            Object.DestroyImmediate(guide);
        }

        // ---------- the measures ----------

        [Test]
        public void TheEdgeAngleOfACubeIsRightAndASphereIsSmooth()
        {
            Mesh cube = Primitive(PrimitiveType.Cube);
            Mesh sphere = Primitive(PrimitiveType.Sphere);
            Assert.AreEqual(90f, StyleFacts.MaxEdgeAngle(cube), 1f);
            Assert.Less(StyleFacts.MaxEdgeAngle(sphere), 30f);
            Assert.AreEqual(12, StyleFacts.Triangles(cube));
        }

        private static Mesh Primitive(PrimitiveType type)
        {
            GameObject made = GameObject.CreatePrimitive(type);
            Mesh mesh = made.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(made);
            return mesh;
        }

        [Test]
        public void TheClipsOfTheGameArePeakNormalized()
        {
            Assert.IsNotEmpty(gathered.Clips);
            StyleGuide guide = StyleGuideContent.Create();
            foreach (ClipFact clip in gathered.Clips)
            {
                Assert.AreEqual(guide.SoundOf(clip.name).peakDb, clip.peakDb, 1.5f, clip.name);
                Assert.IsTrue(clip.licensed, clip.name);
            }

            Object.DestroyImmediate(guide);
        }

        [Test]
        public void EveryGameClassHasItsLimitsMeasuredUnderTheCeiling()
        {
            // The legacy ceilings hold for the whole game: the measures are logged for the next delivery to tighten.
            Debug.Log("[StyleGuideTests] " + StyleGuideValidation.Measures(gathered));
            StyleGuide guide = StyleGuideContent.Create();
            foreach (MeshFact mesh in gathered.Meshes)
            {
                ClassStyle limits = guide.ClassOf(mesh.styleClass);
                Assert.LessOrEqual(mesh.triangles, limits.maxTriangles, mesh.name);
                Assert.LessOrEqual(mesh.drawCalls, limits.maxDrawCalls, mesh.name);
            }

            Object.DestroyImmediate(guide);
        }
    }
}
