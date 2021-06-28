using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OceanSystem
{
    public class OceanRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private OceanRenderingSettings settings;

        RenderOceanPass renderOceanPass;

        public override void Create()
        {
            renderOceanPass = new RenderOceanPass(settings);
            name = "Ocean";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderOceanPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(renderOceanPass);
        }

        private void OnValidate()
        {
            settings.skyMapResolution = Mathf.Clamp(settings.skyMapResolution, 16, 2048);
        }

        [System.Serializable]
        public class OceanRenderingSettings
        {
            public int skyMapResolution = 256;
            public bool updateSkyMap;
            public bool transparency;
            public bool underwaterEffect;
        }
    }
}
