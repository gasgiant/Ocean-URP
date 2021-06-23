using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public static class FFTCompute
    {
        private const string ShaderPath = "ComputeShaders/FFT";
        private const int LocalWorkGroupsX = 8;
        private const int LocalWorkGroupsY = 8;

        private static readonly Dictionary<int, RenderTexture> precomputedDatas = new Dictionary<int, RenderTexture>();
        private static ComputeShader FftShader;
        private static CommandBuffer Cbuff;

        public static void FFT2D(RenderTexture input, RenderTexture buffer, bool outputToInput = false)
        {
            DoFFT(false, input, buffer, outputToInput, false, false);
        }

        public static void IFFT2D(RenderTexture input, RenderTexture buffer, bool outputToInput = false,
            bool scale = true, bool permute = false)
        {
            DoFFT(true, input, buffer, outputToInput, scale, permute);
        }

        public static void ReleasePrecomputedTextures()
        {
            foreach (var item in precomputedDatas.Values)
            {
                item.Release();
            }

            precomputedDatas.Clear();
        }

        private static void DoFFT(bool inverse, RenderTexture input, RenderTexture buffer, bool outputToInput = false,
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
            CommandBuffer cbuff = GetCommandBuffer();
            cbuff.Clear();

            cbuff.SetComputeIntParam(fftShader, TargetsCountID, input.volumeDepth);

            if (inverse)
                cbuff.EnableShaderKeyword("FFT_INVERSE");
            else
                cbuff.DisableShaderKeyword("FFT_INVERSE");

            bool pingPong = false;
            cbuff.SetComputeTextureParam(fftShader, FftStepKernel, PrecomputedDataID, GetPrecomputedData(size));
            cbuff.SetComputeTextureParam(fftShader, FftStepKernel, Buffer0ID, input);
            cbuff.SetComputeTextureParam(fftShader, FftStepKernel, Buffer1ID, buffer);

            cbuff.DisableShaderKeyword("FFT_DIRECTION");
            for (int i = 0; i < logSize; i++)
            {
                pingPong = !pingPong;
                cbuff.SetComputeIntParam(fftShader, StepID, i);
                if (pingPong)
                    cbuff.EnableShaderKeyword("FFT_PING_PONG");
                else
                    cbuff.DisableShaderKeyword("FFT_PING_PONG");
                cbuff.DispatchCompute(fftShader, FftStepKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            cbuff.EnableShaderKeyword("FFT_DIRECTION");
            for (int i = 0; i < logSize; i++)
            {
                pingPong = !pingPong;
                cbuff.SetComputeIntParam(fftShader, StepID, i);
                if (pingPong)
                    cbuff.EnableShaderKeyword("FFT_PING_PONG");
                else
                    cbuff.DisableShaderKeyword("FFT_PING_PONG");
                cbuff.DispatchCompute(fftShader, FftStepKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            if (pingPong && outputToInput)
            {
                cbuff.CopyTexture(buffer, input);
            }

            if (!pingPong && !outputToInput)
            {
                cbuff.CopyTexture(input, buffer);
            }

            if (permute)
            {
                cbuff.SetComputeIntParam(fftShader, SizeID, size);
                cbuff.SetComputeTextureParam(fftShader, PermuteKernel, Buffer0ID, outputToInput ? input : buffer);
                cbuff.DispatchCompute(fftShader, PermuteKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            if (scale)
            {
                cbuff.SetComputeIntParam(fftShader, SizeID, size);
                cbuff.SetComputeTextureParam(fftShader, ScaleKernel, Buffer0ID, outputToInput ? input : buffer);
                cbuff.DispatchCompute(fftShader, ScaleKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            Graphics.ExecuteCommandBuffer(cbuff);
        }

        private static ComputeShader GetFftShader()
        {
            if (FftShader == null)
            {
                FftShader = (ComputeShader) Resources.Load(ShaderPath);
                PrecomputeKernel = FftShader.FindKernel("PrecomputeTwiddleFactorsAndInputIndices");
                FftStepKernel = FftShader.FindKernel("FftStep");
                ScaleKernel = FftShader.FindKernel("Scale");
                PermuteKernel = FftShader.FindKernel("Permute");
            }

            return FftShader;
        }

        private static CommandBuffer GetCommandBuffer()
        {
            if (Cbuff == null)
            {
                Cbuff = new CommandBuffer();
                Cbuff.name = "FFT";
            }

            return Cbuff;
        }

        private static RenderTexture GetPrecomputedData(int size)
        {
            if (precomputedDatas.ContainsKey(size))
                return precomputedDatas[size];
            else
            {
                int logSize = (int) Mathf.Log(size, 2);
                RenderTexture rt = new RenderTexture(logSize, size, 0,
                    RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                rt.enableRandomWrite = true;
                rt.Create();

                FftShader.SetInt(SizeID, size);
                FftShader.SetTexture(PrecomputeKernel, PrecomputedDataID, rt);
                FftShader.Dispatch(PrecomputeKernel, logSize, size / 2 / LocalWorkGroupsY, 1);
                precomputedDatas.Add(size, rt);
                return rt;
            }
        }

        // Kernel IDs:
        private static int PrecomputeKernel;
        private static int FftStepKernel;
        private static int ScaleKernel;
        private static int PermuteKernel;

        // Property IDs:
        private static readonly int PrecomputedDataID = Shader.PropertyToID("PrecomputedData");
        private static readonly int Buffer0ID = Shader.PropertyToID("Buffer0");
        private static readonly int Buffer1ID = Shader.PropertyToID("Buffer1");
        private static readonly int SizeID = Shader.PropertyToID("Size");
        private static readonly int StepID = Shader.PropertyToID("Step");
        private static readonly int TargetsCountID = Shader.PropertyToID("TargetsCount");
    }
}
