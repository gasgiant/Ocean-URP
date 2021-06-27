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

            Shader.SetGlobalFloat(fogGradientScaleID, environment.fogGradientScale);
            Shader.SetGlobalFloat(tintGradientScaleID, environment.tintGradientScale);
            Shader.SetGlobalFloat(fogDensityID, environment.fogDensity);
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

        public static readonly int[] TintGradientIDs =
        {
            Shader.PropertyToID("Ocean_TintGradientParams"),
            Shader.PropertyToID("Ocean_TintColor0"),
            Shader.PropertyToID("Ocean_TintColor1"),
            Shader.PropertyToID("Ocean_TintColor2"),
            Shader.PropertyToID("Ocean_TintColor3")
    };

        private static readonly int specCubeID = Shader.PropertyToID("Ocean_SpecCube");

        private static readonly int fogGradientTextureID = Shader.PropertyToID("Ocean_FogGradientTexture");
        private static readonly int sssGradientTextureID = Shader.PropertyToID("Ocean_SssGradientTexture");
        private static readonly int tintGradientTextureID = Shader.PropertyToID("Ocean_TintGradientTexture");
        private static readonly int murkColorID = Shader.PropertyToID("Ocean_MurkColor");
        private static readonly int fogGradientScaleID = Shader.PropertyToID("Ocean_FogGradientScale");
        private static readonly int tintGradientScaleID = Shader.PropertyToID("Ocean_TintGradientScale");
        private static readonly int fogDensityID = Shader.PropertyToID("Ocean_FogDensity");

        private static readonly int skyMapID = Shader.PropertyToID("Ocean_SkyMap");
        private static readonly int bottomHemisphereRadiusID = Shader.PropertyToID("Ocean_BottomHemisphereRadius");
        private static readonly int bottomHemisphereStrengthID = Shader.PropertyToID("Ocean_BottomHemisphereStrength");
        private static readonly int bottomHemisphereColorID = Shader.PropertyToID("Ocean_BottomHemisphereColor");
    }
}
