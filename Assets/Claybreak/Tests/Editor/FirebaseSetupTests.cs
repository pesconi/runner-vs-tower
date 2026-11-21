using System.IO;
using Claybreak.Editor;
using JungleDegree.StudioKit.Analytics;
using NUnit.Framework;

namespace Claybreak.Tests
{
    /// <summary>RT-8b (D21 = A): the minimal Firebase setup as shipped in the repository.</summary>
    public sealed class FirebaseSetupTests
    {
        [Test]
        public void ConfigurationLivesOutsideTheRepository()
        {
            Assert.IsFalse(ClaybreakBuild.InsideARepository(ClaybreakFirebase.DefaultPath),
                "the default google-services.json path must be outside every repository");
            StringAssert.Contains("google-services.json", File.ReadAllText(".gitignore"));
        }

        [Test]
        public void ModuleKeepsCollectionAndAdvertisingIdOff()
        {
            string manifest = File.ReadAllText(ClaybreakFirebase.ModulePath + "/AndroidManifest.xml");
            foreach (string flag in MonetizationValidation.FirebaseFlagsOff)
            {
                StringAssert.Contains("android:name=\"" + flag + "\" android:value=\"false\"", manifest);
            }

            StringAssert.Contains("firebase-analytics:" + ClaybreakFirebase.FirebaseAnalyticsVersion,
                File.ReadAllText(ClaybreakFirebase.ModulePath + "/build.gradle"));
        }

        [Test]
        public void TheSessionEventKeepsItsApprovedName()
        {
            // "session_start" is reserved by Firebase for its automatic event (D21 second opinion).
            Assert.AreEqual("game_session", PlayerMetrics.SessionStartEvent);
        }
    }
}
