using UnityEngine;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// The lighting look of the toon shaders, as data: the light ramp (hard bands of the direct light, from the
    /// shadowed side to the lit side) and the two-tone ambient (sky color from above, ground color from below).
    /// Applied as global shader values by <see cref="ToonLookApplier"/>; every toon material in the scene reads them.
    /// </summary>
    [CreateAssetMenu(menuName = "StudioKit/Toon Look", fileName = "ToonLook")]
    public sealed class ToonLook : ScriptableObject
    {
        public const int MaximumBands = 8;

        [Tooltip("Direct light level of each band, from the shadowed side (N·L = -1) to the lit side (N·L = 1).")]
        [SerializeField] private float[] rampLevels = { 110f / 255f, 185f / 255f, 1f };
        [SerializeField] private Color skyLight = Color.white;
        [SerializeField] private Color groundLight = new Color32(0xbb, 0xaa, 0xcc, 0xff);
        [Min(0f)] [SerializeField] private float ambientIntensity = 0.55f;
        [Min(0f)] [SerializeField] private float directIntensity = 0.75f;

        public float[] RampLevels => rampLevels;
        public Color SkyLight => skyLight;
        public Color GroundLight => groundLight;
        public float AmbientIntensity => ambientIntensity;
        public float DirectIntensity => directIntensity;

        /// <summary>Null when the look is usable, otherwise what is wrong with it.</summary>
        public string Validate()
        {
            if (rampLevels == null || rampLevels.Length < 2 || rampLevels.Length > MaximumBands)
            {
                return $"the ramp needs 2 to {MaximumBands} levels";
            }

            for (int i = 0; i < rampLevels.Length; i++)
            {
                if (rampLevels[i] < 0f || rampLevels[i] > 1f || (i > 0 && rampLevels[i] < rampLevels[i - 1]))
                {
                    return "ramp levels must be within 0..1 and never decrease";
                }
            }

            return null;
        }

        /// <summary>
        /// A ramp texture of one texel per band: linear data (not sRGB), point filtered and clamped, so the bands
        /// stay hard. The caller owns the texture.
        /// </summary>
        public static Texture2D CreateRampTexture(float[] levels)
        {
            var texture = new Texture2D(levels.Length, 1, TextureFormat.R8, false, true)
            {
                name = "ToonRamp",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
            var texels = new byte[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                texels[i] = (byte)Mathf.RoundToInt(Mathf.Clamp01(levels[i]) * 255f);
            }

            texture.SetPixelData(texels, 0);
            texture.Apply(false, true);
            return texture;
        }
    }
}
