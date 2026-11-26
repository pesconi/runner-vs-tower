using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The game's theme contract: the kit's audio block plus this game's base crowd color. The crowd's look (shapes,
    /// toon palettes, ballistic deaths) is the kit's toon swarm since RT-6t; the shapes are the player's pick
    /// (<see cref="ClaybreakCatalog.Shapes"/>). The old clay roster and splat death effect left with RT-6t.12.
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/Theme")]
    public sealed class ClaybreakTheme : ScriptableObject
    {
        [SerializeField] private Color crowdColor = new Color(0.95f, 0.55f, 0.18f);
        [SerializeField] private ThemeAudio audio = new ThemeAudio();

        /// <summary>RT-6v3.9: the run/assault loop of each world (index = world index), all on the same four-note motif.</summary>
        [SerializeField] private AudioClip[] worldRunMusic = System.Array.Empty<AudioClip>();

        private static readonly System.Reflection.FieldInfo RunMusicField =
            typeof(ThemeAudio).GetField("runMusic", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public Color CrowdColor => crowdColor;
        public ThemeAudio Audio => audio;
        public int WorldMusicCount => worldRunMusic.Length;

        /// <summary>
        /// Points the kit's run music at the loop of <paramref name="world"/> (the kit plays whichever clip the theme holds when the
        /// run starts; it has no per-world music of its own). Reflection on the kit's serialized field is a stopgap: a kit delivery
        /// should expose a setter (see docs/decisoes.md, RT-6v3.9). Returns false when the clip or the field is missing.
        /// </summary>
        public bool SelectWorldMusic(int world)
        {
            if (RunMusicField == null || worldRunMusic.Length == 0)
            {
                return false;
            }

            AudioClip clip = worldRunMusic[Mathf.Clamp(world, 0, worldRunMusic.Length - 1)];
            if (clip == null)
            {
                return false;
            }

            RunMusicField.SetValue(audio, clip);
            return true;
        }

        public bool TryValidate(out string error)
        {
            if (audio == null)
            {
                error = "audio: missing";
                return false;
            }

            if (RunMusicField == null)
            {
                error = "the kit's ThemeAudio no longer has a runMusic field (world music cannot be selected)";
                return false;
            }

            if (!audio.TryValidate(out error))
            {
                error = "audio: " + error;
                return false;
            }

            return true;
        }
    }
}
