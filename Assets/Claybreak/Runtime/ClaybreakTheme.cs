using JungleDegree.StudioKit.Characters;
using JungleDegree.StudioKit.Core;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// The game's theme contract (prototype size): it assembles the kit's data blocks (character
    /// roster, audio, death effect) with this game's own colors and the crowd shape (RT-6t.4).
    /// </summary>
    [CreateAssetMenu(menuName = "Claybreak/Theme")]
    public sealed class ClaybreakTheme : ScriptableObject
    {
        public const string CrowdColorId = "crowd";

        [SerializeField] private ThemeCharacterRoster roster = new ThemeCharacterRoster();
        [SerializeField] private Color crowdColor = new Color(0.95f, 0.55f, 0.18f);
        [Tooltip("Shape the crowd is drawn with (one of the generated crowd shapes). Decision D25 is open: the Bolinha with eyes "
            + "is the provisional choice; changing it is changing this reference.")]
        [SerializeField] private Mesh crowdShape;
        [SerializeField] private ThemeAudio audio = new ThemeAudio();
        [SerializeField] private ThemeDeathEffect deathEffect = new ThemeDeathEffect();

        public ThemeCharacterRoster Roster => roster;
        public Color CrowdColor => crowdColor;
        public Mesh CrowdShape => crowdShape;
        public ThemeAudio Audio => audio;
        public ThemeDeathEffect DeathEffect => deathEffect;

        public bool TryValidate(out string error)
        {
            if (roster == null || roster.BuiltInBody == null)
            {
                error = "roster needs the kit's built-in clay body (ClayRunnerFrames).";
                return false;
            }

            if (!roster.TryValidate(id => id == CrowdColorId, out error))
            {
                return false;
            }

            if (crowdShape == null || crowdShape.subMeshCount != 1)
            {
                error = "crowdShape needs one of the generated crowd shapes (one submesh).";
                return false;
            }

            if (audio == null || !audio.TryValidate(out error))
            {
                error = "audio: " + (error ?? "missing");
                return false;
            }

            if (deathEffect == null || !deathEffect.TryValidate(out error))
            {
                error = "deathEffect: " + (error ?? "missing");
                return false;
            }

            error = null;
            return true;
        }
    }
}
