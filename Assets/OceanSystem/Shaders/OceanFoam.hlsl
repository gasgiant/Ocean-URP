#if !defined(OCEAN_FOAM_INCLUDED)
#define OCEAN_FOAM_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "OceanGlobals.hlsl"
#include "OceanSimulationSampling.hlsl"
#include "OceanMaterialProps.hlsl"

struct FoamInput
{
	float4x4 derivatives;
	float2 worldXZ;
	float4 lodWeights;
	float4 shoreWeights;
    float4 positionNDC;
    float viewDepth;
	float time;
	float3 viewDir;
	float3 normal;
};

struct FoamData
{
	float2 coverage;
	float3 normal;
	float3 tex;
};

float4 MixTurbulence(float4x4 t, float4 foamWeights, float4 mixWeights)
{
	return (t[0] * foamWeights.x + t[1] * foamWeights.y + t[2] * foamWeights.z + t[3] * foamWeights.w) / dot(foamWeights * mixWeights, 1);
}

float Bubbles(float2 worldXZ, float3 viewDir, float3 normal, float time)
{
	float2 parallaxDir = (viewDir.xz / (dot(normal, viewDir)) + 0.5 * normal.xz);
    float bubbles1 = SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture, 
		worldXZ - parallaxDir * _UnderwaterFoamParallax - 2 * Ocean_WindDirection * time).r;
    float bubbles2 = SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture,
		worldXZ - parallaxDir * _UnderwaterFoamParallax * 2 - Ocean_WindDirection * time).r;
	return saturate(5 * saturate(lerp(-0.5, 2, (bubbles1 + bubbles2) * 0.5)));
}

float2 Coverage(float4x4 t, float4 mixWeights, float2 worldXZ, float bubblesTex)
{
	float4 turbulence = MixTurbulence(t, Ocean_FoamCascadesWeights, mixWeights * ACTIVE_CASCADES);
    //float foamValue = lerp(turbulence.x, (turbulence.z + turbulence.w) * 0.5, Ocean_FoamPersistence);
	//foamValue -= 1;
    float foamValueCurrent = (turbulence.x + turbulence.y) * 0.5;
    float foamValuePersistent = (turbulence.z + turbulence.w) * 0.5;
    foamValueCurrent = lerp(foamValueCurrent, foamValuePersistent, 0.5);
    foamValueCurrent -= 1;
    foamValuePersistent -= 1;
	
    float contactTexture = SAMPLE_TEXTURE2D(_ContactFoamTexture, sampler_ContactFoamTexture,
		worldXZ * 0.04 * float2(Ocean_WindDirection.y, Ocean_WindDirection.x) + 0.00 * Ocean_WindDirection * _Time.y).r;
    foamValuePersistent += saturate(foamValuePersistent + 1) * contactTexture * 0.2;
    //foamValuePersistent *= contactTexture * 0.5;
    float foamValue = max(foamValuePersistent + Ocean_FoamPersistence, foamValueCurrent + Ocean_FoamCoverage);
	
	
    float whiteCaps = saturate(foamValue * Ocean_FoamDensity);
    float underwater = saturate((foamValue + 0.05 * Ocean_FoamUnderwater) * Ocean_FoamDensity);
    float bubbles = bubblesTex * saturate((foamValue + Ocean_FoamUnderwater * 0.2) * Ocean_FoamDensity);
	return float2(whiteCaps, max(underwater, bubbles));
}

float ContactFoam(float4 positionNDC, float viewDepth, float2 worldXZ, float time)
{
    float depthDiff = LinearEyeDepth(SampleSceneDepth(positionNDC.xy / positionNDC.w), _ZBufferParams) 
		- viewDepth;
    float contactTexture = SAMPLE_TEXTURE2D(_ContactFoamTexture, sampler_ContactFoamTexture,
		worldXZ * 0.5 + 0.1 * Ocean_WindDirection * time).r;
	contactTexture = saturate(1 - contactTexture);
	depthDiff = abs(depthDiff) * contactTexture;
	return saturate(10 * (_ContactFoam * 2 - depthDiff));
}

FoamData GetFoamData(FoamInput i)
{
	FoamData data;
	data.coverage = 0;
	data.normal = float3(0, 1, 0);
	data.tex = 0;
	
	#if !defined(WAVES_FOAM_ENABLED) && !defined(CONTACT_FOAM_ENABLED)
	return data;
	#endif
	
	#ifdef WAVES_FOAM_ENABLED
	float4x4 turbulence = SampleTurbulence(i.worldXZ, i.lodWeights * i.shoreWeights);
	data.coverage = Coverage(turbulence, i.lodWeights, i.worldXZ, Bubbles(i.worldXZ, i.viewDir, i.normal, i.time));
	float4 normalWeights = saturate(float4(1, 0.66, 0.33, 0) + _FoamNormalsDetail) * ACTIVE_CASCADES;
	data.normal = NormalFromDerivatives(i.derivatives, normalWeights);
	#endif
	
	#ifdef CONTACT_FOAM_ENABLED
	data.coverage.x = saturate(data.coverage.x + ContactFoam(i.positionNDC, i.viewDepth, i.worldXZ, i.time));
	#endif
	
	float foamNoise = SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture, i.worldXZ).r * 0.5;
    foamNoise += SAMPLE_TEXTURE2D(_FoamTexture, sampler_FoamTexture, i.worldXZ - Ocean_WindDirection * i.time * 0.7).r * 0.5;
	foamNoise = 1 - saturate(lerp(-1.1, 4, foamNoise));
	data.tex = foamNoise;
	return data;
}

#endif