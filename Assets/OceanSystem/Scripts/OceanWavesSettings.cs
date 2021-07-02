using UnityEngine;

namespace OceanSystem
{
    public enum EnergySpectrum : uint
    {
        [InspectorName("Pierson-Moskowitz")]
        PM,
        JONSWAP,
        TMA
    };

    [System.Serializable]
    public struct SpectrumSettings
    {
        public EnergySpectrum energySpectrum;
        [Range(0.1f, 30)]
        public float windSpeed;
        public float fetch;
        [Range(1, 10)]
        public float peaking;
        [Range(0, 1)]
        public float scale;
        public float cutoffWavelength;

        [Range(0, 360)]
        public float windDirection;
        [Range(0, 1)]
        public float alignment;
        [Range(0, 1)]
        public float extraAlignment;

        public static SpectrumSettings GetDefaultLocal()
        {
            SpectrumSettings s = new SpectrumSettings
            {
                energySpectrum = EnergySpectrum.PM,
                windSpeed = 5,
                fetch = 100,
                peaking = 3.3f,
                scale = 1,
                cutoffWavelength = 0.01f,
                windDirection = 0,
                alignment = 1,
                extraAlignment = 0
            };
            return s;
        }

        public static SpectrumSettings GetDefaultSwell()
        {
            SpectrumSettings s = new SpectrumSettings
            {
                energySpectrum = EnergySpectrum.JONSWAP,
                windSpeed = 10,
                fetch = 100,
                peaking = 10,
                scale = 0,
                cutoffWavelength = 1,
                windDirection = 0,
                alignment = 1,
                extraAlignment = 0.8f
            };
            return s;
        }
    }

    [System.Serializable]
    public class FoamSettings
    {
        [Range(-0.1f, 1)]
        public float coverage;
        [Range(0, 1)]
        public float underwater;
        public float density = 8.4f;
        [Range(0, 1)]
        public float persistence = 0.5f;
        public float decayRate = 0.1f;
        public Vector4 cascadesWeights = Vector4.one;
    }

    [CreateAssetMenu(fileName = "New Waves Settings", menuName = "Ocean/Waves Settings")]
    public class OceanWavesSettings : ScriptableObject
    {
        [Range(0, 1)]
        public float timeScale = 1;
        public float depth = 1000;
        [Range(0, 4)]
        public float chop = 1;
        public FoamSettings foam;
        public SpectrumSettings local = SpectrumSettings.GetDefaultLocal();
        public SpectrumSettings swell = SpectrumSettings.GetDefaultSwell();

#if UNITY_EDITOR
        public bool spectrumPlot;
#endif
    }
}
