#if !defined(OCEAN_SIMULATION_SAMPLING_INCLUDED)
#define OCEAN_SIMULATION_SAMPLING_INCLUDED

#if defined(OCEAN_FOUR_CASCADES)
	#define ACTIVE_CASCADES float4(1, 1, 1, 1)
#elif defined(OCEAN_THREE_CASCADES)
	#define ACTIVE_CASCADES float4(1, 1, 1, 0)
#else
	#define ACTIVE_CASCADES float4(1, 1, 0, 0)
#endif

#define LOD_THRESHOLD 0.05

Texture2DArray Ocean_DisplacementAndDerivatives;
SamplerState samplerOcean_DisplacementAndDerivatives;
Texture2DArray Ocean_Turbulence;
SamplerState samplerOcean_Turbulence;

float4 Ocean_LengthScales;

float EaseInOutClamped(float x)
{
	x = saturate(x);
	return 3 * x * x - 2 * x * x * x;
}

float4 LodWeights(float viewDist, float lodScale)
{
	float4 x = (viewDist - Ocean_LengthScales * lodScale) / Ocean_LengthScales / lodScale;
	return 1 - float4(EaseInOutClamped(x.x), EaseInOutClamped(x.y), EaseInOutClamped(x.z), EaseInOutClamped(x.w));
}

float3 SampleDisplacement(float2 worldXZ, float4 weights, float4 shoreModulation)
{
	float3 displacement = 0;
	weights *= shoreModulation;
	
	displacement += weights[0] * Ocean_DisplacementAndDerivatives.SampleLevel(samplerOcean_DisplacementAndDerivatives,
		float3(worldXZ / Ocean_LengthScales[0], 0 * 2), 0).xyz;
	
	if (weights[1] > LOD_THRESHOLD)
		displacement += weights[1] * Ocean_DisplacementAndDerivatives.SampleLevel(samplerOcean_DisplacementAndDerivatives,
			float3(worldXZ / Ocean_LengthScales[1], 1 * 2), 0).xyz;
	
	if (ACTIVE_CASCADES[2])
		if (weights[2] > LOD_THRESHOLD)
			displacement += weights[2] * Ocean_DisplacementAndDerivatives.SampleLevel(samplerOcean_DisplacementAndDerivatives,
				float3(worldXZ / Ocean_LengthScales[2], 2 * 2), 0).xyz;
	
	if (ACTIVE_CASCADES[3])
		if (weights[3] > LOD_THRESHOLD)
			displacement += weights[3] * Ocean_DisplacementAndDerivatives.SampleLevel(samplerOcean_DisplacementAndDerivatives,
				float3(worldXZ / Ocean_LengthScales[3], 3 * 2), 0).xyz;
	
	return displacement;
}

float SampleHeight(float2 worldPos, float4 weights, float4 shoreModulation)
{
	float3 displacement = SampleDisplacement(worldPos, weights, shoreModulation);
	displacement = SampleDisplacement(worldPos - displacement.xz, weights, shoreModulation);
	displacement = SampleDisplacement(worldPos - displacement.xz, weights, shoreModulation);
	displacement = SampleDisplacement(worldPos - displacement.xz, weights, shoreModulation);
	
	return displacement.y;
}

float4x4 SampleDerivatives(float2 worldXZ, float4 weights)
{
	float4x4 o = 0;
	
	o[0] = weights[0] * Ocean_DisplacementAndDerivatives.Sample(samplerOcean_DisplacementAndDerivatives,
		float3(worldXZ / Ocean_LengthScales[0], 0 * 2 + 1));
	
	if (weights[1] > LOD_THRESHOLD)
		o[1] = weights[1] * Ocean_DisplacementAndDerivatives.Sample(samplerOcean_DisplacementAndDerivatives,
			float3(worldXZ / Ocean_LengthScales[1], 1 * 2 + 1));
	
	if (ACTIVE_CASCADES[2])
		if (weights[2] > LOD_THRESHOLD)
			o[2] = weights[2] * Ocean_DisplacementAndDerivatives.Sample(samplerOcean_DisplacementAndDerivatives,
				float3(worldXZ / Ocean_LengthScales[2], 2 * 2 + 1));
	
	if (ACTIVE_CASCADES[3])
		if (weights[3] > LOD_THRESHOLD)
			o[3] = weights[3] * Ocean_DisplacementAndDerivatives.Sample(samplerOcean_DisplacementAndDerivatives,
				float3(worldXZ / Ocean_LengthScales[3], 3 * 2 + 1));
	
	return o;
}

float4x4 SampleTurbulence(float2 worldXZ, float4 weights)
{
	float4x4 o = 0;
	
	o[0] = weights[0] * Ocean_Turbulence.Sample(samplerOcean_Turbulence, float3(worldXZ / Ocean_LengthScales[0], 0));
	
	if (weights[1] > LOD_THRESHOLD)
		o[1] = weights[1] * Ocean_Turbulence.Sample(samplerOcean_Turbulence, float3(worldXZ / Ocean_LengthScales[1], 1));
	
	if (ACTIVE_CASCADES[2])
		if (weights[2] > LOD_THRESHOLD)
			o[2] = weights[2] * Ocean_Turbulence.Sample(samplerOcean_Turbulence, float3(worldXZ / Ocean_LengthScales[2], 2));
	
	if (ACTIVE_CASCADES[3])
		if (weights[3] > LOD_THRESHOLD)
			o[3] = weights[3] * Ocean_Turbulence.Sample(samplerOcean_Turbulence, float3(worldXZ / Ocean_LengthScales[3], 3));
	
	return o;
}

float3 NormalFromDerivatives(float4x4 derivativesCascades, float4 weights)
{
	float4 derivatives = derivativesCascades[0] * weights[0]
		+ derivativesCascades[1] * weights[1]
		+ derivativesCascades[2] * weights[2]
		+ derivativesCascades[3] * weights[3];
	float2 slope = float2(derivatives.x / max(0.001, 1 + derivatives.z),
                    derivatives.y / max(0.001, 1 + derivatives.w));
	
	return normalize(float3(-slope.x, 1, -slope.y));
}

#endif