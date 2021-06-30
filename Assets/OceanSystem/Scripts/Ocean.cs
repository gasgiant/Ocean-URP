using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    [ExecuteAlways]
    public class Ocean : MonoBehaviour
    {
#if UNITY_EDITOR
        public static bool RenderInEditMode;
        public static bool IsRendering => Application.isPlaying || RenderInEditMode;
#endif

        public enum OceanReflectionsMode { Default, RealtimeProbe, Custom }

        [Header("Rendering")]
        [SerializeField] private Material material;
        [SerializeField] private OceanReflectionsMode reflectionsMode;
        [SerializeField] private ReflectionProbe probe;
        [SerializeField] private Cubemap cubemap;

        [Header("Simulation")]
        [SerializeField] private OceanSimulationSettings simulationSettings;
        [SerializeField] private OceanWavesSettings wavesSettings;
        [SerializeField] private OceanEqualizerPreset equalizerPreset;

        [Header("Mesh")]
        [SerializeField] private Transform viewer;
        [SerializeField] private float minMeshScale = 15;
        [Range(1, 10)]
        [SerializeField] private int clipMapLevels = 7;
        [Range(16, 50)]
        [SerializeField] private int vertexDensity = 25;

        private OceanSimulation oceanSimulation;
        private GameObject meshObject;
        private MeshFilter meshFilter;
        private Vector2Int currentMeshParams = -Vector2Int.one;


        private void OnDisable()
        {
            if (!Application.isPlaying)
                ReleaseSimulation();
        }

        private void OnDestroy()
        {
            ReleaseSimulation();
        }

        private void Update()
        {
#if UNITY_EDITOR
            EditorSetup();
            if (!IsRendering) return;
#else
            Setup();
#endif
            oceanSimulation.SimulationSettings = simulationSettings;
            oceanSimulation.WavesSettings = wavesSettings;
            oceanSimulation.EqualizerPreset = equalizerPreset;
            oceanSimulation.Update();
            UpdateMesh();
            ConfigureMaterial();
            SetEnvironmentSpecCube();
            SetGlobalColorVariables();
        }

        private void Setup()
        {
            if (oceanSimulation == null)
                oceanSimulation = new OceanSimulation(simulationSettings, wavesSettings, equalizerPreset);
            if (!meshObject)
                InstantiateMeshObject();
        }

#if UNITY_EDITOR
        private void EditorSetup()
        {
            if (IsRendering)
            {
                Setup();
            }
            else
            {
                if (oceanSimulation != null)
                    ReleaseSimulation();
                if (meshObject)
                    DestroyImmediate(meshObject);
            }
        }
#endif

        private void ReleaseSimulation()
        {
            oceanSimulation?.ReleaseResources();
            oceanSimulation = null;
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
            meshRenderer.material = material;
            this.meshObject = go;
            this.meshFilter = meshFilter;
        }

        private void UpdateMesh()
        {
            if (!meshObject) return;

            if (!viewer)
                viewer = Camera.main.transform;
            ClipmapMeshBuilder.SetGlobalShaderVariables(viewer.position, vertexDensity, minMeshScale);
            Vector2Int newMeshParams = new Vector2Int(vertexDensity, clipMapLevels);
            if (newMeshParams == currentMeshParams && meshFilter.sharedMesh != null) return;

            currentMeshParams = newMeshParams;
            meshFilter.sharedMesh = ClipmapMeshBuilder.BuildClipMap(vertexDensity, clipMapLevels);
        }

        private void ConfigureMaterial()
        {
            material.SetFloat("_Cull", (float)
                (Shader.IsKeywordEnabled("OCEAN_UNDERWATER_ENABLED") ? CullMode.Off : CullMode.Back));
        }

        private void SetEnvironmentSpecCube()
        {
            if (reflectionsMode == OceanReflectionsMode.RealtimeProbe && probe != null)
            {
                Shader.SetGlobalTexture(OceanShaderPropIds.SpecCubeID, probe.realtimeTexture);
            }
            else if (reflectionsMode == OceanReflectionsMode.Custom && cubemap != null)
            {
                Shader.SetGlobalTexture(OceanShaderPropIds.SpecCubeID, cubemap);
            }
            else
            {
                Shader.SetGlobalTexture(OceanShaderPropIds.SpecCubeID, ReflectionProbe.defaultTexture);
            }
        }

        private void SetGlobalColorVariables()
        {
            Shader.SetGlobalVector(OceanShaderPropIds.FogColorID, material.GetVector(OceanShaderPropIds.FogColorID));
            Shader.SetGlobalVector(OceanShaderPropIds.SssColorID, material.GetVector(OceanShaderPropIds.SssColorID));
            Shader.SetGlobalVector(OceanShaderPropIds.DiffuseColorID, material.GetVector(OceanShaderPropIds.DiffuseColorID));
            Shader.SetGlobalFloat(OceanShaderPropIds.TintDepthScaleID, material.GetFloat(OceanShaderPropIds.TintDepthScaleID));
            Shader.SetGlobalFloat(OceanShaderPropIds.FogDensityID, material.GetFloat(OceanShaderPropIds.FogDensityID));
            for (int i = 0; i < OceanShaderPropIds.TintGradientIDs.Length; i++)
            {
                Shader.SetGlobalVector(OceanShaderPropIds.TintGradientIDs[i], material.GetVector(OceanShaderPropIds.TintGradientIDs[i]));
            }

            Shader.SetGlobalVector(OceanShaderPropIds.DownwardReflectionsColorID, material.GetVector(OceanShaderPropIds.DownwardReflectionsColorID));
            Shader.SetGlobalFloat(OceanShaderPropIds.DownwardReflectionsRadiusID, material.GetFloat(OceanShaderPropIds.DownwardReflectionsRadiusID));
            Shader.SetGlobalFloat(OceanShaderPropIds.DownwardReflectionsSharpnessID, material.GetFloat(OceanShaderPropIds.DownwardReflectionsSharpnessID));
        }
    }

    public static class OceanShaderPropIds
    {
        public static readonly int FogColorID = Shader.PropertyToID("Ocean_FogColor");
        public static readonly int SssColorID = Shader.PropertyToID("Ocean_SssColor");
        public static readonly int DiffuseColorID = Shader.PropertyToID("Ocean_DiffuseColor");
        public static readonly int TintDepthScaleID = Shader.PropertyToID("Ocean_TintDepthScale");
        public static readonly int FogDensityID = Shader.PropertyToID("Ocean_FogDensity");
        public static readonly int[] TintGradientIDs =
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

        public static readonly int SpecCubeID = Shader.PropertyToID("Ocean_SpecCube");
        public static readonly int DownwardReflectionsColorID = Shader.PropertyToID("Ocean_DownwardReflectionsColor");
        public static readonly int DownwardReflectionsRadiusID = Shader.PropertyToID("Ocean_DownwardReflectionsRadius");
        public static readonly int DownwardReflectionsSharpnessID = Shader.PropertyToID("Ocean_DownwardReflectionsSharpness");
    }
}


