using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Dresses the one scene for a world (RT-6): shows that world's preallocated scenery set (props, fields,
    /// far silhouettes; the others are hidden), puts the world's ground material on the track and applies
    /// its toon palette (sky gradient and fog, RT-6t.6a), ambient and sun. Called at menu/level time only, never per
    /// frame; nothing is created.
    /// </summary>
    public sealed class WorldScenery : MonoBehaviour
    {
        [Tooltip("One scenery set per catalog world, in catalog order.")]
        [SerializeField] private GameObject[] sets = new GameObject[0];
        [Tooltip("The track's material per catalog world.")]
        [SerializeField] private Material[] trackMaterials = new Material[0];
        [SerializeField] private Renderer track;
        [SerializeField] private Light sun;
        [Tooltip("Puts the world's palette on screen (sky and fog).")]
        [SerializeField] private ToonPaletteApplier palette;

        private int shown = -1;

        public int Count => sets == null ? 0 : sets.Length;
        public int Shown => shown;

        public bool TryValidate(int worlds, out string error)
        {
            if (sets == null || trackMaterials == null || sets.Length != worlds || trackMaterials.Length != worlds
                || track == null || sun == null || palette == null)
            {
                error = $"WorldScenery needs one set and one track material per world ({worlds}), the track, the sun and the palette applier.";
                return false;
            }

            string paletteError = palette.Validate();
            if (paletteError != null)
            {
                error = "WorldScenery: " + paletteError;
                return false;
            }

            for (int i = 0; i < worlds; i++)
            {
                if (sets[i] == null || trackMaterials[i] == null)
                {
                    error = $"WorldScenery: world {i + 1} has no set or no track material.";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /// <summary>Shows world <paramref name="world"/> (catalog index) with <paramref name="look"/>.</summary>
        public void Apply(int world, WorldLook look)
        {
            if (sets == null || world < 0 || world >= sets.Length)
            {
                return;
            }

            if (world != shown)
            {
                for (int i = 0; i < sets.Length; i++)
                {
                    sets[i].SetActive(i == world);
                }

                track.sharedMaterial = trackMaterials[world];
                shown = world;
            }

            palette.Apply(look.palette);
            RenderSettings.ambientLight = look.ambient;
            sun.color = look.sun;
            sun.intensity = look.sunIntensity;
        }
    }
}
