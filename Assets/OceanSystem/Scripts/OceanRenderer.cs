using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    [ExecuteAlways]
    public class OceanRenderer : MonoBehaviour
    {
        public enum OceanReflectionsMode { Default, RealtimeProbe, Custom }

        [SerializeField] private Material _material;
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
            if (!_material)
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
                Shader.SetGlobalTexture(OceanGlobalProps.SpecCube, _probe.realtimeTexture);
            }
            else if (_reflectionsMode == OceanReflectionsMode.Custom && _cubemap != null)
            {
                Shader.SetGlobalTexture(OceanGlobalProps.SpecCube, _cubemap);
            }
            else
            {
                Shader.SetGlobalTexture(OceanGlobalProps.SpecCube, ReflectionProbe.defaultTexture);
            }
        }

        private void SetGlobalColorVariables()
        {
            Shader.SetGlobalVector(OceanGlobalProps.FogColor, _material.GetVector(OceanMaterialProps.FogColor));
            Shader.SetGlobalVector(OceanGlobalProps.SssColor, _material.GetVector(OceanMaterialProps.SssColor));
            Shader.SetGlobalVector(OceanGlobalProps.DiffuseColor, _material.GetVector(OceanMaterialProps.DiffuseColor));
            Shader.SetGlobalFloat(OceanGlobalProps.TintDepthScale, _material.GetFloat(OceanMaterialProps.TintDepthScale));
            Shader.SetGlobalFloat(OceanGlobalProps.FogDensity, _material.GetFloat(OceanMaterialProps.FogDensity));
            for (int i = 0; i < OceanGlobalProps.TintGradient.Length; i++)
            {
                Shader.SetGlobalVector(OceanGlobalProps.TintGradient[i], _material.GetVector(OceanMaterialProps.TintGradient[i]));
            }

            Shader.SetGlobalVector(OceanGlobalProps.DownwardReflectionsColorID, _material.GetVector(OceanMaterialProps.DownwardReflectionsColor));
            Shader.SetGlobalFloat(OceanGlobalProps.DownwardReflectionsRadiusID, _material.GetFloat(OceanMaterialProps.DownwardReflectionsRadius));
            Shader.SetGlobalFloat(OceanGlobalProps.DownwardReflectionsSharpnessID, _material.GetFloat(OceanMaterialProps.DownwardReflectionsSharpness));
        }
    }

    public static class OceanMaterialProps
    {
        public static readonly int FogColor = Shader.PropertyToID("_DeepScatterColor");
        public static readonly int SssColor = Shader.PropertyToID("_SssColor");
        public static readonly int DiffuseColor = Shader.PropertyToID("_DiffuseColor");
        public static readonly int TintDepthScale = Shader.PropertyToID("_AbsorptionDepthScale");
        public static readonly int FogDensity = Shader.PropertyToID("_FogDensity");
        public static readonly int[] TintGradient =
        {
            Shader.PropertyToID("_AbsorptionGradientParams"),
            Shader.PropertyToID("_AbsorptionColor0"),
            Shader.PropertyToID("_AbsorptionColor1"),
            Shader.PropertyToID("_AbsorptionColor2"),
            Shader.PropertyToID("_AbsorptionColor3"),
            Shader.PropertyToID("_AbsorptionColor4"),
            Shader.PropertyToID("_AbsorptionColor5"),
            Shader.PropertyToID("_AbsorptionColor6"),
            Shader.PropertyToID("_AbsorptionColor7")
        };

        public static readonly int DownwardReflectionsColor = Shader.PropertyToID("_DownwardReflectionsColor");
        public static readonly int DownwardReflectionsRadius = Shader.PropertyToID("_DownwardReflectionsRadius");
        public static readonly int DownwardReflectionsSharpness = Shader.PropertyToID("_DownwardReflectionsSharpness");
    }

    public static class OceanGlobalProps
    {
        public static readonly int SpecCube = Shader.PropertyToID("Ocean_SpecCube");
        public static readonly int FogColor = Shader.PropertyToID("Ocean_DeepScatterColor");
        public static readonly int SssColor = Shader.PropertyToID("Ocean_SssColor");
        public static readonly int DiffuseColor = Shader.PropertyToID("Ocean_DiffuseColor");
        public static readonly int TintDepthScale = Shader.PropertyToID("Ocean_AbsorptionDepthScale");
        public static readonly int FogDensity = Shader.PropertyToID("Ocean_FogDensity");
        public static readonly int[] TintGradient =
        {
            Shader.PropertyToID("Ocean_AbsorptionGradientParams"),
            Shader.PropertyToID("Ocean_AbsorptionColor0"),
            Shader.PropertyToID("Ocean_AbsorptionColor1"),
            Shader.PropertyToID("Ocean_AbsorptionColor2"),
            Shader.PropertyToID("Ocean_AbsorptionColor3"),
            Shader.PropertyToID("Ocean_AbsorptionColor4"),
            Shader.PropertyToID("Ocean_AbsorptionColor5"),
            Shader.PropertyToID("Ocean_AbsorptionColor6"),
            Shader.PropertyToID("Ocean_AbsorptionColor7")
        };

        public static readonly int DownwardReflectionsColorID = Shader.PropertyToID("Ocean_DownwardReflectionsColor");
        public static readonly int DownwardReflectionsRadiusID = Shader.PropertyToID("Ocean_DownwardReflectionsRadius");
        public static readonly int DownwardReflectionsSharpnessID = Shader.PropertyToID("Ocean_DownwardReflectionsSharpness");
    }
}


