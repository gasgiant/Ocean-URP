using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Simulation Settings", menuName = "Ocean/Simulation settings")]
    public class OceanSimulationSettings : ScriptableObject
    {
        public int Resolution => (int)resolution;
        public int CascadesNumber => (int)cascadesNumber;
        public int ReadbackCascades => (int)readbackCascades;

        public ResolutionValue resolution = ResolutionValue.Seven;
        public CascadesNumberValue cascadesNumber = CascadesNumberValue.Four;
        [Range(0, 9)]
        public int anisoLevel = 6;
        public bool simulateFoam;
        public bool updateSpectrum = false;

        public CascadeDomainsMode domainsMode;
        public float simulationScale = 400;
        public bool allowOverlap = false;
        [Range(1, 10)]
        public float minWavesInCascade = 6;
        public float c0Scale;
        public float c1Scale;
        public float c2Scale;
        public float c3Scale;

        public ReadbackCascadesValue readbackCascades;
        [Range(1, 5)]
        public int samplingIterations = 3;

        const int smallestWaveMultAuto = 4;
        const int minWavesInCascadeAuto = 6;

#if UNITY_EDITOR
        public OceanWavesSettings DisplayWavesSettings;
        public bool spectrumPlot;
#endif

        public Vector4 LengthScales()
        {
            Vector4 lengthScales = Vector4.zero;
            if (domainsMode == CascadeDomainsMode.Auto)
            {
                lengthScales[0] = simulationScale;
                for (int i = 1; i < 4; i++)
                {
                    lengthScales[i] = lengthScales[i - 1] * smallestWaveMultAuto * minWavesInCascadeAuto / Resolution;
                }
            }
            else
            {
                lengthScales = new Vector4(c0Scale, c1Scale, c2Scale, c3Scale);
            }
            return lengthScales;
        }

        public void CalculateCascadeDomains(out Vector4 cutoffsLow, out Vector4 cutoffsHigh)
        {
            if (domainsMode == CascadeDomainsMode.Auto)
            {
                CalculateCascadeDomainsManual(LengthScales(), false, minWavesInCascadeAuto, out cutoffsLow, out cutoffsHigh);
            }
            else
            {
                CalculateCascadeDomainsManual(LengthScales(), allowOverlap, minWavesInCascade, out cutoffsLow, out cutoffsHigh);
            }
        }

        void CalculateCascadeDomainsManual(Vector4 lengthScales, bool allowOverlap, float minWavesInCascade,
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
                highs[i] = 2 * Mathf.PI * Resolution / lengthScales[i] / smallestWaveMultAuto;
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

        public enum ReadbackCascadesValue
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
