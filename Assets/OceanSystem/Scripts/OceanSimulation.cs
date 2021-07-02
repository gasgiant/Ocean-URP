using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public class OceanSimulation
    {
        public OceanCollision Collision => _collision;

        public OceanSimulationSettings SimulationSettings;
        public OceanWavesSettings WavesSettings;
        public OceanEqualizerPreset EqualizerPreset;

        private static float OceanTime => (float)(Time.timeSinceLevelLoadAsDouble % 18000);

        private const string InitialSpectrumShaderPath = "ComputeShaders/InitialSpectrum";
        private const string TimeDependentSpectrumShaderPath = "ComputeShaders/TimeDependentSpectrum";
        private const string FoamSimulationShaderPath = "ComputeShaders/FoamSimulation";
        private const int LocalWorkGroupsX = 8;
        private const int LocalWorkGroupsY = 8;

        private readonly ComputeShader _initialSpectrumShader;
        private readonly ComputeShader _timeDependentSpectrumShader;
        private readonly ComputeShader _foamSimulationShader;

        private readonly int _generateNoiseKernel;
        private readonly int _initialSpectrumKernel;
        private readonly int _conjugateSpectrumKernel;
        private readonly int _calculateAmplitudesKernel;
        private readonly int _simulateFoamKernel;
        private readonly int _initializeFoamKernel;

        private int _size;
        private RenderTexture _gaussianNoise;
        private RenderTexture _initialSpectrum;
        private RenderTexture _wavesData;
        private RenderTexture _initializationBuffer;
        private RenderTexture _fftBuffer;
        private RenderTexture _fftInOut;
        private RenderTexture _turbulence;
        private SpectrumSettings[] _spectrums = new SpectrumSettings[2];
        private ComputeBuffer _spectrumsBuffer;
        private OceanCollision _collision;
        private Vector2Int _currrentTextureParams = -Vector2Int.one;
        private bool _isSpectrumInitialized;
        private bool NeedToCalculateSpectrum => !_isSpectrumInitialized || SimulationSettings.UpdateSpectrum;

        public OceanSimulation(OceanSimulationSettings simulationSettings, 
            OceanWavesSettings wavesSettings, OceanEqualizerPreset equalizerPreset)
        {
            SimulationSettings = simulationSettings;
            WavesSettings = wavesSettings;
            EqualizerPreset = equalizerPreset;

            _initialSpectrumShader = (ComputeShader)Resources.Load(InitialSpectrumShaderPath);
            _timeDependentSpectrumShader = (ComputeShader)Resources.Load(TimeDependentSpectrumShaderPath);
            _foamSimulationShader = (ComputeShader)Resources.Load(FoamSimulationShaderPath);

            _generateNoiseKernel = _initialSpectrumShader.FindKernel("GenerateGaussianNoise");
            _initialSpectrumKernel = _initialSpectrumShader.FindKernel("CalculateInitialSpectrum");
            _conjugateSpectrumKernel = _initialSpectrumShader.FindKernel("CalculateConjugatedSpectrum");
            _calculateAmplitudesKernel = _timeDependentSpectrumShader.FindKernel("CalculateAmplitudes");
            _simulateFoamKernel = _foamSimulationShader.FindKernel("Simulate");
            _initializeFoamKernel = _foamSimulationShader.FindKernel("Initialize");

            _spectrumsBuffer = new ComputeBuffer(2, 8 * sizeof(float) + 1 * sizeof(uint));
            _currrentTextureParams = -Vector2Int.one;
        }

        public void Update()
        {
            InitializeSimulation();
            CommandBuffer cmd = CommandBufferPool.Get("Ocean Simulation");
            if (NeedToCalculateSpectrum)
            {
                CalculateSpectrum(cmd);
            }
            UpdateSimulation(cmd, OceanTime * WavesSettings.timeScale, Time.deltaTime * WavesSettings.timeScale);
            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();
            _collision.DoReadbacks();
            SetGlobalShaderVariables();
        }

        public void ReleaseResources()
        {
            if (_spectrumsBuffer != null) _spectrumsBuffer.Release();
            ReleaseRenderTextures();
        }

        private void InitializeSimulation()
        {
            Vector2Int newTextureParams = new Vector2Int(SimulationSettings.Resolution, 
                SimulationSettings.CascadesNumber);
            if (newTextureParams == _currrentTextureParams)
            {
                _fftInOut.anisoLevel = SimulationSettings.AnisoLevel;
                _turbulence.anisoLevel = SimulationSettings.AnisoLevel;
                return;
            }

            _isSpectrumInitialized = false;
            _currrentTextureParams = newTextureParams;
            _size = SimulationSettings.Resolution;
            InitializeRenderTextures(_size, SimulationSettings.CascadesNumber, SimulationSettings.AnisoLevel);
            SetCascadesKeywords(SimulationSettings.CascadesNumber);
            _collision = new OceanCollision(_size, _fftInOut, SimulationSettings);
        }

        private void InitializeRenderTextures(int size, int cascadesNumber, int anisoLevel)
        {
            ReleaseRenderTextures();

            RenderTextureDescriptor initialsDescriptor = new RenderTextureDescriptor()
            {
                height = size,
                width = size,
                volumeDepth = cascadesNumber,
                enableRandomWrite = true,
                colorFormat = RenderTextureFormat.ARGBHalf,
                sRGB = false,
                msaaSamples = 1,
                depthBufferBits = 0,
                useMipMap = false,
                dimension = TextureDimension.Tex2DArray
            };


            var noiseTextureDescriptor = initialsDescriptor;
            noiseTextureDescriptor.colorFormat = RenderTextureFormat.RGHalf;

            var displacementAndDerivativesTextureDescriptor = initialsDescriptor;
            displacementAndDerivativesTextureDescriptor.useMipMap = true;
            displacementAndDerivativesTextureDescriptor.volumeDepth = 2 * cascadesNumber;

            var turbulenceTextureDescriptor = initialsDescriptor;
            turbulenceTextureDescriptor.useMipMap = true;

            _initialSpectrum = RenderingUtils.CreateRenderTexture(initialsDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            _wavesData = RenderingUtils.CreateRenderTexture(initialsDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            _initializationBuffer = RenderingUtils.CreateRenderTexture(initialsDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            _gaussianNoise = RenderingUtils.CreateRenderTexture(noiseTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            _turbulence = RenderingUtils.CreateRenderTexture(turbulenceTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Trilinear, anisoLevel);
            _fftBuffer = RenderingUtils.CreateRenderTexture(displacementAndDerivativesTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Trilinear, anisoLevel);
            _fftInOut = RenderingUtils.CreateRenderTexture(displacementAndDerivativesTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Trilinear, anisoLevel);

            Shader.SetGlobalTexture(GlobalShaderVariables.DisplacementAndDerivatives, _fftInOut);
            Shader.SetGlobalTexture(GlobalShaderVariables.Turbulence, _turbulence);

            _initialSpectrumShader.SetInt(SimualtionVariables.Size, size);
            _initialSpectrumShader.SetInt(SimualtionVariables.CascadesCount, cascadesNumber);
            _initialSpectrumShader.SetTexture(_generateNoiseKernel, SimualtionVariables.Noise, _gaussianNoise);
            _initialSpectrumShader.Dispatch(_generateNoiseKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            _foamSimulationShader.SetTexture(_initializeFoamKernel, SimualtionVariables.Turbulence, _turbulence);
            _foamSimulationShader.Dispatch(_initializeFoamKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
        }

        private void ReleaseRenderTextures()
        {
            if (_gaussianNoise != null) _gaussianNoise.Release();
            if (_initialSpectrum != null) _initialSpectrum.Release();
            if (_wavesData != null) _wavesData.Release();
            if (_initializationBuffer != null) _initializationBuffer.Release();
            if (_fftBuffer != null) _fftBuffer.Release();
            if (_fftInOut != null) _fftInOut.Release();
            if (_turbulence != null) _turbulence.Release();
        }

        private void SetCascadesKeywords(int cascadesNumber)
        {
            Shader.DisableKeyword("OCEAN_THREE_CASCADES");
            Shader.DisableKeyword("OCEAN_FOUR_CASCADES");
            if (cascadesNumber == 3)
                Shader.EnableKeyword("OCEAN_THREE_CASCADES");
            if (cascadesNumber == 4)
                Shader.EnableKeyword("OCEAN_FOUR_CASCADES");
        }

        private void CalculateSpectrum(CommandBuffer cmd)
        {
            Vector4 cutoffsLow, cutoffsHigh;
            SimulationSettings.CalculateCascadeDomains(out cutoffsLow, out cutoffsHigh);

            cmd.SetComputeIntParam(_initialSpectrumShader, SimualtionVariables.Size, _size);
            cmd.SetComputeIntParam(_initialSpectrumShader, SimualtionVariables.CascadesCount, SimulationSettings.CascadesNumber);

            cmd.SetComputeVectorParam(_initialSpectrumShader, SimualtionVariables.LengthScales, SimulationSettings.LengthScales());
            cmd.SetComputeVectorParam(_initialSpectrumShader, SimualtionVariables.CutoffsHigh, cutoffsHigh);
            cmd.SetComputeVectorParam(_initialSpectrumShader, SimualtionVariables.CutoffsLow, cutoffsLow);
            cmd.SetComputeFloatParam(_initialSpectrumShader, SimualtionVariables.Depth, WavesSettings.depth);
            cmd.SetComputeFloatParam(_initialSpectrumShader, SimualtionVariables.Chop, WavesSettings.chop);
            cmd.SetComputeVectorParam(_initialSpectrumShader,
                SimualtionVariables.RampsXLimits, new Vector4(OceanEqualizerPreset.XMin, OceanEqualizerPreset.XMax));

            _spectrums[0] = WavesSettings.local;
            _spectrums[1] = WavesSettings.swell;
            _spectrumsBuffer.SetData(_spectrums);
            cmd.SetComputeBufferParam(_initialSpectrumShader, _initialSpectrumKernel,
                SimualtionVariables.Spectrums, _spectrumsBuffer);

            cmd.SetComputeTextureParam(_initialSpectrumShader,
                _initialSpectrumKernel, SimualtionVariables.H0K, _initializationBuffer);
            cmd.SetComputeTextureParam(_initialSpectrumShader,
                _initialSpectrumKernel, SimualtionVariables.WavesData, _wavesData);
            cmd.SetComputeTextureParam(_initialSpectrumShader,
                _initialSpectrumKernel, SimualtionVariables.Noise, _gaussianNoise);
            cmd.SetComputeTextureParam(_initialSpectrumShader,
                _initialSpectrumKernel, SimualtionVariables.EqualizerRamp, EqualizerPreset.GetRamp());
            // Calculating initial spectrum
            cmd.DispatchCompute(_initialSpectrumShader,
                _initialSpectrumKernel, _size / LocalWorkGroupsX, _size / LocalWorkGroupsY, 1);

            cmd.SetComputeTextureParam(_initialSpectrumShader,
                _conjugateSpectrumKernel, SimualtionVariables.H0, _initialSpectrum);
            cmd.SetComputeTextureParam(_initialSpectrumShader,
                _conjugateSpectrumKernel, SimualtionVariables.H0K, _initializationBuffer);
            // Calculating complex conjugate of the initial spectrum
            cmd.DispatchCompute(_initialSpectrumShader,
                _conjugateSpectrumKernel, _size / LocalWorkGroupsX, _size / LocalWorkGroupsY, 1);

            _isSpectrumInitialized = true;
        }

        private void UpdateSimulation(CommandBuffer cmd, float time, float deltaTime)
        {
            // Calculating complex amplitudes
            cmd.SetComputeIntParam(_timeDependentSpectrumShader,
                SimualtionVariables.CascadesCount, SimulationSettings.CascadesNumber);
            cmd.SetComputeFloatParam(_timeDependentSpectrumShader, SimualtionVariables.Time, time);
            cmd.SetComputeTextureParam(_timeDependentSpectrumShader,
                _calculateAmplitudesKernel, SimualtionVariables.Result, _fftInOut);
            cmd.SetComputeTextureParam(_timeDependentSpectrumShader,
                _calculateAmplitudesKernel, SimualtionVariables.H0, _initialSpectrum);
            cmd.SetComputeTextureParam(_timeDependentSpectrumShader,
                _calculateAmplitudesKernel, SimualtionVariables.WavesData, _wavesData);
            cmd.DispatchCompute(_timeDependentSpectrumShader,
                _calculateAmplitudesKernel, _size / LocalWorkGroupsX, _size / LocalWorkGroupsY, 1);

            // Calculating IFFTs of complex amplitudes
            FFTCompute.IFFT2D(cmd, _fftInOut, _fftBuffer, true, false, true);
            cmd.GenerateMips(_fftInOut);

            // Simulating foam
            if (SimulationSettings.SimulateFoam)
            {
                cmd.SetComputeIntParam(_foamSimulationShader,
                    SimualtionVariables.CascadesCount, SimulationSettings.CascadesNumber);
                cmd.SetComputeFloatParam(_foamSimulationShader, SimualtionVariables.DeltaTime, deltaTime);
                cmd.SetComputeFloatParam(_foamSimulationShader, SimualtionVariables.FoamDecayRate, WavesSettings.foam.decayRate);

                cmd.SetComputeTextureParam(_foamSimulationShader,
                    _simulateFoamKernel, SimualtionVariables.Input, _fftInOut);
                cmd.SetComputeTextureParam(_foamSimulationShader,
                    _simulateFoamKernel, SimualtionVariables.Turbulence, _turbulence);

                cmd.DispatchCompute(_foamSimulationShader,
                    _simulateFoamKernel, _size / LocalWorkGroupsX, _size / LocalWorkGroupsY, 1);
                cmd.GenerateMips(_turbulence); 
            }
        }

        private void SetGlobalShaderVariables()
        {
            // waves
            Shader.SetGlobalVector(GlobalShaderVariables.LengthScales, SimulationSettings.LengthScales());
            float windAngle = WavesSettings.local.windDirection * Mathf.Deg2Rad;
            Shader.SetGlobalVector(GlobalShaderVariables.WindDirection, new Vector2(Mathf.Cos(windAngle), Mathf.Sin(windAngle)));
            Shader.SetGlobalMatrix(GlobalShaderVariables.WorldToWindSpace,
                Matrix4x4.Rotate(Quaternion.AngleAxis(-WavesSettings.local.windDirection, Vector3.up)));
            Shader.SetGlobalFloat(GlobalShaderVariables.WindSpeed, WavesSettings.local.windSpeed);
            Shader.SetGlobalFloat(GlobalShaderVariables.WavesScale, WavesSettings.local.scale);
            Shader.SetGlobalFloat(GlobalShaderVariables.WavesAlignement, WavesSettings.local.alignment);

            // floam
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamCoverage, WavesSettings.foam.coverage);
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamUnderwater, WavesSettings.foam.underwater);
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamDensity, WavesSettings.foam.density);
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamPersistence, WavesSettings.foam.persistence);
            Shader.SetGlobalVector(GlobalShaderVariables.FoamCascadesWeights, WavesSettings.foam.cascadesWeights);
        }

        private static class SimualtionVariables
        {
            // Shader props IDs
            public static readonly int Size = Shader.PropertyToID("Size");
            public static readonly int CascadesCount = Shader.PropertyToID("CascadesCount");
            public static readonly int LengthScales = Shader.PropertyToID("LengthScales");
            public static readonly int CutoffsLow = Shader.PropertyToID("CutoffsLow");
            public static readonly int CutoffsHigh = Shader.PropertyToID("CutoffsHigh");
            public static readonly int Depth = Shader.PropertyToID("Depth");
            public static readonly int Chop = Shader.PropertyToID("Chop");
            public static readonly int RampsXLimits = Shader.PropertyToID("RampsXLimits");
            public static readonly int Spectrums = Shader.PropertyToID("Spectrums");
            public static readonly int Time = Shader.PropertyToID("Time");
            public static readonly int DeltaTime = Shader.PropertyToID("DeltaTime");
            public static readonly int EqualizerRamp = Shader.PropertyToID("EqualizerRamp");
            public static readonly int H0K = Shader.PropertyToID("H0K");
            public static readonly int WavesData = Shader.PropertyToID("WavesData");
            public static readonly int H0 = Shader.PropertyToID("H0");
            public static readonly int Result = Shader.PropertyToID("Result");
            public static readonly int Input = Shader.PropertyToID("Input");
            public static readonly int Noise = Shader.PropertyToID("Noise");
            public static readonly int Turbulence = Shader.PropertyToID("Turbulence");
            public static readonly int FoamDecayRate = Shader.PropertyToID("FoamDecayRate");
        }

        private static class GlobalShaderVariables
        {
            // textures
            public static readonly int DisplacementAndDerivatives = Shader.PropertyToID("Ocean_DisplacementAndDerivatives");
            public static readonly int Turbulence = Shader.PropertyToID("Ocean_Turbulence");

            // waves
            public static readonly int LengthScales = Shader.PropertyToID("Ocean_LengthScales");
            public static readonly int WindSpeed = Shader.PropertyToID("Ocean_WindSpeed");
            public static readonly int WavesScale = Shader.PropertyToID("Ocean_WavesScale");
            public static readonly int WavesAlignement = Shader.PropertyToID("Ocean_WavesAlignement");
            public static readonly int WindDirection = Shader.PropertyToID("Ocean_WindDirection");
            public static readonly int WorldToWindSpace = Shader.PropertyToID("Ocean_WorldToWindSpace");

            // foam
            public static readonly int FoamCoverage = Shader.PropertyToID("Ocean_FoamCoverage");
            public static readonly int FoamUnderwater = Shader.PropertyToID("Ocean_FoamUnderwater");
            public static readonly int FoamDensity = Shader.PropertyToID("Ocean_FoamDensity");
            public static readonly int FoamPersistence = Shader.PropertyToID("Ocean_FoamPersistence");
            public static readonly int FoamCascadesWeights = Shader.PropertyToID("Ocean_FoamCascadesWeights");
        }
    }
}
