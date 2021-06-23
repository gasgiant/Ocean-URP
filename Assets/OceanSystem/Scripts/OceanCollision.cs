using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public class OceanCollision
    {
        private Texture2D[] displacementReadbackTextures = new Texture2D[2];
        private System.Action<AsyncGPUReadbackRequest>[] readbackCallbacks = new System.Action<AsyncGPUReadbackRequest>[2];
        private bool[] readbackHasValidData = new bool[2];

        private int size;
        private RenderTexture displacementTexture;
        private OceanSimulationSettings simulationSettings;

        public OceanCollision(int size, RenderTexture displacementTexture, OceanSimulationSettings simulationSettings)
        {
            readbackCallbacks[0] = (AsyncGPUReadbackRequest r) => OnCompleteReadback(r, 0);
            readbackCallbacks[1] = (AsyncGPUReadbackRequest r) => OnCompleteReadback(r, 1);
            this.size = size;
            this.displacementTexture = displacementTexture;
            this.simulationSettings = simulationSettings;
            CreateTextures(size);
        }

        public float GetWaterHeight(Vector3 position)
        {
            if (!readbackHasValidData[0] || simulationSettings.ReadbackCascades <= 0)
                return 0;
            if (!readbackHasValidData[1] && simulationSettings.ReadbackCascades > 1)
                return 0;

            Vector3 displacement = GetWaterDisplacement(position);
            for (int i = 1; i < simulationSettings.samplingIterations; i++)
            {
                displacement = GetWaterDisplacement(position - displacement);
            }
            return displacement.y;
        }

        public Vector3 GetWaterDisplacement(Vector3 position)
        {
            Vector3 res = Vector3.zero;
            res += GetWaterDisplacement(position, 0);
            if (simulationSettings.ReadbackCascades > 1)
                res += GetWaterDisplacement(position, 1);
            return res;
        }

        public void DoReadbacks()
        {
            if (simulationSettings.ReadbackCascades > 0)
                AsyncGPUReadback.Request(displacementTexture, 0, 0, size, 0, size, 0, 1, TextureFormat.RGBAHalf, readbackCallbacks[0]);
            if (simulationSettings.ReadbackCascades > 1)
                AsyncGPUReadback.Request(displacementTexture, 0, 0, size, 0, size, 2, 1, TextureFormat.RGBAHalf, readbackCallbacks[1]);
        }

        private void CreateTextures(int size)
        {
            displacementReadbackTextures[0] = CreateReadbackTexture2D(size);
            displacementReadbackTextures[1] = CreateReadbackTexture2D(size);
        }

        private static Texture2D CreateReadbackTexture2D(int size)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBAHalf, false, true);
            tex.SetPixel(0, 0, Color.clear);
            tex.Resize(size, size);
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        private void OnCompleteReadback(AsyncGPUReadbackRequest request, int cascade)
        {
            if (request.hasError)
            {
                Debug.LogError("GPU readback error.");
                return;
            }

            if (displacementReadbackTextures != null
                && displacementReadbackTextures[cascade] != null)
            {
                displacementReadbackTextures[cascade].LoadRawTextureData(request.GetData<Color>());
                displacementReadbackTextures[cascade].Apply();
                readbackHasValidData[cascade] = true;
            }
        }

        private Vector3 GetWaterDisplacement(Vector3 position, int cascade)
        {
            float lengthScale = simulationSettings.LengthScales()[cascade];
            Vector2 uv = new Vector2(position.x, position.z) / lengthScale - 0.5f * Vector2.one / size;
            Vector4 d = displacementReadbackTextures[cascade].GetPixelBilinear(uv.x, uv.y, 0);
            return d;
        }
    }
}
