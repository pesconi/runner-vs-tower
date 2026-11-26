using System.Collections.Generic;
using JungleDegree.StudioKit.Core;
using JungleDegree.StudioKit.Toon;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Claybreak.Editor
{
    /// <summary>
    /// The <see cref="StyleGuide"/> as code, like the rest of the game's content (RT-6v3.1): the asset is rewritten from here, so a
    /// change to the guide is a reviewed edit of this file. Palettes come from <see cref="PaletteContent"/> (one source, not a
    /// copy), sounds from <see cref="ClaybreakAudio"/>'s clip list. RT-6v3.3 (D36): the surfaces of the drawn classes are v3 (the
    /// kit's <c>StudioKit/ToonVinyl</c> through the profiles below, written into the materials by <see cref="ApplySurfaces"/>),
    /// and each world has its vinyl light, its fog and its post stack, written into assets the scene shows per world
    /// (<see cref="WriteWorldAssets"/>). Meshes are still the old ones (shape state legacy until RT-6v3.4 onward).
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

        // Material profiles. Legacy ones name the shaders the game drew with before v3 (still used by the classes not migrated yet);
        // the v3 ones (docs 2.1) name the kit's vinyl sibling (K.v3a) and carry its surface values.
        public const string ProfileToon = "legacy-toon";
        public const string ProfileTranslucent = "legacy-translucent";
        public const string ProfileText = "legacy-text";
        public const string ProfileUiText = "legacy-ui-text";
        public const string ProfileSky = "legacy-sky";
        public const string ProfileTrack = "legacy-track";
        public const string ProfileBlobShadow = "legacy-blob-shadow";

        public const string ProfileVinyl = "vinyl";
        public const string ProfileMatte = "matte-plastic";
        public const string ProfileStructure = "structure";
        public const string ProfileEnemy = "enemy";
        public const string ProfileSkyV3 = "sky";
        public const string ProfileTrackV3 = "track";
        public const string ProfileContactShadow = "contact-shadow";
        public const string ProfileOverlay = "unlit-overlay";

        /// <summary>The threat color of the enemies' rim (warm red, against the cool night of every world).</summary>
        public static readonly Color ThreatRim = new Color(1f, 0.42f, 0.34f);

        // RT-6v3.3 surfaces (ToonVinylSurface: gloss, highlight size, rim, rim tint, top gloss, darken, emission, fog amount,
        // translucent, alpha). Emission and fog amount are left as each material's art set them (glows, structure fog): see
        // ApplySurfaces. "No gloss on the floor" (RT-6v3.2 review): every profile of the floor and the scenery keeps top gloss 0.
        public static readonly ToonVinylSurface VinylSurface = new ToonVinylSurface(0.6f, 0.35f, 1f, Color.white, 1f, 0f, 0f, 1f, false, 1f);
        public static readonly ToonVinylSurface MatteSurface = new ToonVinylSurface(0.12f, 0.6f, 0.45f, Color.white, 0f, 0f, 0f, 1f, false, 1f);
        public static readonly ToonVinylSurface StructureSurface = new ToonVinylSurface(0.22f, 0.5f, 0.7f, Color.white, 0f, 0f, 0f, 1f, false, 1f);
        public static readonly ToonVinylSurface EnemySurface = new ToonVinylSurface(0.45f, 0.35f, 1.4f, ThreatRim, 0.5f, 0.12f, 0f, 1f, false, 1f);

        private static MaterialProfile[] Profiles() => new[]
        {
            Profile(ProfileToon, true, "Toon lit, 3-band ramp (before v3): gate panels' classes still legacy.", "StudioKit/ToonLit"),
            Profile(ProfileTranslucent, true, "URP Unlit, transparent: gate panels, effect shadows.", "Universal Render Pipeline/Unlit"),
            Profile(ProfileText, true, "TextMeshPro distance field: menu, HUD, gate and floating numbers.", "TextMeshPro/"),
            Profile(ProfileUiText, true, "The built-in text shader of the dynamic font (UI.Text and TextMesh: health labels, a few HUD texts).", "GUI/Text Shader"),
            Profile(ProfileSky, true, "The sky gradient (reads the palette globals).", "StudioKit/SkyGradientScreen"),
            Profile(ProfileTrack, true, "The striped track.", "StudioKit/StripedTrack"),
            Profile(ProfileBlobShadow, true, "The instanced blob shadow of the crowd.", "StudioKit/BlobShadowInstanced"),
            Vinyl(ProfileVinyl, "v3: the character's glossy vinyl (soft ramp, warm key, highlight and rim, also on top).", VinylSurface),
            Vinyl(ProfileMatte, "v3: matte plastic for the floor, the rails, the scenery and the gate posts; no gloss on faces looking up.", MatteSurface),
            Vinyl(ProfileStructure, "v3: light matte plastic of the towers and bosses (the night factory's clear structures), a cool rim; no gloss on top.", StructureSurface),
            Vinyl(ProfileEnemy, "v3: darkened vinyl with a warm threat rim (defenses and obstacles).", EnemySurface),
            Profile(ProfileSkyV3, false, "v3: the night sky gradient of the world (palette sky top/bottom).", "StudioKit/SkyGradientScreen"),
            Profile(ProfileTrackV3, false, "v3: the neutral striped track (unlit stripes, no gloss).", "StudioKit/StripedTrack"),
            Profile(ProfileContactShadow, false, "v3: the contact shadow, a round blob under each figure (D36).", "StudioKit/BlobShadowInstanced"),
            Profile(ProfileOverlay, false, "v3: unlit translucent overlays of the play area (lane marker, danger strip, shield dome).", "Universal Render Pipeline/Unlit"),
            Profile("gate-glow", false, "v3: translucent gate with a lit edge, no depth write (RT-6v3.6)."),
            Profile("ui-effect", false, "v3: unlit UI and effects (RT-6v3.8/3.10)."),
        };

        private static MaterialProfile Profile(string id, bool legacy, string note, params string[] shaders)
        {
            return new MaterialProfile { id = id, legacy = legacy, note = note, shaders = shaders };
        }

        private static MaterialProfile Vinyl(string id, string note, ToonVinylSurface surface)
        {
            return new MaterialProfile { id = id, legacy = false, note = note, shaders = new[] { ToonShaderIds.ToonVinylShader }, vinyl = true, surface = surface };
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

            Add(StyleClass.Unit, ProfileVinyl, "CrowdToon");
            Add(StyleClass.Unit, ProfileContactShadow, "CrowdShadow");
            Add(StyleClass.Leader, ProfileVinyl, "LeaderCrown");
            Add(StyleClass.Tower, ProfileStructure, "Stone", "StoneDark", "StoneLight", "StoneWarm", "Wood", "Banner", "Terracotta", "ClayDark",
                "Moss", "Bone", "Health", "Glow", "GlowHot", "WoodDark", "Thatch", "Lantern", "Brick", "Frost", "Basalt", "Ice");
            Add(StyleClass.Enemy, ProfileEnemy, "Iron", "IronWarm", "Soot", "Steel", "Wall", "ShieldCore");
            Add(StyleClass.Enemy, ProfileOverlay, "ShieldDome", "Danger");
            Add(StyleClass.Gate, ProfileTranslucent, "GateGood", "GateBad", "GateGoodLit", "GateBadLit", "GateOff");
            // The gate class stays legacy until RT-6v3.6; its posts and crates already wear the matte plastic of the floor.
            Add(StyleClass.Gate, ProfileMatte, "GatePostToon", "Crate");
            Add(StyleClass.Gate, ProfileText, "Baloo2 Gate Good", "Baloo2 Gate Bad");
            Add(StyleClass.Obstacle, ProfileEnemy, "ObstacleToon");
            Add(StyleClass.Floor, ProfileMatte, "GroundToon", "Line");
            Add(StyleClass.Floor, ProfileTrackV3, "StripedTrack");
            Add(StyleClass.Floor, ProfileOverlay, "LaneMarker");
            Add(StyleClass.Scenery, ProfileMatte, "PropsToon");
            Add(StyleClass.Scenery, ProfileSkyV3, "SkyGradient");
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
        // budgets are the plan's (docs 4.1). Edge angle: no hard edge sharper than 60 degrees between neighbouring faces in a v3 mesh.
        // RT-6v3.4a: the Unit's reserve (docs 3.1, ~500 smooth) is 450-550.
        private static ClassStyle[] Classes() => new[]
        {
            Class(StyleClass.Unit, maxTriangles: 600, maxDrawCalls: 1, v3Min: 800, v3Max: 1200, v3Calls: 1, reserveMin: 450, reserveMax: 550),
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

        // RT-6v3.3: the classes whose surface (materials, palette levels, light) is v3. Gates (6v3.6), effects (6v3.8) and screens
        // (6v3.10) come later; the sound went v3 whole in RT-6v3.9.
        private static readonly StyleClass[] SurfaceV3 =
        {
            StyleClass.Unit, StyleClass.Leader, StyleClass.Enemy, StyleClass.Tower, StyleClass.Boss, StyleClass.Obstacle,
            StyleClass.Floor, StyleClass.Scenery,
        };

        private static ClassStyle Class(StyleClass styleClass, int maxTriangles, int maxDrawCalls, int v3Min, int v3Max, int v3Calls,
            int reserveMin = 0, int reserveMax = 0)
        {
            return new ClassStyle
            {
                styleClass = styleClass,
                status = styleClass == StyleClass.Sound ? StyleStatus.V3 : StyleStatus.Legacy, // RT-6v3.9 migrated the sound
                surface = System.Array.IndexOf(SurfaceV3, styleClass) >= 0 ? StyleStatus.V3 : StyleStatus.Legacy,
                maxTriangles = maxTriangles,
                maxDrawCalls = maxDrawCalls,
                v3MinTriangles = v3Min,
                v3MaxTriangles = v3Max,
                v3MaxDrawCalls = v3Calls,
                reserveMinTriangles = reserveMin,
                reserveMaxTriangles = reserveMax,
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

        /// <summary>The generator's peaks (stampeep_toy_audio / clay_siege_audio.py): effects are normalized to -1 dBFS, the music loops sit at -3.</summary>
        public const float EffectPeakDb = -1f;
        public const float MusicPeakDb = -3f;

        // RT-6v3.9: the sound table, "wind-up toy" signature (ratchet, spring, short bell, burst into pieces, key winding) and one
        // four-note motif (degrees 1-5-3-8) in every music loop. Loudness is the approximate volume (RMS in dBFS of the samples above
        // -40 dBFS, Tools/audio/audio_metrics.py and StyleFacts.WavMetrics measure it the same way); a clip outside its peak, volume or
        // duration window fails the validator.
        private enum SoundKind { Tap, Effect, Warning, Jingle, Music }

        private static SoundStyle[] Sounds()
        {
            var list = new List<SoundStyle>();
            foreach (string clip in ClaybreakAudio.ClipNames())
            {
                SoundKind kind = KindOf(clip);
                bool music = kind == SoundKind.Music;
                list.Add(new SoundStyle
                {
                    clip = clip,
                    playedBy = PlayedBy(clip),
                    peakDb = music ? MusicPeakDb : EffectPeakDb,
                    peakToleranceDb = 1.5f,
                    loudnessDb = kind == SoundKind.Warning ? -7f : music ? -17f : -15f,
                    loudnessToleranceDb = kind == SoundKind.Warning ? 3f : music ? 3f : 5f,
                    minSeconds = kind == SoundKind.Tap ? 0.04f : kind == SoundKind.Jingle ? 0.5f : music ? 10f : 0.05f,
                    maxSeconds = kind == SoundKind.Tap ? 0.25f : kind == SoundKind.Jingle ? 3f : music ? 25f : 1.5f,
                });
            }

            return list.ToArray();
        }

        private static SoundKind KindOf(string clip)
        {
            if (ClaybreakAudio.IsMusic(clip))
            {
                return SoundKind.Music;
            }

            switch (clip)
            {
                case "ButtonClick":
                case "ButtonBack":
                    return SoundKind.Tap;
                case "CueProjectileWarning":
                    return SoundKind.Warning; // a steady whistle: its RMS sits near its peak
                case "Victory":
                case "Defeat":
                case "CueTowerFall":
                case "CueBoss":
                    return SoundKind.Jingle;
                default:
                    return SoundKind.Effect;
            }
        }

        // The event -> sound table (also written into docs/art/v3/checklist.md and docs/audio-licenses.md).
        private static string PlayedBy(string clip)
        {
            switch (clip)
            {
                case "MenuLoop": return "music: menus (the motif in C major, marimba and bell)";
                case "RunLoop": return "music: run and assault, world 1 (C major, 124 BPM, bell lead)";
                case "RunLoopW2": return "music: run and assault, world 2 (A minor, 132 BPM, pluck lead)";
                case "RunLoopW3": return "music: run and assault, world 3 (F major, 112 BPM, swing, brass lead)";
                case "RunLoopW4": return "music: run and assault, world 4 (D minor, 140 BPM, square lead)";
                case "GateGood": return "role GateGood: crowd takes a good gate (ratchet + first three notes of the motif on a short bell)";
                case "GateBad": return "role GateBad: bad gate or obstacle loss (coil spring loosening)";
                case "UnitLoss": return "role UnitLoss: a figure bursts into pieces and confetti (assault)";
                case "Coin": return "role Coin: purchase (short double bell)";
                case "ButtonClick": return "role ButtonClick: button press (ratchet tick)";
                case "ButtonBack": return "role ButtonBack: back (lower tick)";
                case "CueKeyWind": return "cue KeyWind: the wind-up key turns as a level starts";
                case "CueNumberPop": return "cue NumberPop: the number of a good gate jumps";
            }

            return clip.StartsWith("Cue") ? "cue " + clip.Substring(3) : "role " + clip;
        }

        /// <summary>
        /// The roles of one world, from its palette definition. A role of a v3-surface class gets the three levels the vinyl light
        /// gives it on screen before the post: base, the base in full shadow (times the world's shadow tint) and the lit base
        /// under the warm key through the tone shoulder; a legacy role keeps shade and highlight equal to the base.
        /// </summary>
        public static PaletteRole[] RolesOf(PaletteDefinition definition, WorldLight light)
        {
            Color[] colors = PaletteContent.Colors(definition);
            string[] names = ToonPalette.ColorNames;
            var roles = new PaletteRole[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                roles[i] = System.Array.IndexOf(SurfaceV3, RoleClasses[i]) >= 0
                    ? V3Role(names[i], RoleClasses[i], colors[i], light)
                    : Role(names[i], RoleClasses[i], colors[i]);
            }

            return roles;
        }

        private static PaletteRole V3Role(string name, StyleClass styleClass, Color color, WorldLight light)
        {
            Color shadow = color * light.shadowTint;
            shadow.a = 1f;
            Color key = light.keyColor * (light.keyIntensity * light.directIntensity + light.ambientIntensity);
            Vector3 lit = ToonVinylLook.Shoulder(new Vector3(color.r * key.r, color.g * key.g, color.b * key.b), light.shoulderKnee);
            var highlight = new Color(Mathf.Clamp01(lit.x), Mathf.Clamp01(lit.y), Mathf.Clamp01(lit.z), 1f);
            return new PaletteRole { role = name, styleClass = styleClass, levels = new ColorLevels(color, shadow, highlight) };
        }

        private static PaletteRole Role(string name, StyleClass styleClass, Color color)
        {
            return new PaletteRole { role = name, styleClass = styleClass, levels = new ColorLevels(color, color, color) };
        }

        // RT-6v3.3 (D36): the night factory of the A/M concept screens in every world (dark blue sky in a gradient, clear
        // structures, a glow on the rims) with the world's own accent: the rim light takes the world's glow (palette structure
        // detail). Warm key, cool two-tone ambient, rim, soft shoulder (the belly of the vinyl does not burn out, T-Kv3a-3).
        private static WorldLight LightOf(PaletteDefinition definition)
        {
            Color[] colors = PaletteContent.Colors(definition);
            Color glow = colors[System.Array.IndexOf(ToonPalette.ColorNames, "structureDetail")];
            Color ground = colors[System.Array.IndexOf(ToonPalette.ColorNames, "ground")];
            return new WorldLight
            {
                softness = 0.12f,
                lambert = 0.35f,
                // Review rounds 1-2: a light cool shade, so the clear structures, the whites and the cream belly do not read lilac.
                shadowTint = new Color(0.84f, 0.83f, 0.92f),
                keyColor = new Color(1f, 0.91f, 0.78f),
                keyIntensity = 1.05f,
                directIntensity = 0.78f,
                skyAmbient = new Color(0.66f, 0.78f, 1f),
                groundAmbient = Color.Lerp(new Color(0.55f, 0.52f, 0.72f), ground, 0.35f),
                ambientIntensity = 0.7f,
                rimColor = Color.Lerp(Color.white, glow, 0.55f),
                rimStrength = 0.55f,
                rimWidth = 3f,
                shoulderKnee = 0.78f,
                highlightPeak = 1.1f,
            };
        }

        // The cheap post of every world (docs 2.1, D36 "bloom leve e vinheta"): a little contrast and saturation, a navy vignette
        // and a light bloom that only the highlight cores pass (over 1 in HDR). No tonemapping (T-RT6v3.2-3).
        private static WorldPost PostOf(int world) => new WorldPost
        {
            exposure = 0f,
            contrast = 10f,
            saturation = 10f,
            filter = Color.white,
            vignette = 0.22f,
            vignetteColor = new Color(0.04f, 0.05f, 0.14f),
            // Review round 1: at 1.15 nothing reached the bloom; at 1.0 only the vinyl highlights and rims (which pass 1 in HDR)
            // glow a little, the matte track and structures stay under it.
            bloom = 0.4f,
            bloomThreshold = 1f,
        };

        private static readonly string[] Moods =
        {
            "fábrica noturna, céu azul-marinho, pista clara neutra, multidão rosa-choque, brilho ciano, defesas petróleo-escuro",
            "fábrica noturna ao fim do turno, céu violeta-escuro, pista clara neutra, multidão laranja, brilho âmbar, defesas azul-cobalto",
            "fábrica noturna de doces, céu azul-petróleo, pista clara neutra, multidão violeta, brilho menta, defesas âmbar-escuro",
            "fábrica noturna de gelo, céu azul-profundo, pista escura neutra sobre chão petróleo, multidão lima, brilho ciano-gelo, defesas roxas",
        };

        private static WorldStyle[] Worlds()
        {
            PaletteDefinition[] palettes = PaletteContent.Palettes;
            var worlds = new WorldStyle[palettes.Length];
            for (int w = 0; w < worlds.Length; w++)
            {
                WorldLight light = LightOf(palettes[w]);
                worlds[w] = new WorldStyle
                {
                    name = palettes[w].Name,
                    roles = RolesOf(palettes[w], light),
                    // ClaybreakSceneBuilder.CreateEnvironment: one directional sun, the same in every world, no shadows.
                    sunEuler = new Vector3(50f, -30f, 0f),
                    light = light,
                    post = PostOf(w),
                    fogStart = palettes[w].FogStart,
                    fogEnd = palettes[w].FogEnd,
                    mood = w < Moods.Length ? Moods[w] : palettes[w].Name,
                };
            }

            return worlds;
        }

        /// <summary>
        /// The D36 contrast rule (RT-6v3.3). Palette thresholds are what the night palettes were designed to (crowd chroma 81–108
        /// against at most 68 for anything else, |ΔL*| 34–62, hue gap 120–164°); captured ones leave room for the light and the
        /// post, which move every color.
        /// </summary>
        public static ContrastRule Contrast() => new ContrastRule
        {
            crowdChromaMargin = 5f,
            trackMaxChroma = 15f,
            crowdTrackMinLightness = 30f,
            enemyMinHue = 115f,
            capturedCrowdChromaMargin = 0f,
            capturedCrowdTrackMinLightness = 20f,
            capturedEnemyMinHue = 90f,
            gateLabelMinRatio = 3f,
            skinMinDeltaE = 20f,
        };

        /// <summary>Builds the guide the game has today, in memory (the tests start from it).</summary>
        public static StyleGuide Create()
        {
            var guide = ScriptableObject.CreateInstance<StyleGuide>();
            guide.EditorAssign(Classes(), Worlds(), new[]
            {
                Role(GateGoodRole, StyleClass.Gate, GateGood),
                Role(GateBadRole, StyleClass.Gate, GateBad),
            }, Profiles(), Materials(), Effects(), Sounds(), ColorTolerance, Contrast());
            guide.EditorAssignFaces(Faces());
            return guide;
        }

        /// <summary>
        /// RT-6v3.4b (docs 3.2): event → expression of the Bibopzi's face atlas, with duration and priority. Base faces (0 s)
        /// hold: the run is neutral and blinks, the assault is effort, the level's end holds victory or defeat. Reactions show
        /// over a base of lower or equal priority: a bad gate or an obstacle (scared, hands wide open) wins over a good gate
        /// (happy, hands half open); the units in flight say "ui!", and so do the ones thrown out.
        /// </summary>
        public static FaceRules Faces()
        {
            return new FaceRules
            {
                rules = new[]
                {
                    new FaceRule(FaceEvent.Run, FaceExpression.Neutral, 0f, 0, 0f),
                    new FaceRule(FaceEvent.Blink, FaceExpression.Blink, 0.13f, 1, 0f),
                    new FaceRule(FaceEvent.GoodGate, FaceExpression.Happy, 0.9f, 2, 0.55f),
                    new FaceRule(FaceEvent.BadGate, FaceExpression.Scared, 0.8f, 3, 1f),
                    new FaceRule(FaceEvent.ObstacleHit, FaceExpression.Scared, 0.6f, 3, 1f),
                    new FaceRule(FaceEvent.Assault, FaceExpression.Effort, 0f, 1, 0.25f),
                    new FaceRule(FaceEvent.Launch, FaceExpression.Ouch, 0f, 0, 1f),
                    new FaceRule(FaceEvent.Thrown, FaceExpression.Ouch, 0f, 0, 1f),
                    new FaceRule(FaceEvent.Victory, FaceExpression.Victory, 0f, 5, 1f),
                    new FaceRule(FaceEvent.Defeat, FaceExpression.Defeat, 0f, 5, 0f),
                },
                blinkEvery = 3.4f,
            };
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
                    fresh.Sounds, fresh.ColorTolerance, fresh.Contrast);
                guide.EditorAssignFaces(fresh.Faces);
                EditorUtility.SetDirty(guide);
                Object.DestroyImmediate(fresh);
            }

            WriteWorldAssets(guide ?? AssetDatabase.LoadAssetAtPath<StyleGuide>(StyleGuide.AssetPath));
            AssetDatabase.SaveAssets();
            Debug.Log("[StyleGuideContent] wrote " + StyleGuide.AssetPath);
        }

        // ---------- RT-6v3.3: the world assets and the materials, from the guide ----------

        public static string LightPath(int world) => $"{Folder}/Light{PaletteContent.Palettes[world].Name}.asset";
        public static string PostPath(int world) => $"{Folder}/Post{PaletteContent.Palettes[world].Name}.asset";

        /// <summary>Writes each world's vinyl light (a kit <see cref="ToonVinylLook"/>) and post profile from the guide.</summary>
        public static void WriteWorldAssets(StyleGuide guide)
        {
            for (int w = 0; w < guide.Worlds.Length; w++)
            {
                WorldStyle world = guide.Worlds[w];
                var look = AssetDatabase.LoadAssetAtPath<ToonVinylLook>(LightPath(w));
                if (look == null)
                {
                    look = ScriptableObject.CreateInstance<ToonVinylLook>();
                    AssetDatabase.CreateAsset(look, LightPath(w));
                }

                FillLook(look, world.light);
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostPath(w));
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<VolumeProfile>();
                    AssetDatabase.CreateAsset(profile, PostPath(w));
                }

                FillPost(profile, world.post);
                EditorUtility.SetDirty(look);
                EditorUtility.SetDirty(profile);
            }
        }

        /// <summary>The kit look's serialized fields, in the guide's values.</summary>
        public static void FillLook(ToonVinylLook look, WorldLight light)
        {
            var so = new SerializedObject(look);
            so.FindProperty("softness").floatValue = light.softness;
            so.FindProperty("lambert").floatValue = light.lambert;
            so.FindProperty("shadowTint").colorValue = light.shadowTint;
            so.FindProperty("keyColor").colorValue = light.keyColor;
            so.FindProperty("keyIntensity").floatValue = light.keyIntensity;
            so.FindProperty("directIntensity").floatValue = light.directIntensity;
            so.FindProperty("skyAmbient").colorValue = light.skyAmbient;
            so.FindProperty("groundAmbient").colorValue = light.groundAmbient;
            so.FindProperty("ambientIntensity").floatValue = light.ambientIntensity;
            so.FindProperty("rimColor").colorValue = light.rimColor;
            so.FindProperty("rimStrength").floatValue = light.rimStrength;
            so.FindProperty("rimWidth").floatValue = light.rimWidth;
            so.FindProperty("shoulderKnee").floatValue = light.shoulderKnee;
            so.FindProperty("highlightPeak").floatValue = light.highlightPeak;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>True when the look asset holds the guide's light (validator).</summary>
        public static bool LookMatches(ToonVinylLook look, WorldLight light)
        {
            return look != null && Same(look.Softness, light.softness) && Same(look.Lambert, light.lambert) && look.ShadowTint == light.shadowTint
                && look.KeyColor == light.keyColor && Same(look.KeyIntensity, light.keyIntensity) && Same(look.DirectIntensity, light.directIntensity)
                && look.SkyAmbient == light.skyAmbient && look.GroundAmbient == light.groundAmbient && Same(look.AmbientIntensity, light.ambientIntensity)
                && look.RimColor == light.rimColor && Same(look.RimStrength, light.rimStrength) && Same(look.RimWidth, light.rimWidth)
                && Same(look.ShoulderKnee, light.shoulderKnee) && Same(look.HighlightPeak, light.highlightPeak);
        }

        /// <summary>The post profile in the guide's values: no tonemapping, color adjustments, vignette, bloom (or none).</summary>
        public static void FillPost(VolumeProfile profile, WorldPost post)
        {
            Override<Tonemapping>(profile).mode.Override(TonemappingMode.None);
            ColorAdjustments color = Override<ColorAdjustments>(profile);
            color.postExposure.Override(post.exposure);
            color.contrast.Override(post.contrast);
            color.saturation.Override(post.saturation);
            color.colorFilter.Override(post.filter);
            Vignette vignette = Override<Vignette>(profile);
            vignette.intensity.Override(post.vignette);
            vignette.smoothness.Override(0.45f);
            vignette.color.Override(post.vignetteColor);
            if (post.bloom > 0f)
            {
                Bloom bloom = Override<Bloom>(profile);
                bloom.intensity.Override(post.bloom);
                bloom.threshold.Override(post.bloomThreshold);
                bloom.scatter.Override(0.6f);
                bloom.highQualityFiltering.Override(false);
                bloom.downscale.Override(BloomDownscaleMode.Quarter);
                bloom.maxIterations.Override(4);
            }
            else if (profile.Has<Bloom>())
            {
                profile.Remove<Bloom>();
            }
        }

        /// <summary>True when the profile holds the guide's post (validator).</summary>
        public static bool PostMatches(VolumeProfile profile, WorldPost post)
        {
            if (profile == null || !profile.TryGet(out Tonemapping tonemapping) || tonemapping.mode.value != TonemappingMode.None
                || !profile.TryGet(out ColorAdjustments color) || !profile.TryGet(out Vignette vignette))
            {
                return false;
            }

            bool bloomOk = post.bloom > 0f
                ? profile.TryGet(out Bloom bloom) && bloom.active && Same(bloom.intensity.value, post.bloom) && Same(bloom.threshold.value, post.bloomThreshold)
                : !profile.Has<Bloom>();
            return bloomOk && Same(color.postExposure.value, post.exposure) && Same(color.contrast.value, post.contrast)
                && Same(color.saturation.value, post.saturation) && color.colorFilter.value == post.filter
                && Same(vignette.intensity.value, post.vignette) && vignette.color.value == post.vignetteColor;
        }

        private static bool Same(float a, float b) => Mathf.Abs(a - b) < 1e-4f;

        private static T Override<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
            {
                component = profile.Add<T>(true);
                // Sub-asset of the profile asset, as the volume editor would store it.
                if (AssetDatabase.Contains(profile))
                {
                    component.name = typeof(T).Name;
                    component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(component, profile);
                }
            }

            component.active = true;
            return component;
        }

        private static readonly int EmissionId = ToonShaderIds.Emission;
        private static readonly int FogAmountId = ToonShaderIds.FogAmount;

        /// <summary>
        /// Moves every material whose guide profile is a vinyl one onto <c>StudioKit/ToonVinyl</c> (same properties, keywords and
        /// instance data as ToonLit: the owners, the routes and GPU instancing stay as they are) and writes the profile's surface.
        /// Emission and fog amount stay as the material's art set them (the glows of the towers, the crown's light, the structures'
        /// lower fog): the surface brings gloss, highlight, rim, darkening and the blend. Run after every material exists (the
        /// scene builder's last step; the art builders recreate their materials on ToonLit and this moves them again).
        /// </summary>
        public static int ApplySurfaces(StyleGuide guide)
        {
            return ApplySurfacesTo(guide);
        }

        /// <summary>
        /// The shader an art builder should set on <paramref name="material"/> when it asks for <paramref name="wanted"/>: a
        /// material the guide already moved to the vinyl sibling keeps <c>StudioKit/ToonVinyl</c> when <c>StudioKit/ToonLit</c>
        /// is asked (same properties; RT-6v3.4a: the art builders that run outside the scene builder, such as the captures and
        /// the art tests, put the game's materials back on ToonLit and the repository ended up with them so).
        /// </summary>
        public static Shader KeepVinyl(Material material, Shader wanted)
        {
            return wanted != null && material != null && material.shader != null && wanted.name == ToonShaderIds.ToonLitShader
                && material.shader.name == ToonShaderIds.ToonVinylShader
                ? material.shader
                : wanted;
        }

        private static int ApplySurfacesTo(StyleGuide guide)
        {
            Shader vinyl = Shader.Find(ToonShaderIds.ToonVinylShader) ?? throw new System.InvalidOperationException("missing " + ToonShaderIds.ToonVinylShader);
            int moved = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { ClaybreakSceneBuilder.Root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MaterialStyle entry = guide.MaterialNamed(System.IO.Path.GetFileNameWithoutExtension(path));
                MaterialProfile profile = entry == null ? null : guide.ProfileOf(entry.profile);
                if (profile == null || !profile.vinyl)
                {
                    continue;
                }

                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material.shader != vinyl)
                {
                    if (!WorldScenery.IsToonFamily(material.shader.name))
                    {
                        throw new System.InvalidOperationException($"{path}: {material.shader.name} is not a toon shader; the vinyl profile needs a ToonLit material");
                    }

                    string[] keywords = material.shaderKeywords;
                    material.shader = vinyl;
                    material.shaderKeywords = keywords;
                }

                float emission = material.GetFloat(EmissionId);
                float fog = material.GetFloat(FogAmountId);
                profile.surface.ApplyTo(material);
                material.SetFloat(EmissionId, emission);
                material.SetFloat(FogAmountId, fog);
                EditorUtility.SetDirty(material);
                moved++;
            }

            return moved;
        }

        public static void WriteAndQuit()
        {
            BatchRunner.Run("StyleGuideContent", Write);
        }
    }
}
