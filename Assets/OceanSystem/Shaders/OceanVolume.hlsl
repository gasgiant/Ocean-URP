#if !defined(OCEAN_VOLUME_INCLUDED)
#define OCEAN_VOLUME_INCLUDED
#include "OceanGlobals.hlsl"

float3 ColorThroughWater(float3 color, float3 volumeColor, float distThroughWater, float depth)
{
	distThroughWater = max(0, distThroughWater);
	depth = max(0, depth);
	color *= TintGradient(exp(-(distThroughWater + depth) / Ocean_TintGradientScale));
	return lerp(color, volumeColor, 1 - saturate(exp(-Ocean_FogDensity * distThroughWater)));
}

float3 UnderwaterFogColor(float3 viewDir, float3 lightDir, float depth)
{
	float depthScale = 0;//saturate(exp(Ocean_ElevationBelowCamera / Ocean_FogGradientScale));
	float bias = min(0, depth * 0.02);
	float sssFactor = 0.1 * pow(max(0, 1 - viewDir.y + bias), 3);
	sssFactor *= 1 + pow(saturate(dot(lightDir, -viewDir)), 4);
	sssFactor *= saturate(1 - depthScale);
    float3 color = FogGradient(depthScale) * max(0.5, saturate(2 - viewDir.y + bias));
	float3 sssColor = SssGradient(depthScale);
	color = color + sssColor * sssFactor;
	return color;
}

#endif