using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Simulation Settings", menuName = "Ocean/Simulation settings")]
    public class OceanSimulationSettings : ScriptableObject
    {
        public int Resolution => (int)_resolution;
        public int CascadesNumber => (int)_cascadesNumber;
        public int AnisoLevel => _anisoLevel;
        public bool UpdateSpectrum => _updateSpectrum;
        public bool SimulateFoam => _simulateFoam;
        public int ReadbackCascades => (int)_readbackCascades;
        public int SamplingIterations => _samplingIterations;


        [SerializeField] private ResolutionValue _resolution = ResolutionValue.Seven;
        [SerializeField] private CascadesNumberValue _cascadesNumber = CascadesNumberValue.Four;
        [Range(0, 9)]
        [SerializeField] private int _anisoLevel = 6;
        [SerializeField] private bool _simulateFoam;
        [SerializeField] private bool _updateSpectrum = false;

        [SerializeField] private CascadeDomainsMode _domainsMode;
        [SerializeField] private float _simulationScale = 400;
        [SerializeField] private bool _allowOverlap = false;
        [Range(1, 10)]
        [SerializeField] private float _minWavesInCascade = 6;
        [SerializeField] private float _c0Scale;
        [SerializeField] private float _c1Scale;
        [SerializeField] private float _c2Scale;
        [SerializeField] private float _c3Scale;

        [SerializeField] private ReadbackCascadesMode _readbackCascades;
        [Range(1, 5)]
        [SerializeField] private int _samplingIterations = 3;

        const int SmallestWaveMultiplierAuto = 4;
        const int MinWavesInCascadeAuto = 6;

#if UNITY_EDITOR
        [SerializeField] private OceanWavesSettings _displayWavesSettings;
#endif

        public Vector4 LengthScales()
        {
            Vector4 lengthScales = Vector4.zero;
            if (_domainsMode == CascadeDomainsMode.Auto)
            {
                lengthScales[0] = _simulationScale;
                for (int i = 1; i < 4; i++)
                {
                    lengthScales[i] = lengthScales[i - 1] * SmallestWaveMultiplierAuto * MinWavesInCascadeAuto / Resolution;
                }
            }
            else
            {
                lengthScales = new Vector4(_c0Scale, _c1Scale, _c2Scale, _c3Scale);
            }
            return lengthScales;
        }

        public void CalculateCascadeDomains(out Vector4 cutoffsLow, out Vector4 cutoffsHigh)
        {
            if (_domainsMode == CascadeDomainsMode.Auto)
            {
                CalculateCascadeDomainsManual(LengthScales(), false, MinWavesInCascadeAuto, out cutoffsLow, out cutoffsHigh);
            }
            else
            {
                CalculateCascadeDomainsManual(LengthScales(), _allowOverlap, _minWavesInCascade, out cutoffsLow, out cutoffsHigh);
            }
        }

        private void CalculateCascadeDomainsManual(Vector4 lengthScales, bool allowOverlap, float minWavesInCascade,
            out Vector4 cutoffsLow, out Vector4 cutoffsHigh)
        {
            Vector4 lows = new Vector4();
            for (int i = 0; i < 4; i++)
            {
                lows[i] = 2 * Mathf.PI / lengthScales[i] * minWavesInCascade;
            }

            Vector4 highs = new Vector4();
            for (int i = 0; i < 4; i++)
            {
                highs[i] = 2 * Mathf.PI * Resolution / lengthScales[i] / SmallestWaveMultiplierAuto;
            }

            cutoffsHigh = highs;
            cutoffsLow = allowOverlap ? lows : Vector4.Max(lows, new Vector4(0, highs[0], highs[1], highs[2]));

            if (CascadesNumber < 4)
            {
                cutoffsLow.w = 0;
                cutoffsHigh.w = 0;
            }

            if (CascadesNumber < 3)
            {
                cutoffsLow.z = 0;
                cutoffsHigh.z = 0;
            }
        }

        public enum ResolutionValue
        {
            [InspectorName("64")]
            Six = 64,
            [InspectorName("128")]
            Seven = 128,
            [InspectorName("256")]
            Eight = 256,
            [InspectorName("512")]
            Nine = 512
        }

        public enum CascadesNumberValue
        {
            [InspectorName("Two Cascades")]
            Two = 2,
            [InspectorName("Three Cascades")]
            Three = 3,
            [InspectorName("Four Cascades")]
            Four = 4,
        }

        public enum ReadbackCascadesMode
        {
            [InspectorName("None")]
            None = 0,
            [InspectorName("Largest")]
            One = 1,
            [InspectorName("Two Largest")]
            Two = 2
        }

        public enum CascadeDomainsMode { Auto, Manual }
    }
}
