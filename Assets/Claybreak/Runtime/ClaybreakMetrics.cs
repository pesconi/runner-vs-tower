using JungleDegree.StudioKit.Analytics;

namespace Claybreak
{
    /// <summary>
    /// The game's side of the minimum measurement (D4): which sink is installed and the names of the rewarded
    /// placements. No analytics SDK is installed until the tool is chosen (ficha D21): release builds drop every
    /// event; development and measurement builds write them to the device log only, so the events can be checked
    /// with logcat. When a tool is chosen, its adapter replaces <see cref="NullMetricsSink"/> here and collection
    /// follows the player's consent.
    /// </summary>
    public static class ClaybreakMetrics
    {
        public const string DoubleCoinsPlacement = "double_coins";
        public const string ContinuePlacement = "continue";

        public static void Install()
        {
#if DEVELOPMENT_BUILD || RELEASE_MEASUREMENT || UNITY_EDITOR
            PlayerMetrics.Install(new LogMetricsSink());
            PlayerMetrics.SetCollectionAllowed(true);
#else
            PlayerMetrics.Install(new NullMetricsSink());
            PlayerMetrics.SetCollectionAllowed(false);
#endif
        }

        /// <summary>The level id the events carry: world id and level number (from 1).</summary>
        public static string LevelId(string worldId, int level)
        {
            return worldId + "_l" + (level + 1);
        }
    }
}
