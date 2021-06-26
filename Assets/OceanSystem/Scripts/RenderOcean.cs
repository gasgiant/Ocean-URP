using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OceanSystem
{
    public class RenderOcean : ScriptableRendererFeature
    {
        RenderOceanPass renderOceanPass;

        public override void Create()
        {
            renderOceanPass = new RenderOceanPass();
            name = "Ocean";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderOceanPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(renderOceanPass);
        }
    }

    public class RenderOceanPass : ScriptableRenderPass
    {
        private readonly static ShaderTagId OceanShaderTagId = new ShaderTagId("OceanMain");
        private readonly Material underwaterEffectMaterial;
        private FilteringSettings filteringSettings;

        public RenderOceanPass()
        {
            filteringSettings = new FilteringSettings(RenderQueueRange.all);
            underwaterEffectMaterial = new Material(Shader.Find("Ocean/UnderwaterEffect"));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CameraData cameraData = renderingData.cameraData;
            Camera camera = cameraData.camera;
            SetupCameraGlobals(camera);

            CommandBuffer cmd = CommandBufferPool.Get("Ocean Prepasses");
            cmd.GetTemporaryRT(submergenceTextureID, 32, 32, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear);
            Blit(cmd, -1, submergenceTextureID, underwaterEffectMaterial, 0);
            cmd.SetGlobalTexture(cameraSubmergenceTextureID, submergenceTextureID);
            Blit(cmd, -1, cameraData.renderer.cameraColorTarget, underwaterEffectMaterial, 1);
            cmd.SetRenderTarget(cameraData.renderer.cameraColorTarget, cameraData.renderer.cameraDepthTarget);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            
            DrawingSettings drawingSettings = new DrawingSettings(OceanShaderTagId,
                new SortingSettings(camera));
            
            drawingSettings.perObjectData = PerObjectData.LightProbe;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(submergenceTextureID);
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

        static readonly int submergenceTextureID = Shader.PropertyToID("SubmergenceBuffer");
        static readonly int cameraSubmergenceTextureID = Shader.PropertyToID("Ocean_CameraSubmergenceTexture");

        private readonly int CameraToWorld = Shader.PropertyToID("Ocean_CameraToWorld");
        private readonly int CameraInverseProjection = Shader.PropertyToID("Ocean_CameraInverseProjection");
        static readonly int CameraNearPlaneParamsID = Shader.PropertyToID("Ocean_CameraNearPlaneParams");
    }
}
