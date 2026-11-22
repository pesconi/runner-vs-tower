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

        public Color CrowdColor => crowdColor;
        public ThemeAudio Audio => audio;

        public bool TryValidate(out string error)
        {
            if (audio == null)
            {
                error = "audio: missing";
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
