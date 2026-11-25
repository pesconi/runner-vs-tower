using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Claybreak.Lab
{
    /// <summary>
    /// RT-6v3.2: one candidate of the "premium vinyl toy" direction (decision D36), as numbers: the lab shader's light
    /// (<c>Claybreak/VinylLab</c>: soft ramp, warm shadow, highlight, rim, how glossy the scenery is next to the character),
    /// the sun, the sky and the cheap post stack. One source for the concept screens (PlayMode captures in the editor) and
    /// for the phone rehearsal (the toon lab, <c>--ei vinyl N</c>): the editor builder turns <see cref="Post"/> into the
    /// volume profile assets the measurement player carries. Laboratory only: the chosen numbers move into the style guide
    /// and the kit's vinyl shader (K.v3a) after D36.
    /// </summary>
    [Serializable]
    public struct VinylDirection
    {
        public string id;
        public string label;
        // Shader light (globals of Claybreak/VinylLab).
        public float softness;
        public float lambert;
        public Color shadowTint;
        public float spec;
        public float specSize;
        public Color rimColor;
        public float rim;
        public float rimWidth;
        public float sceneGloss;
        public Color ambientTint;
        // Sun: where the light travels to (world), its color and intensity; sky and fog (null-like: alpha 0 keeps the world's).
        public Vector3 sunTravel;
        public Color sunColor;
        public float sunIntensity;
        public Color skyTop;
        public Color skyBottom;
        public Color fog;
        public PostSettings post;

        /// <summary>The cheap post stack of the style guide (2.1): neutral tonemapping, color, light vignette, bloom only if it fits.</summary>
        [Serializable]
        public struct PostSettings
        {
            public float exposure;
            public float contrast;
            public float saturation;
            public Color filter;
            public float vignette;
            /// <summary>Bloom intensity; 0 = no bloom override at all.</summary>
            public float bloom;
            public float bloomThreshold;
        }

        public static readonly int Softness = Shader.PropertyToID("_VinylSoftness");
        public static readonly int Lambert = Shader.PropertyToID("_VinylLambert");
        public static readonly int ShadowTint = Shader.PropertyToID("_VinylShadowTint");
        public static readonly int Spec = Shader.PropertyToID("_VinylSpec");
        public static readonly int SpecSize = Shader.PropertyToID("_VinylSpecSize");
        public static readonly int RimColor = Shader.PropertyToID("_VinylRimColor");
        public static readonly int RimWidth = Shader.PropertyToID("_VinylRimWidth");
        public static readonly int SceneGloss = Shader.PropertyToID("_VinylSceneGloss");
        public static readonly int AmbientTint = Shader.PropertyToID("_VinylAmbientTint");
        public static readonly int Hero = Shader.PropertyToID("_VinylHero");
        private static readonly int SkyTop = Shader.PropertyToID("_StudioKitSkyTop");
        private static readonly int SkyBottom = Shader.PropertyToID("_StudioKitSkyBottom");

        public const string ShaderName = "Claybreak/VinylLab";
        public const string ToonLitShaderName = "StudioKit/ToonLit";

        /// <summary>The candidates, in the order of the D36 card (index 0 = A; M = the reviewed mix). <c>--ei vinyl N</c> picks N - 1; 0 is ToonLit.</summary>
        public static readonly VinylDirection[] All =
        {
            new VinylDirection
            {
                id = "A", label = "Vinil brilhante e saturado",
                softness = 0.12f, lambert = 0.35f, shadowTint = new Color(0.66f, 0.52f, 0.66f),
                spec = 0.6f, specSize = 0.35f, rimColor = new Color(1f, 0.96f, 0.92f), rim = 0.45f, rimWidth = 3f,
                sceneGloss = 0.5f, ambientTint = Color.white,
                sunTravel = new Vector3(-6f, -12f, 4f), sunColor = new Color(1f, 0.97f, 0.93f), sunIntensity = 1.05f,
                skyTop = Color.clear, skyBottom = Color.clear, fog = Color.clear,
                post = new PostSettings
                {
                    exposure = 0.05f, contrast = 14f, saturation = 22f, filter = Color.white, vignette = 0.18f,
                    bloom = 0.5f, bloomThreshold = 1.1f,
                },
            },
            new VinylDirection
            {
                id = "B", label = "Pastel fosco, brilho só no personagem",
                softness = 0.3f, lambert = 0.5f, shadowTint = new Color(0.78f, 0.70f, 0.80f),
                spec = 0.5f, specSize = 0.5f, rimColor = new Color(1f, 0.98f, 0.96f), rim = 0.4f, rimWidth = 2.5f,
                sceneGloss = 0.04f, ambientTint = new Color(1.04f, 1.02f, 1.05f),
                sunTravel = new Vector3(-6f, -12f, 4f), sunColor = new Color(1f, 0.98f, 0.96f), sunIntensity = 0.95f,
                skyTop = Color.clear, skyBottom = Color.clear, fog = Color.clear,
                post = new PostSettings
                {
                    exposure = 0f, contrast = -4f, saturation = -10f, filter = Color.white, vignette = 0.1f,
                    bloom = 0f, bloomThreshold = 1f,
                },
            },
            new VinylDirection
            {
                id = "C", label = "Contraste alto, luz de fim de tarde",
                softness = 0.07f, lambert = 0.25f, shadowTint = new Color(0.44f, 0.38f, 0.66f),
                spec = 0.55f, specSize = 0.3f, rimColor = new Color(1f, 0.72f, 0.46f), rim = 0.65f, rimWidth = 2.5f,
                sceneGloss = 0.45f, ambientTint = new Color(0.86f, 0.82f, 1f),
                sunTravel = new Vector3(-9f, -5f, -5f), sunColor = new Color(1f, 0.80f, 0.58f), sunIntensity = 1.15f,
                skyTop = new Color(0.42f, 0.37f, 0.66f), skyBottom = new Color(1f, 0.70f, 0.48f), fog = new Color(0.98f, 0.74f, 0.6f),
                post = new PostSettings
                {
                    exposure = 0f, contrast = 26f, saturation = 10f, filter = new Color(1f, 0.96f, 0.92f), vignette = 0.28f,
                    bloom = 0.4f, bloomThreshold = 1.1f,
                },
            },
            // M: the mix the independent review (fable) recommended after A..C: the character's vinyl from A, the matte
            // scenery rule of B without its grey veil, more contrast between crowd and track (from C), a smaller highlight
            // so the belly does not burn out, bloom only on the brightest highlights.
            new VinylDirection
            {
                id = "M", label = "Mistura: vinil no personagem, cenário fosco",
                softness = 0.15f, lambert = 0.4f, shadowTint = new Color(0.70f, 0.56f, 0.70f),
                spec = 0.45f, specSize = 0.4f, rimColor = new Color(1f, 0.96f, 0.92f), rim = 0.45f, rimWidth = 3f,
                sceneGloss = 0.06f, ambientTint = Color.white,
                sunTravel = new Vector3(-6f, -12f, 4f), sunColor = new Color(1f, 0.97f, 0.93f), sunIntensity = 1f,
                skyTop = Color.clear, skyBottom = Color.clear, fog = Color.clear,
                post = new PostSettings
                {
                    exposure = 0f, contrast = 14f, saturation = 10f, filter = Color.white, vignette = 0.12f,
                    bloom = 0.35f, bloomThreshold = 1.3f,
                },
            },
        };

        /// <summary>
        /// Writes the shader globals, the sun and the sky of this direction. Materials still on ToonLit are untouched (see
        /// <see cref="SwitchMaterials"/>). The sky and fog are left as the world set them when the direction has none.
        /// </summary>
        public void ApplyLight(Light sun)
        {
            Shader.SetGlobalFloat(Softness, softness);
            Shader.SetGlobalFloat(Lambert, lambert);
            Shader.SetGlobalColor(ShadowTint, shadowTint);
            Shader.SetGlobalFloat(Spec, spec);
            Shader.SetGlobalFloat(SpecSize, specSize);
            Color rimValue = rimColor;
            rimValue.a = rim;
            Shader.SetGlobalColor(RimColor, rimValue);
            Shader.SetGlobalFloat(RimWidth, rimWidth);
            Shader.SetGlobalFloat(SceneGloss, sceneGloss);
            Shader.SetGlobalVector(AmbientTint, new Vector4(ambientTint.r, ambientTint.g, ambientTint.b, 1f));
            if (sun != null)
            {
                sun.transform.rotation = Quaternion.LookRotation(sunTravel.normalized);
                sun.color = sunColor;
                sun.intensity = sunIntensity;
            }

            if (skyTop.a > 0f)
            {
                // Raw sRGB, as ToonPaletteApplier writes them (the sky blends in sRGB).
                Shader.SetGlobalVector(SkyTop, new Vector4(skyTop.r, skyTop.g, skyTop.b, 1f));
                Shader.SetGlobalVector(SkyBottom, new Vector4(skyBottom.r, skyBottom.g, skyBottom.b, 1f));
            }

            if (fog.a > 0f)
            {
                RenderSettings.fogColor = fog;
            }
        }

        /// <summary>Fills <paramref name="profile"/> with this direction's post overrides (editor builder and tests).</summary>
        public void FillProfile(VolumeProfile profile)
        {
            // The palette is authored for display values (no tonemapping today); Neutral tonemapping greys the near-white
            // track and sky (seen in the first concept run), so the lab keeps tonemapping off and lets only the
            // highlight and rim, which go above 1 in HDR, reach the bloom.
            Tonemapping tonemapping = Override<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.None);
            ColorAdjustments color = Override<ColorAdjustments>(profile);
            color.postExposure.Override(post.exposure);
            color.contrast.Override(post.contrast);
            color.saturation.Override(post.saturation);
            color.colorFilter.Override(post.filter);
            Vignette vignetteOverride = Override<Vignette>(profile);
            vignetteOverride.intensity.Override(post.vignette);
            vignetteOverride.smoothness.Override(0.45f);
            vignetteOverride.color.Override(new Color(0.16f, 0.09f, 0.18f));
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
            else if (profile.TryGet(out Bloom unused))
            {
                profile.Remove<Bloom>();
            }
        }

        private static T Override<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
            {
                component = profile.Add<T>(true);
            }

            component.active = true;
            return component;
        }

        /// <summary>
        /// Moves every loaded material on <c>StudioKit/ToonLit</c> to <paramref name="vinyl"/> (same properties and keywords),
        /// marking the ones whose name is in <paramref name="heroes"/> as hero gloss. Returns what to give back to
        /// <see cref="RestoreMaterials"/> (the editor must not keep asset materials switched).
        /// </summary>
        public static (Material material, Shader shader)[] SwitchMaterials(Shader vinyl, params string[] heroes)
        {
            Material[] loaded = Resources.FindObjectsOfTypeAll<Material>();
            var switched = new System.Collections.Generic.List<(Material, Shader)>();
            foreach (Material material in loaded)
            {
                if (material == null || material.shader == null || material.shader.name != ToonLitShaderName)
                {
                    continue;
                }

                string[] keywords = material.shaderKeywords;
                Shader previous = material.shader;
                material.shader = vinyl;
                material.shaderKeywords = keywords;
                material.SetFloat(Hero, Array.IndexOf(heroes, material.name.Replace(" (Instance)", "")) >= 0 ? 1f : 0f);
                switched.Add((material, previous));
            }

            return switched.ToArray();
        }

        public static void RestoreMaterials((Material material, Shader shader)[] switched)
        {
            foreach ((Material material, Shader shader) in switched)
            {
                if (material != null)
                {
                    string[] keywords = material.shaderKeywords;
                    material.shader = shader;
                    material.shaderKeywords = keywords;
                }
            }
        }

        /// <summary>Turns the camera's post on or off (URP additional camera data).</summary>
        public static void SetPost(Camera camera, bool on)
        {
            if (camera != null)
            {
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = on;
            }
        }
    }
}
