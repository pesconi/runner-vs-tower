using System;
using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The CIELAB measures of the D36 contrast rule (RT-6v3.3, <see cref="ContrastRule"/>): value (L*), chroma and hue of a
    /// color, the hue angle between two colors, and the record the capture of the four worlds writes and the style validator
    /// reads (<see cref="ContrastMeasures"/>). Pure; shared by the editor validator and the PlayMode capture.
    /// </summary>
    public static class StyleContrast
    {
        /// <summary>Where the capture writes the measures and the validator reads them (repository relative).</summary>
        public const string MeasuresPath = "docs/art/v3/contraste.json";

        /// <summary>The roles the capture measures, each from its own objects on screen.</summary>
        public const string Crowd = "crowd";
        public const string Track = "track";
        public const string Ground = "ground";
        public const string Props = "props";
        /// <summary>The defenses (cannon, wall, sweeper, shield): the palette's defense color, the hue rule's enemy.</summary>
        public const string Enemy = "enemy";
        /// <summary>The run's obstacles: defense-colored bodies with fixed details (wood, steel, danger stripe) until RT-6v3.6.</summary>
        public const string Obstacle = "obstacle";

        /// <summary>Fewest pixels of a role for its hue to count (a sliver of a far defense says nothing).</summary>
        public const int MinimumHuePixels = 500;
        public const string Tower = "tower";
        public const string Sky = "sky";

        public static float Lightness(Color color) => ToonContrast.ToLab(color).x;

        public static float Chroma(Color color)
        {
            Vector3 lab = ToonContrast.ToLab(color);
            return Mathf.Sqrt(lab.y * lab.y + lab.z * lab.z);
        }

        /// <summary>Hue angle in degrees, 0..360.</summary>
        public static float Hue(Color color)
        {
            Vector3 lab = ToonContrast.ToLab(color);
            float hue = Mathf.Atan2(lab.z, lab.y) * Mathf.Rad2Deg;
            return hue < 0f ? hue + 360f : hue;
        }

        /// <summary>The smaller angle between two hues (0..180).</summary>
        public static float HueGap(float a, float b)
        {
            float gap = Mathf.Abs(a - b) % 360f;
            return gap > 180f ? 360f - gap : gap;
        }
    }

    /// <summary>One role on screen in one capture: how many pixels it showed and their mean color and its CIELAB values.</summary>
    [Serializable]
    public sealed class RoleMeasure
    {
        public string role;
        public int pixels;
        public Color mean;
        public float lightness;
        public float chroma;
        public float hue;

        /// <summary>A role measured by its mean color alone (tests).</summary>
        public static RoleMeasure Of(string role, int pixels, Color mean)
        {
            return Of(role, pixels, mean, StyleContrast.Lightness(mean), StyleContrast.Chroma(mean));
        }

        /// <summary>
        /// A role on screen: <paramref name="lightness"/> and <paramref name="chroma"/> are the means over its pixels (how bright
        /// and how saturated it looks, without a mix of hues greying the mean); the hue is the mean color's.
        /// </summary>
        public static RoleMeasure Of(string role, int pixels, Color mean, float lightness, float chroma)
        {
            return new RoleMeasure
            {
                role = role, pixels = pixels, mean = mean, lightness = lightness, chroma = chroma, hue = StyleContrast.Hue(mean),
            };
        }
    }

    /// <summary>A skin worn by the crowd in one world: its color on screen against the track on screen.</summary>
    [Serializable]
    public sealed class SkinMeasure
    {
        public string skin;
        public Color mean;
        public float deltaE;
    }

    /// <summary>The run (and its gates) and the assault of one world, after the post.</summary>
    [Serializable]
    public sealed class WorldMeasure
    {
        public int world;
        public string shot;
        public RoleMeasure[] roles = Array.Empty<RoleMeasure>();
        /// <summary>Contrast ratio (WCAG) of the gate numbers: bright fill against dark outline, the weakest label on screen.</summary>
        public float gateLabelRatio;
        public SkinMeasure[] skins = Array.Empty<SkinMeasure>();

        public RoleMeasure Role(string name)
        {
            foreach (RoleMeasure each in roles)
            {
                if (each.role == name)
                {
                    return each;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// What the capture measured (RT-6v3.3). <see cref="fingerprint"/> is the guide's look (palettes, light, post, fog, surfaces)
    /// when it was taken: the validator refuses measures of another look (capture again after a change).
    /// </summary>
    [Serializable]
    public sealed class ContrastMeasures
    {
        public string fingerprint;
        public string taken;
        public WorldMeasure[] shots = Array.Empty<WorldMeasure>();
    }
}
