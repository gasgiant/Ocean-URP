using UnityEngine;

namespace OceanSystem
{
    public class WindWavesSimulationInputs
    {
        public float timeScale = 1;
        public float depth = 1000;
        public float localWindDirection;
        public float swellDirection;
        public float chop = 1;
        public WindWavesFoamSettings foam = WindWavesFoamSettings.GetDefault();
        public SpectrumSettings local = SpectrumSettings.GetDefaultLocal();
        public SpectrumSettings swell = SpectrumSettings.GetDefaultSwell();
        public Texture2D equalizerRamp = OceanEqualizerPreset.GetDefaultRamp();
    }
}


