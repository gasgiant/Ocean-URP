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
            SpectrumSettings s = new SpectrumSettings();
            s.energySpectrum = EnergySpectrum.PM;
            s.windSpeed = 5;
            s.fetch = 100;
            s.peaking = 3.3f;
            s.scale = 1;
            s.cutoffWavelength = 0.01f;
            s.windDirection = 0;
            s.alignment = 1;
            s.extraAlignment = 0;
            return s;
        }

        public static SpectrumSettings GetDefaultSwell()
        {
            SpectrumSettings s = new SpectrumSettings();
            s.energySpectrum = EnergySpectrum.JONSWAP;
            s.windSpeed = 10;
            s.fetch = 100;
            s.peaking = 10;
            s.scale = 0;
            s.cutoffWavelength = 1;
            s.windDirection = 0;
            s.alignment = 1;
            s.extraAlignment = 0.8f;
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
        public SpectrumSettings local;
        public SpectrumSettings swell;

#if UNITY_EDITOR
        public bool spectrumPlot;
#endif
    }
}
