using UnityEngine;

namespace OceanSystem
{
    public class OceanSimulationInputs
    {
        public float timeScale = 1;
        public float depth = 1000;
        public float chop = 1;
        public FoamParams foam = FoamParams.GetDefault();
        public SpectrumParams local = SpectrumParams.GetDefaultLocal();
        public SpectrumParams swell = SpectrumParams.GetDefaultSwell();
        public Texture2D equalizerRamp0;
        public Texture2D equalizerRamp1;
        public float equalizerLerpValue;
    }
}


