using UnityEngine;

namespace OceanSystem
{
    public static class ClipmapMeshBuilder
    {
        private static class GlobalShaderVariables
        {
            public static readonly int ViewerPosition = Shader.PropertyToID("ClipMap_ViewerPosition");
            public static readonly int Scale = Shader.PropertyToID("ClipMap_Scale");
            public static readonly int LevelHalfSize = Shader.PropertyToID("ClipMap_LevelHalfSize");
        }

        private const int Overlap = 2;

        // ClipLevelHalfSize + 1 must be divisible by 4 for correct geomorphing
        public static int ClipLevelHalfSize(int vertexDensity) => (vertexDensity + 1) * 4 - 1;

        public static void SetGlobalShaderVariables(Vector3 viewerPosition, int vertexDensity, float minMeshScale)
        {
            int clipLevelHalfSize = ClipLevelHalfSize(vertexDensity);
            int pow = Mathf.FloorToInt(Mathf.Max(0,
                    Mathf.Log(Mathf.Abs(viewerPosition.y) / (2 * minMeshScale), 2) + 1));
            float meshScale = minMeshScale / clipLevelHalfSize * Mathf.Pow(2, pow);

            Shader.SetGlobalVector(GlobalShaderVariables.ViewerPosition, viewerPosition);
            Shader.SetGlobalFloat(GlobalShaderVariables.Scale, meshScale);
            Shader.SetGlobalFloat(GlobalShaderVariables.LevelHalfSize, clipLevelHalfSize);
        }

        public static Mesh BuildClipMap(int vertexDensity, int clipMapLevels)
        {
            int clipLevelHalfSize = ClipLevelHalfSize(vertexDensity);

            Mesh mesh = new Mesh();
            mesh.name = "GeoClipmap";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            CombineInstance[] combine = new CombineInstance[clipMapLevels + 2];

            combine[0].mesh = BuildPlane(2 * clipLevelHalfSize + Overlap, 2 * clipLevelHalfSize + Overlap,
                (Vector3.right + Vector3.forward) * (clipLevelHalfSize + 1), true);
            combine[0].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

            Mesh ring = BuildRing(clipLevelHalfSize);

            for (int i = 1; i < clipMapLevels + 1; i++)
            {
                combine[i].mesh = ring;
                combine[i].transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * Mathf.Pow(2, i));
            }

            combine[clipMapLevels + 1].mesh = BuildSkirt(clipLevelHalfSize, 10);
            combine[clipMapLevels + 1].transform = Matrix4x4.TRS(
                Vector3.zero, Quaternion.identity, Vector3.one * Mathf.Pow(2, clipMapLevels));

            mesh.CombineMeshes(combine, true);
            return mesh;
        }

        private static Mesh BuildRing(int clipLevelHalfSize)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap ring";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            int k = clipLevelHalfSize;

            int shortSide = (k + 1) / 2 + Overlap;
            int longSide = k - 1;
            int sum = longSide + shortSide;

            bool shortMorphShift = (shortSide / 2) % 2 == 1;

            CombineInstance[] combine = new CombineInstance[8];

            Vector3 pivot = (Vector3.right + Vector3.forward) * (k + 1);

            // bottom left
            combine[0].mesh = BuildPlane(shortSide, shortSide, pivot, true, false, false);
            combine[0].transform = Matrix4x4.TRS(
                Vector3.zero, Quaternion.identity, Vector3.one);

            // middle left
            combine[1].mesh = BuildPlane(shortSide, longSide, pivot, true, false, shortMorphShift);
            combine[1].transform = Matrix4x4.TRS(
                Vector3.forward * shortSide, Quaternion.identity, Vector3.one);

            // top left
            combine[2].mesh = BuildPlane(shortSide, shortSide, pivot, true, false, !shortMorphShift);
            combine[2].transform = Matrix4x4.TRS(
                Vector3.forward * sum, Quaternion.identity, Vector3.one);

            // top middle
            combine[3].mesh = BuildPlane(longSide, shortSide, pivot, true, shortMorphShift, !shortMorphShift);
            combine[3].transform = Matrix4x4.TRS(
                Vector3.forward * sum + Vector3.right * shortSide, Quaternion.identity, Vector3.one);

            // top right
            combine[4].mesh = BuildPlane(shortSide, shortSide, pivot, true, !shortMorphShift, !shortMorphShift);
            combine[4].transform = Matrix4x4.TRS(
                Vector3.forward * sum + Vector3.right * sum, Quaternion.identity, Vector3.one);

            // middle right
            combine[5].mesh = BuildPlane(shortSide, longSide, pivot, true, !shortMorphShift, shortMorphShift);
            combine[5].transform = Matrix4x4.TRS(
                Vector3.forward * shortSide + Vector3.right * sum, Quaternion.identity, Vector3.one);

            // bottom right
            combine[6].mesh = BuildPlane(shortSide, shortSide, pivot, true, !shortMorphShift, false);
            combine[6].transform = Matrix4x4.TRS(
                Vector3.right * sum, Quaternion.identity, Vector3.one);

            // bottom middle
            combine[7].mesh = BuildPlane(longSide, shortSide, pivot, true, shortMorphShift, false);
            combine[7].transform = Matrix4x4.TRS(
                Vector3.right * shortSide, Quaternion.identity, Vector3.one);

            mesh.CombineMeshes(combine, true);
            return mesh;
        }

        private static Mesh BuildSkirt(int clipLevelHalfSize, float outerBorderScale)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap skirt";
            CombineInstance[] combine = new CombineInstance[8];

            int borderVertCount = clipLevelHalfSize + Overlap;
            int scale = 2;

            Vector3 pivot = new Vector3(-1f, 0, -1f) * borderVertCount * (1 + 2 * outerBorderScale) + new Vector3(1, 0, 1);

            Mesh quad = BuildPlane(1, 1, Vector3.zero, false);
            Mesh hStrip = BuildPlane(borderVertCount, 1, Vector3.zero, false);
            Mesh vStrip = BuildPlane(1, borderVertCount, Vector3.zero, false);

            outerBorderScale *= borderVertCount * scale;
            Vector3 cornerQuadScale = new Vector3(outerBorderScale, 1, outerBorderScale);
            Vector3 stripScaleVert = new Vector3(scale, 1, outerBorderScale);
            Vector3 stripScaleHor = new Vector3(outerBorderScale, 1, scale);

            combine[0].mesh = quad;
            combine[0].transform = Matrix4x4.TRS(pivot + Vector3.zero, Quaternion.identity, cornerQuadScale);

            combine[1].mesh = hStrip;
            combine[1].transform = Matrix4x4.TRS(pivot + Vector3.right * outerBorderScale, Quaternion.identity, stripScaleVert);

            combine[2].mesh = quad;
            combine[2].transform = Matrix4x4.TRS(pivot + Vector3.right * (outerBorderScale + borderVertCount * scale), Quaternion.identity, cornerQuadScale);

            combine[3].mesh = vStrip;
            combine[3].transform = Matrix4x4.TRS(pivot + Vector3.forward * outerBorderScale, Quaternion.identity, stripScaleHor);

            combine[4].mesh = vStrip;
            combine[4].transform = Matrix4x4.TRS(pivot + Vector3.right * (outerBorderScale + borderVertCount * scale)
                + Vector3.forward * outerBorderScale, Quaternion.identity, stripScaleHor);

            combine[5].mesh = quad;
            combine[5].transform = Matrix4x4.TRS(pivot + Vector3.forward * (outerBorderScale + borderVertCount * scale), Quaternion.identity, cornerQuadScale);

            combine[6].mesh = hStrip;
            combine[6].transform = Matrix4x4.TRS(pivot + Vector3.right * outerBorderScale
                + Vector3.forward * (outerBorderScale + borderVertCount * scale), Quaternion.identity, stripScaleVert);

            combine[7].mesh = quad;
            combine[7].transform = Matrix4x4.TRS(pivot + Vector3.right * (outerBorderScale + borderVertCount * scale)
                + Vector3.forward * (outerBorderScale + borderVertCount * scale), Quaternion.identity, cornerQuadScale);
            mesh.CombineMeshes(combine, true);

            return mesh;
        }

        private static Mesh BuildPlane(int width, int height, Vector3 pivot, bool geomorphOffsetInUv,
            bool morphShiftX = false, bool morphShiftZ = false, int trianglesShift = 0)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Clipmap plane";
            if ((width + 1) * (height + 1) >= 256 * 256)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
            Vector2[] uvs = new Vector2[(width + 1) * (height + 1)];
            int[] triangles = new int[width * height * 2 * 3];
            Vector3[] normals = new Vector3[(width + 1) * (height + 1)];

            for (int i = 0; i < height + 1; i++)
            {
                for (int j = 0; j < width + 1; j++)
                {
                    int x = j;
                    int z = i;

                    Vector3 normalPosition = new Vector3(x, 1, z);

                    if (x % 2 != 0)
                        x += morphShiftX ^ x % 4 == 3 ? 1 : -1;

                    if (z % 2 != 0)
                        z += morphShiftZ ^ z % 4 == 3 ? 1 : -1;

                    vertices[j + i * (width + 1)] = normalPosition - pivot;

                    if (geomorphOffsetInUv)
                        uvs[j + i * (width + 1)] = new Vector2(x - normalPosition.x, z - normalPosition.z);

                    normals[j + i * (width + 1)] = Vector3.up;
                }
            }

            int tris = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = j + i * (width + 1);
                    if ((i + j + trianglesShift) % 2 == 0)
                    {
                        triangles[tris++] = k;
                        triangles[tris++] = k + width + 1;
                        triangles[tris++] = k + width + 2;

                        triangles[tris++] = k;
                        triangles[tris++] = k + width + 2;
                        triangles[tris++] = k + 1;
                    }
                    else
                    {
                        triangles[tris++] = k;
                        triangles[tris++] = k + width + 1;
                        triangles[tris++] = k + 1;

                        triangles[tris++] = k + 1;
                        triangles[tris++] = k + width + 1;
                        triangles[tris++] = k + width + 2;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = normals;
            return mesh;
        }
    }
}

