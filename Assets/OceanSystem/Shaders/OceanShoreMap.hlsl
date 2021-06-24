#if !defined(OCEAN_SHORE_MAP_INCLUDED)
#define OCEAN_SHORE_MAP_INCLUDED

#define OCEAN_PI 3.1415926

TEXTURE2D(Ocean_ShoreMap);
SAMPLER(samplerOcean_ShoreMap);
// xyz - shore map position, 
// w - shore map orthographic size
float4 Ocean_ShoreMapPosition;
// xy - elevation min max
// zw - distance field min max
float4 Ocean_ShoreMapRanges;
float4 Ocean_ShoreModulationValue;
float4 Ocean_ShoreModulationScale;

float4 SampleShore(float2 worldPosXZ)
{
	#ifdef SHORE_ENABLED
	float2 elevationUV = (worldPosXZ - Ocean_ShoreMapPosition.xz) / Ocean_ShoreMapPosition.w;
	elevationUV = (elevationUV + 1) * 0.5;
	float4 val = tex2Dlod(Ocean_ShoreMap, float4(elevationUV, 0, 0));
	val.x = lerp(Ocean_ShoreMapRanges.x, Ocean_ShoreMapRanges.y, val.x);
	val.y = lerp(Ocean_ShoreMapRanges.z, Ocean_ShoreMapRanges.w, val.y);
	val.zw = val.zw * 2 - 1;
	return val;
	#else
	return float4(-1000, 0, 0, 0);
	#endif
}

float4 ShoreModulation(float elevation)
{
	return 1 - saturate(Ocean_ShoreModulationValue * saturate(1 + elevation / Ocean_ShoreModulationScale));
}
#endif