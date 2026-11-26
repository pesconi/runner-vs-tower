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
                Looks = new List<WorldLookFact>(gathered.Looks),
                UiOutsidePost = gathered.UiOutsidePost,
                CameraPost = gathered.CameraPost,
                Measures = gathered.Measures,
            };
        }

        // RT-6v3.3: measures as the capture would write them for this guide, every world passing (a test then breaks one).
        private static ContrastMeasures GoodMeasures(StyleGuide guide)
        {
            var measures = new ContrastMeasures { fingerprint = guide.LookFingerprint() };
            var shots = new List<WorldMeasure>();
            for (int w = 0; w < guide.Worlds.Length; w++)
            {
                shots.Add(new WorldMeasure
                {
                    world = w,
                    shot = "corrida",
                    gateLabelRatio = 6f,
                    roles = new[]
                    {
                        RoleMeasure.Of(StyleContrast.Crowd, 1000, new Color(1f, 0.18f, 0.6f)),
                        RoleMeasure.Of(StyleContrast.Track, 1000, new Color(0.92f, 0.92f, 0.94f)),
                        RoleMeasure.Of(StyleContrast.Enemy, 1000, new Color(0.06f, 0.43f, 0.52f)),
                    },
                    skins = new[] { new SkinMeasure { skin = "ember", deltaE = 40f } },
                });
            }

            measures.shots = shots.ToArray();
            return measures;
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
        public void TheGuideAssetExistsAndOnlyTheDrawnSurfacesAndTheSoundAreV3()
        {
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            Assert.IsNotNull(guide, StyleGuide.AssetPath + " is missing (run Claybreak > Write Style Guide)");
            var surfaces = new[]
            {
                StyleClass.Unit, StyleClass.Leader, StyleClass.Enemy, StyleClass.Tower, StyleClass.Boss, StyleClass.Obstacle,
                StyleClass.Floor, StyleClass.Scenery,
            };
            foreach (StyleClass styleClass in System.Enum.GetValues(typeof(StyleClass)))
            {
                StyleStatus expected = styleClass == StyleClass.Sound ? StyleStatus.V3 : StyleStatus.Legacy; // RT-6v3.9 migrated the sound
                Assert.AreEqual(expected, guide.ClassOf(styleClass).status, $"{styleClass} shape is not {expected}");
                StyleStatus surface = surfaces.Contains(styleClass) ? StyleStatus.V3 : StyleStatus.Legacy; // RT-6v3.3, meshes unchanged
                Assert.AreEqual(surface, guide.ClassOf(styleClass).surface, $"{styleClass} surface is not {surface}");
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
                guide.Effects.Where(e => e.effect != StyleEffect.TowerBreak).ToArray(), guide.Sounds, guide.ColorTolerance, guide.Contrast);
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
            guide.ClassOf(StyleClass.Sound).status = StyleStatus.Legacy;
            foreach (ClassStyle entry in guide.Classes)
            {
                entry.surface = StyleStatus.Legacy;
            }

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
            Assert.IsEmpty(About(guide, Facts(), "ObstacleToon"), "the obstacles are on the enemy vinyl");
            // A v3 class must follow a v3 profile.
            MaterialStyle entry = guide.MaterialNamed("ObstacleToon");
            entry.profile = StyleGuideContent.ProfileToon;
            AssertFails(StyleGuideValidation.Run(guide, Facts()), "ObstacleToon is in the v3 class Obstacle but follows the legacy profile");

            // On a v3 profile, but with a shader the profile does not accept.
            entry.profile = StyleGuideContent.ProfileSkyV3;
            AssertFails(StyleGuideValidation.Run(guide, Facts()), "ObstacleToon uses shader 'StudioKit/ToonVinyl', outside the profile 'sky'");
            Object.DestroyImmediate(guide);
        }

        // ---------- RT-6v3.3: surfaces, light and post, the contrast logic ----------

        [Test]
        public void TheVinylMaterialsHoldTheirProfilesSurface()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            Assert.IsEmpty(About(guide, facts, "does not hold the surface"));
            int index = facts.Materials.FindIndex(m => m.name == "CrowdToon");
            MaterialFact crowd = facts.Materials[index];
            Assert.IsTrue(crowd.vinyl && crowd.shader == "StudioKit/ToonVinyl", "the crowd is vinyl");
            crowd.surface = new Vector4(0f, 0f, 0f, 0f);
            facts.Materials[index] = crowd;
            AssertFails(StyleGuideValidation.Run(guide, facts), "CrowdToon does not hold the surface of the profile 'vinyl'");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void AGlossyFloorFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            MaterialStyle entry = guide.MaterialNamed("GroundToon");
            entry.profile = StyleGuideContent.ProfileVinyl;
            AssertFails(StyleGuideValidation.Run(guide, Facts()), "Floor material GroundToon shines on faces looking up");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void EveryWorldShowsTheGuidesLightPostAndFog()
        {
            StyleGuide guide = StyleGuideContent.Create();
            Assert.IsEmpty(About(guide, Facts(), "shows"), "the scene shows the guide's light and post");
            Assert.IsEmpty(About(guide, Facts(), " fog "), "the palettes hold the guide's fog");
            guide.Worlds[1].light.keyIntensity += 0.5f;
            guide.Worlds[2].post.bloom = 0f;
            List<string> failures = StyleGuideValidation.Run(guide, Facts());
            AssertFails(failures, "world 2 (PorDoSol) shows a vinyl light that is not the guide's");
            AssertFails(failures, "world 3 (Doce) shows a post stack that is not the guide's");
            StyleFacts facts = Facts();
            WorldLookFact white = facts.Looks[0];
            white.fog = new Color(0.95f, 0.95f, 0.96f);
            facts.Looks[0] = white;
            facts.UiOutsidePost = false;
            failures = StyleGuideValidation.Run(guide, facts);
            AssertFails(failures, "is a white veil");
            AssertFails(failures, "the UI stays outside the post");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void ThePalettesFollowTheContrastLogicAndAGreyCrowdFails()
        {
            StyleGuide guide = StyleGuideContent.Create();
            Assert.IsEmpty(About(guide, Facts(), "D36"), "the night palettes were designed to the rule");
            PaletteRole crowd = guide.Worlds[0].roles.First(r => r.role == "crowd");
            crowd.levels = new ColorLevels(new Color(0.75f, 0.72f, 0.74f), crowd.levels.shadow, crowd.levels.highlight);
            PaletteRole track = guide.Worlds[1].roles.First(r => r.role == "track");
            track.levels = new ColorLevels(new Color(1f, 0.85f, 0.5f), track.levels.shadow, track.levels.highlight);
            PaletteRole defense = guide.Worlds[2].roles.First(r => r.role == "defense");
            defense.levels = new ColorLevels(new Color(0.6f, 0.2f, 0.9f), defense.levels.shadow, defense.levels.highlight);
            List<string> failures = StyleGuideValidation.Run(guide, Facts());
            AssertFails(failures, "world 1 (Pastel) crowd chroma");
            AssertFails(failures, "world 1 (Pastel) crowd and track are");
            AssertFails(failures, "world 2 (PorDoSol) track chroma");
            AssertFails(failures, "world 3 (Doce) defenses are");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void TheCapturedContrastIsCheckedAndMustBeFresh()
        {
            StyleGuide guide = StyleGuideContent.Create();
            var failures = new List<string>();
            StyleGuideValidation.CheckCapturedContrast(guide, GoodMeasures(guide), failures);
            Assert.IsEmpty(failures);

            StyleGuideValidation.CheckCapturedContrast(guide, null, failures);
            AssertFails(failures, "contraste.json is missing");
            failures.Clear();
            ContrastMeasures stale = GoodMeasures(guide);
            stale.fingerprint = "0";
            StyleGuideValidation.CheckCapturedContrast(guide, stale, failures);
            AssertFails(failures, "was measured with another look");

            failures.Clear();
            ContrastMeasures bad = GoodMeasures(guide);
            WorldMeasure run = bad.shots[0];
            run.roles = new[]
            {
                RoleMeasure.Of(StyleContrast.Crowd, 1000, new Color(0.85f, 0.7f, 0.78f)),
                RoleMeasure.Of(StyleContrast.Track, 1000, new Color(0.9f, 0.9f, 0.9f)),
                RoleMeasure.Of(StyleContrast.Enemy, 1000, new Color(1f, 0.2f, 0.4f)),
                RoleMeasure.Of(StyleContrast.Sky, 1000, new Color(0.1f, 0.2f, 0.9f)),
            };
            run.gateLabelRatio = 1.5f;
            run.skins = new[] { new SkinMeasure { skin = "frost", deltaE = 8f } };
            bad.shots = bad.shots.Skip(0).Take(3).ToArray(); // world 4 not measured
            StyleGuideValidation.CheckCapturedContrast(guide, bad, failures);
            AssertFails(failures, "world 1 corrida: on screen the crowd's chroma");
            AssertFails(failures, "world 1 corrida: on screen crowd and track are");
            AssertFails(failures, "world 1 corrida: on screen the enemies are");
            AssertFails(failures, "world 1 corrida: the gate numbers measure");
            AssertFails(failures, "world 1 corrida: skin frost is");
            AssertFails(failures, "world 4 has no measured run");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void TheFingerprintFollowsTheLook()
        {
            StyleGuide guide = StyleGuideContent.Create();
            string before = guide.LookFingerprint();
            Assert.AreEqual(before, guide.LookFingerprint());
            guide.Worlds[3].post.vignette += 0.01f;
            Assert.AreNotEqual(before, guide.LookFingerprint());
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

        // RT-6v3.4a: a mesh already remade in v3 answers to the v3 range while its class is still legacy (the Bibopzi before the
        // other shapes), and the class's reserve answers to the reserve range.
        [Test]
        public void AV3OrReserveMeshAnswersToItsOwnBudgetInALegacyClass()
        {
            StyleGuide guide = StyleGuideContent.Create();
            Assert.AreNotEqual(StyleStatus.V3, guide.ClassOf(StyleClass.Unit).status);
            StyleFacts facts = Facts();
            facts.Meshes.RemoveAll(m => m.styleClass == StyleClass.Unit);
            facts.Meshes.Add(new MeshFact { name = "OldShape", styleClass = StyleClass.Unit, triangles = 490, drawCalls = 1, maxEdgeAngle = 90f });
            facts.Meshes.Add(new MeshFact { name = "NewLow", styleClass = StyleClass.Unit, tier = MeshTier.V3, triangles = 490, drawCalls = 1 });
            facts.Meshes.Add(new MeshFact { name = "NewGood", styleClass = StyleClass.Unit, tier = MeshTier.V3, triangles = 1100, drawCalls = 1 });
            facts.Meshes.Add(new MeshFact { name = "NewSharp", styleClass = StyleClass.Unit, tier = MeshTier.V3, triangles = 1100, drawCalls = 1, maxEdgeAngle = 80f });
            facts.Meshes.Add(new MeshFact { name = "SpareGood", styleClass = StyleClass.Unit, tier = MeshTier.Reserve, triangles = 500, drawCalls = 1 });
            facts.Meshes.Add(new MeshFact { name = "SpareBig", styleClass = StyleClass.Unit, tier = MeshTier.Reserve, triangles = 900, drawCalls = 1 });
            facts.Meshes.Add(new MeshFact { name = "NoSpare", styleClass = StyleClass.Leader, tier = MeshTier.Reserve, triangles = 500, drawCalls = 1 });
            List<string> failures = StyleGuideValidation.Run(guide, facts);
            AssertFails(failures, "v3 Unit NewLow has 490 triangles");
            AssertFails(failures, "v3 Unit NewSharp has an unbevelled edge");
            AssertFails(failures, "reserve Unit SpareBig has 900 triangles");
            AssertFails(failures, "reserve Leader NoSpare");
            Assert.IsFalse(failures.Any(f => f.Contains("OldShape") || f.Contains("NewGood") || f.Contains("SpareGood")), string.Join("\n", failures));
            Object.DestroyImmediate(guide);
        }

        // RT-6v3.4a (the K.v3b finding): an art builder run outside the scene builder (captures, art tests) used to put the
        // game's vinyl materials back on ToonLit, and the repository was committed so. The builders keep the vinyl sibling.
        [Test]
        public void TheArtBuildersKeepTheVinylMaterials()
        {
            Shader lit = Shader.Find(JungleDegree.StudioKit.Toon.ToonShaderIds.ToonLitShader);
            Shader vinyl = Shader.Find(JungleDegree.StudioKit.Toon.ToonShaderIds.ToonVinylShader);
            var onVinyl = new Material(vinyl);
            var onLit = new Material(lit);
            Assert.AreSame(vinyl, StyleGuideContent.KeepVinyl(onVinyl, lit));
            Assert.AreSame(lit, StyleGuideContent.KeepVinyl(onLit, lit));
            Assert.AreSame(lit, StyleGuideContent.KeepVinyl(null, lit));
            Object.DestroyImmediate(onVinyl);
            Object.DestroyImmediate(onLit);

            const string crown = "Assets/Claybreak/Content/Generated/Units/LeaderCrown.mat";
            Assert.AreSame(vinyl, AssetDatabase.LoadAssetAtPath<Material>(crown).shader, "the committed crown material is vinyl");
            LeaderArt.BuildAssets();
            Assert.AreSame(vinyl, AssetDatabase.LoadAssetAtPath<Material>(crown).shader, "rebuilding the crown art keeps it vinyl");
        }

        // RT-6v3.4a: only hard edges count; the same cube shaded smooth (one normal per corner) reads round to the light.
        [Test]
        public void ASmoothShadedEdgeIsNotAHardEdge()
        {
            var corners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                corners[i] = new Vector3(i & 1, (i >> 1) & 1, (i >> 2) & 1) - Vector3.one * 0.5f;
            }

            var cube = new Mesh { vertices = corners };
            cube.triangles = new[]
            {
                0, 2, 3, 0, 3, 1, 4, 5, 7, 4, 7, 6, 0, 1, 5, 0, 5, 4, 2, 6, 7, 2, 7, 3, 0, 4, 6, 0, 6, 2, 1, 3, 7, 1, 7, 5,
            };
            cube.RecalculateNormals();
            Assert.AreEqual(0f, StyleFacts.MaxEdgeAngle(cube), 1e-3f);
            Object.DestroyImmediate(cube);
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

        [Test]
        public void AClipOffTheVolumeOrDurationWindowFailsInAV3Class()
        {
            StyleGuide guide = StyleGuideContent.Create();
            StyleFacts facts = Facts();
            ClipFact first = facts.Clips[0];
            SoundStyle entry = guide.SoundOf(first.name);
            facts.Clips[0] = new ClipFact { name = first.name, peakDb = first.peakDb, loudnessDb = entry.loudnessDb - 20f, seconds = first.seconds, licensed = true };
            AssertFails(StyleGuideValidation.Run(guide, facts), $"v3 clip {first.name} has volume");
            facts.Clips[0] = new ClipFact { name = first.name, peakDb = first.peakDb, loudnessDb = first.loudnessDb, seconds = entry.maxSeconds + 5f, licensed = true };
            AssertFails(StyleGuideValidation.Run(guide, facts), $"v3 clip {first.name} lasts");
            Object.DestroyImmediate(guide);
        }

        [Test]
        public void EveryWorldHasItsOwnLoopAndEveryClipHasAnEventInTheTable()
        {
            StyleGuide guide = StyleGuideContent.Create();
            Assert.AreEqual(4, ClaybreakAudio.WorldMusicClips.Length);
            foreach (string clip in ClaybreakAudio.WorldMusicClips)
            {
                Assert.IsNotNull(guide.SoundOf(clip), clip);
                StringAssert.StartsWith("music: run and assault, world", guide.SoundOf(clip).playedBy);
            }

            foreach (SoundStyle sound in guide.Sounds)
            {
                Assert.IsNotEmpty(sound.playedBy, sound.clip);
            }

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
                // RT-6v3.4a: a mesh already remade in v3 (or the reserve) answers to its own range, not the legacy ceiling.
                int ceiling = mesh.tier == MeshTier.V3 ? limits.v3MaxTriangles
                    : mesh.tier == MeshTier.Reserve ? limits.reserveMaxTriangles
                    : limits.maxTriangles;
                Assert.LessOrEqual(mesh.triangles, ceiling, mesh.name);
                Assert.LessOrEqual(mesh.drawCalls, mesh.tier == MeshTier.FollowsClass ? limits.maxDrawCalls : limits.v3MaxDrawCalls, mesh.name);
            }

            Object.DestroyImmediate(guide);
        }
    }
}
