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

        [System.Serializable]
        public class OceanRenderingSettings
        {
            public bool transparency;
            public bool underwaterEffect;
        }
    }
}
