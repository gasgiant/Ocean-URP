using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    [ExecuteAlways]
    public class OceanRenderer : MonoBehaviour
    {
        public enum OceanReflectionsMode { Default, RealtimeProbe, Custom }

        [SerializeField] private Material _material;
        [SerializeField] private OceanColorsPreset _colorsPreset;
        [SerializeField] private OceanReflectionsMode _reflectionsMode;
        [SerializeField] private ReflectionProbe _probe;
        [SerializeField] private Cubemap _cubemap;

        [Header("Mesh")]
        [SerializeField] private Transform _viewer;
        [SerializeField] private float _minMeshScale = 15;
        [Range(1, 10)]
        [SerializeField] private int _clipMapLevels = 7;
        [Range(16, 50)]
        [SerializeField] private int _vertexDensity = 25;

        private GameObject _meshObject;
        private MeshFilter _meshFilter;
        private Vector2Int _currentMeshParams = -Vector2Int.one;

        private void Update()
        {
            bool initialized;
#if UNITY_EDITOR
            initialized = EditorSetup();
#else
            initialized = Setup();
#endif
            if (initialized)
            {
                UpdateMesh();
                ConfigureMaterial();
                SetEnvironmentSpecCube();
                SetGlobalColorVariables();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private bool Setup()
        {
            if (!_material || !_colorsPreset)
            {
                Cleanup();
                return false;
            }

            if (!_meshObject)
                InstantiateMeshObject();

            return true;
        }

#if UNITY_EDITOR
        private bool EditorSetup()
        {
            bool initialized = false;
            if (OceanRendererFeature.IsRendering)
            {
                initialized = Setup();
            }
            else
            {
                if (_meshObject)
                    DestroyImmediate(_meshObject);
            }

            return initialized && OceanRendererFeature.IsRendering;
        }
#endif

        private void Cleanup()
        {
            if (_meshObject)
            {
#if UNITY_EDITOR
                DestroyImmediate(_meshObject);
#else
                Destroy(_meshObject);
#endif
            }
        }

        private void InstantiateMeshObject()
        {
            GameObject go = new GameObject();
            go.name = "Ocean Mesh";
            go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            go.layer = LayerMask.NameToLayer("Water");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            MeshFilter meshFilter = go.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            meshRenderer.allowOcclusionWhenDynamic = false;
            meshRenderer.material = _material;
            _meshObject = go;
            _meshFilter = meshFilter;
        }

        private void UpdateMesh()
        {
            if (!_meshObject) return;

            if (!_viewer)
                _viewer = Camera.main.transform;
            ClipmapMeshBuilder.SetGlobalShaderVariables(_viewer.position, _vertexDensity, _minMeshScale);
            Vector2Int newMeshParams = new Vector2Int(_vertexDensity, _clipMapLevels);
            if (newMeshParams == _currentMeshParams && _meshFilter.sharedMesh != null) return;

            _currentMeshParams = newMeshParams;
            _meshFilter.sharedMesh = ClipmapMeshBuilder.BuildClipMap(_vertexDensity, _clipMapLevels);
        }

        private void ConfigureMaterial()
        {
            _material.SetFloat("_Cull", (float)
                (Shader.IsKeywordEnabled("OCEAN_UNDERWATER_ENABLED") ? CullMode.Off : CullMode.Back));
        }

        private void SetEnvironmentSpecCube()
        {
            if (_reflectionsMode == OceanReflectionsMode.RealtimeProbe && _probe != null)
            {
                Shader.SetGlobalTexture(GlobalShaderVariables.Misc.SpecCube, _probe.realtimeTexture);
            }
            else if (_reflectionsMode == OceanReflectionsMode.Custom && _cubemap != null)
            {
                Shader.SetGlobalTexture(GlobalShaderVariables.Misc.SpecCube, _cubemap);
            }
            else
            {
                Shader.SetGlobalTexture(GlobalShaderVariables.Misc.SpecCube, ReflectionProbe.defaultTexture);
            }
        }

        private void SetGlobalColorVariables()
        {
            Shader.SetGlobalVector(GlobalShaderVariables.Colors.DeepScatterColor, _colorsPreset.DeepScatter);
            Shader.SetGlobalVector(GlobalShaderVariables.Colors.ShallowScatterColor, _colorsPreset.ShallowScatter);
            Shader.SetGlobalVector(GlobalShaderVariables.Colors.DiffuseColor, _colorsPreset.Diffuse);
            Shader.SetGlobalVector(GlobalShaderVariables.Colors.ReflectionMaskColor, _colorsPreset.ReflectionMask);
            SetGlobalGradient(GlobalShaderVariables.Colors.AbsorbtionGradient, _colorsPreset.Absorbtion);

            Shader.SetGlobalFloat(GlobalShaderVariables.Misc.FogDensity, _material.GetFloat(MaterialProps.FogDensity));
            Shader.SetGlobalFloat(GlobalShaderVariables.Misc.AbsorbtionDepthScale, _material.GetFloat(MaterialProps.AbsorbtionDepthScale));
            Shader.SetGlobalFloat(GlobalShaderVariables.Misc.ReflectionMaskRadius, _material.GetFloat(MaterialProps.ReflectionMaskRadius));
            Shader.SetGlobalFloat(GlobalShaderVariables.Misc.ReflectionMaskSharpness, _material.GetFloat(MaterialProps.ReflectionMaskSharpness));
        }

        private void SetGlobalGradient(int[] propIDs, Gradient grad)
        {
            for (int i = 0; i < grad.colorKeys.Length; i++)
            {
                Vector4 v = grad.colorKeys[i].color.linear;
                v.w = grad.colorKeys[i].time;
                Shader.SetGlobalVector(propIDs[i + 1], v);
            }
            Shader.SetGlobalVector(propIDs[0],
                new Vector2(grad.colorKeys.Length, (float)grad.mode));
        }
    }

    
}


