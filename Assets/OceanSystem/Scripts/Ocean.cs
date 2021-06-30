using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public class Ocean : MonoBehaviour
    {
        public enum OceanReflectionsMode { Default, RealtimeProbe, Custom }

        [SerializeField] private OceanReflectionsMode reflectionsMode;
        [SerializeField] private ReflectionProbe probe;
        [SerializeField] private Cubemap cubemap;
        [SerializeField] private GeoClipmap geoClipmap;
        [SerializeField] private Material material;

        void Start()
        {
            geoClipmap.InstantiateMesh(material);
        }

        void Update()
        {
            SetEnvironmentSpecCube();

            material.SetFloat("_Cull", (float)
                (Shader.IsKeywordEnabled("OCEAN_UNDERWATER_ENABLED") ? CullMode.Off : CullMode.Back));
            SetGlobalColorVariables();
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


