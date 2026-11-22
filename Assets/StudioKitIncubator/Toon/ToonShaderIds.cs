using UnityEngine;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>Shader names, keywords and property ids shared by the toon shaders and their C# owners.</summary>
    public static class ToonShaderIds
    {
        public const string ToonLitShader = "StudioKit/ToonLit";
        public const string SkyGradientShader = "StudioKit/SkyGradientScreen";

        /// <summary>Local keyword of <c>StudioKit/ToonLit</c>: read the transform from <see cref="Instances"/>.</summary>
        public const string InstancesKeyword = "STUDIOKIT_TOON_INSTANCES";

        public static readonly int Instances = Shader.PropertyToID("_ToonInstances");
        public static readonly int TintColor = Shader.PropertyToID("_TintColor");
        public static readonly int Ramp = Shader.PropertyToID("_StudioKitToonRamp");
        public static readonly int SkyLight = Shader.PropertyToID("_StudioKitToonSky");
        public static readonly int GroundLight = Shader.PropertyToID("_StudioKitToonGround");
        public static readonly int AmbientIntensity = Shader.PropertyToID("_StudioKitToonAmbient");
        public static readonly int DirectIntensity = Shader.PropertyToID("_StudioKitToonDirect");

        /// <summary>Sky color at the top of the screen, raw sRGB (written by <see cref="ToonPaletteApplier"/>).</summary>
        public static readonly int SkyTop = Shader.PropertyToID("_StudioKitSkyTop");

        /// <summary>Sky color at the bottom of the screen, raw sRGB.</summary>
        public static readonly int SkyBottom = Shader.PropertyToID("_StudioKitSkyBottom");
    }
}
