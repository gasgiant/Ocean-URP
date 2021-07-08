using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OceanSystem
{
    public class OceanGeometryPass : ScriptableRenderPass
    {
        private readonly static ShaderTagId OceanShaderTagId = new ShaderTagId("OceanMain");
        private readonly static ProfilingSampler _profilingSampler = new ProfilingSampler("Ocean Geometry");
        private readonly OceanRenderer.OceanRenderingSettings _settings;
        private FilteringSettings _filteringSettings;

        public OceanGeometryPass(OceanRenderer.OceanRenderingSettings settings)
        {
            _settings = settings;
            _filteringSettings = new FilteringSettings(RenderQueueRange.all);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!OceanRenderer.IsRendering) return;
#endif

            CameraData cameraData = renderingData.cameraData;
            if (!OceanRenderer.IsCorrectCameraType(cameraData.cameraType)) return;
            Camera camera = cameraData.camera;

            DrawingSettings drawingSettings = new DrawingSettings(OceanShaderTagId,
                new SortingSettings(camera));

            CommandBuffer cmd = CommandBufferPool.Get();
            SetupGlobalKeywords(cmd);
            SetupCameraGlobals(cmd, camera);
            context.ExecuteCommandBuffer(cmd);

            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                drawingSettings.perObjectData = PerObjectData.LightProbe;
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
            }
            context.ExecuteCommandBuffer(cmd);

            
            CommandBufferPool.Release(cmd);
        }

        private void SetupCameraGlobals(CommandBuffer cmd, Camera cam)
        {
            cmd.SetGlobalMatrix(GlobalShaderVariables.InverseViewMatrix, cam.cameraToWorldMatrix);
            cmd.SetGlobalMatrix(GlobalShaderVariables.InverseProjectionViewMatrix,
                GL.GetGPUProjectionMatrix(cam.projectionMatrix, false).inverse);
        }

        private void SetupGlobalKeywords(CommandBuffer cmd)
        {
            SetGlobalKeyword(cmd, "OCEAN_TRANSPARENCY_ENABLED", _settings.transparency);
            SetGlobalKeyword(cmd, "OCEAN_UNDERWATER_ENABLED", _settings.underwaterEffect);
        }

        private void SetGlobalKeyword(CommandBuffer cmd, string keyword, bool b)
        {
            if (b)
                cmd.EnableShaderKeyword(keyword);
            else
                cmd.DisableShaderKeyword(keyword);
        }

        private static class GlobalShaderVariables
        {
            public static readonly int InverseViewMatrix = Shader.PropertyToID("Ocean_InverseViewMatrix");
            public static readonly int InverseProjectionViewMatrix = Shader.PropertyToID("Ocean_InverseProjectionMatrix");
        }
    }

    public class OceanUnderwaterEffectPass : ScriptableRenderPass
    {
        private readonly OceanRenderer.OceanRenderingSettings _settings;
        private readonly Material _underwaterEffectMaterial;
        private RenderTargetIdentifier _submergenceTarget;
        private static readonly int _submergenceTargetID = Shader.PropertyToID("SubmergenceTarget");
        private static readonly int _cameraSubmergenceTextureID = Shader.PropertyToID("Ocean_CameraSubmergenceTexture");

        public OceanUnderwaterEffectPass(OceanRenderer.OceanRenderingSettings settings)
        {
            _settings = settings;
            _underwaterEffectMaterial = new Material(Shader.Find("Hidden/Ocean/UnderwaterEffect"));
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(_submergenceTargetID, 32, 32, 0, FilterMode.Bilinear, RenderTextureFormat.R8, RenderTextureReadWrite.Linear, 1);
            _submergenceTarget = new RenderTargetIdentifier(_submergenceTargetID);
            ConfigureTarget(_submergenceTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!OceanRenderer.IsRendering) return;
#endif

            CameraData cameraData = renderingData.cameraData;
            if (!OceanRenderer.IsCorrectCameraType(cameraData.cameraType)) return;

            if (_settings.underwaterEffect)
            {
                CommandBuffer cmd = CommandBufferPool.Get("Underwater Effect");
                RenderingUtils.DrawPreceduralFullscreenQuad(cmd, _submergenceTarget,
                    RenderBufferLoadAction.DontCare, _underwaterEffectMaterial, 0);
                cmd.SetGlobalTexture(_cameraSubmergenceTextureID, _submergenceTargetID);

                RenderingUtils.DrawPreceduralFullscreenQuad(cmd, cameraData.renderer.cameraColorTarget,
                    RenderBufferLoadAction.Load, _underwaterEffectMaterial, 1);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_submergenceTargetID);
        }
    }

    public class OceanSkyMapPass : ScriptableRenderPass
    {
        private OceanRenderer.OceanRenderingSettings _settings;

        private static readonly int _skyMapID = Shader.PropertyToID("Ocean_SkyMap");
        private readonly Material _skyMapMaterial;
        private RenderTexture _skyMap;
        private bool _skyMapRendered;
        private bool NeedToRenderSkyMap => _settings.updateSkyMap || !_skyMapRendered;

        public OceanSkyMapPass(OceanRenderer.OceanRenderingSettings settings)
        {
            this._settings = settings;
            _skyMapMaterial = new Material(Shader.Find("Hidden/Ocean/StereographicSky"));
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (NeedToRenderSkyMap)
            {
                CreateSkyMapTexture();
                ConfigureTarget(_skyMap);
            } 
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!OceanRenderer.IsRendering) return;
#endif

            if (NeedToRenderSkyMap)
            {
                CameraData cameraData = renderingData.cameraData;
                if (!OceanRenderer.IsCorrectCameraType(cameraData.cameraType)) return;

                CommandBuffer cmd = CommandBufferPool.Get("Ocean Sky Map");
                RenderSkyMap(cmd);
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        private void RenderSkyMap(CommandBuffer cmd)
        {
            Blit(cmd, (RenderTexture)null, _skyMap, _skyMapMaterial, 0);
            cmd.SetGlobalTexture(_skyMapID, _skyMap);
            _skyMapRendered = true;
        }

        private void CreateSkyMapTexture()
        {
            if (_skyMap == null || _skyMap.height != _settings.skyMapResolution)
            {
                if (_skyMap != null)
                    _skyMap.Release();
                _skyMap = new RenderTexture(_settings.skyMapResolution, _settings.skyMapResolution, 0, 
                    RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear)
                {
                    name = "SkyMap",
                    antiAliasing = 1,
                    wrapMode = TextureWrapMode.Clamp,
                    useMipMap = true,
                    autoGenerateMips = true,
                    filterMode = FilterMode.Trilinear,
                    anisoLevel = 9
                };
                _skyMap.Create();
            }
        }
    }
}
