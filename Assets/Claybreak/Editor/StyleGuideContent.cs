using System.Collections.Generic;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The <see cref="StyleGuide"/> as code, like the rest of the game's content (RT-6v3.1): the asset is rewritten from here, so a
    /// change to the guide is a reviewed edit of this file. It holds the values the game has TODAY and marks every class
    /// <see cref="StyleStatus.Legacy"/>: nothing on screen changes. Palettes come from <see cref="PaletteContent"/> (one source,
    /// not a copy), light from the scene's own constants and the toon look asset, sounds from <see cref="ClaybreakAudio"/>'s
    /// clip list. The shade and highlight levels equal the base color while a role is legacy (the toon ramp does the shading
    /// today); the v3 art series gives them values when it migrates the class.
    /// Batchmode: <c>-executeMethod Claybreak.Editor.StyleGuideContent.WriteAndQuit</c>.
    /// </summary>
    public static class StyleGuideContent
    {
        public const string Folder = ClaybreakSceneBuilder.Root + "/Style";

        /// <summary>CIE76 distance (docs 2.2) a v3 color may be from the nearest of its three levels.</summary>
        public const float ColorTolerance = 3f;

        // The palette role of each ToonPalette color, in ToonPalette.ColorNames order, and who wears it.
        private static readonly StyleClass[] RoleClasses =
        {
            StyleClass.Scenery,  // skyTop
            StyleClass.Scenery,  // skyBottom
            StyleClass.Scenery,  // fog (nothing draws it since RT-6t.13, kept by the kit's palette)
            StyleClass.Floor,    // ground
            StyleClass.Floor,    // track
            StyleClass.Floor,    // trackStripe
            StyleClass.Unit,     // crowd
            StyleClass.Scenery,  // foliageDark
            StyleClass.Scenery,  // foliageLight
            StyleClass.Scenery,  // trunk
            StyleClass.Scenery,  // rock
            StyleClass.Screen,   // uiAccent
            StyleClass.Screen,   // uiInk
            StyleClass.Tower,    // structureLight
            StyleClass.Tower,    // structureDark
            StyleClass.Tower,    // structureDetail
            StyleClass.Enemy,    // defense
        };

        /// <summary>The gate colors of the art direction, the same in every world (RT-6t.9).</summary>
        public const string GateGoodRole = "gateGood";
        public const string GateBadRole = "gateBad";
        public static readonly Color GateGood = new Color32(0x5a, 0xd1, 0xa0, 0xff);
        public static readonly Color GateBad = new Color32(0xff, 0x6b, 0x81, 0xff);

        // Material profiles. Legacy ones name the shaders the game draws with today; the v3 ones (docs 2.1) have no shader until the
        // sibling shader of the K.v3a delivery exists, so no material can claim one yet.
        public const string ProfileToon = "legacy-toon";
        public const string ProfileTranslucent = "legacy-translucent";
        public const string ProfileText = "legacy-text";
        public const string ProfileUiText = "legacy-ui-text";
        public const string ProfileSky = "legacy-sky";
        public const string ProfileTrack = "legacy-track";
        public const string ProfileBlobShadow = "legacy-blob-shadow";

        private static MaterialProfile[] Profiles() => new[]
        {
            Profile(ProfileToon, true, "Toon lit, 3-band ramp: structures, crowd, obstacles, props.", "StudioKit/ToonLit"),
            Profile(ProfileTranslucent, true, "URP Unlit, transparent: gate panels, shadows, danger strip, dome.", "Universal Render Pipeline/Unlit"),
            Profile(ProfileText, true, "TextMeshPro distance field: menu, HUD, gate and floating numbers.", "TextMeshPro/"),
            Profile(ProfileUiText, true, "The built-in text shader of the dynamic font (UI.Text and TextMesh: health labels, a few HUD texts).", "GUI/Text Shader"),
            Profile(ProfileSky, true, "The sky gradient (reads the palette globals).", "StudioKit/SkyGradientScreen"),
            Profile(ProfileTrack, true, "The striped track.", "StudioKit/StripedTrack"),
            Profile(ProfileBlobShadow, true, "The instanced blob shadow of the crowd.", "StudioKit/BlobShadowInstanced"),
            Profile("vinyl", false, "v3: the character (edge light, soft specular, warm shade). Shader: K.v3a."),
            Profile("matte-plastic", false, "v3: scenery, floor, towers. Shader: K.v3a."),
            Profile("gate-glow", false, "v3: translucent gate with a lit edge, no depth write. Shader: K.v3a."),
            Profile("enemy", false, "v3: darkened vinyl with a threat-colored edge. Shader: K.v3a."),
            Profile("ui-effect", false, "v3: unlit UI and effects. Shader: K.v3a."),
        };

        private static MaterialProfile Profile(string id, bool legacy, string note, params string[] shaders)
        {
            return new MaterialProfile { id = id, legacy = legacy, note = note, shaders = shaders };
        }

        // Every material asset of the game and the class that owns it. The validator refuses a material asset not listed here.
        private static MaterialStyle[] Materials()
        {
            var list = new List<MaterialStyle>();
            void Add(StyleClass styleClass, string profile, params string[] names)
            {
                foreach (string name in names)
                {
                    list.Add(new MaterialStyle { material = name, styleClass = styleClass, profile = profile });
                }
            }

            Add(StyleClass.Unit, ProfileToon, "CrowdToon");
            Add(StyleClass.Unit, ProfileBlobShadow, "CrowdShadow");
            Add(StyleClass.Leader, ProfileToon, "LeaderCrown");
            Add(StyleClass.Tower, ProfileToon, "Stone", "StoneDark", "StoneLight", "StoneWarm", "Wood", "Banner", "Terracotta", "ClayDark",
                "Moss", "Bone", "Health", "Glow", "GlowHot", "WoodDark", "Thatch", "Lantern", "Brick", "Frost", "Basalt", "Ice");
            Add(StyleClass.Enemy, ProfileToon, "Iron", "IronWarm", "Soot", "Steel", "Wall", "ShieldCore");
            Add(StyleClass.Enemy, ProfileTranslucent, "ShieldDome", "Danger");
            Add(StyleClass.Gate, ProfileTranslucent, "GateGood", "GateBad", "GateGoodLit", "GateBadLit", "GateOff");
            Add(StyleClass.Gate, ProfileToon, "GatePostToon", "Crate");
            Add(StyleClass.Gate, ProfileText, "Baloo2 Gate Good", "Baloo2 Gate Bad");
            Add(StyleClass.Obstacle, ProfileToon, "ObstacleToon");
            Add(StyleClass.Floor, ProfileToon, "GroundToon", "Line");
            Add(StyleClass.Floor, ProfileTrack, "StripedTrack");
            Add(StyleClass.Floor, ProfileTranslucent, "LaneMarker");
            Add(StyleClass.Scenery, ProfileToon, "PropsToon");
            Add(StyleClass.Scenery, ProfileSky, "SkyGradient");
            Add(StyleClass.Effect, ProfileTranslucent, "Shadow");
            Add(StyleClass.Effect, ProfileText, "Baloo2 Float");
            // Sub-assets of the font atlas and of the icon sprite sheet count too.
            Add(StyleClass.Screen, ProfileText, "Baloo2 Counter", "Baloo2 Ink", "Baloo2 Ui", "Baloo2-ExtraBold SDF", "GameIcons");
            Add(StyleClass.Screen, ProfileUiText, "Baloo2-ExtraBold");
            return list.ToArray();
        }

        // The class table. The legacy ceilings are the present game's as the validator measured it (StyleGuideTests logs the measures:
        // Unit 492 triangles, Leader 250, Enemy 1744 and 10 draws, Tower 4976 and 55, Boss 11976 and 60, Gate 264 and 7, Obstacle 552,
        // Floor 36, Scenery 160), rounded up about 20% so a small edit does not fail but a regression does; the v3
        // budgets are the plan's (docs 4.1). Edge angle: no sharper than 60 degrees between neighbouring faces in a v3 mesh.
        private static ClassStyle[] Classes() => new[]
        {
            Class(StyleClass.Unit, maxTriangles: 600, maxDrawCalls: 1, v3Min: 800, v3Max: 1200, v3Calls: 1),
            Class(StyleClass.Leader, 400, 1, 3000, 5000, 1),
            Class(StyleClass.Enemy, 2200, 16, 1500, 3000, 1),
            Class(StyleClass.Tower, 6000, 70, 6000, 12000, 4),
            Class(StyleClass.Boss, 13000, 75, 8000, 15000, 4),
            Class(StyleClass.Gate, 400, 10, 100, 2000, 2),
            Class(StyleClass.Obstacle, 700, 1, 100, 2500, 1),
            Class(StyleClass.Floor, 200, 2, 100, 4000, 1),
            Class(StyleClass.Scenery, 300, 1, 300, 1500, 1),
            Class(StyleClass.Effect, 0, 0, 0, 0, 0),
            Class(StyleClass.Sound, 0, 0, 0, 0, 0),
            Class(StyleClass.Screen, 0, 0, 0, 0, 0),
        };

        private static ClassStyle Class(StyleClass styleClass, int maxTriangles, int maxDrawCalls, int v3Min, int v3Max, int v3Calls)
        {
            return new ClassStyle
            {
                styleClass = styleClass,
                status = StyleStatus.Legacy,
                maxTriangles = maxTriangles,
                maxDrawCalls = maxDrawCalls,
                v3MinTriangles = v3Min,
                v3MaxTriangles = v3Max,
                v3MaxDrawCalls = v3Calls,
                maxEdgeAngle = 60f,
            };
        }

        // The limits of every effect: what the game preallocates today (rule 2.2.1), so the guide and the pools cannot drift apart.
        private static EffectStyle[] Effects() => new[]
        {
            new EffectStyle { effect = StyleEffect.FloatingNumber, maxActive = 12, note = "numbers that float up from a gate (FloatingNumbers pool)" },
            new EffectStyle { effect = StyleEffect.CounterPop, maxActive = 1, note = "the army counter pop in the HUD" },
            new EffectStyle { effect = StyleEffect.UnitDeath, maxActive = ClaybreakSceneBuilder.FlightDyingCapacity, note = "ballistic deaths of the instanced swarms (assault); the obstacles' ring is smaller" },
            new EffectStyle { effect = StyleEffect.CameraShake, maxActive = 1, note = "the run camera's shake on a bad gate" },
            new EffectStyle { effect = StyleEffect.ProjectileImpact, maxActive = ClaybreakSceneBuilder.ProjectilePoolSize, note = "stone and landing shadow of the cannon (pool)" },
            new EffectStyle { effect = StyleEffect.TowerBreak, maxActive = AssaultTower.BlocksPerBody, note = "tower floors crumbling (blocks per body)" },
        };

        /// <summary>The generator's peaks (clay_siege_audio.py): effects are normalized to -1 dBFS, the two music loops sit at -3.</summary>
        public const float EffectPeakDb = -1f;
        public const float MusicPeakDb = -3f;

        // The sounds: one entry per clip the game uses, the event that plays it (docs/audio-licenses.md has the same table with
        // licenses). The loudness target is the generator's.
        private static SoundStyle[] Sounds()
        {
            var list = new List<SoundStyle>();
            foreach (string clip in ClaybreakAudio.ClipNames())
            {
                list.Add(new SoundStyle { clip = clip, playedBy = PlayedBy(clip), peakDb = clip == ClaybreakAudio.MenuMusicClip || clip == ClaybreakAudio.RunMusicClip ? MusicPeakDb : EffectPeakDb, peakToleranceDb = 1.5f });
            }

            return list.ToArray();
        }

        private static string PlayedBy(string clip)
        {
            if (clip == ClaybreakAudio.MenuMusicClip)
            {
                return "music: menus";
            }

            if (clip == ClaybreakAudio.RunMusicClip)
            {
                return "music: run and assault";
            }

            return clip.StartsWith("Cue") ? "cue " + clip.Substring(3) : "role " + clip;
        }

        /// <summary>The roles of one world, from its palette definition: the base color of each, shade and highlight equal to it (legacy).</summary>
        public static PaletteRole[] RolesOf(PaletteDefinition definition)
        {
            Color[] colors = PaletteContent.Colors(definition);
            string[] names = ToonPalette.ColorNames;
            var roles = new PaletteRole[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                roles[i] = Role(names[i], RoleClasses[i], colors[i]);
            }

            return roles;
        }

        private static PaletteRole Role(string name, StyleClass styleClass, Color color)
        {
            return new PaletteRole { role = name, styleClass = styleClass, levels = new ColorLevels(color, color, color) };
        }

        private static WorldStyle[] Worlds()
        {
            var look = AssetDatabase.LoadAssetAtPath<ToonLook>(ClaybreakSceneBuilder.CrowdLookPath);
            PaletteDefinition[] palettes = PaletteContent.Palettes;
            var worlds = new WorldStyle[palettes.Length];
            for (int w = 0; w < worlds.Length; w++)
            {
                worlds[w] = new WorldStyle
                {
                    name = palettes[w].Name,
                    roles = RolesOf(palettes[w]),
                    // ClaybreakSceneBuilder.CreateEnvironment: one directional sun, the same in every world, no shadows, flat ambient.
                    sunEuler = new Vector3(50f, -30f, 0f),
                    sunColor = Color.white,
                    sunIntensity = 1f,
                    ambientSky = look != null ? look.SkyLight : Color.white,
                    ambientGround = look != null ? look.GroundLight : Color.gray,
                    ambientIntensity = look != null ? look.AmbientIntensity : 1f,
                    directIntensity = look != null ? look.DirectIntensity : 1f,
                    postProcessing = "none",
                };
            }

            return worlds;
        }

        /// <summary>Builds the guide the game has today, in memory (the tests start from it).</summary>
        public static StyleGuide Create()
        {
            var guide = ScriptableObject.CreateInstance<StyleGuide>();
            guide.EditorAssign(Classes(), Worlds(), new[]
            {
                Role(GateGoodRole, StyleClass.Gate, GateGood),
                Role(GateBadRole, StyleClass.Gate, GateBad),
            }, Profiles(), Materials(), Effects(), Sounds(), ColorTolerance);
            return guide;
        }

        [MenuItem("Claybreak/Write Style Guide")]
        public static void Write()
        {
            ClaybreakSceneBuilder.EnsureFolder(Folder);
            StyleGuide fresh = Create();
            var guide = AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath);
            if (guide == null)
            {
                AssetDatabase.CreateAsset(fresh, StyleGuide.AssetPath);
            }
            else
            {
                // The migration state is part of the content (a class goes v3 by editing Classes()), so nothing is kept from the asset.
                guide.EditorAssign(fresh.Classes, fresh.Worlds, fresh.SharedRoles, fresh.Profiles, fresh.Materials, fresh.Effects,
                    fresh.Sounds, fresh.ColorTolerance);
                EditorUtility.SetDirty(guide);
                Object.DestroyImmediate(fresh);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[StyleGuideContent] wrote " + StyleGuide.AssetPath);
        }

        public static void WriteAndQuit()
        {
            BatchRunner.Run("StyleGuideContent", Write);
        }
    }
}
