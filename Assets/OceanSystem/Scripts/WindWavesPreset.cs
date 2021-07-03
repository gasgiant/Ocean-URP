using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Waves Preset", menuName = "Ocean/Wind Waves Preset")]
    public class WindWavesPreset : ScriptableObject
    {
        public enum Mode { Local, Swell }

        [SerializeField] private Mode type;
        [SerializeField] private SpectrumSettings _spectrum;
        [SerializeField] private WindWavesFoamSettings _foam = WindWavesFoamSettings.GetDefault();
        [SerializeField] private float _chop;
        [SerializeField] private OceanEqualizerPreset _equalizer;

        public SpectrumSettings Spectrum => _spectrum;
        public OceanEqualizerPreset Equalizer => _equalizer;
        public float Chop => _chop;
        public WindWavesFoamSettings Foam => _foam;

    }
}


