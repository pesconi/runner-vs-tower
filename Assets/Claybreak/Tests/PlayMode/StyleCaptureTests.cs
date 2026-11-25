using System.Collections;
using System.IO;
using NUnit.Framework;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.1: writes the six framings of <see cref="StyleCaptureRig"/> into <c>-styleCaptureDir &lt;folder&gt;</c> (docs/art/v3/antes
    /// is the capture of the game before the v3 art). Only with a GPU and the argument; otherwise ignored (the validation never
    /// needs it). <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.StyleCaptureTests -styleCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class StyleCaptureTests
    {
        [TearDown]
        public void TearDown()
        {
            StyleCaptureRig.End();
        }

        [UnityEngine.TestTools.UnityTest]
        public IEnumerator CaptureTheSixFramings()
        {
            string folder = StyleCaptureRig.Folder();
            StyleCaptureRig.Begin();
            yield return StyleCaptureRig.CaptureRun(folder);
            yield return StyleCaptureRig.CaptureAssault(folder);
            yield return StyleCaptureRig.CaptureLeader(folder);
            yield return StyleCaptureRig.CaptureScreens(folder);
            StyleCaptureRig.CaptureIcon(folder);
            foreach (string framing in StyleCaptureRig.Framings)
            {
                Assert.IsTrue(File.Exists(Path.Combine(folder, framing + ".png")), framing + ".png was not written");
            }
        }
    }
}
