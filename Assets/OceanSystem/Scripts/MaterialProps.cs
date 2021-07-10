using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public static class MaterialProps
    {
        public static readonly int SurfaceEditorExpanded = Shader.PropertyToID(Names.SurfaceEditorExpanded);
        public static readonly int VolumeEditorExpanded = Shader.PropertyToID(Names.VolumeEditorExpanded);
        public static readonly int DistantViewEditorExpanded = Shader.PropertyToID(Names.DistantViewEditorExpanded);
        public static readonly int FoamEditorExpanded = Shader.PropertyToID(Names.FoamEditorExpanded);

        // toggles
        public static readonly int WavesFoamEnabled = Shader.PropertyToID(Names.WavesFoamEnabled);
        public static readonly int ContactFoamEnabled = Shader.PropertyToID(Names.ContactFoamEnabled);
        public static readonly int ReceiveShadows = Shader.PropertyToID(Names.ReceiveShadows);

        // surface
        public static readonly int RoughnessScale = Shader.PropertyToID(Names.RoughnessScale);
        public static readonly int SpecularStrength = Shader.PropertyToID(Names.SpecularStrength);
        public static readonly int SpecularMinRoughness = Shader.PropertyToID(Names.SpecularMinRoughness);
        public static readonly int ReflectionNormalStength = Shader.PropertyToID(Names.ReflectionNormalStength);
        public static readonly int RefractionStrength = Shader.PropertyToID(Names.RefractionStrength);
        public static readonly int RefractionStrengthUnderwater = Shader.PropertyToID(Names.RefractionStrengthUnderwater);
        // reflections mask
        public static readonly int ReflectionMaskRadius = Shader.PropertyToID(Names.ReflectionMaskRadius);
        public static readonly int ReflectionMaskSharpness = Shader.PropertyToID(Names.ReflectionMaskSharpness);

        // volume
        public static readonly int FogDensity = Shader.PropertyToID(Names.FogDensity);
        public static readonly int AbsorbtionDepthScale = Shader.PropertyToID(Names.AbsorbtionDepthScale);
        // subsurface scattering
        public static readonly int SssSunStrength = Shader.PropertyToID(Names.SssSunStrength);
        public static readonly int SssEnvironmentStrength = Shader.PropertyToID(Names.SssEnvironmentStrength);
        public static readonly int SssSpread = Shader.PropertyToID(Names.SssSpread);
        public static readonly int SssNormalStrength = Shader.PropertyToID(Names.SssNormalStrength);
        public static readonly int SssHeightBias = Shader.PropertyToID(Names.SssHeightBias);
        public static readonly int SssFadeDistance = Shader.PropertyToID(Names.SssFadeDistance);

        // distant view
        public static readonly int RoughnessDistance = Shader.PropertyToID(Names.RoughnessDistance);
        public static readonly int HorizonFog = Shader.PropertyToID(Names.HorizonFog);
        public static readonly int CascadesFadeDist = Shader.PropertyToID(Names.CascadesFadeDist);
        public static readonly int UvWarpStrength = Shader.PropertyToID(Names.UvWarpStrength);
        public static readonly int DistantRoughnessMap = Shader.PropertyToID(Names.DistantRoughnessMap);
        public static readonly int FoamDetailMap = Shader.PropertyToID(Names.FoamDetailMap);

        // foam
        public static readonly int FoamAlbedo = Shader.PropertyToID(Names.FoamAlbedo);
        public static readonly int FoamUnderwaterTexture = Shader.PropertyToID(Names.FoamUnderwaterTexture);
        public static readonly int FoamTrailTexture = Shader.PropertyToID(Names.FoamTrailTexture);
        public static readonly int ContactFoamTexture = Shader.PropertyToID(Names.ContactFoamTexture);
        public static readonly int FoamNormalsDetail = Shader.PropertyToID(Names.FoamNormalsDetail);
        public static readonly int SurfaceFoamTint = Shader.PropertyToID(Names.SurfaceFoamTint);
        public static readonly int UnderwaterFoamParallax = Shader.PropertyToID(Names.UnderwaterFoamParallax);
        public static readonly int ContactFoam = Shader.PropertyToID(Names.ContactFoam);

        public static class Names
        {
            public static readonly string SurfaceEditorExpanded = "surfaceEditorExpanded";
            public static readonly string VolumeEditorExpanded = "volumeEditorExpanded";
            public static readonly string DistantViewEditorExpanded = "distantViewEditorExpanded";
            public static readonly string FoamEditorExpanded = "foamEditorExpanded";

            // toggles
            public static readonly string WavesFoamEnabled = "_WAVES_FOAM_ENABLED";
            public static readonly string ContactFoamEnabled = "_CONTACT_FOAM_ENABLED";
            public static readonly string ReceiveShadows = "_ReceiveShadows";

            // surface
            public static readonly string RoughnessScale = "_RoughnessScale";
            public static readonly string SpecularStrength = "_SpecularStrength";
            public static readonly string SpecularMinRoughness = "_SpecularMinRoughness";
            public static readonly string ReflectionNormalStength = "_ReflectionNormalStength";
            public static readonly string RefractionStrength = "_RefractionStrength";
            public static readonly string RefractionStrengthUnderwater = "_RefractionStrengthUnderwater";
            // reflections mask
            public static readonly string ReflectionMaskRadius = "_ReflectionMaskRadius";
            public static readonly string ReflectionMaskSharpness = "_ReflectionMaskSharpness";

            // volume
            public static readonly string FogDensity = "_FogDensity";
            public static readonly string AbsorbtionDepthScale = "_AbsorptionDepthScale";
            // subsurface scattering
            public static readonly string SssSunStrength = "_SssSunStrength";
            public static readonly string SssEnvironmentStrength = "_SssEnvironmentStrength";
            public static readonly string SssSpread = "_SssSpread";
            public static readonly string SssNormalStrength = "_SssNormalStrength";
            public static readonly string SssHeightBias = "_SssHeightBias";
            public static readonly string SssFadeDistance = "_SssFadeDistance";

            // distant view
            public static readonly string RoughnessDistance = "_RoughnessDistance";
            public static readonly string HorizonFog = "_HorizonFog";
            public static readonly string CascadesFadeDist = "_CascadesFadeDist";
            public static readonly string UvWarpStrength = "_UvWarpStrength";
            public static readonly string DistantRoughnessMap = "_DistantRoughnessMap";
            public static readonly string FoamDetailMap = "_FoamDetailMap";

            // foam
            public static readonly string FoamAlbedo = "_FoamAlbedo";
            public static readonly string FoamUnderwaterTexture = "_FoamUnderwaterTexture";
            public static readonly string FoamTrailTexture = "_FoamTrailTexture";
            public static readonly string ContactFoamTexture = "_ContactFoamTexture";
            public static readonly string FoamNormalsDetail = "_FoamNormalsDetail";
            public static readonly string SurfaceFoamTint = "_FoamTint";
            public static readonly string UnderwaterFoamParallax = "_UnderwaterFoamParallax";
            public static readonly string ContactFoam = "_ContactFoam";
        }
    }
}
