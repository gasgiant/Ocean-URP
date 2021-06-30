using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace OceanSystem
{
    //[ExecuteAlways]
    public class OceanSimulation : MonoBehaviour
    {
        private const string InitialSpectrumShaderPath = "ComputeShaders/InitialSpectrum";
        private const string TimeDependentSpectrumShaderPath = "ComputeShaders/TimeDependentSpectrum";
        private const string FoamSimulationShaderPath = "ComputeShaders/FoamSimulation";
        private const int LocalWorkGroupsX = 8;
        private const int LocalWorkGroupsY = 8;

        public OceanCollision Collision => collision;

        [SerializeField]
        private OceanSimulationSettings simulationSettings;
        [SerializeField]
        private WavesSettings wavesSettings;
        [SerializeField]
        private OceanEqualizerPreset equalizerPreset;

        private int size;
        private ComputeShader initialSpectrumShader;
        private ComputeShader timeDependentSpectrumShader;
        private ComputeShader foamSimulationShader;

        private RenderTexture gaussianNoise;
        private RenderTexture initialSpectrum;
        private RenderTexture wavesData;
        private RenderTexture initializationBuffer;
        private RenderTexture fftBuffer;
        private RenderTexture fftInOut;
        private RenderTexture turbulence;
        private SpectrumSettings[] spectrums = new SpectrumSettings[2];
        private ComputeBuffer spectrumsBuffer;
        private OceanCollision collision;
        private bool spectrumCalculated;
        private bool NeedToCalculateSpectrum => !spectrumCalculated || simulationSettings.updateSpectrum;
        Vector3Int currrentTextureParams = -Vector3Int.one;

        private static float OceanTime => (float)(Time.timeSinceLevelLoadAsDouble % 18000);

        private void OnEnable()
        {
            initialSpectrumShader = (ComputeShader)Resources.Load(InitialSpectrumShaderPath);
            timeDependentSpectrumShader = (ComputeShader)Resources.Load(TimeDependentSpectrumShaderPath);
            foamSimulationShader = (ComputeShader)Resources.Load(FoamSimulationShaderPath);

            GenerateNoiseKernel = initialSpectrumShader.FindKernel("GenerateGaussianNoise");
            InitialSpectrumKernel = initialSpectrumShader.FindKernel("CalculateInitialSpectrum");
            ConjugateSpectrumKernel = initialSpectrumShader.FindKernel("CalculateConjugatedSpectrum");
            CalculateAmplitudesKernel = timeDependentSpectrumShader.FindKernel("CalculateAmplitudes");
            SimulateFoamKernel = foamSimulationShader.FindKernel("Simulate");
            InitializeFoamKernel = foamSimulationShader.FindKernel("Initialize");

            if (spectrumsBuffer != null) spectrumsBuffer.Release();
            spectrumsBuffer = new ComputeBuffer(2, 8 * sizeof(float) + 1 * sizeof(uint));

            currrentTextureParams = -Vector3Int.one;
        }

        private void Update()
        {
            InitializeSimulation();

            CommandBuffer cmd = CommandBufferPool.Get("Ocean Simulation");
            if (NeedToCalculateSpectrum)
            {
                CalculateSpectrum(cmd);
            }
            UpdateSimulation(cmd, OceanTime * wavesSettings.timeScale, UnityEngine.Time.deltaTime * wavesSettings.timeScale);
            Graphics.ExecuteCommandBuffer(cmd);
            cmd.Release();
            collision.DoReadbacks();
            SetGlobalShaderVariables();
        }

        private void OnDisable()
        {
            spectrumsBuffer?.Release();
        }

        private void InitializeSimulation()
        {
            Vector3Int newTextureParams = new Vector3Int(simulationSettings.Resolution, 
                simulationSettings.CascadesNumber, simulationSettings.anisoLevel);
            if (newTextureParams == currrentTextureParams)
                return;

            currrentTextureParams = newTextureParams;
            size = simulationSettings.Resolution;
            InitializeRenderTextures(size, simulationSettings.CascadesNumber, simulationSettings.anisoLevel);
            SetCascadesKeywords(simulationSettings.CascadesNumber);
            collision = new OceanCollision(size, fftInOut, simulationSettings);
        }

        private void InitializeRenderTextures(int size, int cascadesNumber, int anisoLevel)
        {
            if (gaussianNoise != null) gaussianNoise.Release();
            if (initialSpectrum != null) initialSpectrum.Release();
            if (wavesData != null) wavesData.Release();
            if (initializationBuffer != null) initializationBuffer.Release();
            if (fftBuffer != null) fftBuffer.Release();
            if (fftInOut != null) fftInOut.Release();
            if (turbulence != null) turbulence.Release();

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

            initialSpectrum = RenderingUtils.CreateRenderTexture(initialsDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            wavesData = RenderingUtils.CreateRenderTexture(initialsDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            initializationBuffer = RenderingUtils.CreateRenderTexture(initialsDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            gaussianNoise = RenderingUtils.CreateRenderTexture(noiseTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Point, 0);
            turbulence = RenderingUtils.CreateRenderTexture(turbulenceTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Trilinear, anisoLevel);
            fftBuffer = RenderingUtils.CreateRenderTexture(displacementAndDerivativesTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Trilinear, anisoLevel);
            fftInOut = RenderingUtils.CreateRenderTexture(displacementAndDerivativesTextureDescriptor, TextureWrapMode.Repeat, FilterMode.Trilinear, anisoLevel);

            Shader.SetGlobalTexture(GlobalShaderVariables.DisplacementAndDerivatives, fftInOut);
            Shader.SetGlobalTexture(GlobalShaderVariables.Turbulence, turbulence);

            initialSpectrumShader.SetInt(SimualtionVariables.Size, size);
            initialSpectrumShader.SetInt(SimualtionVariables.CascadesCount, cascadesNumber);
            initialSpectrumShader.SetTexture(GenerateNoiseKernel, SimualtionVariables.Noise, gaussianNoise);
            initialSpectrumShader.Dispatch(GenerateNoiseKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            foamSimulationShader.SetTexture(InitializeFoamKernel, SimualtionVariables.Turbulence, turbulence);
            foamSimulationShader.Dispatch(InitializeFoamKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
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
            simulationSettings.CalculateCascadeDomains(out cutoffsLow, out cutoffsHigh);
            equalizerPreset.BakeRamp();

            cmd.SetComputeIntParam(initialSpectrumShader, SimualtionVariables.Size, size);
            cmd.SetComputeIntParam(initialSpectrumShader, SimualtionVariables.CascadesCount, simulationSettings.CascadesNumber);

            cmd.SetComputeVectorParam(initialSpectrumShader, SimualtionVariables.LengthScales, simulationSettings.LengthScales());
            cmd.SetComputeVectorParam(initialSpectrumShader, SimualtionVariables.CutoffsHigh, cutoffsHigh);
            cmd.SetComputeVectorParam(initialSpectrumShader, SimualtionVariables.CutoffsLow, cutoffsLow);
            cmd.SetComputeFloatParam(initialSpectrumShader, SimualtionVariables.Depth, wavesSettings.depth);
            cmd.SetComputeFloatParam(initialSpectrumShader, SimualtionVariables.Chop, wavesSettings.chop);
            cmd.SetComputeVectorParam(initialSpectrumShader,
                SimualtionVariables.RampsXLimits, new Vector4(OceanEqualizerPreset.XMin, OceanEqualizerPreset.XMax));

            spectrums[0] = wavesSettings.local;
            spectrums[1] = wavesSettings.swell;
            spectrumsBuffer.SetData(spectrums);
            cmd.SetComputeBufferParam(initialSpectrumShader, InitialSpectrumKernel,
                SimualtionVariables.Spectrums, spectrumsBuffer);

            cmd.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, SimualtionVariables.H0K, initializationBuffer);
            cmd.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, SimualtionVariables.WavesData, wavesData);
            cmd.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, SimualtionVariables.Noise, gaussianNoise);
            cmd.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, SimualtionVariables.EqualizerRamp, equalizerPreset.Ramp);
            // Calculating initial spectrum
            cmd.DispatchCompute(initialSpectrumShader,
                InitialSpectrumKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            cmd.SetComputeTextureParam(initialSpectrumShader,
                ConjugateSpectrumKernel, SimualtionVariables.H0, initialSpectrum);
            cmd.SetComputeTextureParam(initialSpectrumShader,
                ConjugateSpectrumKernel, SimualtionVariables.H0K, initializationBuffer);
            // Calculating complex conjugate of the initial spectrum
            cmd.DispatchCompute(initialSpectrumShader,
                ConjugateSpectrumKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            spectrumCalculated = true;
        }

        private void UpdateSimulation(CommandBuffer cmd, float time, float deltaTime)
        {
            // Calculating complex amplitudes
            cmd.SetComputeIntParam(timeDependentSpectrumShader,
                SimualtionVariables.CascadesCount, simulationSettings.CascadesNumber);
            cmd.SetComputeFloatParam(timeDependentSpectrumShader, SimualtionVariables.Time, time);
            cmd.SetComputeTextureParam(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, SimualtionVariables.Result, fftInOut);
            cmd.SetComputeTextureParam(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, SimualtionVariables.H0, initialSpectrum);
            cmd.SetComputeTextureParam(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, SimualtionVariables.WavesData, wavesData);
            cmd.DispatchCompute(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            // Calculating IFFTs of complex amplitudes
            FFTCompute.IFFT2D(cmd, fftInOut, fftBuffer, true, false, true);
            cmd.GenerateMips(fftInOut);

            // Simulating foam
            if (simulationSettings.simulateFoam)
            {
                cmd.SetComputeIntParam(foamSimulationShader,
                    SimualtionVariables.CascadesCount, simulationSettings.CascadesNumber);
                cmd.SetComputeFloatParam(foamSimulationShader, SimualtionVariables.DeltaTime, deltaTime);
                cmd.SetComputeFloatParam(foamSimulationShader, SimualtionVariables.FoamDecayRate, wavesSettings.foam.decayRate);

                cmd.SetComputeTextureParam(foamSimulationShader,
                    SimulateFoamKernel, SimualtionVariables.Input, fftInOut);
                cmd.SetComputeTextureParam(foamSimulationShader,
                    SimulateFoamKernel, SimualtionVariables.Turbulence, turbulence);

                cmd.DispatchCompute(foamSimulationShader,
                    SimulateFoamKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
                cmd.GenerateMips(turbulence); 
            }
        }

        private void SetGlobalShaderVariables()
        {
            // waves
            Shader.SetGlobalVector(GlobalShaderVariables.LengthScales, simulationSettings.LengthScales());
            float windAngle = wavesSettings.local.windDirection * Mathf.Deg2Rad;
            Shader.SetGlobalVector(GlobalShaderVariables.WindDirection, new Vector2(Mathf.Cos(windAngle), Mathf.Sin(windAngle)));
            Shader.SetGlobalMatrix(GlobalShaderVariables.WorldToWindSpace,
                Matrix4x4.Rotate(Quaternion.AngleAxis(-wavesSettings.local.windDirection, Vector3.up)));
            Shader.SetGlobalFloat(GlobalShaderVariables.WindSpeed, wavesSettings.local.windSpeed);
            Shader.SetGlobalFloat(GlobalShaderVariables.WavesScale, wavesSettings.local.scale);
            Shader.SetGlobalFloat(GlobalShaderVariables.WavesAlignement, wavesSettings.local.alignment);

            // floam
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamCoverage, wavesSettings.foam.coverage);
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamUnderwater, wavesSettings.foam.underwater);
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamDensity, wavesSettings.foam.density);
            Shader.SetGlobalFloat(GlobalShaderVariables.FoamPersistence, wavesSettings.foam.persistence);
            Shader.SetGlobalVector(GlobalShaderVariables.FoamCascadesWeights, wavesSettings.foam.cascadesWeights);
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

        

        // Kernel IDs
        private int GenerateNoiseKernel;
        private int InitialSpectrumKernel;
        private int ConjugateSpectrumKernel;
        private int CalculateAmplitudesKernel;
        private int SimulateFoamKernel;
        private int InitializeFoamKernel;
    }
}
