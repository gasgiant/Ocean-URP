using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OceanSystem
{
    public class OceanRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] private OceanRenderingSettings settings;

        RenderOceanGeometryPass geometryPass;
        RenderOceanSubmergencePass submergencePass;
        RenderOceanUnderwaterEffectPass underwaterPass;
        RenderOceanSkyMapPass skyMapPass;

        public override void Create()
        {
            submergencePass = new RenderOceanSubmergencePass(settings);
            underwaterPass = new RenderOceanUnderwaterEffectPass(settings);
            skyMapPass = new RenderOceanSkyMapPass(settings);
            geometryPass = new RenderOceanGeometryPass(settings);
            name = "Ocean";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            submergencePass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(submergencePass);
            //underwaterPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            //renderer.EnqueuePass(underwaterPass);
            //skyMapPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            //renderer.EnqueuePass(skyMapPass);
            geometryPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(geometryPass);
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
