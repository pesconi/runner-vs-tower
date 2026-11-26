using System;
using System.Collections;
using System.Collections.Generic;
using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.3: measures the D36 contrast logic on a capture, after the light and the post (<see cref="ContrastRule"/>). The
    /// simulation is frozen (time scale 0, so every frame is the same picture), each role's objects go to a layer of their own,
    /// and the camera renders each layer alone twice, over black and over white, with the post off: a pixel the role covers is
    /// the same over both backgrounds; it is the role on screen when that pixel also matches the full picture rendered the
    /// same way (nothing in front of it). The role's color is the mean of those pixels in the picture with the post, as the
    /// player sees it. The sky is what no object covers. Gate numbers: the contrast ratio (WCAG) between the bright fill and the
    /// dark outline of the labels (90th against 10th percentile of their luminance), the weaker side. Skins: the crowd
    /// re-dressed in every skin, its mean on screen against the track's, ΔE CIE76.
    /// The kit's instanced renderers (crowd, flyers, props) take their object's layer when they create their draws: after the
    /// layers move (and after they move back) the other draw route is forced for one frame and released, so they recreate
    /// their draws on the layer of the moment (<see cref="ToonInstanceRouting.Forced"/>).
    /// </summary>
    public sealed class ContrastProbe
    {
        // User layers nothing in the game uses.
        private const int FirstLayer = 20;
        private const float CoverTolerance = 0.03f;
        private const float VisibleTolerance = 0.08f;

        private readonly Camera view;
        private readonly RenderTexture target;
        private readonly Texture2D read;
        private readonly List<(GameObject owner, int layer)> moved = new List<(GameObject, int)>();
        private readonly List<string> roles = new List<string>();

        public ContrastProbe(Camera camera)
        {
            view = camera;
            target = view.targetTexture;
            if (target == null)
            {
                throw new InvalidOperationException("the probe reads the camera's target texture (take a shot first)");
            }

            read = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
        }

        /// <summary>Puts <paramref name="objects"/> on the role's own layer (only that object, or with its children).</summary>
        public void Role(string role, bool children, params Component[] objects)
        {
            int layer = FirstLayer + roles.Count;
            roles.Add(role);
            foreach (Component each in objects)
            {
                if (each != null)
                {
                    Move(each.gameObject, layer, children);
                }
            }
        }

        private void Move(GameObject owner, int layer, bool children)
        {
            moved.Add((owner, owner.layer));
            owner.layer = layer;
            if (children)
            {
                foreach (Transform child in owner.transform)
                {
                    Move(child.gameObject, layer, true);
                }
            }
        }

        /// <summary>
        /// Renders the passes and measures every role, the sky and (when the roles hold them) the gate labels; with
        /// <paramref name="skins"/> also each skin against the track. Leaves the camera, the layers and the time as found.
        /// </summary>
        public IEnumerator Measure(WorldMeasure into, IList<(string id, Color color)> skins, Action<Color> wear, Color restore)
        {
            float timeScale = Time.timeScale;
            Time.timeScale = 0f;
            int mask = view.cullingMask;
            CameraClearFlags clear = view.clearFlags;
            Color background = view.backgroundColor;
            UniversalAdditionalCameraData data = view.GetUniversalAdditionalCameraData();
            bool post = data.renderPostProcessing;
            yield return Redraw();
            try
            {
                Color[] shown = null;
                yield return Pass(mask, null, true, result => shown = result);
                Color[] plain = null;
                yield return Pass(mask, null, false, result => plain = result);
                Color[] allBlack = null;
                Color[] allWhite = null;
                yield return Pass(mask, Color.black, false, result => allBlack = result);
                yield return Pass(mask, Color.white, false, result => allWhite = result);

                var measures = new List<RoleMeasure>();
                var masks = new Dictionary<string, bool[]>();
                for (int r = 0; r < roles.Count; r++)
                {
                    int only = 1 << (FirstLayer + r);
                    Color[] black = null;
                    Color[] white = null;
                    yield return Pass(only, Color.black, false, result => black = result);
                    yield return Pass(only, Color.white, false, result => white = result);
                    // Gate numbers stand in front of their translucent panel: covered is enough (the panel may tint them).
                    bool label = roles[r].StartsWith("label", StringComparison.Ordinal);
                    bool[] visible = label ? Covered(black, white) : Visible(black, white, plain);
                    masks[roles[r]] = visible;
                    if (!roles[r].StartsWith("label", StringComparison.Ordinal))
                    {
                        measures.Add(Mean(roles[r], visible, shown));
                    }
                }

                var sky = new bool[shown.Length];
                for (int i = 0; i < sky.Length; i++)
                {
                    sky[i] = Distance(allBlack[i], allWhite[i]) > 0.5f;
                }

                measures.Add(Mean(StyleContrast.Sky, sky, shown));
                into.roles = measures.ToArray();
                into.gateLabelRatio = LabelRatio(masks, shown);

                if (skins != null && masks.TryGetValue(StyleContrast.Crowd, out bool[] crowd) && into.Role(StyleContrast.Track) != null)
                {
                    Color track = into.Role(StyleContrast.Track).mean;
                    var measured = new List<SkinMeasure>();
                    foreach ((string id, Color color) in skins)
                    {
                        wear(color);
                        Color[] dressed = null;
                        yield return Pass(mask, null, true, result => dressed = result);
                        Color mean = Mean(StyleContrast.Crowd, crowd, dressed).mean;
                        measured.Add(new SkinMeasure { skin = id, mean = mean, deltaE = ToonContrast.DeltaE(mean, track) });
                    }

                    wear(restore);
                    into.skins = measured.ToArray();
                }
            }
            finally
            {
                view.cullingMask = mask;
                view.clearFlags = clear;
                view.backgroundColor = background;
                data.renderPostProcessing = post;
                Time.timeScale = timeScale;
                for (int i = moved.Count - 1; i >= 0; i--)
                {
                    if (moved[i].owner != null)
                    {
                        moved[i].owner.layer = moved[i].layer;
                    }
                }

                moved.Clear();
                Object.Destroy(read);
            }

            yield return Redraw();
        }

        // The instanced renderers recreate their draws (on the layer their object has now): one frame on the other route, one back.
        private static IEnumerator Redraw()
        {
            ToonInstanceRoute? forced = ToonInstanceRouting.Forced;
            ToonInstanceRoute current = ToonInstanceRouting.Current;
            ToonInstanceRouting.Forced = current == ToonInstanceRoute.Chunks ? ToonInstanceRoute.Buffer : ToonInstanceRoute.Chunks;
            yield return null;
            yield return null;
            ToonInstanceRouting.Forced = forced;
            yield return null;
            yield return null;
        }

        // One picture: the layers in mask, over the sky (null) or a solid color, with or without the post.
        private IEnumerator Pass(int mask, Color? solid, bool post, Action<Color[]> done)
        {
            view.cullingMask = mask;
            view.clearFlags = solid.HasValue ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            view.backgroundColor = solid ?? Color.black;
            view.GetUniversalAdditionalCameraData().renderPostProcessing = post;
            yield return null;
            yield return null;
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            read.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            read.Apply();
            RenderTexture.active = previous;
            done(read.GetPixels());
        }

        private static bool[] Visible(Color[] black, Color[] white, Color[] plain)
        {
            var visible = new bool[black.Length];
            for (int i = 0; i < visible.Length; i++)
            {
                visible[i] = Distance(black[i], white[i]) < CoverTolerance && Distance(black[i], plain[i]) < VisibleTolerance;
            }

            return visible;
        }

        private static bool[] Covered(Color[] black, Color[] white)
        {
            var covered = new bool[black.Length];
            for (int i = 0; i < covered.Length; i++)
            {
                covered[i] = Distance(black[i], white[i]) < CoverTolerance;
            }

            return covered;
        }

        private static float Distance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }

        private static RoleMeasure Mean(string role, bool[] mask, Color[] picture)
        {
            double r = 0, g = 0, b = 0, lightness = 0, chroma = 0;
            int count = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i])
                {
                    var c = new Color(Mathf.Clamp01(picture[i].r), Mathf.Clamp01(picture[i].g), Mathf.Clamp01(picture[i].b));
                    r += c.r;
                    g += c.g;
                    b += c.b;
                    Vector3 lab = ToonContrast.ToLab(c);
                    lightness += lab.x;
                    chroma += Mathf.Sqrt(lab.y * lab.y + lab.z * lab.z);
                    count++;
                }
            }

            // The target texture is sRGB-encoded 8 bit read as display values: the means are of what the screen shows.
            if (count == 0)
            {
                return RoleMeasure.Of(role, 0, Color.black, 0f, 0f);
            }

            var mean = new Color((float)(r / count), (float)(g / count), (float)(b / count), 1f);
            return RoleMeasure.Of(role, count, mean, (float)(lightness / count), (float)(chroma / count));
        }

        private static float LabelRatio(Dictionary<string, bool[]> masks, Color[] picture)
        {
            float weakest = float.MaxValue;
            foreach (KeyValuePair<string, bool[]> entry in masks)
            {
                if (!entry.Key.StartsWith("label", StringComparison.Ordinal))
                {
                    continue;
                }

                var luminance = new List<float>();
                for (int i = 0; i < entry.Value.Length; i++)
                {
                    if (entry.Value[i])
                    {
                        Color c = picture[i];
                        luminance.Add(ToonContrast.Luminance(new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b))));
                    }
                }

                if (luminance.Count < 20)
                {
                    continue; // that side's labels are not on screen
                }

                luminance.Sort();
                float dark = luminance[luminance.Count / 10];
                float bright = luminance[luminance.Count * 9 / 10];
                float ratio = (bright + 0.05f) / (dark + 0.05f);
                Debug.Log($"[ContrastProbe] {entry.Key}: {luminance.Count} px, fill {bright:0.000} outline {dark:0.000}, {ratio:0.00}:1");
                weakest = Mathf.Min(weakest, ratio);
            }

            return weakest == float.MaxValue ? 0f : weakest;
        }
    }
}
