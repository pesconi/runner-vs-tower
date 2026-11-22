using UnityEngine;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// How far apart two sRGB colors look, for palette checks (tools and tests, not per frame): the WCAG contrast ratio of
    /// their luminances (1 = same brightness, 21 = black on white) and the CIE76 color difference ΔE in CIELAB (D65;
    /// about 2 is barely visible, under ~20 the two read as "the same color, a bit different").
    /// </summary>
    public static class ToonContrast
    {
        /// <summary>Below this ΔE two colors that must be told apart at a glance (a crowd on its track) are too close.</summary>
        public const float DistinctDeltaE = 20f;

        /// <summary>WCAG minimum for large text: text color on its background must reach it.</summary>
        public const float LargeTextRatio = 3f;

        /// <summary>WCAG relative luminance (0 black .. 1 white) of an sRGB color.</summary>
        public static float Luminance(Color color)
        {
            return 0.2126f * ToLinear(color.r) + 0.7152f * ToLinear(color.g) + 0.0722f * ToLinear(color.b);
        }

        /// <summary>WCAG contrast ratio, 1..21, whatever the order.</summary>
        public static float Ratio(Color a, Color b)
        {
            float la = Luminance(a);
            float lb = Luminance(b);
            return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
        }

        /// <summary>CIE76 ΔE: the distance between the two colors in CIELAB (D65 white).</summary>
        public static float DeltaE(Color a, Color b)
        {
            Vector3 la = ToLab(a);
            Vector3 lb = ToLab(b);
            return Vector3.Distance(la, lb);
        }

        /// <summary>CIELAB (L*, a*, b*) of an sRGB color, D65 white.</summary>
        public static Vector3 ToLab(Color color)
        {
            float r = ToLinear(color.r);
            float g = ToLinear(color.g);
            float b = ToLinear(color.b);
            float x = (0.4124f * r + 0.3576f * g + 0.1805f * b) / 0.95047f;
            float y = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            float z = (0.0193f * r + 0.1192f * g + 0.9505f * b) / 1.08883f;
            float fx = LabCurve(x);
            float fy = LabCurve(y);
            float fz = LabCurve(z);
            return new Vector3(116f * fy - 16f, 500f * (fx - fy), 200f * (fy - fz));
        }

        private static float ToLinear(float channel)
        {
            return channel <= 0.04045f ? channel / 12.92f : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
        }

        private static float LabCurve(float t)
        {
            return t > 0.008856f ? Mathf.Pow(t, 1f / 3f) : 7.787f * t + 16f / 116f;
        }
    }
}
