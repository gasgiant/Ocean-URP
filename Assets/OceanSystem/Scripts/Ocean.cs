using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public class Ocean : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private GeoClipmap geoClipmap;
        [SerializeField] private Material material;
        [SerializeField] private OceanEnvironment environment;

        RenderTexture skyMap;
        Material skyMapMaterial;

        void Start()
        {
            geoClipmap.InstantiateMesh(material);
            Shader.SetGlobalTexture(specCubeID, ReflectionProbe.defaultTexture);

            skyMapMaterial = new Material(Shader.Find("Ocean/StereographicSky"));
            RenderSkyMap();
        }

        void Update()
        {
            material.SetFloat("_Cull", (float)
                (Shader.IsKeywordEnabled("OCEAN_UNDERWATER_ENABLED") ? CullMode.Off : CullMode.Back));
            SetGlobalColorVariables();
        }

        private void SetGlobalColorVariables()
        {
            Shader.SetGlobalVector(FogColorID, material.GetVector(FogColorID));
            Shader.SetGlobalVector(SssColorID, material.GetVector(SssColorID));
            Shader.SetGlobalVector(DiffuseColorID, material.GetVector(DiffuseColorID));
            Shader.SetGlobalFloat(TintDepthScaleID, material.GetFloat(TintDepthScaleID));
            Shader.SetGlobalFloat(FogDensityID, material.GetFloat(FogDensityID));
            for (int i = 0; i < TintGradientIDs.Length; i++)
            {
                Shader.SetGlobalVector(TintGradientIDs[i], material.GetVector(TintGradientIDs[i]));
            }
        }

        private void RenderSkyMap()
        {
            int res = Mathf.Min(2048, 256);
            if (skyMap == null || skyMap.height != res)
            {
                skyMap = new RenderTexture(res, res, 0,
                RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.Linear);
                skyMap.wrapMode = TextureWrapMode.Clamp;
                skyMap.useMipMap = true;
                skyMap.autoGenerateMips = true;
                skyMap.filterMode = FilterMode.Trilinear;
                skyMap.anisoLevel = 9;
                skyMap.Create();
            }
            Shader.SetGlobalVector(bottomHemisphereColorID, environment.bottomHemisphereColor.linear);
            Shader.SetGlobalFloat(bottomHemisphereRadiusID, environment.bottomHemisphereRadius);
            Shader.SetGlobalFloat(bottomHemisphereStrengthID, environment.bottomHemisphereStrength);
            Graphics.Blit(null, skyMap, skyMapMaterial);
            Shader.SetGlobalTexture(skyMapID, skyMap);
        }

        private static readonly int FogColorID = Shader.PropertyToID("Ocean_FogColor");
        private static readonly int SssColorID = Shader.PropertyToID("Ocean_SssColor");
        private static readonly int DiffuseColorID = Shader.PropertyToID("Ocean_DiffuseColor");
        private static readonly int TintDepthScaleID = Shader.PropertyToID("Ocean_TintDepthScale");
        private static readonly int FogDensityID = Shader.PropertyToID("Ocean_FogDensity");
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

        private static readonly int specCubeID = Shader.PropertyToID("Ocean_SpecCube");
        private static readonly int skyMapID = Shader.PropertyToID("Ocean_SkyMap");
        private static readonly int bottomHemisphereRadiusID = Shader.PropertyToID("Ocean_BottomHemisphereRadius");
        private static readonly int bottomHemisphereStrengthID = Shader.PropertyToID("Ocean_BottomHemisphereStrength");
        private static readonly int bottomHemisphereColorID = Shader.PropertyToID("Ocean_BottomHemisphereColor");
    }
}
