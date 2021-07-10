#if !defined(OCEAN_GLOBALS_INCLUDED)
#define OCEAN_GLOBALS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "OceanGradient.hlsl"
#define OCEAN_PI 3.1415926

// submergence
TEXTURE2D(Ocean_CameraSubmergenceTexture);
SAMPLER(samplerOcean_CameraSubmergenceTexture);
float Ocean_ElevationBelowCamera;

// simulation
float Ocean_WindSpeed;
float Ocean_WavesScale;
float Ocean_WavesAlignement;
float2 Ocean_WindDirection;
float4x4 Ocean_WorldToWindSpace;
float Ocean_ReferenceWaveHeight;

// foam
float Ocean_FoamCoverage;
float Ocean_FoamDensity;
float Ocean_FoamSharpness;
float Ocean_FoamPersistence;
float Ocean_FoamTrail;
float Ocean_FoamTrailTextureStrength;
float Ocean_FoamUnderwater;
float4 Ocean_FoamCascadesWeights;
float2 Ocean_FoamTrailTextureSize0;
float2 Ocean_FoamTrailTextureSize1;
float2 Ocean_FoamTrailDirection0;
float2 Ocean_FoamTrailDirection1;
float Ocean_FoamTrailBlendValue;

// camera
float4x4 Ocean_InverseViewMatrix;
float4x4 Ocean_InverseProjectionMatrix;

// environment maps
TEXTURECUBE(Ocean_SpecCube);
SAMPLER(samplerOcean_SpecCube);
float4 Ocean_SpecCube_HDR;
TEXTURE2D(Ocean_SkyMap);
SAMPLER(samplerOcean_SkyMap);
// reflections mask
float4 Ocean_ReflectionsMaskColor;
float Ocean_ReflectionsMaskRadius;
float Ocean_ReflectionsMaskSharpness;

// colors
float3 Ocean_DeepScatterColor;
float3 Ocean_SssColor;
float3 Ocean_DiffuseColor;

float4 Ocean_AbsorptionColor0;
float4 Ocean_AbsorptionColor1;
float4 Ocean_AbsorptionColor2;
float4 Ocean_AbsorptionColor3;
float4 Ocean_AbsorptionColor4;
float4 Ocean_AbsorptionColor5;
float4 Ocean_AbsorptionColor6;
float4 Ocean_AbsorptionColor7;
float2 Ocean_AbsorptionGradientParams;

float Ocean_FogDensity;
float Ocean_AbsorptionDepthScale;


float3 SampleOceanSpecCube(float3 dir)
{
    float4 envSample = SAMPLE_TEXTURECUBE_LOD(Ocean_SpecCube, samplerOcean_SpecCube, dir, 0);
    return DecodeHDREnvironment(envSample, Ocean_SpecCube_HDR);
}

float3 OceanEnvironmentDiffuse(float3 dir)
{
    float4 coefficients[7];
    coefficients[0] = unity_SHAr;
    coefficients[1] = unity_SHAg;
    coefficients[2] = unity_SHAb;
    coefficients[3] = unity_SHBr;
    coefficients[4] = unity_SHBg;
    coefficients[5] = unity_SHBb;
    coefficients[6] = unity_SHC;
    return max(0.0, SampleSH9(coefficients, dir));
}

float3 DeepScatterColor(float t)
{
    return Ocean_DeepScatterColor;
}

float3 SssColor(float t)
{
    return Ocean_SssColor;
}

float3 DiffuseColor(float t)
{
    return Ocean_DiffuseColor;
}

float3 AbsorptionTint(float t)
{
    float4 colors[GRADIENT_MAX_KEYS] =
    {
        Ocean_AbsorptionColor0,
        Ocean_AbsorptionColor1,
        Ocean_AbsorptionColor2,
        Ocean_AbsorptionColor3,
        Ocean_AbsorptionColor4,
        Ocean_AbsorptionColor5,
        Ocean_AbsorptionColor6,
        Ocean_AbsorptionColor7
    };
    Gradient g = CreateGradient(colors, Ocean_AbsorptionGradientParams);
    return SampleGradient(g, t);
}

#endif