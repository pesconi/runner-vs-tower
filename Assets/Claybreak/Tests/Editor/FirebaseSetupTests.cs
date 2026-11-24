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
        public void AFileOfAnotherPackageIsSkippedNotBroken()
        {
            // RT-9b.1: a google-services.json registered under the retired application ID has no app for the new one,
            // so the build leaves Firebase out with a warning; a file of the right package, or one that is not a
            // services file at all, goes on to the kit's parser (which writes it or says what is wrong).
            const string oldFile = "{\"project_info\":{\"project_number\":\"1\"},\"client\":[{\"client_info\":"
                + "{\"android_client_info\":{\"package_name\":\"" + ProjectConfiguration.RetiredApplicationId + "\"}}}]}";
            Assert.IsFalse(ClaybreakFirebase.HasAndroidApp(oldFile, ProjectConfiguration.ApplicationId));
            Assert.IsTrue(ClaybreakFirebase.HasAndroidApp(oldFile, ProjectConfiguration.RetiredApplicationId));
            Assert.IsTrue(ClaybreakFirebase.HasAndroidApp("{}", ProjectConfiguration.ApplicationId));
        }

        [Test]
        public void TheApplicationIdIsTheStoreOne()
        {
            // D34 = B: fixed once the first build reaches Google Play; the provisional one must not come back.
            Assert.AreEqual("com.jungledegree.stampeep", ProjectConfiguration.ApplicationId);
            var failures = new System.Collections.Generic.List<string>();
            ClaybreakValidation.CheckApplicationId(failures);
            CollectionAssert.IsEmpty(failures);
        }

        [Test]
        public void TheSessionEventKeepsItsApprovedName()
        {
            // "session_start" is reserved by Firebase for its automatic event (D21 second opinion).
            Assert.AreEqual("game_session", PlayerMetrics.SessionStartEvent);
        }
    }
}
