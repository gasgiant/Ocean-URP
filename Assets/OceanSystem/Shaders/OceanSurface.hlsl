#if !defined(OCEAN_SURFACE_INCLUDED)
#define OCEAN_SURFACE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
float4 _CameraDepthTexture_TexelSize;
//#include "PlanarReflections.hlsl"
#include "BrunetonLighting.hlsl"

#include "OceanGlobals.hlsl"
#include "OceanMaterialProps.hlsl"
#include "OceanVolume.hlsl"
#include "OceanFoam.hlsl"

struct LightingInput
{
	float3 normal;
	float3 viewDir;
	float viewDist;
    float roughnessMap;
	float3 positionWS;
	float4 shore;
	float4 positionNDC;
    float viewDepth;
	float3 cameraPos;
    Light mainLight;
};

struct BrunetonInputs
{
	float3 lightDir_windSpace;
	float3 viewDir_windSpace;
    float3 normal_windSpace;
    float3 tangentX_windSpace;
    float3 tangentY_windSpace;
	float2 slopeVarianceSquared;
};

float Pow5(float x)
{
	return x * x * x * x * x;
}

float3 PositionWsFromDepth(float rawDepth, float2 screenUV, float4x4 inverseProj, float4x4 inverseView)
{
    float4 positionCS = float4(screenUV * 2 - 1, rawDepth, 1);
    float4 positionVS = mul(inverseProj, positionCS);
    positionVS /= positionVS.w;
    return mul(inverseView, positionVS).xyz;
}

float2 SlopeVarianceSquared(float windSpeed, float viewDist, float alignement, float scale)
{
	float upwind = 0.01 * sqrt(windSpeed) * viewDist / (viewDist + scale);
	return float2(upwind, upwind * (1 - 0.3 * alignement));
}

float EffectiveFresnel(BrunetonInputs bi)
{
	const float R = 0.02;
	float fresnel = R + (1 - R) * MeanFresnel(
		bi.viewDir_windSpace,
		bi.normal_windSpace,
		bi.slopeVarianceSquared);
	return saturate(fresnel);
}

float ShlickFresnel(float3 viewDir, float3 normal)
{
	const float R = 0.02;
	return R + (1 - R) * Pow5(1 - saturate(abs(dot(viewDir, normal))));
}

float3 Specular(LightingInput li, BrunetonInputs bi)
{
    float radiance = li.mainLight.shadowAttenuation * ReflectedSunRadiance(
		bi.lightDir_windSpace,
		bi.viewDir_windSpace,
		bi.normal_windSpace,
		bi.tangentX_windSpace,
		bi.tangentY_windSpace,
		max(1e-4, bi.slopeVarianceSquared + _SpecularMinRoughness * 0.2));
    return radiance * _SpecularStrength * li.mainLight.color;
}

float3 Reflection(LightingInput li, BrunetonInputs bi)
{
    float3 sky = MeanSkyRadiance(Ocean_SkyMap, samplerOcean_SkyMap,
		transpose(Ocean_WorldToWindSpace),
        bi.viewDir_windSpace,
        bi.normal_windSpace,
        bi.tangentX_windSpace,
        bi.tangentY_windSpace,
        bi.slopeVarianceSquared);

	#ifdef PLANAR_REFLECTIONS_ENABLED
	float4 local = GetPlanarReflection(li.viewDir, li.normal, li.positionWS, 
		bi.slopeVarianceSquared.x * 100, 1 - _ReflectionNormalStength);
	return lerp(sky, local.rgb, local.a);
	#else
	return sky;
	#endif
}

float3 ReflectionBackface(LightingInput li)
{
	float3 normal = li.normal;
	normal.xz *= 0.2;
	normal = normalize(normal);
	float3 dir = reflect(li.viewDir, normal);
    float3 volume = UnderwaterFogColor(dir, li.mainLight.direction, 0);
	return volume;
	
	//#ifdef PLANAR_REFLECTIONS_ENABLED
	//float4 color = GetPlanarReflection(li.viewDir, li.normal, li.positionWS, 0, 1);
	//color.rgb = ColorThroughWater(color.rgb, 0, 0, -li.shore.x).rgb;
	//return lerp(volume, color.rgb, color.a);
	//#else
	//return volume;
	//#endif
}

float3 LitFoamColor(LightingInput li, FoamData foamData)
{
    float ndotl = (0.2 + 0.8 * saturate(dot(foamData.normal, li.mainLight.direction))) 
		* li.mainLight.shadowAttenuation;
    return foamData.albedo * _FoamTint.rgb * 
		(ndotl * li.mainLight.color + OceanEnvironmentDiffuse(foamData.normal));
}

float2 SubsurfaceScatteringFactor(LightingInput li)
{
    float normalFactor = saturate(dot(normalize(lerp(li.viewDir, li.normal, _SssNormalStrength)), li.viewDir));
    float heightFactor = saturate((li.positionWS.y + Ocean_ReferenceWaveHeight * (1 + _SssHeightBias)) * 0.5 / max(0.5, Ocean_ReferenceWaveHeight));
    heightFactor = pow(abs(heightFactor), max(1, Ocean_ReferenceWaveHeight * 0.4));
    float sun = _SssSunStrength * normalFactor * heightFactor * pow(saturate(dot(li.mainLight.direction, -li.viewDir)), min(50, 1 / _SssSpread));
    float environment = _SssEnvironmentStrength * normalFactor * heightFactor * saturate(1 - li.viewDir.y);
	float2 factor = float2(sun, environment);
	factor *= _SssFadeDistance / (_SssFadeDistance + li.viewDist);
	return factor;
}

float3 RefractionCoords(float refractionStrength, float4 positionNDC, float viewDepth, float3 normal)
{
	float2 uvOffset = normal.xz * refractionStrength;
	uvOffset.y *=
		_CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);
	float2 refractedScreenUV = (positionNDC.xy + uvOffset) / positionNDC.w;
    float rawDepth = SampleSceneDepth(refractedScreenUV);
    float refractedDepthDiff = LinearEyeDepth(rawDepth, _ZBufferParams) - viewDepth;
	uvOffset *= saturate(refractedDepthDiff);
	refractedScreenUV = (positionNDC.xy + uvOffset) / positionNDC.w;
    rawDepth = SampleSceneDepth(refractedScreenUV);
	return float3(refractedScreenUV, rawDepth);
}

float3 Refraction(LightingInput li, FoamData foamData, float2 sss, float3 foamColor)
{
	float depthScale = 0;//exp(li.shore.x / Ocean_FogGradientScale);
    float3 color = DeepScatterColor(0 * (1 - abs(li.viewDir.y)) * (1 - abs(li.viewDir.y)) * depthScale);
    float3 sssColor = SssColor(depthScale);
	color += sssColor * saturate(sss.x + sss.y);
    float ndotl = saturate(dot(li.normal, li.mainLight.direction));
    color += (ndotl * 0.8 + 0.2f) * li.mainLight.color  * Ocean_DiffuseColor;
	
	#ifdef OCEAN_TRANSPARENCY_ENABLED
	float3 refractionCoords = RefractionCoords(_RefractionStrength, li.positionNDC, li.viewDepth, li.normal);
	float3 backgroundColor = SampleSceneColor(refractionCoords.xy);
	
	float3 backgroundPositionWS = PositionWsFromDepth(refractionCoords.z, refractionCoords.xy, Ocean_InverseProjectionMatrix, Ocean_InverseViewMatrix);
	float backgroundDistance = length(backgroundPositionWS - li.cameraPos) - li.viewDist;
	color = ColorThroughWater(backgroundColor, color, backgroundDistance, -backgroundPositionWS.y);
	#endif
	
	#ifdef WAVES_FOAM_ENABLED
	float underwaterFoamVisibility = 20 / (20 + li.viewDist);
	float3 tint = AbsorptionTint(0.8);
	float3 underwaterFoamColor = foamColor * tint * tint;
	color = lerp(color, underwaterFoamColor, foamData.coverage.y * underwaterFoamVisibility);
	#endif
	return color;
}

float3 RefractionBackface(LightingInput li, float3 refractionDir)
{
	#ifdef OCEAN_TRANSPARENCY_ENABLED
	float3 refractionCoords = RefractionCoords(_RefractionStrengthUnderwater, li.positionNDC, li.viewDepth, li.normal);
	return SampleSceneColor(refractionCoords.xy);
	#else
	return SampleOceanSpecCube(refractionDir);
	#endif
}

float4 HorizonBlend(LightingInput li)
{
	float3 dir = -float3(li.viewDir.x, 0, li.viewDir.z);
	float3 horizonColor = SampleOceanSpecCube(dir);
	
	float distanceScale = 100 + 7 * abs(li.cameraPos.y);
	float t = exp(-5 / max(_HorizonFog, 0.01) * (abs(li.viewDir.y) + distanceScale / (li.viewDist + distanceScale)));
	return float4(horizonColor, t);	
}

float3 GetOceanColor(LightingInput li, FoamData foamData)
{
	float3 tangentY = float3(0.0, li.normal.z, -li.normal.y);
	tangentY /= max(0.001, length(tangentY));
	float3 tangentX = cross(tangentY, li.normal);
    
	BrunetonInputs bi;
	bi.lightDir_windSpace = mul(Ocean_WorldToWindSpace, float4(li.mainLight.direction, 0)).xyz;
    bi.viewDir_windSpace = mul(Ocean_WorldToWindSpace, float4(li.viewDir, 0)).xyz;
    bi.normal_windSpace = mul(Ocean_WorldToWindSpace, float4(li.normal, 0)).xyz;
    bi.tangentX_windSpace = mul(Ocean_WorldToWindSpace, float4(tangentX, 0)).xyz;
    bi.tangentY_windSpace = mul(Ocean_WorldToWindSpace, float4(tangentY, 0)).xyz;
    bi.slopeVarianceSquared = _RoughnessScale * (1 + li.roughnessMap * 0.3)
		* SlopeVarianceSquared(Ocean_WindSpeed * Ocean_WavesScale, li.viewDist,
		Ocean_WavesAlignement, _RoughnessDistance);
	
	float2 sss = SubsurfaceScatteringFactor(li);
    float3 foamLitColor = 0;
	#if defined(WAVES_FOAM_ENABLED) || defined(CONTACT_FOAM_ENABLED)
	foamLitColor = LitFoamColor(li, foamData);
	#endif
	
	float fresnel = EffectiveFresnel(bi);
	float3 specular = Specular(li, bi) * Pow5(1 - foamData.coverage.y);
	float3 reflected = Reflection(li, bi);
    float3 refracted = Refraction(li, foamData, sss, foamLitColor);
	float4 horizon = HorizonBlend(li);
	float3 color = specular + lerp(refracted, reflected, fresnel);
	#if defined(WAVES_FOAM_ENABLED) || defined(CONTACT_FOAM_ENABLED)
	color = lerp(color, foamLitColor, foamData.coverage.x);
	#endif
	color = lerp(color, horizon.rgb, horizon.a);
	return color;
}

float3 GetOceanColorUnderwater(LightingInput li)
{
	const float n = 1.1;
	float3 refractionDir = refract(-li.viewDir, -li.normal, n);
	
	float fresnel = max(ShlickFresnel(li.viewDir, li.normal), dot(refractionDir, refractionDir) < 0.5);
	float3 refracted = RefractionBackface(li, refractionDir);
	float3 reflected = ReflectionBackface(li);
	float3 color = lerp(refracted, reflected, fresnel);
	float3 volume = UnderwaterFogColor(li.viewDir, li.mainLight.direction, li.cameraPos.y);
    color = ColorThroughWater(color, volume, li.viewDist - _ProjectionParams.y, 0);
	return color;
}

#endif