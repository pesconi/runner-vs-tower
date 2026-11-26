using System;
using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak
{
    /// <summary>The kinds of thing the game draws or plays, each with its own migration state in the <see cref="StyleGuide"/> (RT-6v3.1).</summary>
    public enum StyleClass
    {
        Unit,
        Leader,
        Enemy,
        Tower,
        Boss,
        Gate,
        Obstacle,
        Floor,
        Scenery,
        Effect,
        Sound,
        Screen
    }

    /// <summary>Where a class stands on the v3 art direction: <c>Legacy</c> is the look before it, <c>V3</c> is under every v3 rule.</summary>
    public enum StyleStatus
    {
        Legacy,
        V3
    }

    /// <summary>
    /// Every effect the game plays on screen (RT-6v3.1, <c>docs/direcao-visual-v3.md</c> 2.1). A new effect is a new value here, and
    /// the style validator refuses the guide until it has an <see cref="EffectStyle"/> entry for it.
    /// </summary>
    public enum StyleEffect
    {
        /// <summary>The number that floats up from a gate (<see cref="FloatingNumbers"/>).</summary>
        FloatingNumber,
        /// <summary>The pop of the army counter in the HUD when the crowd changes (<see cref="ClaybreakHud.CounterPop"/>).</summary>
        CounterPop,
        /// <summary>Units thrown out by a bad gate, a hit or a defense: the ballistic death of the instanced swarms.</summary>
        UnitDeath,
        /// <summary>The run camera's shake.</summary>
        CameraShake,
        /// <summary>The landing shadow and the stone of the cannon.</summary>
        ProjectileImpact,
        /// <summary>A tower floor crumbling and the tower falling.</summary>
        TowerBreak
    }

    /// <summary>One color in the guide's three levels: the base color, its shade and its highlight (valid before the post effects).</summary>
    [Serializable]
    public struct ColorLevels
    {
        public Color baseColor;
        public Color shadow;
        public Color highlight;

        public ColorLevels(Color baseColor, Color shadow, Color highlight)
        {
            this.baseColor = baseColor;
            this.shadow = shadow;
            this.highlight = highlight;
        }

        /// <summary>The three levels in order.</summary>
        public Color this[int level] => level == 0 ? baseColor : level == 1 ? shadow : highlight;

        public const int Count = 3;
    }

    /// <summary>A palette role: the name is a <c>ToonPalette.ColorNames</c> entry or a shared gate role, the class says who wears it.</summary>
    [Serializable]
    public sealed class PaletteRole
    {
        public string role;
        public StyleClass styleClass;
        public ColorLevels levels;
    }

    /// <summary>
    /// The light of a world on the vinyl shader (RT-6v3.3): the values of a <c>ToonVinylLook</c> (kit), written into the world's
    /// look asset by the content. Warm key, cool two-tone ambient, rim, soft shoulder (D36).
    /// </summary>
    [Serializable]
    public struct WorldLight
    {
        public float softness;
        public float lambert;
        public Color shadowTint;
        public Color keyColor;
        public float keyIntensity;
        public float directIntensity;
        public Color skyAmbient;
        public Color groundAmbient;
        public float ambientIntensity;
        public Color rimColor;
        public float rimStrength;
        public float rimWidth;
        public float shoulderKnee;
        public float highlightPeak;
    }

    /// <summary>
    /// The cheap post stack of a world (RT-6v3.3, docs 2.1): color adjustments, a light vignette and a light bloom over the
    /// highlights only. No tonemapping: the palette is authored as screen values (T-RT6v3.2-3). The UI is never under it
    /// (screen-space overlay canvas).
    /// </summary>
    [Serializable]
    public struct WorldPost
    {
        public float exposure;
        public float contrast;
        public float saturation;
        public Color filter;
        public float vignette;
        public Color vignetteColor;
        /// <summary>Bloom intensity; 0 = no bloom.</summary>
        public float bloom;
        public float bloomThreshold;
    }

    /// <summary>One world's palette, light, sky, fog and post in the guide.</summary>
    [Serializable]
    public sealed class WorldStyle
    {
        public string name;
        public PaletteRole[] roles;
        /// <summary>Where the sun stands (euler of the directional light), the same in every world so far.</summary>
        public Vector3 sunEuler;
        public WorldLight light;
        public WorldPost post;
        /// <summary>Linear fog range (m) in the palette's fog color: colored depth, not a white veil (D36).</summary>
        public float fogStart;
        public float fogEnd;
        /// <summary>One line on the look of the world (checklist).</summary>
        public string mood;
    }

    /// <summary>
    /// D36 contrast logic as measured rules (RT-6v3.3): the crowd is the most saturated thing on screen, the track is neutral and
    /// far from the crowd in value, the enemies sit on the other side of the hue wheel. CIELAB: chroma = sqrt(a²+b²), value =
    /// L*, hue = atan2(b, a). Checked on the palette (before the post) and on the captures of the four worlds (after it).
    /// </summary>
    [Serializable]
    public sealed class ContrastRule
    {
        [Header("Palette, before the post")]
        [Tooltip("The crowd's chroma is at least this much above every other drawn color of its world.")]
        public float crowdChromaMargin;
        [Tooltip("Most chroma of the track and its stripe (neutral).")]
        public float trackMaxChroma;
        [Tooltip("Least |L*| between the crowd and the track.")]
        public float crowdTrackMinLightness;
        [Tooltip("Least hue angle (degrees) between the crowd and the defenses.")]
        public float enemyMinHue;
        [Header("Captured, after the post")]
        public float capturedCrowdChromaMargin;
        public float capturedCrowdTrackMinLightness;
        public float capturedEnemyMinHue;
        [Tooltip("Least contrast ratio (WCAG) inside a gate number: its fill against its outline.")]
        public float gateLabelMinRatio;
        [Tooltip("Least ΔE (CIE76) between any skin on screen and the track on screen.")]
        public float skinMinDeltaE;
    }

    /// <summary>
    /// A family of materials: the shaders it may use. A <c>legacy</c> profile describes what the game draws with today; the v3
    /// profiles (vinyl, matte plastic, gate glow, enemy, UI/effect) are listed with no shader until the sibling shader exists.
    /// </summary>
    [Serializable]
    public sealed class MaterialProfile
    {
        public string id;
        public bool legacy;
        /// <summary>Shader names the profile accepts; a name ending in "/" accepts every shader under that folder.</summary>
        public string[] shaders;
        public string note;
        /// <summary>True for a profile drawn by the kit's vinyl shader; <see cref="surface"/> is then written into its materials.</summary>
        public bool vinyl;
        public ToonVinylSurface surface;

        public bool Accepts(string shaderName)
        {
            if (shaders == null || string.IsNullOrEmpty(shaderName))
            {
                return false;
            }

            foreach (string accepted in shaders)
            {
                if (accepted == shaderName || (accepted.EndsWith("/") && shaderName.StartsWith(accepted, StringComparison.Ordinal)))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>A material asset of the game, the class that owns it and the profile it follows.</summary>
    [Serializable]
    public sealed class MaterialStyle
    {
        public string material;
        public StyleClass styleClass;
        public string profile;
    }

    /// <summary>The limits of a class (the budgets of <c>docs/direcao-visual-v3.md</c> 4.1) and its migration state.</summary>
    [Serializable]
    public sealed class ClassStyle
    {
        public StyleClass styleClass;
        /// <summary>Shape state: the meshes (triangle ranges, bevels, draw calls) answer to the v3 rules.</summary>
        public StyleStatus status;
        /// <summary>
        /// Surface state (RT-6v3.3): materials on a v3 profile, palette within its three levels, light and post from the guide.
        /// A class can be v3 on the surface while its meshes are still the old ones (<see cref="status"/>).
        /// </summary>
        public StyleStatus surface;
        /// <summary>
        /// Most triangles of one mesh (one object for scene-built classes), always checked; 0 = no meshes in this class. While the
        /// class is legacy this is the ceiling of what the game draws today; the v3 budget is <see cref="v3MaxTriangles"/>.
        /// </summary>
        public int maxTriangles;
        /// <summary>Most draw calls (renderers) of one object of the class, always checked; legacy ceiling like <see cref="maxTriangles"/>.</summary>
        public int maxDrawCalls;
        /// <summary>The v3 triangle range of a mesh (docs 4.1; the v3 look is not made of boxes), checked only when the class is v3.</summary>
        public int v3MinTriangles;
        public int v3MaxTriangles;
        /// <summary>The v3 draw-call budget of one object, checked only when the class is v3.</summary>
        public int v3MaxDrawCalls;
        /// <summary>Largest angle (degrees) between two neighbouring faces of a v3 mesh; a sharper edge needs a bevel. Only for v3 classes.</summary>
        public float maxEdgeAngle;
    }

    /// <summary>The limit of an effect: how many of it may be alive at once.</summary>
    [Serializable]
    public sealed class EffectStyle
    {
        public StyleEffect effect;
        public int maxActive;
        public string note;
    }

    /// <summary>A clip of the game and the event that plays it, with the loudness target of the v3 sound.</summary>
    [Serializable]
    public sealed class SoundStyle
    {
        public string clip;
        public string playedBy;
        /// <summary>Target peak (dBFS) of the clip and how far it may be from it; checked only when the Sound class is v3.</summary>
        public float peakDb;
        public float peakToleranceDb;
        /// <summary>Target approximate volume (RMS of the samples above -40 dBFS, in dBFS) and its tolerance; v3 only.</summary>
        public float loudnessDb;
        public float loudnessToleranceDb;
        /// <summary>Allowed duration of the clip in seconds; v3 only.</summary>
        public float minSeconds;
        public float maxSeconds;
    }

    /// <summary>
    /// The art direction as data (RT-6v3.1, <c>docs/direcao-visual-v3.md</c> section 2): one source for palettes per world (three
    /// levels per role), the material families, the shape and budget limits per class, light and sky per world, the effects, the
    /// sounds, and which classes already follow the v3 direction. It describes the game as it is; the art series flips a class
    /// to <see cref="StyleStatus.V3"/> as it migrates. Written by <c>Claybreak.Editor.StyleGuideContent</c>, checked by
    /// <c>Claybreak.Editor.StyleGuideValidation</c>. Content lives in this game (the studio kit never names a game or theme).
    /// </summary>
    public sealed class StyleGuide : ScriptableObject
    {
        public const string AssetPath = "Assets/Claybreak/Content/Style/StyleGuide.asset";

        [SerializeField] private ClassStyle[] classes = Array.Empty<ClassStyle>();
        [SerializeField] private WorldStyle[] worlds = Array.Empty<WorldStyle>();
        [SerializeField] private PaletteRole[] sharedRoles = Array.Empty<PaletteRole>();
        [SerializeField] private MaterialProfile[] profiles = Array.Empty<MaterialProfile>();
        [SerializeField] private MaterialStyle[] materials = Array.Empty<MaterialStyle>();
        [SerializeField] private EffectStyle[] effects = Array.Empty<EffectStyle>();
        [SerializeField] private SoundStyle[] sounds = Array.Empty<SoundStyle>();
        /// <summary>CIE76 distance a v3 color may be from the nearest of its three levels (docs 2.2).</summary>
        [SerializeField] private float colorTolerance = 3f;
        [SerializeField] private ContrastRule contrast = new ContrastRule();

        public ClassStyle[] Classes => classes;
        public WorldStyle[] Worlds => worlds;
        /// <summary>Roles that are the same in every world (the gate colors).</summary>
        public PaletteRole[] SharedRoles => sharedRoles;
        public MaterialProfile[] Profiles => profiles;
        public MaterialStyle[] Materials => materials;
        public EffectStyle[] Effects => effects;
        public SoundStyle[] Sounds => sounds;
        public float ColorTolerance => colorTolerance;
        public ContrastRule Contrast => contrast;

        public ClassStyle ClassOf(StyleClass styleClass)
        {
            foreach (ClassStyle each in classes)
            {
                if (each.styleClass == styleClass)
                {
                    return each;
                }
            }

            return null;
        }

        public MaterialProfile ProfileOf(string id)
        {
            foreach (MaterialProfile each in profiles)
            {
                if (each.id == id)
                {
                    return each;
                }
            }

            return null;
        }

        public MaterialStyle MaterialNamed(string name)
        {
            foreach (MaterialStyle each in materials)
            {
                if (each.material == name)
                {
                    return each;
                }
            }

            return null;
        }

        public EffectStyle EffectOf(StyleEffect effect)
        {
            foreach (EffectStyle each in effects)
            {
                if (each.effect == effect)
                {
                    return each;
                }
            }

            return null;
        }

        public SoundStyle SoundOf(string clip)
        {
            foreach (SoundStyle each in sounds)
            {
                if (each.clip == clip)
                {
                    return each;
                }
            }

            return null;
        }

        /// <summary>
        /// A short hash of everything that sets what the screen looks like (worlds: palettes, light, post, fog; material
        /// profiles and their surfaces). The captured contrast measures carry the one they were taken with.
        /// </summary>
        public string LookFingerprint()
        {
            var text = new System.Text.StringBuilder();
            foreach (WorldStyle world in worlds)
            {
                text.Append(JsonUtility.ToJson(world));
            }

            foreach (MaterialProfile profile in profiles)
            {
                text.Append(JsonUtility.ToJson(profile));
            }

            ulong hash = 14695981039346656037UL;
            foreach (char c in text.ToString())
            {
                hash = (hash ^ c) * 1099511628211UL;
            }

            return hash.ToString("x16");
        }

        /// <summary>True when the class's surface (materials, palette levels, light) has migrated to v3.</summary>
        public bool SurfaceIsV3(StyleClass styleClass)
        {
            ClassStyle entry = ClassOf(styleClass);
            return entry != null && (entry.surface == StyleStatus.V3 || entry.status == StyleStatus.V3);
        }

        /// <summary>True when the class has migrated and answers to the v3 rules.</summary>
        public bool IsV3(StyleClass styleClass)
        {
            ClassStyle entry = ClassOf(styleClass);
            return entry != null && entry.status == StyleStatus.V3;
        }

#if UNITY_EDITOR
        /// <summary>Editor only (content and tests): replaces every table.</summary>
        public void EditorAssign(ClassStyle[] classes, WorldStyle[] worlds, PaletteRole[] sharedRoles, MaterialProfile[] profiles,
            MaterialStyle[] materials, EffectStyle[] effects, SoundStyle[] sounds, float colorTolerance, ContrastRule contrast)
        {
            this.classes = classes;
            this.worlds = worlds;
            this.sharedRoles = sharedRoles;
            this.profiles = profiles;
            this.materials = materials;
            this.effects = effects;
            this.sounds = sounds;
            this.colorTolerance = colorTolerance;
            this.contrast = contrast;
        }
#endif
    }
}
