using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Dresses the one scene for a world (RT-6t.8): the world's toon palette on screen (sky gradient and fog), on the
    /// striped track (track and stripe colors), on the ground and rails (ground and stripe colors) and on the recycled side
    /// props (trees: trunk and two foliage tones; rocks: rock and light foliage), the world's tree and rock density, and its
    /// sun and ambient light. The scenery itself is the same preallocated set in every world: applying is a few property
    /// writes at menu or level time, never per frame, and nothing is created.
    /// </summary>
    public sealed class WorldScenery : MonoBehaviour
    {
        /// <summary>Prop layers of <see cref="props"/>, in this order.</summary>
        public const int TreeLayer = 0;
        public const int RockLayer = 1;

        [Tooltip("The striped track (StudioKit/StripedTrack); its bounds are the play area.")]
        [SerializeField] private Renderer track;
        [Tooltip("The ground and the side rails (StudioKit/ToonLit, slots: ground, stripe).")]
        [SerializeField] private Renderer ground;
        [Tooltip("Trees (layer 0) and rocks (layer 1) along the road.")]
        [SerializeField] private InstancedProps props;
        [SerializeField] private Light sun;
        [Tooltip("Puts the world's palette on screen (sky and fog).")]
        [SerializeField] private ToonPaletteApplier palette;

        private MaterialPropertyBlock trackBlock;
        private MaterialPropertyBlock groundBlock;
        private int shown = -1;

        public int Shown => shown;
        public Renderer Track => track;
        public Renderer Ground => ground;
        public InstancedProps Props => props;

        /// <summary>Draw calls of the side scenery (rails with the ground, trees, rocks) for the world shown.</summary>
        public int SideDrawCalls => (ground != null && ground.enabled ? 1 : 0) + (props == null ? 0 : props.DrawCalls);

        public bool TryValidate(int worlds, out string error)
        {
            if (track == null || ground == null || props == null || sun == null || palette == null)
            {
                error = "WorldScenery needs the track, the ground, the side props, the sun and the palette applier.";
                return false;
            }

            if (track.sharedMaterial == null || track.sharedMaterial.shader.name != ToonShaderIds.StripedTrackShader)
            {
                error = $"WorldScenery: the track must use {ToonShaderIds.StripedTrackShader}.";
                return false;
            }

            Material groundMaterial = ground.sharedMaterial;
            if (groundMaterial == null || groundMaterial.shader.name != ToonShaderIds.ToonLitShader
                || !groundMaterial.IsKeywordEnabled(ToonShaderIds.SlotsKeyword))
            {
                error = $"WorldScenery: the ground must use {ToonShaderIds.ToonLitShader} with {ToonShaderIds.SlotsKeyword}.";
                return false;
            }

            string propsError = props.Validate();
            if (propsError == null && props.LayerCount != 2)
            {
                propsError = "two prop layers (trees, rocks) are expected";
            }

            if (propsError != null)
            {
                error = "WorldScenery: " + propsError;
                return false;
            }

            string paletteError = palette.Validate();
            if (paletteError != null)
            {
                error = "WorldScenery: " + paletteError;
                return false;
            }

            error = worlds > 0 ? null : "WorldScenery: no world to show.";
            return error == null;
        }

        /// <summary>
        /// Shows world <paramref name="world"/> (catalog index) with <paramref name="look"/>; its density is clamped to the
        /// preallocated props. Also usable in edit mode (tools dressing a scene for a capture).
        /// </summary>
        public void Apply(int world, WorldLook look)
        {
            ToonPalette colors = look.palette;
            if (colors == null || !palette.Apply(colors))
            {
                return;
            }

            trackBlock ??= new MaterialPropertyBlock();
            groundBlock ??= new MaterialPropertyBlock();
            trackBlock.SetVector(ToonShaderIds.TrackColor, ToonShaderIds.ShaderColor(colors.Track));
            trackBlock.SetVector(ToonShaderIds.StripeColor, ToonShaderIds.ShaderColor(colors.TrackStripe));
            track.SetPropertyBlock(trackBlock);
            // Shader colors in the working space (the slots variant mixes them as they are).
            groundBlock.SetVector(ToonShaderIds.Slot0, ToonShaderIds.ShaderColor(colors.Ground));
            groundBlock.SetVector(ToonShaderIds.Slot1, ToonShaderIds.ShaderColor(colors.TrackStripe));
            groundBlock.SetVector(ToonShaderIds.Slot2, ToonShaderIds.ShaderColor(colors.TrackStripe));
            ground.SetPropertyBlock(groundBlock);
            props.SetLayer(TreeLayer, look.trees, colors.Trunk, colors.FoliageDark, colors.FoliageLight);
            props.SetLayer(RockLayer, look.rocks, colors.Rock, colors.FoliageLight, colors.Rock);
            RenderSettings.ambientLight = look.ambient;
            sun.color = look.sun;
            sun.intensity = look.sunIntensity;
            shown = world;
        }
    }
}
