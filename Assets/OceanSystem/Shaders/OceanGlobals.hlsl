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

// foam
float Ocean_FoamCoverage;
float Ocean_FoamUnderwater;
float Ocean_FoamDensity;
float Ocean_FoamPersistence;
float4 Ocean_FoamCascadesWeights;

// camera
float4x4 Ocean_InverseViewMatrix;
float4x4 Ocean_InverseProjectionMatrix;

// environment maps
TEXTURECUBE(Ocean_SpecCube);
SAMPLER(samplerOcean_SpecCube);
float4 Ocean_SpecCube_HDR;
TEXTURE2D(Ocean_SkyMap);
SAMPLER(samplerOcean_SkyMap);
// downward reflections mask
float4 Ocean_DownwardReflectionsColor;
float Ocean_DownwardReflectionsRadius;
float Ocean_DownwardReflectionsSharpness;

// colors
float3 Ocean_FogColor;
float3 Ocean_SssColor;
float3 Ocean_DiffuseColor;
float Ocean_TintDepthScale;
float Ocean_FogDensity;
float4 Ocean_TintColor0;
float4 Ocean_TintColor1;
float4 Ocean_TintColor2;
float4 Ocean_TintColor3;
float4 Ocean_TintColor4;
float4 Ocean_TintColor5;
float4 Ocean_TintColor6;
float4 Ocean_TintColor7;
float2 Ocean_TintGradientParams;


float3 SampleOceanSpecCube(float3 dir)
{
    //float4 envSample = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, dir, 0);
    //return DecodeHDREnvironment(envSample, unity_SpecCube0_HDR);
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

float3 FogColor(float t)
{
    return Ocean_FogColor;
}

float3 SssColor(float t)
{
    return Ocean_SssColor;
}

float3 DiffuseColor(float t)
{
    return Ocean_DiffuseColor;
}

float3 TintColor(float t)
{
    float4 colors[GRADIENT_MAX_KEYS] =
    {
        Ocean_TintColor0,
        Ocean_TintColor1,
        Ocean_TintColor2,
        Ocean_TintColor3,
        Ocean_TintColor4,
        Ocean_TintColor5,
        Ocean_TintColor6,
        Ocean_TintColor7
    };
    Gradient g = CreateGradient(colors, Ocean_TintGradientParams);
    return SampleGradient(g, t);
}

#endif