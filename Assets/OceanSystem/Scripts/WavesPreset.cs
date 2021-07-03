using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Waves Preset", menuName = "Ocean/Wind Waves Preset")]
    public class WavesPreset : ScriptableObject
    {
        public enum Mode { Local, Swell }

        [SerializeField] private Mode type;
        [SerializeField] private SpectrumParams _spectrum = SpectrumParams.GetDefaultLocal();
        [SerializeField] private FoamParams _foam = FoamParams.GetDefault();
        [SerializeField] private float _chop = 1;
        [SerializeField] private EqualizerPreset _equalizer;

        public SpectrumParams Spectrum => _spectrum;
        public EqualizerPreset Equalizer => _equalizer;
        public float Chop => _chop;
        public FoamParams Foam => _foam;
    }
}


