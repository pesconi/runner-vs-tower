using Claybreak.Editor;
using JungleDegree.StudioKit.Swarm;
using NUnit.Framework;
using UnityEngine;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6t.13: the guard that replaced "the fog ends before the props' window" (it finds where a game camera sees props
    /// wrap in), the shipped numbers against it, and the leader's crown mesh.
    /// </summary>
    public sealed class LeaderAndFogTests
    {
        private const float LaneHalfWidth = ClaybreakSceneBuilder.TrackWidth * 0.5f;

        private static PropScatterSettings Window(float length)
        {
            return new PropScatterSettings
            {
                Behind = PropScatterSettings.Default.Behind,
                Length = length,
                CenterX = 0f,
                InnerX = LaneHalfWidth + SceneryArt.PropMarginFromTrack,
                BandWidth = PropScatterSettings.Default.BandWidth,
            };
        }

        private static float Nearest(float length)
        {
            return PropWrapGuard.NearestVisibleWrapDepth(SwarmCameraSettings.Default, ClaybreakSceneBuilder.CrowdCapacity,
                LaneHalfWidth, Window(length), SceneryArt.PropHeight, null);
        }

        [Test]
        public void TheReferenceWindowWouldPopPropsInClearOfTheNewFog()
        {
            // Up to RT-6t.12 the 90 m window hid its wraps in a fog ending at 75 m; the near-zero fog starts at 110 m.
            float nearest = Nearest(PropScatterSettings.Default.Length);
            Assert.Less(nearest, 110f);
            Assert.Greater(nearest, 75f, "the old rule held for the old fog");
        }

        [Test]
        public void TheShippedWindowWrapsPropsInFullFogBeforeTheFarClip()
        {
            PaletteDefinition[] palettes = PaletteContent.Palettes;
            float nearest = Nearest(SceneryArt.PropWindowLength);
            foreach (PaletteDefinition palette in palettes)
            {
                Assert.LessOrEqual(palette.FogEnd, nearest, palette.Name);
                Assert.Less(palette.FogEnd, ClaybreakSceneBuilder.CameraFarClip, palette.Name);
                Assert.Less(palette.FogStart, palette.FogEnd, palette.Name);
            }
        }

        [Test]
        public void AWrapPointOutOfEveryViewIsNeverSeen()
        {
            // A window that ends behind the camera has no wrap point on screen.
            var behind = Window(1f);
            behind.Behind = 10f;
            Assert.AreEqual(float.MaxValue, PropWrapGuard.NearestVisibleWrapDepth(SwarmCameraSettings.Default, 300, LaneHalfWidth,
                behind, SceneryArt.PropHeight, null));
        }

        [Test]
        public void TheCrownTakesTheTintAndSitsOnItsBase()
        {
            Mesh crown = LeaderArt.Crown();
            try
            {
                Bounds bounds = crown.bounds;
                Assert.AreEqual(0f, bounds.min.y, 0.02f, "the crown base is at y = 0");
                Assert.That(bounds.size.x, Is.InRange(0.95f, 1.15f), "about 1 m across");
                int tinted = 0;
                int jewels = 0;
                foreach (Color32 color in crown.colors32)
                {
                    tinted += color.a == 255 ? 1 : 0;
                    jewels += color.a == 0 ? 1 : 0;
                }

                Assert.Greater(tinted, 0, "the band takes the crown color");
                Assert.Greater(jewels, 0, "the jewels keep their white");
                Assert.AreEqual(crown.vertexCount, tinted + jewels);
                Assert.Less(crown.triangles.Length / 3, 400, "a small mesh");
            }
            finally
            {
                Object.DestroyImmediate(crown);
            }
        }
    }
}
