#if !defined(OCEAN_GLOBALS_INCLUDED)
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

#define OCEAN_GLOBALS_INCLUDED
#define OCEAN_PI 3.1415926


TEXTURE2D(Ocean_CameraSubmergenceTexture);
SAMPLER(samplerOcean_CameraSubmergenceTexture);
float Ocean_ElevationBelowCamera;

float Ocean_WindSpeed;
float Ocean_WavesScale;
float Ocean_WavesAlignement;
float2 Ocean_WindDirection;
float4x4 Ocean_WorldToWindSpace;

float4x4 Ocean_CameraToWorld;
float4x4 Ocean_CameraInverseProjection;
// x, y - near clip plane width, height
// z - near clip plane
float4 Ocean_CameraNearPlaneParams;

TEXTURECUBE(Ocean_SpecCube);
SAMPLER(samplerOcean_SpecCube);
float4 Ocean_SpecCube_HDR;

TEXTURE2D(Ocean_SkyMap);
SAMPLER(samplerOcean_SkyMap);

// colors
TEXTURE2D(Ocean_FogGradientTexture);
SAMPLER(samplerOcean_FogGradientTexture);
TEXTURE2D(Ocean_SssGradientTexture);
SAMPLER(samplerOcean_SssGradientTexture);
TEXTURE2D(Ocean_TintGradientTexture);
SAMPLER(samplerOcean_TintGradientTexture);
float Ocean_FogGradientScale;
float Ocean_TintGradientScale;
float Ocean_FogDensity;
float3 Ocean_MurkColor;

// refelctions bottom hemisphere
float Ocean_BottomHemisphereRadius;
float Ocean_BottomHemisphereStrength;
float4 Ocean_BottomHemisphereColor;


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

float3 FogGradient(float t)
{
    return SAMPLE_TEXTURE2D(Ocean_FogGradientTexture, samplerOcean_FogGradientTexture, t).rgb;
}

float3 SssGradient(float t)
{
    return SAMPLE_TEXTURE2D(Ocean_SssGradientTexture, samplerOcean_SssGradientTexture, t).rgb;
}

float3 TintGradient(float t)
{
    return SAMPLE_TEXTURE2D(Ocean_TintGradientTexture, samplerOcean_TintGradientTexture, t).rgb;
}

float3 MurkColor()
{
    return 0;
}
#endif