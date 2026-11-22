using UnityEngine;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// Puts a <see cref="ToonPalette"/> on screen: the sky gradient (global values read by <c>StudioKit/SkyGradientScreen</c>,
    /// drawn as the skybox) and the linear fog of the render settings, which every fogged shader reads. Applying is a
    /// handful of global writes (world or menu time, not per frame); nothing is created. Cameras that should show the sky
    /// clear to the skybox.
    /// </summary>
    public sealed class ToonPaletteApplier : MonoBehaviour
    {
        [Tooltip("Applied when the component is enabled; Apply switches it later.")]
        [SerializeField] private ToonPalette palette;
        [Tooltip("A material with the StudioKit/SkyGradientScreen shader; it becomes the skybox.")]
        [SerializeField] private Material skyMaterial;

        private ToonPalette applied;

        /// <summary>The palette on screen now (null before the first apply).</summary>
        public ToonPalette Applied => applied;

        public Material SkyMaterial => skyMaterial;

        /// <summary>Null when the wiring is usable, otherwise what is wrong with it.</summary>
        public string Validate()
        {
            if (skyMaterial == null || skyMaterial.shader == null || skyMaterial.shader.name != ToonShaderIds.SkyGradientShader)
            {
                return $"the applier needs a sky material with the {ToonShaderIds.SkyGradientShader} shader";
            }

            return palette == null ? null : palette.Validate();
        }

        private void OnEnable()
        {
            if (palette != null)
            {
                Apply(palette);
            }
        }

        /// <summary>Shows <paramref name="next"/>; false (and nothing changes) when it is missing or invalid.</summary>
        public bool Apply(ToonPalette next)
        {
            string error = next == null ? "no palette" : next.Validate();
            if (error == null && skyMaterial == null)
            {
                error = "no sky material";
            }

            if (error != null)
            {
                Debug.LogError("[ToonPalette] " + error, this);
                return false;
            }

            ApplyGlobally(next, skyMaterial);
            applied = next;
            return true;
        }

        /// <summary>
        /// Writes <paramref name="palette"/> into the render settings and the global shader values, with
        /// <paramref name="sky"/> as the skybox (left as it is when null). For tools that dress a scene without the
        /// component (captures); the caller has validated the palette.
        /// </summary>
        public static void ApplyGlobally(ToonPalette palette, Material sky)
        {
            // The sky blends in sRGB like the reference (a canvas gradient), so it gets the raw sRGB values, not the
            // linear ones SetGlobalColor would convert to.
            Color top = palette.SkyTop;
            Color bottom = palette.SkyBottom;
            Shader.SetGlobalVector(ToonShaderIds.SkyTop, new Vector4(top.r, top.g, top.b, 1f));
            Shader.SetGlobalVector(ToonShaderIds.SkyBottom, new Vector4(bottom.r, bottom.g, bottom.b, 1f));
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = palette.Fog;
            RenderSettings.fogStartDistance = palette.FogStart;
            RenderSettings.fogEndDistance = palette.FogEnd;
            if (sky != null)
            {
                RenderSettings.skybox = sky;
            }
        }
    }
}
