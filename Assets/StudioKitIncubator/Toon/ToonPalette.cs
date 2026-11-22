using UnityEngine;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// The colors of one world of a toon game, as data: the screen-space sky gradient, the linear fog, the ground and
    /// the track, the crowd's default body color, the props, the UI accent and ink, and the colors of the structures to attack and their
    /// defenses.
    /// <see cref="ToonPaletteApplier"/> writes it into the render settings and the global shader values; switching worlds
    /// is switching the asset, nothing is rebuilt. Every color is opaque (alpha 1).
    /// </summary>
    [CreateAssetMenu(menuName = "StudioKit/Toon Palette", fileName = "ToonPalette")]
    public sealed class ToonPalette : ScriptableObject
    {
        [Header("Sky and fog")]
        [Tooltip("Sky color at the top of the screen.")]
        [SerializeField] private Color skyTop = Color.white;
        [Tooltip("Sky color at the bottom of the screen.")]
        [SerializeField] private Color skyBottom = Color.white;
        [Tooltip("Fog color (the horizon); linear fog from start to end.")]
        [SerializeField] private Color fog = Color.white;
        [Min(0f)] [SerializeField] private float fogStart = 22f;
        [Min(0f)] [SerializeField] private float fogEnd = 75f;

        [Header("Ground")]
        [Tooltip("The fields on both sides of the track.")]
        [SerializeField] private Color ground = Color.white;
        [SerializeField] private Color track = Color.white;
        [Tooltip("The track's second stripe color (and its side rails).")]
        [SerializeField] private Color trackStripe = Color.white;

        [Header("Crowd and props")]
        [Tooltip("The crowd's body color when it wears no color of its own.")]
        [SerializeField] private Color crowd = Color.white;
        [SerializeField] private Color foliageDark = Color.white;
        [SerializeField] private Color foliageLight = Color.white;
        [SerializeField] private Color trunk = Color.white;
        [SerializeField] private Color rock = Color.white;

        [Header("UI")]
        [Tooltip("Buttons and highlights.")]
        [SerializeField] private Color uiAccent = Color.white;
        [Tooltip("Text and outlines drawn on the accent and the sky.")]
        [SerializeField] private Color uiInk = Color.black;

        [Header("Structures and defenses")]
        [Tooltip("Light side of the structures the crowd attacks.")]
        [SerializeField] private Color structureLight = Color.white;
        [SerializeField] private Color structureDark = Color.white;
        [Tooltip("Trims, windows and banners of the structures.")]
        [SerializeField] private Color structureDetail = Color.white;
        [SerializeField] private Color defense = Color.white;

        public Color SkyTop => skyTop;
        public Color SkyBottom => skyBottom;
        public Color Fog => fog;
        public float FogStart => fogStart;
        public float FogEnd => fogEnd;
        public Color Ground => ground;
        public Color Track => track;
        public Color TrackStripe => trackStripe;
        public Color Crowd => crowd;
        public Color FoliageDark => foliageDark;
        public Color FoliageLight => foliageLight;
        public Color Trunk => trunk;
        public Color Rock => rock;
        public Color UiAccent => uiAccent;
        public Color UiInk => uiInk;
        public Color StructureLight => structureLight;
        public Color StructureDark => structureDark;
        public Color StructureDetail => structureDetail;
        public Color Defense => defense;

        /// <summary>Null when the palette is usable, otherwise what is wrong with it.</summary>
        public string Validate()
        {
            if (fogStart < 0f || fogEnd <= fogStart)
            {
                return $"the fog must end after it starts (start {fogStart}, end {fogEnd})";
            }

            // Field by field (no arrays): it runs on every world switch.
            return Opaque(skyTop, nameof(skyTop)) ?? Opaque(skyBottom, nameof(skyBottom)) ?? Opaque(fog, nameof(fog))
                ?? Opaque(ground, nameof(ground)) ?? Opaque(track, nameof(track)) ?? Opaque(trackStripe, nameof(trackStripe))
                ?? Opaque(crowd, nameof(crowd)) ?? Opaque(foliageDark, nameof(foliageDark)) ?? Opaque(foliageLight, nameof(foliageLight))
                ?? Opaque(trunk, nameof(trunk)) ?? Opaque(rock, nameof(rock)) ?? Opaque(uiAccent, nameof(uiAccent))
                ?? Opaque(uiInk, nameof(uiInk)) ?? Opaque(structureLight, nameof(structureLight))
                ?? Opaque(structureDark, nameof(structureDark)) ?? Opaque(structureDetail, nameof(structureDetail))
                ?? Opaque(defense, nameof(defense));
        }

        private static string Opaque(Color color, string name)
        {
            return Mathf.Approximately(color.a, 1f) ? null : name + " must be opaque (alpha 1)";
        }

        /// <summary>The field names of <see cref="Colors"/>, in the same order.</summary>
        public static string[] ColorNames => new[]
        {
            nameof(skyTop), nameof(skyBottom), nameof(fog), nameof(ground), nameof(track), nameof(trackStripe), nameof(crowd),
            nameof(foliageDark), nameof(foliageLight), nameof(trunk), nameof(rock), nameof(uiAccent), nameof(uiInk),
            nameof(structureLight), nameof(structureDark), nameof(structureDetail), nameof(defense),
        };

        /// <summary>Every color of the palette (a new array; for tools and checks, not per frame).</summary>
        public Color[] Colors => new[]
        {
            skyTop, skyBottom, fog, ground, track, trackStripe, crowd, foliageDark, foliageLight, trunk, rock, uiAccent, uiInk,
            structureLight, structureDark, structureDetail, defense,
        };

#if UNITY_EDITOR
        /// <summary>Editor only (content and tests): every color in <see cref="ColorNames"/> order, then the fog range.</summary>
        public void EditorAssign(Color[] colors, float start, float end)
        {
            if (colors == null || colors.Length != ColorNames.Length)
            {
                throw new System.ArgumentException($"a palette has {ColorNames.Length} colors", nameof(colors));
            }

            skyTop = colors[0];
            skyBottom = colors[1];
            fog = colors[2];
            ground = colors[3];
            track = colors[4];
            trackStripe = colors[5];
            crowd = colors[6];
            foliageDark = colors[7];
            foliageLight = colors[8];
            trunk = colors[9];
            rock = colors[10];
            uiAccent = colors[11];
            uiInk = colors[12];
            structureLight = colors[13];
            structureDark = colors[14];
            structureDetail = colors[15];
            defense = colors[16];
            fogStart = start;
            fogEnd = end;
        }
#endif
    }
}
