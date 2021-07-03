using UnityEngine;

namespace OceanSystem
{
    [System.Serializable]
    public struct SpectrumParams
    {
        public enum EnergySpectrumModel
        {
            [InspectorName("Pierson-Moskowitz")]
            PM,
            JONSWAP,
            TMA
        };

        public EnergySpectrumModel energySpectrum;
        [Range(0.1f, 30)]
        public float windSpeed;
        public float fetch;
        [Range(1, 10)]
        public float peaking;
        [Range(0, 1)]
        public float scale;
        public float cutoffWavelength;

        [Range(0, 1)]
        public float alignment;
        [Range(0, 1)]
        public float extraAlignment;

        public static SpectrumParams Lerp(SpectrumParams lhs, SpectrumParams rhs, float t)
        {
            return new SpectrumParams()
            {
                energySpectrum = t < 0.5 ? lhs.energySpectrum : rhs.energySpectrum,
                windSpeed = Mathf.Lerp(lhs.windSpeed, rhs.windSpeed, t),
                fetch = Mathf.Lerp(lhs.fetch, rhs.fetch, t),
                peaking = Mathf.Lerp(lhs.peaking, rhs.peaking, t),
                scale = Mathf.Lerp(lhs.scale, rhs.scale, t),
                cutoffWavelength = Mathf.Lerp(lhs.cutoffWavelength, rhs.cutoffWavelength, t),
                alignment = Mathf.Lerp(lhs.alignment, rhs.alignment, t),
                extraAlignment = Mathf.Lerp(lhs.extraAlignment, rhs.extraAlignment, t)
            };
        }

        public static SpectrumParams GetDefaultLocal()
        {
            SpectrumParams s = new SpectrumParams
            {
                energySpectrum = EnergySpectrumModel.PM,
                windSpeed = 5,
                fetch = 100,
                peaking = 3.3f,
                scale = 1,
                cutoffWavelength = 0.01f,
                alignment = 1,
                extraAlignment = 0
            };
            return s;
        }

        public static SpectrumParams GetDefaultSwell()
        {
            SpectrumParams s = new SpectrumParams
            {
                energySpectrum = EnergySpectrumModel.JONSWAP,
                windSpeed = 10,
                fetch = 100,
                peaking = 10,
                scale = 0,
                cutoffWavelength = 1,
                alignment = 1,
                extraAlignment = 0.8f
            };
            return s;
        }
    }
}
