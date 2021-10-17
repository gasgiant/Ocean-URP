using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public static class ComputeFFT
    {
        private const string ShaderPath = "ComputeShaders/FFT";

        private static ComputeShader _fftShader;
        private static int _fftKernel;
        private static int _postProcessKernel;

        private static readonly int[] _supportedSizes = { 64, 128, 256, 512 };
        private static readonly string[] _sizeKeywords;
        private static readonly string _sizeErrorMessage;

        static ComputeFFT()
        {
            _sizeKeywords = new string[_supportedSizes.Length];
            _sizeErrorMessage = "ComputeFFT supports only textures of sizes ";
            for (int i = 0; i < _supportedSizes.Length; i++)
            {
                // Smallest size is used when none of the keywords are enabled.
                if (i > 0)
                    _sizeKeywords[i] = "FFT_SIZE_" + _supportedSizes[i];
                _sizeErrorMessage += _supportedSizes[i]
                    + ((i < _supportedSizes.Length - 1) ? ", " : ".");
            }
        }

        public static void FFT2D(CommandBuffer cmd, RenderTexture input)
        {
            DoFFT(cmd, input, false, false, false);
        }

        public static void IFFT2D(CommandBuffer cmd, RenderTexture input,
            bool scale = true, bool permute = false)
        {
            DoFFT(cmd, input, true, scale, permute);
        }

        private static void DoFFT(CommandBuffer cmd, RenderTexture input,
            bool inverse, bool scale, bool permute)
        {
            int size = input.height;

            bool isSquare = input.width == input.height;
            Debug.Assert(isSquare, "ComputeFFT supports only square textures.");

            bool isSizeSupported = SetSizeKeywords(cmd, size);
            Debug.Assert(isSizeSupported, _sizeErrorMessage);

            bool isSupportedDimension = input.dimension == TextureDimension.Tex2D
                || input.dimension == TextureDimension.Tex2DArray;
            Debug.Assert(isSquare, "ComputeFFT supports only Tex2D and Tex2DArray texture dimensions.");

            ComputeShader fftShader = GetFftShader();
            bool isFftShaderFound = fftShader;
            Debug.Assert(isFftShaderFound, "Couldn't find FFT.compute in Resources at path ''" + ShaderPath + "''.");

            if (!isSizeSupported || !isSquare || !isSupportedDimension || !isFftShaderFound) return;


            cmd.SetComputeFloatParam(fftShader, ShaderVariables.Inverse, inverse ? 1 : 0);

            if (input.dimension == TextureDimension.Tex2DArray)
            {
                cmd.SetComputeIntParam(fftShader, ShaderVariables.TargetsCount, input.volumeDepth);
                cmd.EnableShaderKeyword("FFT_ARRAY_TARGET");
            }
            else
            {
                cmd.DisableShaderKeyword("FFT_ARRAY_TARGET");
            }

            cmd.SetComputeTextureParam(fftShader, _fftKernel, ShaderVariables.Target, input);
            cmd.SetComputeFloatParam(fftShader, ShaderVariables.Direction, 0);
            cmd.DispatchCompute(fftShader, _fftKernel, 1, size, 1);
            cmd.SetComputeFloatParam(fftShader, ShaderVariables.Direction, 1);
            cmd.DispatchCompute(fftShader, _fftKernel, 1, size, 1);

            if (scale || permute)
            {
                cmd.SetComputeFloatParam(fftShader, ShaderVariables.Scale, scale ? 1 : 0);
                cmd.SetComputeFloatParam(fftShader, ShaderVariables.Permute, permute ? 1 : 0);
                cmd.SetComputeTextureParam(fftShader, _postProcessKernel, ShaderVariables.Target, input);
                int groupsCount = Mathf.CeilToInt((float)size / 8);
                cmd.DispatchCompute(fftShader, _postProcessKernel, groupsCount, groupsCount, 1);
            }
        }

        private static ComputeShader GetFftShader()
        {
            if (_fftShader)
            {
                return _fftShader;
            }
            else
            {
                _fftShader = (ComputeShader)Resources.Load(ShaderPath);
                if (_fftShader)
                {
                    _fftKernel = _fftShader.FindKernel("Fft");
                    _postProcessKernel = _fftShader.FindKernel("PostProcess");
                }
                return _fftShader;
            }
        }

        private static bool SetSizeKeywords(CommandBuffer cmd, int size)
        {
            bool validSize = false;

            for (int i = 0; i < _supportedSizes.Length; i++)
            {
                if (size == _supportedSizes[i])
                {
                    validSize = true;
                    break;
                }
            }

            if (validSize)
            {
                for (int i = 0; i < _supportedSizes.Length; i++)
                {
                    if (_sizeKeywords[i] == null) continue;
                    if (size == _supportedSizes[i])
                        cmd.EnableShaderKeyword(_sizeKeywords[i]);
                    else
                        cmd.DisableShaderKeyword(_sizeKeywords[i]);
                }
            }

            return validSize;
        }

        private static class ShaderVariables
        {
            public static readonly int Target = Shader.PropertyToID("Target");
            public static readonly int TargetsCount = Shader.PropertyToID("TargetsCount");
            public static readonly int Direction = Shader.PropertyToID("Direction");
            public static readonly int Inverse = Shader.PropertyToID("Inverse");
            public static readonly int Scale = Shader.PropertyToID("Scale");
            public static readonly int Permute = Shader.PropertyToID("Permute");
        }
    }
}
