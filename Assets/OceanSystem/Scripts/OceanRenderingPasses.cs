using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OceanSystem
{
    public class OceanGeometryPass : ScriptableRenderPass
    {
        private OceanRenderer.OceanRenderingSettings settings;

        private readonly static ShaderTagId OceanShaderTagId = new ShaderTagId("OceanMain");
        private FilteringSettings filteringSettings;
        private RenderStateBlock renderStateBlock;

        public OceanGeometryPass(OceanRenderer.OceanRenderingSettings settings)
        {
            this.settings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.all);
            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CameraData cameraData = renderingData.cameraData;
            Camera camera = cameraData.camera;
            
            DrawingSettings drawingSettings = new DrawingSettings(OceanShaderTagId,
                new SortingSettings(camera));

            CommandBuffer cmd = CommandBufferPool.Get();
            SetupGlobalKeywords(cmd);
            SetupCameraGlobals(cmd, camera);
            context.ExecuteCommandBuffer(cmd);

            using (new ProfilingScope(cmd, new ProfilingSampler("Ocean Geometry")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                drawingSettings.perObjectData = PerObjectData.LightProbe;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
            }
            context.ExecuteCommandBuffer(cmd);

            
            CommandBufferPool.Release(cmd);
        }

        private void SetupCameraGlobals(CommandBuffer cmd, Camera cam)
        {
            cmd.SetGlobalMatrix(CameraToWorld, cam.cameraToWorldMatrix);
            cmd.SetGlobalMatrix(CameraInverseProjection,
                GL.GetGPUProjectionMatrix(cam.projectionMatrix, false).inverse);
            float height = 2 * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView) * cam.nearClipPlane;
            Vector4 v = new Vector4(height * Screen.width / Screen.height, height, cam.nearClipPlane);
            cmd.SetGlobalVector(CameraNearPlaneParamsID, v);
        }

        private void SetupGlobalKeywords(CommandBuffer cmd)
        {
            SetGlobalKeyword(cmd, "OCEAN_TRANSPARENCY_ENABLED", settings.transparency);
            SetGlobalKeyword(cmd, "OCEAN_UNDERWATER_ENABLED", settings.underwaterEffect);
        }

        private void SetGlobalKeyword(CommandBuffer cmd, string keyword, bool b)
        {
            if (b)
                cmd.EnableShaderKeyword(keyword);
            else
                cmd.DisableShaderKeyword(keyword);
        }

        private static readonly int CameraToWorld = Shader.PropertyToID("Ocean_CameraToWorld");
        private static readonly int CameraInverseProjection = Shader.PropertyToID("Ocean_CameraInverseProjection");
        private static readonly int CameraNearPlaneParamsID = Shader.PropertyToID("Ocean_CameraNearPlaneParams");
    }

    public class OceanUnderwaterEffectPass : ScriptableRenderPass
    {
        private OceanRenderer.OceanRenderingSettings settings;
        private readonly Material underwaterEffectMaterial;
        private RenderTargetIdentifier submergenceTarget;
        private static readonly int SubmergenceTargetID = Shader.PropertyToID("SubmergenceTarget");
        private static readonly int CameraSubmergenceTextureID = Shader.PropertyToID("Ocean_CameraSubmergenceTexture");

        public OceanUnderwaterEffectPass(OceanRenderer.OceanRenderingSettings settings)
        {
            this.settings = settings;
            underwaterEffectMaterial = new Material(Shader.Find("Ocean/UnderwaterEffect"));
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(SubmergenceTargetID, 32, 32, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
            submergenceTarget = new RenderTargetIdentifier(SubmergenceTargetID);
            ConfigureTarget(submergenceTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.underwaterEffect)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Underwater Effect");
                OceanRenderingUtils.DrawPreceduralFullscreenQuad(cmd, submergenceTarget,
                    RenderBufferLoadAction.DontCare, underwaterEffectMaterial, 0);
                cmd.SetGlobalTexture(CameraSubmergenceTextureID, SubmergenceTargetID);

                OceanRenderingUtils.DrawPreceduralFullscreenQuad(cmd, renderingData.cameraData.renderer.cameraColorTarget,
                    RenderBufferLoadAction.Load, underwaterEffectMaterial, 1);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(SubmergenceTargetID);
        }
    }

    public class OceanSkyMapPass : ScriptableRenderPass
    {
        private OceanRenderer.OceanRenderingSettings settings;

        private static readonly int SkyMapID = Shader.PropertyToID("Ocean_SkyMap");
        private readonly Material skyMapMaterial;
        private RenderTexture skyMap;
        private bool skyMapRendered;
        private bool NeedToRenderSkyMap => settings.updateSkyMap || !skyMapRendered;

        public OceanSkyMapPass(OceanRenderer.OceanRenderingSettings settings)
        {
            this.settings = settings;
            skyMapMaterial = new Material(Shader.Find("Ocean/StereographicSky"));
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (NeedToRenderSkyMap)
            {
                CreateSkyMapTexture();
                ConfigureTarget(skyMap);
            } 
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (NeedToRenderSkyMap)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Ocean Sky Map");
                RenderSkyMap(cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        private void RenderSkyMap(CommandBuffer cmd)
        {
            OceanRenderingUtils.DrawPreceduralFullscreenQuad(cmd, skyMap, 
                RenderBufferLoadAction.DontCare, skyMapMaterial, 0);
            cmd.SetGlobalTexture(SkyMapID, skyMap);
            skyMapRendered = true;
        }

        private void CreateSkyMapTexture()
        {
            if (skyMap == null || skyMap.height != settings.skyMapResolution)
            {
                if (skyMap != null)
                    skyMap.Release();
                skyMap = new RenderTexture(settings.skyMapResolution, settings.skyMapResolution, 0,
                    RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
                skyMap.name = "SkyMap";
                skyMap.antiAliasing = 1;
                skyMap.wrapMode = TextureWrapMode.Clamp;
                skyMap.useMipMap = true;
                skyMap.autoGenerateMips = true;
                skyMap.filterMode = FilterMode.Trilinear;
                skyMap.anisoLevel = 9;
                skyMap.Create();
            }
        }
    }

    public static class OceanRenderingUtils
    {
        public static void DrawPreceduralFullscreenQuad(CommandBuffer cmd, RenderTargetIdentifier target,
            RenderBufferLoadAction loadAction, Material material, int pass)
        {
            cmd.SetRenderTarget(target, loadAction, RenderBufferStoreAction.Store);
            cmd.DrawProcedural(Matrix4x4.identity, material, pass, MeshTopology.Quads, 4, 1, null);
        }
    }
}
