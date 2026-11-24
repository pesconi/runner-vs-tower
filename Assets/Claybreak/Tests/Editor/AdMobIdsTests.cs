using System.IO;
using Claybreak.Editor;
using NUnit.Framework;

namespace Claybreak.Tests
{
    /// <summary>
    /// D35 = A: the studio's AdMob IDs live outside the repository and are checked before a game build. The IDs below
    /// are made up (no real account).
    /// </summary>
    public sealed class AdMobIdsTests
    {
        private const string App = "ca-app-pub-1111222233334444~5555666677";
        private const string Unit = "ca-app-pub-1111222233334444/8888999900";

        [Test]
        public void AProductionPairOfOneAccountPasses()
        {
            Assert.IsNull(ClaybreakAdMob.Problem(App, Unit));
        }

        [Test]
        public void MalformedMixedOrTestIdsAreRefused()
        {
            Assert.IsNotNull(ClaybreakAdMob.Problem(Unit, Unit), "the app ID needs '~'");
            Assert.IsNotNull(ClaybreakAdMob.Problem(App, App), "the unit needs '/'");
            Assert.IsNotNull(ClaybreakAdMob.Problem(App, "ca-app-pub-9999222233334444/8888999900"), "two accounts");
            Assert.IsNotNull(ClaybreakAdMob.Problem(ProjectConfiguration.SampleAdMobAppId, "ca-app-pub-3940256099942544/5224354917"),
                "Google's test IDs");
            Assert.IsNotNull(ClaybreakAdMob.Problem(null, Unit));
        }

        [Test]
        public void TheIdsFileLivesOutsideTheRepositoryAndIsIgnoredByGit()
        {
            Assert.IsFalse(ClaybreakBuild.InsideARepository(ClaybreakAdMob.DefaultPath),
                "the default admob-ids.json path must be outside every repository");
            StringAssert.Contains("admob-ids.json", File.ReadAllText(".gitignore"));
        }

        [Test]
        public void AMissingFileStopsTheGameBuildWithAClearMessage()
        {
            string missing = Path.Combine(Path.GetTempPath(), "claybreak-no-such-admob-ids.json");
            var error = Assert.Throws<System.InvalidOperationException>(() => ClaybreakAdMob.Load(missing));
            StringAssert.Contains(missing, error.Message);
            StringAssert.Contains("D35", error.Message);
        }
    }
}
