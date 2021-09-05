using MarkupAttributes;
using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Swell", menuName = "Ocean/Swell")]
    public class SwellPreset : ScriptableObject
    {
        [TitleGroup("Spectrum")]
        [MarkedUpField(false, false)]
        [SerializeField] private SpectrumParams _spectrum = SpectrumParams.GetDefaultSwell();

        [TitleGroup("Others")]
        [SerializeField] private float _referenceWaveHeight;

        public float ReferenceWaveHeight => _referenceWaveHeight;
        public SpectrumParams Spectrum => _spectrum;
    }
}
