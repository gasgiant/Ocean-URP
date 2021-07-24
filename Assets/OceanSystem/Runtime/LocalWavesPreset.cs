using EditorExtras;
using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Local Waves", menuName = "Ocean/Local Waves")]
    public class LocalWavesPreset : ScriptableObject
    {
        [TabScope("Tabs", "Waves|Foam")]
        [Tab("Tabs/Waves")]
        [Box("./Spectrum")]
        [ExtendedProperty(true)]
        [SerializeField] private SpectrumParams _spectrum = SpectrumParams.GetDefaultLocal();
        [Box("../Others")]
        [SerializeField] private float _referenceWaveHeight = 1;
        [SerializeField, Range(0, 2)] private float _chop = 1;
        [SerializeField] private EqualizerPreset _equalizer;
        [Tab("Tabs/Foam")]
        [ExtendedProperty(true)]
        [SerializeField] private FoamParams _foam = FoamParams.GetDefault();

        [SerializeField, HideInInspector] private float _windForce;

        public float WindForce => _windForce;
        public float ReferenceWaveHeight => _referenceWaveHeight;
        public SpectrumParams Spectrum => _spectrum;
        public FoamParams Foam => _foam;
        public float Chop => _chop;
        public EqualizerPreset Equalizer => _equalizer;
    }
}
