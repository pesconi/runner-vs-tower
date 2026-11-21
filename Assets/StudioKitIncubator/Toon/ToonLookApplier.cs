using UnityEngine;

namespace JungleDegree.StudioKit.Toon
{
    /// <summary>
    /// Writes a <see cref="ToonLook"/> into the global shader values read by the toon shaders. The ramp texture is
    /// created once when the component is enabled (scene load) and released when it is disabled.
    /// </summary>
    public sealed class ToonLookApplier : MonoBehaviour
    {
        [SerializeField] private ToonLook look;

        private Texture2D ramp;

        public ToonLook Look => look;

        private void OnEnable()
        {
            string error = look == null ? "no look assigned" : look.Validate();
            if (error != null)
            {
                Debug.LogError("[ToonLook] " + error, this);
                enabled = false;
                return;
            }

            ramp = ToonLook.CreateRampTexture(look.RampLevels);
            Shader.SetGlobalTexture(ToonShaderIds.Ramp, ramp);
            Shader.SetGlobalColor(ToonShaderIds.SkyLight, look.SkyLight);
            Shader.SetGlobalColor(ToonShaderIds.GroundLight, look.GroundLight);
            Shader.SetGlobalFloat(ToonShaderIds.AmbientIntensity, look.AmbientIntensity);
            Shader.SetGlobalFloat(ToonShaderIds.DirectIntensity, look.DirectIntensity);
        }

        private void OnDisable()
        {
            if (ramp != null)
            {
                Shader.SetGlobalTexture(ToonShaderIds.Ramp, Texture2D.whiteTexture);
                Destroy(ramp); // load-time: the ramp lives exactly as long as this component (rule 2.2.1)
                ramp = null;
            }
        }
    }
}
