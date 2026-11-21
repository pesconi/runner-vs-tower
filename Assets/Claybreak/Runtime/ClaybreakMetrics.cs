using JungleDegree.StudioKit.Ads;
using JungleDegree.StudioKit.Analytics;

namespace Claybreak
{
    /// <summary>
    /// The game's side of the minimum measurement (D4, tool D21 = A: Firebase Analytics, minimal setup). Events go
    /// to Firebase when the build carries its configuration (otherwise nowhere); development and measurement
    /// builds also write them to the device log (logcat). Nothing is forwarded, and Firebase's own collection
    /// stays off (manifest flag), until the consent flow ends and <see cref="MeasurementConsent"/> allows it. The
    /// studio is established in the EU (Portugal), so the GDPR applies to every player: measurement needs explicit
    /// consent everywhere. In the EEA and the UK that is the player's "yes" in the UMP form; elsewhere the game has
    /// no consent request of its own yet (<see cref="GameConsentGiven"/>, ficha D23), so nothing is measured there.
    /// The answer is read again each time the flow ends (also after the privacy options form).
    /// In the Editor events only go to the console.
    /// </summary>
    public static class ClaybreakMetrics
    {
        public const string DoubleCoinsPlacement = "double_coins";
        public const string ContinuePlacement = "continue";

        /// <summary>
        /// The game's own explicit consent where the UMP form does not ask (outside the EEA and the UK). The game does
        /// not ask yet (ficha D23), so it is never given and nothing is measured there.
        /// </summary>
        public static bool GameConsentGiven => false;

        public static void Install()
        {
#if UNITY_EDITOR
            PlayerMetrics.Install(new LogMetricsSink());
            PlayerMetrics.SetCollectionAllowed(true);
#else
            IMetricsSink firebase = FirebaseAnalyticsSink.TryCreate();
#if DEVELOPMENT_BUILD || RELEASE_MEASUREMENT
            PlayerMetrics.Install(firebase == null ? new LogMetricsSink() : new TeeMetricsSink(new LogMetricsSink(), firebase));
#else
            PlayerMetrics.Install(firebase);
#endif
            PlayerMetrics.SetCollectionAllowed(false);
            MobileAdsGate.ConsentFlowFinished -= RefreshConsent;
            MobileAdsGate.ConsentFlowFinished += RefreshConsent;
            RefreshConsent();
#endif
        }

        /// <summary>Reads the player's answer again and switches collection to match.</summary>
        public static void RefreshConsent()
        {
            PlayerMetrics.SetCollectionAllowed(MeasurementConsent.FromDevice(GameConsentGiven));
        }

        /// <summary>The level id the events carry: world id and level number (from 1).</summary>
        public static string LevelId(string worldId, int level)
        {
            return worldId + "_l" + (level + 1);
        }
    }
}
