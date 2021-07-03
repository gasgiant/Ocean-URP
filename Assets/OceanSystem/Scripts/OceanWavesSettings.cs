using UnityEngine;

namespace OceanSystem
{

    [CreateAssetMenu(fileName = "New Waves Settings", menuName = "Ocean/Waves Settings")]
    public class OceanWavesSettings : ScriptableObject
    {
        [Range(0, 1)]
        public float timeScale = 1;
        public float depth = 1000;
        [Range(0, 4)]
        public float chop = 1;
        public FoamParams foam;
        public SpectrumParams local;
        public SpectrumParams swell;

#if UNITY_EDITOR
        public bool spectrumPlot;
#endif
    }
}
