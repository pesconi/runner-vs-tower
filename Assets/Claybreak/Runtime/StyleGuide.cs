using System;
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

    /// <summary>One world's palette, light and sky in the guide (RT-6v3.1: the values the game has now).</summary>
    [Serializable]
    public sealed class WorldStyle
    {
        public string name;
        public PaletteRole[] roles;
        public Vector3 sunEuler;
        public Color sunColor;
        public float sunIntensity;
        public Color ambientSky;
        public Color ambientGround;
        public float ambientIntensity;
        public float directIntensity;
        /// <summary>The post effects of the world, by name ("none" until the v3 art direction adds some).</summary>
        public string postProcessing;
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
        public StyleStatus status;
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

        public ClassStyle[] Classes => classes;
        public WorldStyle[] Worlds => worlds;
        /// <summary>Roles that are the same in every world (the gate colors).</summary>
        public PaletteRole[] SharedRoles => sharedRoles;
        public MaterialProfile[] Profiles => profiles;
        public MaterialStyle[] Materials => materials;
        public EffectStyle[] Effects => effects;
        public SoundStyle[] Sounds => sounds;
        public float ColorTolerance => colorTolerance;

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

        /// <summary>True when the class has migrated and answers to the v3 rules.</summary>
        public bool IsV3(StyleClass styleClass)
        {
            ClassStyle entry = ClassOf(styleClass);
            return entry != null && entry.status == StyleStatus.V3;
        }

#if UNITY_EDITOR
        /// <summary>Editor only (content and tests): replaces every table.</summary>
        public void EditorAssign(ClassStyle[] classes, WorldStyle[] worlds, PaletteRole[] sharedRoles, MaterialProfile[] profiles,
            MaterialStyle[] materials, EffectStyle[] effects, SoundStyle[] sounds, float colorTolerance)
        {
            this.classes = classes;
            this.worlds = worlds;
            this.sharedRoles = sharedRoles;
            this.profiles = profiles;
            this.materials = materials;
            this.effects = effects;
            this.sounds = sounds;
            this.colorTolerance = colorTolerance;
        }
#endif
    }
}
