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

float4 ClipMapVertex(float4 vertex, float4 morphOffset)
{
	vertex *= ClipMap_Scale;
	float meshScale = vertex.y;
	float step = meshScale * 4;

	float3 snappedViewerPos = float3(floor(ClipMap_ViewerPosition.x / step) * step, 0, floor(ClipMap_ViewerPosition.z / step) * step);
	float4 worldPos = float4(snappedViewerPos.x + vertex.x, 0, snappedViewerPos.z + vertex.z, 1);

	float morphStart = ((ClipMap_LevelHalfSize + 1) * 0.5 + 8) * meshScale;
	float morphEnd = (ClipMap_LevelHalfSize - 2) * meshScale;

	float t = saturate((ModifiedManhattanDistance(worldPos.xyz, ClipMap_ViewerPosition) - morphStart) / (morphEnd - morphStart));
	worldPos += morphOffset * meshScale * t;
	return worldPos;
}
#endif