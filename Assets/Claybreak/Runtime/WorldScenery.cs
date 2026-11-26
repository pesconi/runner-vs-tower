using JungleDegree.StudioKit.Swarm;
using JungleDegree.StudioKit.Toon;
using UnityEngine;
using UnityEngine.Rendering;

namespace Claybreak
{
    /// <summary>
    /// Dresses the one scene for a world (RT-6t.8): the world's toon palette on screen (sky gradient; no fog since RT-6t.13), on the
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
        [Tooltip("Puts the world's palette on screen (the sky; the game keeps the fog off).")]
        [SerializeField] private ToonPaletteApplier palette;
        [Header("RT-6v3.3: light and post per world, from the style guide")]
        [Tooltip("Writes the world's vinyl light (StudioKit/ToonVinyl globals) and its warm key on the sun.")]
        [SerializeField] private ToonVinylLookApplier vinylLight;
        [Tooltip("The world's vinyl light, by catalog index.")]
        [SerializeField] private ToonVinylLook[] worldLights = System.Array.Empty<ToonVinylLook>();
        [Tooltip("The global volume of the game camera; its profile is the world's post.")]
        [SerializeField] private Volume post;
        [Tooltip("The world's post profile, by catalog index (shared assets, never edited at run time).")]
        [SerializeField] private VolumeProfile[] worldPosts = System.Array.Empty<VolumeProfile>();

        private MaterialPropertyBlock trackBlock;
        private MaterialPropertyBlock groundBlock;
        private int shown = -1;

        public int Shown => shown;
        public Renderer Track => track;
        public Renderer Ground => ground;
        public InstancedProps Props => props;

        /// <summary>Draw calls of the side scenery (rails with the ground, trees, rocks) for the world shown.</summary>
        public int SideDrawCalls => (ground != null && ground.enabled ? 1 : 0) + (props == null ? 0 : props.DrawCalls);

        /// <summary>True for the kit's toon shaders that share properties, keywords and instance data (ToonLit and its vinyl sibling).</summary>
        public static bool IsToonFamily(string shaderName)
        {
            return shaderName == ToonShaderIds.ToonLitShader || shaderName == ToonShaderIds.ToonVinylShader;
        }

        public ToonVinylLook LightOf(int world) => worldLights != null && world >= 0 && world < worldLights.Length ? worldLights[world] : null;

        public VolumeProfile PostOf(int world) => worldPosts != null && world >= 0 && world < worldPosts.Length ? worldPosts[world] : null;

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
            if (groundMaterial == null || !IsToonFamily(groundMaterial.shader.name)
                || !groundMaterial.IsKeywordEnabled(ToonShaderIds.SlotsKeyword))
            {
                error = $"WorldScenery: the ground must use {ToonShaderIds.ToonVinylShader} (or {ToonShaderIds.ToonLitShader}) with {ToonShaderIds.SlotsKeyword}.";
                return false;
            }

            if (vinylLight == null || post == null || !post.isGlobal || worldLights == null || worldPosts == null
                || worldLights.Length != worlds || worldPosts.Length != worlds)
            {
                error = "WorldScenery: needs the vinyl light applier, the global post volume and one light and one post profile per world.";
                return false;
            }

            for (int w = 0; w < worlds; w++)
            {
                string lightError = worldLights[w] == null ? "missing" : worldLights[w].Validate();
                if (lightError != null || worldPosts[w] == null)
                {
                    error = $"WorldScenery: world {w + 1} light or post: {lightError ?? "post missing"}.";
                    return false;
                }
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

            // RT-6v3.3 (D36, which revises RT-6t.13's "no fog"): the palette applier turns the world's linear fog on, in the
            // palette's color (a dark blue of the night factory, never white) and range (far, past the play area).

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
            // The vinyl light after the sun: its warm key replaces the color and intensity above.
            if (vinylLight != null && worldLights != null && world >= 0 && world < worldLights.Length && worldLights[world] != null)
            {
                vinylLight.Apply(worldLights[world]);
            }

            if (post != null && worldPosts != null && world >= 0 && world < worldPosts.Length && worldPosts[world] != null)
            {
                post.sharedProfile = worldPosts[world];
            }

            shown = world;
        }
    }
}
