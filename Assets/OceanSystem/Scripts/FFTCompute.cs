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
            foreach (var item in precomputedDatas.Values)
            {
                item.Release();
            }

            precomputedDatas.Clear();
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

            cmd.SetComputeIntParam(fftShader, TargetsCountID, input.volumeDepth);

            if (inverse)
                cmd.EnableShaderKeyword("FFT_INVERSE");
            else
                cmd.DisableShaderKeyword("FFT_INVERSE");

            bool pingPong = false;
            cmd.SetComputeTextureParam(fftShader, FftStepKernel, PrecomputedDataID, GetPrecomputedData(size));
            cmd.SetComputeTextureParam(fftShader, FftStepKernel, Buffer0ID, input);
            cmd.SetComputeTextureParam(fftShader, FftStepKernel, Buffer1ID, buffer);

            cmd.DisableShaderKeyword("FFT_DIRECTION");
            for (int i = 0; i < logSize; i++)
            {
                pingPong = !pingPong;
                cmd.SetComputeIntParam(fftShader, StepID, i);
                if (pingPong)
                    cmd.EnableShaderKeyword("FFT_PING_PONG");
                else
                    cmd.DisableShaderKeyword("FFT_PING_PONG");
                cmd.DispatchCompute(fftShader, FftStepKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            cmd.EnableShaderKeyword("FFT_DIRECTION");
            for (int i = 0; i < logSize; i++)
            {
                pingPong = !pingPong;
                cmd.SetComputeIntParam(fftShader, StepID, i);
                if (pingPong)
                    cmd.EnableShaderKeyword("FFT_PING_PONG");
                else
                    cmd.DisableShaderKeyword("FFT_PING_PONG");
                cmd.DispatchCompute(fftShader, FftStepKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
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
                cmd.SetComputeIntParam(fftShader, SizeID, size);
                cmd.SetComputeTextureParam(fftShader, PermuteKernel, Buffer0ID, outputToInput ? input : buffer);
                cmd.DispatchCompute(fftShader, PermuteKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            if (scale)
            {
                cmd.SetComputeIntParam(fftShader, SizeID, size);
                cmd.SetComputeTextureParam(fftShader, ScaleKernel, Buffer0ID, outputToInput ? input : buffer);
                cmd.DispatchCompute(fftShader, ScaleKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            }

            Graphics.ExecuteCommandBuffer(cmd);
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
