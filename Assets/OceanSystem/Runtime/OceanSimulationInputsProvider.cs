using MarkupAttributes;
using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Inputs Provider", menuName = "Ocean/Inputs Provider")]
    public class OceanSimulationInputsProvider : ScriptableObject
    {
        public enum InputsProviderMode { Fixed, Scale }

        [Box("Main")]
        [SerializeField] private InputsProviderMode _mode;
        [Range(0, 1)]
        [SerializeField] private float _timeScale = 1;
        [SerializeField] private float _depth = 1000;
        [EndGroup]

        [InlineEditor]
        [SerializeField] private SwellPreset _swell;
        [InlineEditor, ShowIf(nameof(_mode), InputsProviderMode.Fixed)]
        [SerializeField] private LocalWavesPreset _localWaves;

        [ShowIfGroup("ShowIfScaleMode", nameof(_mode), InputsProviderMode.Scale)]
        [Foldout("./Local Waves")]
        [SerializeField, Range(0, 1)] private float _displayWindForce01;
        [SerializeField] private EqualizerPreset _defaultEqualizer;

        [SerializeField, HideInInspector] private LocalWavesPreset[] _localWavesArray;
        [SerializeField, HideInInspector] private float _maxWindForce;

        private void OnValidate()
        {
            _maxWindForce = 0;
            if (_localWavesArray != null)
                for (int i = 0; i < _localWavesArray.Length; i++)
                {
                    if (_localWavesArray[i].WindForce > _maxWindForce)
                        _maxWindForce = _localWavesArray[i].WindForce;
                }
        }

        public void SetDisplayWindForce(float displayWindForce01)
        {
            _displayWindForce01 = displayWindForce01;
        }

        public void PopulateInputs(OceanSimulationInputs target)
        {
            PopulateInputs(target, _displayWindForce01);
        }

        public void PopulateInputs(OceanSimulationInputs target, float windForce01)
        {
            windForce01 = _displayWindForce01;

            target.timeScale = _timeScale;
            target.depth = _depth;

            float referenceWaveHeight = 0;
            if (_swell)
            {
                target.swell = _swell.Spectrum;
                referenceWaveHeight += _swell.ReferenceWaveHeight;
            }

            if (_mode == InputsProviderMode.Fixed || _localWavesArray == null || _localWavesArray.Length < 2)
            {
                target.foamTrailUpdateTime = 0;
                if (_localWaves != null)
                {
                    SetValues(target, _localWaves);
                    referenceWaveHeight += _localWaves.ReferenceWaveHeight;
                }
            }
            else
            {
                target.foamTrailUpdateTime = 1;
                LerpVars lerp = GetLerpVars(windForce01, _maxWindForce, _localWavesArray);

                if (lerp.start != null && lerp.end != null)
                {
                    SetValues(target, lerp.start, lerp.end, lerp.t);
                    referenceWaveHeight += Mathf.Lerp(lerp.start.ReferenceWaveHeight,
                        lerp.end.ReferenceWaveHeight, lerp.t);
                }
            }
            target.referenceWaveHeight = referenceWaveHeight;
        }

        private void SetValues(OceanSimulationInputs target, LocalWavesPreset preset)
        {
            target.chop = preset.Chop;
            target.local = preset.Spectrum;
            target.foam = preset.Foam;
            target.equalizerLerpValue = 0;
            target.equalizerRamp0 = GetSafeRamp(preset.Equalizer);
            target.equalizerRamp1 = EqualizerPreset.GetDefaultRamp();
        }

        private void SetValues(OceanSimulationInputs target, LocalWavesPreset start, LocalWavesPreset end, float t)
        {
            target.chop = Mathf.Lerp(start.Chop, end.Chop, t);
            target.local = SpectrumParams.Lerp(start.Spectrum, end.Spectrum, t);
            target.foam = FoamParams.Lerp(start.Foam, end.Foam, t);
            target.equalizerLerpValue = t;
            target.equalizerRamp0 = GetSafeRamp(start.Equalizer);
            target.equalizerRamp1 = GetSafeRamp(end.Equalizer);
        }

        private Texture2D GetSafeRamp(EqualizerPreset eq)
        {
            if (eq)
                return eq.GetRamp();
            else if (_defaultEqualizer)
                return _defaultEqualizer.GetRamp();
            else
                return EqualizerPreset.GetDefaultRamp();
        }

        private static LerpVars GetLerpVars(float windForce01, float maxWindForce, LocalWavesPreset[] presets)
        {
            LerpVars res = new LerpVars();
            if (presets.Length < 2) return res;
            float windForce = Mathf.Clamp01(windForce01) * maxWindForce;
            int i;
            for (i = 0; i < presets.Length; i++)
            {
                if (windForce < presets[i].WindForce)
                    break;
            }
            if (i == 0) return res;
            if (i == presets.Length) i -= 1;
            res.start = presets[i - 1];
            res.end = presets[i];

            res.t = Mathf.InverseLerp(presets[i - 1].WindForce, presets[i].WindForce, windForce);
            return res;
        }

        private struct LerpVars
        {
            public float t;
            public LocalWavesPreset start;
            public LocalWavesPreset end;
        }
    }
}


