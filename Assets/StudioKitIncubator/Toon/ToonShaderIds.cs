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

        /// <summary>
        /// Local keyword of <c>StudioKit/ToonLit</c>: the vertex color RGB weigh the three palette slots
        /// <see cref="Slot0"/>..<see cref="Slot2"/> instead of being the color (one mesh, any palette).
        /// </summary>
        public const string SlotsKeyword = "STUDIOKIT_TOON_SLOTS";

        public const string StripedTrackShader = "StudioKit/StripedTrack";

        /// <summary>Slot colors of the slots variant: vertex red, green and blue.</summary>
        public static readonly int Slot0 = Shader.PropertyToID("_ToonSlot0");
        public static readonly int Slot1 = Shader.PropertyToID("_ToonSlot1");
        public static readonly int Slot2 = Shader.PropertyToID("_ToonSlot2");

        /// <summary>The two band colors and the period (m) of <c>StudioKit/StripedTrack</c>.</summary>
        public static readonly int TrackColor = Shader.PropertyToID("_TrackColor");
        public static readonly int StripeColor = Shader.PropertyToID("_StripeColor");
        public static readonly int StripePeriod = Shader.PropertyToID("_StripePeriod");

        public static readonly int Instances = Shader.PropertyToID("_ToonInstances");
        public static readonly int TintColor = Shader.PropertyToID("_TintColor");
        public static readonly int Ramp = Shader.PropertyToID("_StudioKitToonRamp");
        public static readonly int SkyLight = Shader.PropertyToID("_StudioKitToonSky");
        public static readonly int GroundLight = Shader.PropertyToID("_StudioKitToonGround");
        public static readonly int AmbientIntensity = Shader.PropertyToID("_StudioKitToonAmbient");
        public static readonly int DirectIntensity = Shader.PropertyToID("_StudioKitToonDirect");

        /// <summary>
        /// A color as a raw shader vector in the project's working space (linear when the project is linear), for
        /// <c>SetVector</c> on values the shaders use as they are (slot colors, track colors).
        /// </summary>
        public static Vector4 ShaderColor(Color color)
        {
            return QualitySettings.activeColorSpace == ColorSpace.Linear ? (Vector4)color.linear : (Vector4)color;
        }

        /// <summary>Sky color at the top of the screen, raw sRGB (written by <see cref="ToonPaletteApplier"/>).</summary>
        public static readonly int SkyTop = Shader.PropertyToID("_StudioKitSkyTop");

        /// <summary>Sky color at the bottom of the screen, raw sRGB.</summary>
        public static readonly int SkyBottom = Shader.PropertyToID("_StudioKitSkyBottom");
    }
}
