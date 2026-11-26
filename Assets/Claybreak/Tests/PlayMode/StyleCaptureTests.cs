using System.Collections;
using System.IO;
using NUnit.Framework;

namespace Claybreak.Tests
{
    /// <summary>
    /// RT-6v3.1: writes the six framings of <see cref="StyleCaptureRig"/> into <c>-styleCaptureDir &lt;folder&gt;</c> (docs/art/v3/antes
    /// is the capture of the game before the v3 art). RT-6v3.3: the run and the assault in the four worlds, each measured for the
    /// D36 contrast rule, the measures written to <see cref="StyleContrast.MeasuresPath"/> (the style validator reads them). Only
    /// with a GPU and the argument; otherwise ignored.
    /// <c>-runTests -testPlatform PlayMode -testFilter Claybreak.Tests.StyleCaptureTests -styleCaptureDir &lt;folder&gt;</c>.
    /// </summary>
    public sealed class StyleCaptureTests
    {
        public const int Worlds = 4;

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
            StyleCaptureRig.Measured.Clear();
            for (int world = 1; world <= Worlds; world++)
            {
                yield return StyleCaptureRig.CaptureRun(folder, world);
            }

            for (int world = 1; world <= Worlds; world++)
            {
                yield return StyleCaptureRig.CaptureAssault(folder, world);
            }

            StyleCaptureRig.WriteMeasures(folder);
            yield return StyleCaptureRig.CaptureLeader(folder);
            yield return StyleCaptureRig.CaptureScreens(folder);
            StyleCaptureRig.CaptureIcon(folder);
            foreach (string framing in StyleCaptureRig.Framings)
            {
                Assert.IsTrue(File.Exists(Path.Combine(folder, framing + ".png")), framing + ".png was not written");
            }

            for (int world = 1; world <= Worlds; world++)
            {
                Assert.IsTrue(File.Exists(Path.Combine(folder, StyleCaptureRig.RunFile(world) + ".png")), StyleCaptureRig.RunFile(world));
                Assert.IsTrue(File.Exists(Path.Combine(folder, StyleCaptureRig.AssaultFile(world) + ".png")), StyleCaptureRig.AssaultFile(world));
            }
        }
    }
}
