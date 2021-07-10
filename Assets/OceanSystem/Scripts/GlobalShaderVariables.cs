using UnityEngine;

namespace OceanSystem
{
    public static class GlobalShaderVariables
    {
        public static class Misc
        {
            public static readonly int SpecCube = Shader.PropertyToID("Ocean_SpecCube");
            public static readonly int AbsorbtionDepthScale = Shader.PropertyToID("Ocean_AbsorptionDepthScale");
            public static readonly int FogDensity = Shader.PropertyToID("Ocean_FogDensity");
            public static readonly int ReflectionMaskRadius = Shader.PropertyToID("Ocean_ReflectionsMaskRadius");
            public static readonly int ReflectionMaskSharpness = Shader.PropertyToID("Ocean_ReflectionsMaskSharpness");
            public static readonly int InverseViewMatrix = Shader.PropertyToID("Ocean_InverseViewMatrix");
            public static readonly int InverseProjectionMatrix = Shader.PropertyToID("Ocean_InverseProjectionMatrix");
            public static readonly int SubmergenceTexture = Shader.PropertyToID("Ocean_CameraSubmergenceTexture");
            public static readonly int SkyMap = Shader.PropertyToID("Ocean_SkyMap");
        }

        public static class Simulation
        {
            public static readonly int DisplacementAndDerivatives = Shader.PropertyToID("Ocean_DisplacementAndDerivatives");
            public static readonly int Turbulence = Shader.PropertyToID("Ocean_Turbulence");
            public static readonly int LengthScales = Shader.PropertyToID("Ocean_LengthScales");
            public static readonly int WindSpeed = Shader.PropertyToID("Ocean_WindSpeed");
            public static readonly int WavesScale = Shader.PropertyToID("Ocean_WavesScale");
            public static readonly int WavesAlignement = Shader.PropertyToID("Ocean_WavesAlignement");
            public static readonly int WindDirection = Shader.PropertyToID("Ocean_WindDirection");
            public static readonly int WorldToWindSpace = Shader.PropertyToID("Ocean_WorldToWindSpace");
            public static readonly int ReferenceWaveHeight = Shader.PropertyToID("Ocean_ReferenceWaveHeight");
        }

        public static class Colors
        {
            public static readonly int DeepScatterColor = Shader.PropertyToID("Ocean_DeepScatterColor");
            public static readonly int ShallowScatterColor = Shader.PropertyToID("Ocean_SssColor");
            public static readonly int DiffuseColor = Shader.PropertyToID("Ocean_DiffuseColor");
            public static readonly int[] AbsorbtionGradient =
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

            public static readonly int ReflectionMaskColor = Shader.PropertyToID("Ocean_ReflectionsMaskColor");
        }

        public static class Foam
        {
            public static readonly int Coverage = Shader.PropertyToID("Ocean_FoamCoverage");
            public static readonly int Density = Shader.PropertyToID("Ocean_FoamDensity");
            public static readonly int Sharpness = Shader.PropertyToID("Ocean_FoamSharpness");
            public static readonly int Persistence = Shader.PropertyToID("Ocean_FoamPersistence");
            public static readonly int Trail = Shader.PropertyToID("Ocean_FoamTrail");
            public static readonly int TrailTextureStrength = Shader.PropertyToID("Ocean_FoamTrailTextureStrength");
            public static readonly int Underwater = Shader.PropertyToID("Ocean_FoamUnderwater");
            public static readonly int CascadesWeights = Shader.PropertyToID("Ocean_FoamCascadesWeights");

            public static readonly int TrailTextureSize0 = Shader.PropertyToID("Ocean_FoamTrailTextureSize0");
            public static readonly int TrailTextureSize1 = Shader.PropertyToID("Ocean_FoamTrailTextureSize1");
            public static readonly int TrailDirection0 = Shader.PropertyToID("Ocean_FoamTrailDirection0");
            public static readonly int TrailDirection1 = Shader.PropertyToID("Ocean_FoamTrailDirection1");
            public static readonly int TrailBlendValue = Shader.PropertyToID("Ocean_FoamTrailBlendValue");
        }
    }
}
