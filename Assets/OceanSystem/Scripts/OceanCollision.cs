using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public class OceanCollision
    {
        private Texture2D[] _displacementReadbackTextures = new Texture2D[2];
        private System.Action<AsyncGPUReadbackRequest>[] _readbackCallbacks = new System.Action<AsyncGPUReadbackRequest>[2];
        private bool[] _readbackHasValidData = new bool[2];

        private int _size;
        private RenderTexture _displacementTexture;
        private OceanSimulationSettings _simulationSettings;

        public OceanCollision(int size, RenderTexture displacementTexture, OceanSimulationSettings simulationSettings)
        {
            _readbackCallbacks[0] = (AsyncGPUReadbackRequest r) => OnCompleteReadback(r, 0);
            _readbackCallbacks[1] = (AsyncGPUReadbackRequest r) => OnCompleteReadback(r, 1);
            _size = size;
            _displacementTexture = displacementTexture;
            _simulationSettings = simulationSettings;
            CreateTextures(size);
        }

        public float GetWaterHeight(Vector3 position)
        {
            if (!_readbackHasValidData[0] || _simulationSettings.ReadbackCascades <= 0)
                return 0;
            if (!_readbackHasValidData[1] && _simulationSettings.ReadbackCascades > 1)
                return 0;

            Vector3 displacement = GetWaterDisplacement(position);
            for (int i = 1; i < _simulationSettings.SamplingIterations; i++)
            {
                displacement = GetWaterDisplacement(position - displacement);
            }
            return displacement.y;
        }

        public Vector3 GetWaterDisplacement(Vector3 position)
        {
            Vector3 res = Vector3.zero;
            res += GetWaterDisplacement(position, 0);
            if (_simulationSettings.ReadbackCascades > 1)
                res += GetWaterDisplacement(position, 1);
            return res;
        }

        public void DoReadbacks()
        {
            if (_simulationSettings.ReadbackCascades > 0)
                AsyncGPUReadback.Request(_displacementTexture, 0, 0, _size, 0, _size, 0, 1, TextureFormat.RGBAHalf, _readbackCallbacks[0]);
            if (_simulationSettings.ReadbackCascades > 1)
                AsyncGPUReadback.Request(_displacementTexture, 0, 0, _size, 0, _size, 2, 1, TextureFormat.RGBAHalf, _readbackCallbacks[1]);
        }

        private void CreateTextures(int size)
        {
            _displacementReadbackTextures[0] = CreateReadbackTexture2D(size);
            _displacementReadbackTextures[1] = CreateReadbackTexture2D(size);
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

            if (_displacementReadbackTextures != null
                && _displacementReadbackTextures[cascade] != null)
            {
                _displacementReadbackTextures[cascade].LoadRawTextureData(request.GetData<Color>());
                _displacementReadbackTextures[cascade].Apply();
                _readbackHasValidData[cascade] = true;
            }
        }

        private Vector3 GetWaterDisplacement(Vector3 position, int cascade)
        {
            float lengthScale = _simulationSettings.LengthScales()[cascade];
            Vector2 uv = new Vector2(position.x, position.z) / lengthScale - 0.5f * Vector2.one / _size;
            Vector4 d = _displacementReadbackTextures[cascade].GetPixelBilinear(uv.x, uv.y, 0);
            return d;
        }
    }
}
