using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public static class FFTCompute
    {
        private const string ShaderPath = "ComputeShaders/FFT";
        private const int LocalWorkGroups = 8;

        private static readonly Dictionary<int, RenderTexture> _precomputedData = new Dictionary<int, RenderTexture>();
        private static ComputeShader _fftShader;
        private static int _precomputeKernel;
        private static int _fftStepKernel;
        private static int _scaleKernel;
        private static int _permuteKernel;

        public static void FFT2D(CommandBuffer cmd, RenderTexture input, RenderTexture buffer, bool outputToInput = false)
        {
            DoFFT(cmd, false, input, buffer, outputToInput, false, false);
        }

        public static void IFFT2D(CommandBuffer cmd, RenderTexture input, RenderTexture buffer, bool outputToInput = false,
            bool scale = true, bool permute = false)
        {
            DoFFT(cmd, true, input, buffer, outputToInput, scale, permute);
        }

        public static void ReleasePrecomputedTextures()
        {
            foreach (var item in _precomputedData.Values)
            {
                item.Release();
            }

            _precomputedData.Clear();
        }

        private static void DoFFT(CommandBuffer cmd, bool inverse, RenderTexture input, RenderTexture buffer, bool outputToInput = false,
            bool scale = true, bool permute = false)
        {
            bool dimensionsEqual = input.width == buffer.width
                                   && input.height == buffer.height
                                   && input.volumeDepth == buffer.volumeDepth;
            Debug.Assert(dimensionsEqual, "FFT input and buffer textures width, height and volumeDepth must be equal.");

            bool isSquare = input.width == input.height;
            Debug.Assert(isSquare, "FFT supports only square textures.");

            int size = input.height;
            float floatLogSize = Mathf.Log(input.height, 2);
            int logSize = (int) floatLogSize;
            bool isPowerOfTwo = Mathf.Abs(logSize - floatLogSize) < 1e-3;
            Debug.Assert(isPowerOfTwo, "FFT supports only power of two texture sizes.");

            bool isAtLeastEight = input.width >= 8;
            Debug.Assert(isAtLeastEight, "FFT size must be at least 8.");

            if (!dimensionsEqual || !isSquare || !isPowerOfTwo || !isAtLeastEight)
                return;

            ComputeShader fftShader = GetFftShader();

            cmd.SetComputeIntParam(fftShader, ShaderVariables.TargetsCount, input.volumeDepth);

            cmd.SetComputeFloatParam(fftShader, ShaderVariables.Inverse, inverse ? 1 : 0);

            bool pingPong = false;
            cmd.SetComputeTextureParam(fftShader, _fftStepKernel, ShaderVariables.PrecomputedData, GetPrecomputedData(size));
            cmd.SetComputeTextureParam(fftShader, _fftStepKernel, ShaderVariables.Buffer0, input);
            cmd.SetComputeTextureParam(fftShader, _fftStepKernel, ShaderVariables.Buffer1, buffer);

            cmd.SetComputeFloatParam(fftShader, ShaderVariables.Direction, 0);
            for (int i = 0; i < logSize; i++)
            {
                pingPong = !pingPong;
                cmd.SetComputeIntParam(fftShader, ShaderVariables.Step, i);
                cmd.SetComputeFloatParam(fftShader, ShaderVariables.PingPong, pingPong ? 1 : 0);
                cmd.DispatchCompute(fftShader, _fftStepKernel, WorkGroups(size), WorkGroups(size), 1);
            }

            cmd.SetComputeFloatParam(fftShader, ShaderVariables.Direction, 1);
            for (int i = 0; i < logSize; i++)
            {
                pingPong = !pingPong;
                cmd.SetComputeIntParam(fftShader, ShaderVariables.Step, i);
                cmd.SetComputeFloatParam(fftShader, ShaderVariables.PingPong, pingPong ? 1 : 0);
                cmd.DispatchCompute(fftShader, _fftStepKernel, WorkGroups(size), WorkGroups(size), 1);
            }

            if (pingPong && outputToInput)
            {
                cmd.CopyTexture(buffer, input);
            }

            if (!pingPong && !outputToInput)
            {
                cmd.CopyTexture(input, buffer);
            }

            if (permute)
            {
                cmd.SetComputeIntParam(fftShader, ShaderVariables.Size, size);
                cmd.SetComputeTextureParam(fftShader, _permuteKernel, ShaderVariables.Buffer0, outputToInput ? input : buffer);
                cmd.DispatchCompute(fftShader, _permuteKernel, WorkGroups(size), WorkGroups(size), 1);
            }

            if (scale)
            {
                cmd.SetComputeIntParam(fftShader, ShaderVariables.Size, size);
                cmd.SetComputeTextureParam(fftShader, _scaleKernel, ShaderVariables.Buffer0, outputToInput ? input : buffer);
                cmd.DispatchCompute(fftShader, _scaleKernel, WorkGroups(size), WorkGroups(size), 1);
            }
        }

        private static int WorkGroups(int size) => Mathf.CeilToInt(((float)size) / LocalWorkGroups);

        private static ComputeShader GetFftShader()
        {
            if (_fftShader != null && Application.isPlaying)
            {
                return _fftShader;
            }
            else
            {
                _fftShader = (ComputeShader)Resources.Load(ShaderPath);
                _precomputeKernel = _fftShader.FindKernel("PrecomputeTwiddleFactorsAndInputIndices");
                _fftStepKernel = _fftShader.FindKernel("FftStep");
                _scaleKernel = _fftShader.FindKernel("Scale");
                _permuteKernel = _fftShader.FindKernel("Permute");
                return _fftShader;
            }
        }

        private static RenderTexture GetPrecomputedData(int size)
        {
            if (_precomputedData.ContainsKey(size) && Application.isPlaying)
                return _precomputedData[size];
            else
            {
                int logSize = (int) Mathf.Log(size, 2);
                RenderTexture rt = new RenderTexture(logSize, size, 0,
                    RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                rt.enableRandomWrite = true;
                rt.Create();

                _fftShader.SetInt(ShaderVariables.Size, size);
                _fftShader.SetTexture(_precomputeKernel, ShaderVariables.PrecomputedData, rt);
                _fftShader.Dispatch(_precomputeKernel, logSize, WorkGroups(size / 2), 1);
                if (Application.isPlaying)
                    _precomputedData.Add(size, rt);
                return rt;
            }
        }

        private static class ShaderVariables
        {
            public static readonly int PrecomputedData = Shader.PropertyToID("PrecomputedData");
            public static readonly int Buffer0 = Shader.PropertyToID("Buffer0");
            public static readonly int Buffer1 = Shader.PropertyToID("Buffer1");
            public static readonly int Size = Shader.PropertyToID("Size");
            public static readonly int Step = Shader.PropertyToID("Step");
            public static readonly int TargetsCount = Shader.PropertyToID("TargetsCount");
            public static readonly int Direction = Shader.PropertyToID("Direction");
            public static readonly int PingPong = Shader.PropertyToID("PingPong");
            public static readonly int Inverse = Shader.PropertyToID("Inverse");
        }
    }
}
