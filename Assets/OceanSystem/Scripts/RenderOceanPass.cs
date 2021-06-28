using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OceanSystem
{
    public class RenderOceanPass : ScriptableRenderPass
    {
        private OceanRendererFeature.OceanRenderingSettings settings;

        private readonly static ShaderTagId OceanShaderTagId = new ShaderTagId("OceanMain");
        private readonly Material underwaterEffectMaterial;
        private readonly Material skyMapMaterial;
        private FilteringSettings filteringSettings;
        private RenderTexture skyMap;
        private bool skyMapRendered;
        private bool NeedToRenderSkyMap => settings.updateSkyMap || !skyMapRendered;
        private bool NeedCommandBuffer => settings.underwaterEffect || NeedToRenderSkyMap;

        public RenderOceanPass(OceanRendererFeature.OceanRenderingSettings settings)
        {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.all);
            underwaterEffectMaterial = new Material(Shader.Find("Ocean/UnderwaterEffect"));
            skyMapMaterial = new Material(Shader.Find("Ocean/StereographicSky"));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CameraData cameraData = renderingData.cameraData;
            Camera camera = cameraData.camera;
            SetupCameraGlobals(camera);
            SetupGlobalKeywords();

            CommandBuffer cmd = null;
            if (NeedCommandBuffer)
            {
                cmd = CommandBufferPool.Get("Ocean Prepasses");
            }

            if (settings.underwaterEffect)
            {
                cmd.GetTemporaryRT(SubmergenceTextureID, 32, 32, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
                Blit(cmd, -1, SubmergenceTextureID, underwaterEffectMaterial, 0);
                cmd.SetGlobalTexture(CameraSubmergenceTextureID, SubmergenceTextureID);
                Blit(cmd, -1, cameraData.renderer.cameraColorTarget, underwaterEffectMaterial, 1);
            }

            if (NeedToRenderSkyMap) RenderSkyMap(cmd);

            if (cmd != null)
            {
                cmd.SetRenderTarget(cameraData.renderer.cameraColorTarget, cameraData.renderer.cameraDepthTarget);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            DrawingSettings drawingSettings = new DrawingSettings(OceanShaderTagId,
                new SortingSettings(camera));
            drawingSettings.perObjectData = PerObjectData.LightProbe;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (settings.underwaterEffect)
                cmd.ReleaseTemporaryRT(SubmergenceTextureID);
        }

        private void RenderSkyMap(CommandBuffer cmd)
        {
            if (skyMap == null || skyMap.height != settings.skyMapResolution)
            {
                skyMap = new RenderTexture(settings.skyMapResolution, settings.skyMapResolution, 0,
                RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
                skyMap.wrapMode = TextureWrapMode.Clamp;
                skyMap.useMipMap = true;
                skyMap.autoGenerateMips = true;
                skyMap.filterMode = FilterMode.Trilinear;
                skyMap.anisoLevel = 9;
                skyMap.Create();
            }
            Blit(cmd, (RenderTexture)null, skyMap, skyMapMaterial);
            cmd.SetGlobalTexture(skyMapID, skyMap);
            skyMapRendered = true;
        }

        private void SetupCameraGlobals(Camera cam)
        {
            Shader.SetGlobalMatrix(CameraToWorld, cam.cameraToWorldMatrix);
            Shader.SetGlobalMatrix(CameraInverseProjection,
                GL.GetGPUProjectionMatrix(cam.projectionMatrix, false).inverse);
            float height = 2 * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView) * cam.nearClipPlane;
            Vector4 v = new Vector4(height * Screen.width / Screen.height, height, cam.nearClipPlane);
            Shader.SetGlobalVector(CameraNearPlaneParamsID, v);
        }

        private void SetupGlobalKeywords()
        {
            SetGlobalKeyword("OCEAN_TRANSPARENCY_ENABLED", settings.transparency);
            SetGlobalKeyword("OCEAN_UNDERWATER_ENABLED", settings.underwaterEffect);
        }

        private void SetGlobalKeyword(string keyword, bool b)
        {
            if (b)
                Shader.EnableKeyword(keyword);
            else
                Shader.DisableKeyword(keyword);
        }

        private static readonly int SubmergenceTextureID = Shader.PropertyToID("SubmergenceBuffer");
        private static readonly int CameraSubmergenceTextureID = Shader.PropertyToID("Ocean_CameraSubmergenceTexture");

        private static readonly int skyMapID = Shader.PropertyToID("Ocean_SkyMap");
        private static readonly int CameraToWorld = Shader.PropertyToID("Ocean_CameraToWorld");
        private static readonly int CameraInverseProjection = Shader.PropertyToID("Ocean_CameraInverseProjection");
        private static readonly int CameraNearPlaneParamsID = Shader.PropertyToID("Ocean_CameraNearPlaneParams");
    }
}
