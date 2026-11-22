using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using NUnit.Framework;
using Unity.Mathematics;

namespace JungleDegree.StudioKit.Tests
{
    public sealed class PropScatterTests
    {
        private const uint Seed = 7;
        private const int Props = 130;
        private static readonly float2 Scale = new float2(0.7f, 1.5f);

        [Test]
        public void EveryPropStandsInTheWindowBesideTheLane()
        {
            PropScatterSettings settings = PropScatterSettings.Default;
            settings.CenterX = 0.5f;
            foreach (float anchor in new[] { -30f, 0f, 12.3f, 417f, 5000f })
            {
                float start = PropScatter.WindowStart(settings, anchor);
                for (int i = 0; i < Props; i++)
                {
                    ToonInstance prop = PropScatter.Place(settings, Seed, 1, i, anchor, Scale);
                    float3 p = prop.Position;
                    Assert.GreaterOrEqual(p.z, start - 1e-3f, $"prop {i} behind the window at anchor {anchor}");
                    Assert.Less(p.z, start + settings.Length + 1e-3f, $"prop {i} past the window at anchor {anchor}");
                    float across = math.abs(p.x - settings.CenterX);
                    Assert.GreaterOrEqual(across, settings.InnerX - 1e-4f, $"prop {i} on the lane");
                    Assert.LessOrEqual(across, settings.InnerX + settings.BandWidth + 1e-4f, $"prop {i} past the band");
                    Assert.AreEqual(0f, p.y);
                    float scale = math.length(prop.Row1.xyz);
                    Assert.GreaterOrEqual(scale, Scale.x - 1e-4f);
                    Assert.LessOrEqual(scale, Scale.y + 1e-4f);
                }
            }
        }

        [Test]
        public void EveryPropIsTurnedAboutYAndScaledEvenly()
        {
            PropScatterSettings settings = PropScatterSettings.Default;
            for (int i = 0; i < Props; i++)
            {
                ToonInstance prop = PropScatter.Place(settings, Seed, 0, i, 33f, Scale);
                float3 x = new float3(prop.Row0.x, prop.Row1.x, prop.Row2.x);
                float3 y = new float3(prop.Row0.y, prop.Row1.y, prop.Row2.y);
                float3 z = new float3(prop.Row0.z, prop.Row1.z, prop.Row2.z);
                float scale = math.length(y);
                Assert.AreEqual(scale, math.length(x), 1e-4f, $"prop {i}: uneven scale");
                Assert.AreEqual(scale, math.length(z), 1e-4f, $"prop {i}: uneven scale");
                Assert.AreEqual(0f, math.dot(x, z), 1e-4f, $"prop {i}: skewed");
                Assert.AreEqual(scale, y.y, 1e-4f, $"prop {i}: not upright");
                Assert.AreEqual(0f, prop.Tint.w, "the instance tint is not used");
            }
        }

        [Test]
        public void APropPassedByTheAnchorWrapsToTheFarEndSomewhereElse()
        {
            PropScatterSettings settings = PropScatterSettings.Default;
            int moved = 0;
            for (int i = 0; i < Props; i++)
            {
                float3 now = PropScatter.Place(settings, Seed, 0, i, 0f, Scale).Position;
                // Move the anchor just past the prop: it leaves at the near end.
                float anchor = now.z + settings.Behind + 0.01f;
                int lap = PropScatter.Lap(settings, Seed, 0, i, anchor);
                Assert.AreEqual(PropScatter.Lap(settings, Seed, 0, i, 0f) + 1, lap, $"prop {i} must start its next lap");
                float3 next = PropScatter.Place(settings, Seed, 0, i, anchor, Scale).Position;
                Assert.AreEqual(now.z + settings.Length, next.z, 1e-2f, $"prop {i} comes back one window further");
                moved += math.abs(next.x - now.x) > 1e-3f ? 1 : 0;
            }

            Assert.Greater(moved, Props * 9 / 10, "a new lap draws a new place across");
        }

        [Test]
        public void PropsSpreadAlongTheWholeWindowAndBothSides()
        {
            PropScatterSettings settings = PropScatterSettings.Default;
            var perQuarter = new int[4];
            int left = 0;
            for (int i = 0; i < Props; i++)
            {
                float3 p = PropScatter.Place(settings, Seed, 0, i, 0f, Scale).Position;
                float t = (p.z - PropScatter.WindowStart(settings, 0f)) / settings.Length;
                perQuarter[math.min(3, (int)(t * 4f))]++;
                left += p.x < 0f ? 1 : 0;
            }

            foreach (int count in perQuarter)
            {
                Assert.Greater(count, Props / 8, "every quarter of the window has props");
            }

            Assert.That(left, Is.InRange(Props / 4, Props * 3 / 4), "both sides have props");
        }

        [Test]
        public void LayersAndSeedsDoNotShareALayout()
        {
            PropScatterSettings settings = PropScatterSettings.Default;
            float3 a = PropScatter.Place(settings, Seed, 0, 3, 0f, Scale).Position;
            float3 b = PropScatter.Place(settings, Seed, 1, 3, 0f, Scale).Position;
            float3 c = PropScatter.Place(settings, Seed + 1, 0, 3, 0f, Scale).Position;
            Assert.IsFalse(math.all(a == b));
            Assert.IsFalse(math.all(a == c));
        }

        [Test]
        public void SettingsRejectAnEmptyWindow()
        {
            PropScatterSettings settings = PropScatterSettings.Default;
            Assert.IsNull(settings.Validate());
            settings.Length = 0f;
            Assert.IsNotNull(settings.Validate());
            settings = PropScatterSettings.Default;
            settings.InnerX = -1f;
            Assert.IsNotNull(settings.Validate());
        }
    }
}
