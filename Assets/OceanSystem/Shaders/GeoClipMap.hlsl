#if !defined(GEO_CLIP_MAP_INCLUDED)
#define GEO_CLIP_MAP_INCLUDED
float ClipMap_Scale;
float ClipMap_LevelHalfSize;
float3 ClipMap_ViewerPosition;

float ModifiedManhattanDistance(float3 a, float3 b)
{
	float3 v = a - b;
	return max(abs(v.x + v.z) + abs(v.x - v.z), abs(v.y)) * 0.5;
}

float3 ClipMapVertex(float3 positionOS, float2 uv)
{
    float3 morphOffset = float3(uv.x, 0, uv.y);
    positionOS *= ClipMap_Scale;
    float meshScale = positionOS.y;
	float step = meshScale * 4;

	float3 snappedViewerPos = float3(floor(ClipMap_ViewerPosition.x / step) * step, 0, floor(ClipMap_ViewerPosition.z / step) * step);
    float3 worldPos = float3(snappedViewerPos.x + positionOS.x, 0, snappedViewerPos.z + positionOS.z);

	float morphStart = ((ClipMap_LevelHalfSize + 1) * 0.5 + 8) * meshScale;
	float morphEnd = (ClipMap_LevelHalfSize - 2) * meshScale;

	float t = saturate((ModifiedManhattanDistance(worldPos, ClipMap_ViewerPosition) - morphStart) / (morphEnd - morphStart));
	worldPos += morphOffset * meshScale * t;
	return worldPos;
}
#endif