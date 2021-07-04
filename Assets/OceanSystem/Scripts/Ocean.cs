using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    [ExecuteAlways]
    public class Ocean : MonoBehaviour
    {
        public enum OceanReflectionsMode { Default, RealtimeProbe, Custom }

        [Header("Simulation")]
        [SerializeField] private OceanSimulationSettings _simulationSettings;
        [SerializeField] private OceanSimulationInputsProvider _waveInputsProvider;

        [Range(0, 360)]
        [SerializeField] private float _windDirection;
        [Range(0, 360)]
        [SerializeField] private float _swellDirection;
        [Range(0, 1)]
        [SerializeField] private float _windForce;

        [Header("Rendering")]
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

        public OceanCollision Collision => _oceanSimulation.Collision;

        private OceanSimulation _oceanSimulation;
        private GameObject _meshObject;
        private MeshFilter _meshFilter;
        private Vector2Int _currentMeshParams = -Vector2Int.one;

        private void OnDestroy()
        {
            ReleaseSimulation();
        }

        private void Update()
        {
#if UNITY_EDITOR
            EditorSetup();
            if (!OceanRenderer.IsRendering) return;
#else
            Setup();
#endif
            UpdateSimulation();
            UpdateMesh();
            ConfigureMaterial();
            SetEnvironmentSpecCube();
            SetGlobalColorVariables();
        }

        private void Setup()
        {
            if (_oceanSimulation == null)
                _oceanSimulation = new OceanSimulation(_simulationSettings);
            if (!_meshObject)
                InstantiateMeshObject();
        }

#if UNITY_EDITOR
        private void EditorSetup()
        {
            if (OceanRenderer.IsRendering)
            {
                Setup();
            }
            else
            {
                if (_oceanSimulation != null)
                    ReleaseSimulation();
                if (_meshObject)
                    DestroyImmediate(_meshObject);
            }
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                ReleaseSimulation();
        }
#endif

        private void ReleaseSimulation()
        {
            _oceanSimulation?.ReleaseResources();
            _oceanSimulation = null;
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

        private void UpdateSimulation()
        {
            _oceanSimulation.SimulationSettings = _simulationSettings;
            _oceanSimulation.InputsProvider = _waveInputsProvider;
            _oceanSimulation.LocalWindDirection = _windDirection;
            _oceanSimulation.SwellDirection = _swellDirection;
            _oceanSimulation.WindForce01 = _windForce;

            _oceanSimulation.Update();
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
        public static readonly int FogColor = Shader.PropertyToID("_FogColor");
        public static readonly int SssColor = Shader.PropertyToID("_SssColor");
        public static readonly int DiffuseColor = Shader.PropertyToID("_DiffuseColor");
        public static readonly int TintDepthScale = Shader.PropertyToID("_TintDepthScale");
        public static readonly int FogDensity = Shader.PropertyToID("_FogDensity");
        public static readonly int[] TintGradient =
        {
            Shader.PropertyToID("_TintGradientParams"),
            Shader.PropertyToID("_TintColor0"),
            Shader.PropertyToID("_TintColor1"),
            Shader.PropertyToID("_TintColor2"),
            Shader.PropertyToID("_TintColor3"),
            Shader.PropertyToID("_TintColor4"),
            Shader.PropertyToID("_TintColor5"),
            Shader.PropertyToID("_TintColor6"),
            Shader.PropertyToID("_TintColor7")
        };

        public static readonly int DownwardReflectionsColor = Shader.PropertyToID("_DownwardReflectionsColor");
        public static readonly int DownwardReflectionsRadius = Shader.PropertyToID("_DownwardReflectionsRadius");
        public static readonly int DownwardReflectionsSharpness = Shader.PropertyToID("_DownwardReflectionsSharpness");
    }

    public static class OceanGlobalProps
    {
        public static readonly int SpecCube = Shader.PropertyToID("Ocean_SpecCube");
        public static readonly int FogColor = Shader.PropertyToID("Ocean_FogColor");
        public static readonly int SssColor = Shader.PropertyToID("Ocean_SssColor");
        public static readonly int DiffuseColor = Shader.PropertyToID("Ocean_DiffuseColor");
        public static readonly int TintDepthScale = Shader.PropertyToID("Ocean_TintDepthScale");
        public static readonly int FogDensity = Shader.PropertyToID("Ocean_FogDensity");
        public static readonly int[] TintGradient =
        {
            Shader.PropertyToID("Ocean_TintGradientParams"),
            Shader.PropertyToID("Ocean_TintColor0"),
            Shader.PropertyToID("Ocean_TintColor1"),
            Shader.PropertyToID("Ocean_TintColor2"),
            Shader.PropertyToID("Ocean_TintColor3"),
            Shader.PropertyToID("Ocean_TintColor4"),
            Shader.PropertyToID("Ocean_TintColor5"),
            Shader.PropertyToID("Ocean_TintColor6"),
            Shader.PropertyToID("Ocean_TintColor7")
        };

        public static readonly int DownwardReflectionsColorID = Shader.PropertyToID("Ocean_DownwardReflectionsColor");
        public static readonly int DownwardReflectionsRadiusID = Shader.PropertyToID("Ocean_DownwardReflectionsRadius");
        public static readonly int DownwardReflectionsSharpnessID = Shader.PropertyToID("Ocean_DownwardReflectionsSharpness");
    }
}


