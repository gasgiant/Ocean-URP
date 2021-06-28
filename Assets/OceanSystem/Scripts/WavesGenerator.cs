using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{

    public class WavesGenerator : MonoBehaviour
    {
        private const string InitialSpectrumShaderPath = "ComputeShaders/InitialSpectrum";
        private const string TimeDependentSpectrumShaderPath = "ComputeShaders/TimeDependentSpectrum";
        private const string FoamSimulationShaderPath = "ComputeShaders/FoamSimulation";
        private const int LocalWorkGroupsX = 8;
        private const int LocalWorkGroupsY = 8;

        public OceanCollision Collision;

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
        private CommandBuffer staticSpectrumCbuff;
        private CommandBuffer dynamicSpectrumCbuff;
        private CommandBuffer foamCbuff;

        private RenderTexture gaussianNoise;
        private RenderTexture initialSpectrum;
        private RenderTexture wavesData;
        private RenderTexture initializationBuffer;
        private RenderTexture fftBuffer;
        private RenderTexture fftInOut;
        private RenderTexture turbulence;
        private SpectrumSettings[] spectrums = new SpectrumSettings[2];
        private ComputeBuffer spectrumsBuffer;

        private static float OceanTime => (float)(Time.timeSinceLevelLoadAsDouble % 18000);

        private void Awake()
        {
            initialSpectrumShader = (ComputeShader)Resources.Load(InitialSpectrumShaderPath);
            timeDependentSpectrumShader = (ComputeShader)Resources.Load(TimeDependentSpectrumShaderPath);
            foamSimulationShader = (ComputeShader)Resources.Load(FoamSimulationShaderPath);
            staticSpectrumCbuff = new CommandBuffer();
            staticSpectrumCbuff.name = "Ocean Static Spectrum";
            dynamicSpectrumCbuff = new CommandBuffer();
            dynamicSpectrumCbuff.name = "Ocean Dynamic Spectrum";
            foamCbuff = new CommandBuffer();
            foamCbuff.name = "Ocean Foam Simulation";

            GenerateNoiseKernel = initialSpectrumShader.FindKernel("GenerateGaussianNoise");
            InitialSpectrumKernel = initialSpectrumShader.FindKernel("CalculateInitialSpectrum");
            ConjugateSpectrumKernel = initialSpectrumShader.FindKernel("CalculateConjugatedSpectrum");
            CalculateAmplitudesKernel = timeDependentSpectrumShader.FindKernel("CalculateAmplitudes");
            SimulateFoamKernel = foamSimulationShader.FindKernel("Simulate");
            InitializeFoamKernel = foamSimulationShader.FindKernel("Initialize");
            spectrumsBuffer = new ComputeBuffer(2, 8 * sizeof(float) + 1 * sizeof(uint));

            InitializeSimulation();
        }

        private void Update()
        {
            if (simulationSettings.updateSpectrum)
            {
                CalculateSpectrum();
            }
            UpdateSimulation(OceanTime * wavesSettings.timeScale, Time.deltaTime * wavesSettings.timeScale);
            Collision.DoReadbacks();
        }

        private void OnDestroy()
        {
            spectrumsBuffer?.Release();
        }

        private void InitializeSimulation()
        {
            size = simulationSettings.Resolution;
            InitializeRenderTextures(size, simulationSettings.CascadesNumber, simulationSettings.anisoLevel);
            SetCascadesKeywords(simulationSettings.CascadesNumber);
            Collision = new OceanCollision(size, fftInOut, simulationSettings);
            CalculateSpectrum();
        }

        private void InitializeRenderTextures(int size, int cascadesNumber, int anisoLevel)
        {
            gaussianNoise?.Release();
            initialSpectrum?.Release();
            wavesData?.Release();
            initializationBuffer?.Release();
            fftBuffer?.Release();
            fftInOut?.Release();
            turbulence?.Release();

            var initialsTextureParams = new RenderTextureParams()
            {
                size = size,
                volumeDepth = cascadesNumber,
                enableRandomWrite = true,
                format = RenderTextureFormat.ARGBHalf,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Trilinear,
                useMips = false,
                anisoLevel = anisoLevel
            };

            var noiseTextureParams = initialsTextureParams;
            noiseTextureParams.format = RenderTextureFormat.RGHalf;

            var displacementAndDerivativesTextureParams = initialsTextureParams;
            displacementAndDerivativesTextureParams.useMips = true;
            displacementAndDerivativesTextureParams.volumeDepth = 2 * cascadesNumber;

            var turbulenceTextureParams = initialsTextureParams;
            turbulenceTextureParams.useMips = true;

            initialSpectrum = CreateRenderTexture(initialsTextureParams);
            wavesData = CreateRenderTexture(initialsTextureParams);
            initializationBuffer = CreateRenderTexture(initialsTextureParams);
            gaussianNoise = CreateRenderTexture(noiseTextureParams);
            turbulence = CreateRenderTexture(turbulenceTextureParams);
            fftBuffer = CreateRenderTexture(displacementAndDerivativesTextureParams);
            fftInOut = CreateRenderTexture(displacementAndDerivativesTextureParams);

            Shader.SetGlobalTexture(DisplacementAndDerivativesGlobalID, fftInOut);
            Shader.SetGlobalTexture(TurbulenceGlobalID, turbulence);

            initialSpectrumShader.SetInt(SizeID, size);
            initialSpectrumShader.SetInt(CascadesCountID, cascadesNumber);
            initialSpectrumShader.SetTexture(GenerateNoiseKernel, NoiseID, gaussianNoise);
            initialSpectrumShader.Dispatch(GenerateNoiseKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            foamSimulationShader.SetTexture(InitializeFoamKernel, TurbulenceID, turbulence);
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

        private void CalculateSpectrum()
        {
            Vector4 cutoffsLow, cutoffsHigh;
            simulationSettings.CalculateCascadeDomains(out cutoffsLow, out cutoffsHigh);
            equalizerPreset.BakeRamp();

            staticSpectrumCbuff.Clear();
            staticSpectrumCbuff.SetComputeIntParam(initialSpectrumShader, SizeID, size);
            staticSpectrumCbuff.SetComputeIntParam(initialSpectrumShader, CascadesCountID, simulationSettings.CascadesNumber);

            staticSpectrumCbuff.SetComputeVectorParam(initialSpectrumShader, LengthScalesID, simulationSettings.LengthScales());
            staticSpectrumCbuff.SetComputeVectorParam(initialSpectrumShader, CutoffsHighID, cutoffsHigh);
            staticSpectrumCbuff.SetComputeVectorParam(initialSpectrumShader, CutoffsLowID, cutoffsLow);
            staticSpectrumCbuff.SetComputeFloatParam(initialSpectrumShader, DepthID, wavesSettings.depth);
            staticSpectrumCbuff.SetComputeFloatParam(initialSpectrumShader, ChopID, wavesSettings.chop);
            staticSpectrumCbuff.SetComputeVectorParam(initialSpectrumShader,
                RampsXLimitsID, new Vector4(OceanEqualizerPreset.XMin, OceanEqualizerPreset.XMax));

            spectrums[0] = wavesSettings.local;
            spectrums[1] = wavesSettings.swell;
            spectrumsBuffer.SetData(spectrums);
            staticSpectrumCbuff.SetComputeBufferParam(initialSpectrumShader, InitialSpectrumKernel,
                SpectrumsID, spectrumsBuffer);

            staticSpectrumCbuff.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, H0KID, initializationBuffer);
            staticSpectrumCbuff.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, WavesDataID, wavesData);
            staticSpectrumCbuff.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, NoiseID, gaussianNoise);
            staticSpectrumCbuff.SetComputeTextureParam(initialSpectrumShader,
                InitialSpectrumKernel, EqualizerRampID, equalizerPreset.Ramp);
            // Calculating initial spectrum
            staticSpectrumCbuff.DispatchCompute(initialSpectrumShader,
                InitialSpectrumKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            staticSpectrumCbuff.SetComputeTextureParam(initialSpectrumShader,
                ConjugateSpectrumKernel, H0ID, initialSpectrum);
            staticSpectrumCbuff.SetComputeTextureParam(initialSpectrumShader,
                ConjugateSpectrumKernel, H0KID, initializationBuffer);
            // Calculating complex conjugate of the initial spectrum
            staticSpectrumCbuff.DispatchCompute(initialSpectrumShader,
                ConjugateSpectrumKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);

            Graphics.ExecuteCommandBuffer(staticSpectrumCbuff);

            // Setting global variables for ocean shader
            Shader.SetGlobalVector(LengthScalesGlobalID, simulationSettings.LengthScales());
            float windAngle = wavesSettings.local.windDirection * Mathf.Deg2Rad;
            Shader.SetGlobalVector(WindDirectionID, new Vector2(Mathf.Cos(windAngle), Mathf.Sin(windAngle)));
            Shader.SetGlobalMatrix(WorldToWindSpaceID,
                Matrix4x4.Rotate(Quaternion.AngleAxis(-wavesSettings.local.windDirection, Vector3.up)));
            Shader.SetGlobalFloat(WindSpeedID, wavesSettings.local.windSpeed);
            Shader.SetGlobalFloat(WavesScaleID, wavesSettings.local.scale);
            Shader.SetGlobalFloat(WavesAlignementID, wavesSettings.local.alignment);
        }

        private void UpdateSimulation(float time, float deltaTime)
        {
            dynamicSpectrumCbuff.Clear();
            // Calculating complex amplitudes
            dynamicSpectrumCbuff.SetComputeIntParam(timeDependentSpectrumShader,
                CascadesCountID, simulationSettings.CascadesNumber);
            dynamicSpectrumCbuff.SetComputeFloatParam(timeDependentSpectrumShader, TimeID, time);
            dynamicSpectrumCbuff.SetComputeTextureParam(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, ResultID, fftInOut);
            dynamicSpectrumCbuff.SetComputeTextureParam(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, H0ID, initialSpectrum);
            dynamicSpectrumCbuff.SetComputeTextureParam(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, WavesDataID, wavesData);
            dynamicSpectrumCbuff.DispatchCompute(timeDependentSpectrumShader,
                CalculateAmplitudesKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
            Graphics.ExecuteCommandBuffer(dynamicSpectrumCbuff);

            // Calculating IFFTs of complex amplitudes
            FFTCompute.IFFT2D(fftInOut, fftBuffer, true, false, true);
            fftInOut.GenerateMips();

            // Simulating foam
            if (simulationSettings.simulateFoam)
            {
                foamCbuff.Clear();
                foamCbuff.SetComputeIntParam(foamSimulationShader,
                    CascadesCountID, simulationSettings.CascadesNumber);
                foamCbuff.SetComputeFloatParam(foamSimulationShader, DeltaTimeID, deltaTime);
                foamCbuff.SetComputeFloatParam(foamSimulationShader, FoamDecayRateID, wavesSettings.foam.decayRate);

                foamCbuff.SetComputeTextureParam(foamSimulationShader,
                    SimulateFoamKernel, InputID, fftInOut);
                foamCbuff.SetComputeTextureParam(foamSimulationShader,
                    SimulateFoamKernel, TurbulenceID, turbulence);

                foamCbuff.DispatchCompute(foamSimulationShader,
                    SimulateFoamKernel, size / LocalWorkGroupsX, size / LocalWorkGroupsY, 1);
                foamCbuff.GenerateMips(turbulence);
                Graphics.ExecuteCommandBuffer(foamCbuff);

                // Setting global foam variables for ocean shader
                Shader.SetGlobalFloat(FoamCoverageID, wavesSettings.foam.coverage);
                Shader.SetGlobalFloat(FoamUnderwaterID, wavesSettings.foam.underwater);
                Shader.SetGlobalFloat(FoamDensityID, wavesSettings.foam.density);
                Shader.SetGlobalFloat(FoamPersistenceID, wavesSettings.foam.persistence);
                Shader.SetGlobalVector(FoamCascadesWeightsID, wavesSettings.foam.cascadesWeights);
            }
        }

        public struct RenderTextureParams
        {
            public int size;
            public int volumeDepth;
            public bool enableRandomWrite;
            public RenderTextureFormat format;
            public TextureWrapMode wrapMode;
            public FilterMode filterMode;
            public bool useMips;
            public int anisoLevel;
        }

        public static RenderTexture CreateRenderTexture(RenderTextureParams pars)
        {
            return CreateRenderTexture(
                pars.size,
                pars.volumeDepth,
                pars.enableRandomWrite,
                pars.format,
                pars.wrapMode,
                pars.filterMode,
                pars.useMips,
                pars.anisoLevel);
        }

        public static RenderTexture CreateRenderTexture(
            int size,
            int volumeDepth,
            bool enableRandomWrite,
            RenderTextureFormat format,
            TextureWrapMode wrapMode,
            FilterMode filterMode,
            bool useMips,
            int anisoLevel)
        {
            RenderTexture rt = new RenderTexture(size, size, 0,
                format, RenderTextureReadWrite.Linear);

            if (volumeDepth > 1)
            {
                rt.dimension = TextureDimension.Tex2DArray;
                rt.volumeDepth = volumeDepth;
            }

            rt.useMipMap = useMips;
            rt.autoGenerateMips = false;
            rt.anisoLevel = anisoLevel;
            rt.wrapMode = wrapMode;
            rt.filterMode = filterMode;
            rt.enableRandomWrite = enableRandomWrite;
            rt.Create();
            return rt;
        }

        // Shader props IDs
        private static readonly int SizeID = Shader.PropertyToID("Size");
        private static readonly int CascadesCountID = Shader.PropertyToID("CascadesCount");
        private static readonly int LengthScalesID = Shader.PropertyToID("LengthScales");
        private static readonly int CutoffsLowID = Shader.PropertyToID("CutoffsLow");
        private static readonly int CutoffsHighID = Shader.PropertyToID("CutoffsHigh");
        private static readonly int DepthID = Shader.PropertyToID("Depth");
        private static readonly int ChopID = Shader.PropertyToID("Chop");
        private static readonly int RampsXLimitsID = Shader.PropertyToID("RampsXLimits");
        private static readonly int SpectrumsID = Shader.PropertyToID("Spectrums");
        private static readonly int TimeID = Shader.PropertyToID("Time");
        private static readonly int DeltaTimeID = Shader.PropertyToID("DeltaTime");
        private static readonly int EqualizerRampID = Shader.PropertyToID("EqualizerRamp");
        private static readonly int H0KID = Shader.PropertyToID("H0K");
        private static readonly int WavesDataID = Shader.PropertyToID("WavesData");
        private static readonly int H0ID = Shader.PropertyToID("H0");
        private static readonly int ResultID = Shader.PropertyToID("Result");
        private static readonly int InputID = Shader.PropertyToID("Input");
        private static readonly int NoiseID = Shader.PropertyToID("Noise");
        private static readonly int TurbulenceID = Shader.PropertyToID("Turbulence");

        private static readonly int DisplacementAndDerivativesGlobalID = Shader.PropertyToID("Ocean_DisplacementAndDerivatives");
        private static readonly int TurbulenceGlobalID = Shader.PropertyToID("Ocean_Turbulence");
        private static readonly int LengthScalesGlobalID = Shader.PropertyToID("Ocean_LengthScales");

        private static readonly int WindSpeedID = Shader.PropertyToID("Ocean_WindSpeed");
        private static readonly int WavesScaleID = Shader.PropertyToID("Ocean_WavesScale");
        private static readonly int WavesAlignementID = Shader.PropertyToID("Ocean_WavesAlignement");
        private static readonly int WindDirectionID = Shader.PropertyToID("Ocean_WindDirection");
        private static readonly int WorldToWindSpaceID = Shader.PropertyToID("Ocean_WorldToWindSpace");

        private static readonly int FoamDecayRateID = Shader.PropertyToID("FoamDecayRate");
        private static readonly int FoamCoverageID = Shader.PropertyToID("Ocean_FoamCoverage");
        private static readonly int FoamUnderwaterID = Shader.PropertyToID("Ocean_FoamUnderwater");
        private static readonly int FoamDensityID = Shader.PropertyToID("Ocean_FoamDensity");
        private static readonly int FoamPersistenceID = Shader.PropertyToID("Ocean_FoamPersistence");
        private static readonly int FoamCascadesWeightsID = Shader.PropertyToID("Ocean_FoamCascadesWeights");

        // Kernel IDs
        private int GenerateNoiseKernel;
        private int InitialSpectrumKernel;
        private int ConjugateSpectrumKernel;
        private int CalculateAmplitudesKernel;
        private int SimulateFoamKernel;
        private int InitializeFoamKernel;
    }
}
