using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OceanSystem
{
    public class OceanRendererFeature : ScriptableRendererFeature
    {
        public static bool IsCorrectCameraType(CameraType t) => t == CameraType.Game 
            || t == CameraType.SceneView || t == CameraType.VR;

        [SerializeField] private OceanRenderingSettings _settings;

        private OceanGeometryPass _geometryPass;
        private OceanUnderwaterEffectPass _underwaterPass;
        private OceanSkyMapPass _skyMapPass;

        public override void Create()
        {
            _underwaterPass = new OceanUnderwaterEffectPass(_settings);
            _skyMapPass = new OceanSkyMapPass(_settings);
            _geometryPass = new OceanGeometryPass(_settings);
            name = "Ocean";
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _skyMapPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(_skyMapPass);
            _underwaterPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(_underwaterPass);
            _geometryPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
            renderer.EnqueuePass(_geometryPass);
        }

        private void OnValidate()
        {
            _settings.skyMapResolution = Mathf.Clamp(_settings.skyMapResolution, 16, 2048);
        }

        [System.Serializable]
        public class OceanRenderingSettings
        {
            public int skyMapResolution = 256;
            public bool updateSkyMap;
            public bool transparency;
            public bool underwaterEffect;
        }

#if UNITY_EDITOR
        public const string RenderInEditModePrefName = "RenderOceanInEditMode";
        public static bool RenderInEditMode
        {
            get
            {
                if (_renderInEditMode == null)
                    _renderInEditMode = EditorPrefs.GetBool(RenderInEditModePrefName);
                return _renderInEditMode.Value;
            }

            set
            {
                _renderInEditMode = value;
            }
        }
        private static bool? _renderInEditMode = null;
        public static bool IsRendering => Application.isPlaying || RenderInEditMode;
#endif
    }
}
