using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public class Ocean : MonoBehaviour
    {
        [SerializeField] private GeoClipmap geoClipmap;
        [SerializeField] private Material material;
        [SerializeField] private OceanEnvironment environment;

        private Material skyMapMaterial;
        private RenderTexture skyMap;

        void Start()
        {
            geoClipmap.InstantiateMesh(material);
            skyMapMaterial = new Material(Shader.Find("Ocean/StereographicSky"));
            
        }

        void Update()
        {
            Shader.SetGlobalTexture(fogGradientTextureID, environment.GetFogTex());
            Shader.SetGlobalTexture(sssGradientTextureID, environment.GetSssTex());
            Shader.SetGlobalTexture(tintGradientTextureID, environment.GetTintTex());
            Shader.SetGlobalFloat(fogGradientScaleID, environment.fogGradientScale);
            Shader.SetGlobalFloat(tintGradientScaleID, environment.tintGradientScale);
            Shader.SetGlobalFloat(fogDensityID, environment.fogDensity);
            Shader.SetGlobalVector(murkColorID, environment.murkColor.linear);
            RenderSkyMap();
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

        static readonly int fogGradientTextureID = Shader.PropertyToID("Ocean_FogGradientTexture");
        static readonly int sssGradientTextureID = Shader.PropertyToID("Ocean_SssGradientTexture");
        static readonly int tintGradientTextureID = Shader.PropertyToID("Ocean_TintGradientTexture");
        static readonly int murkColorID = Shader.PropertyToID("Ocean_MurkColor");
        static readonly int fogGradientScaleID = Shader.PropertyToID("Ocean_FogGradientScale");
        static readonly int tintGradientScaleID = Shader.PropertyToID("Ocean_TintGradientScale");
        static readonly int fogDensityID = Shader.PropertyToID("Ocean_FogDensity");

        static readonly int skyMapID = Shader.PropertyToID("Ocean_SkyMap");
        static readonly int bottomHemisphereRadiusID = Shader.PropertyToID("Ocean_BottomHemisphereRadius");
        static readonly int bottomHemisphereStrengthID = Shader.PropertyToID("Ocean_BottomHemisphereStrength");
        static readonly int bottomHemisphereColorID = Shader.PropertyToID("Ocean_BottomHemisphereColor");
    }
}
