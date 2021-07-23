using EditorExtras;
using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Inputs Provider", menuName = "Ocean/Inputs Provider")]
    public class OceanSimulationInputsProvider : ScriptableObject
    {
        public enum InputsProviderMode { Fixed, Scale }

        public InputsProviderMode Mode => _mode;

        [SerializeField] private InputsProviderMode _mode;
        [Range(0, 1)]
        [SerializeField] private float _timeScale = 1;
        [SerializeField] private float _depth = 1000;

        [InlineEditor]
        [SerializeField] private SwellPreset _swellPreset;
        [InlineEditor, HideIf(nameof(IsScale))]
        [SerializeField] private LocalWavesPreset _localWavesPreset;

        [SerializeField, Range(0, 1), ShowIf(nameof(IsScale))] private float _defaultWindForce;
        [SerializeField, ShowIf(nameof(IsScale))] private EqualizerPreset _defaultEqualizer;
        [SerializeField, HideInInspector] private LocalWavesPreset[] _localWavesPresets;
        
        [SerializeField, HideInInspector] private float _maxWindForce;

        private bool IsScale => _mode == InputsProviderMode.Scale;

        private void OnValidate()
        {
            _maxWindForce = 0;
            if (_localWavesPresets != null)
                for (int i = 0; i < _localWavesPresets.Length; i++)
                {
                    if (_localWavesPresets[i].WindForce > _maxWindForce)
                        _maxWindForce = _localWavesPresets[i].WindForce;
                }
        }

        public void PopulateInputs(OceanSimulationInputs target)
        {
            PopulateInputs(target, _defaultWindForce);
        }

        public void PopulateInputs(OceanSimulationInputs target, float windForce01)
        {
            target.timeScale = _timeScale;
            target.depth = _depth;

            float referenceWaveHeight = 0;
            if (_swellPreset)
            {
                target.swell = _swellPreset.Spectrum;
                referenceWaveHeight += _swellPreset.ReferenceWaveHeight;
            }

            if (_mode == InputsProviderMode.Fixed || _localWavesPresets == null || _localWavesPresets.Length < 2)
            {
                target.foamTrailUpdateTime = 0;
                if (_localWavesPreset != null)
                {
                    SetValues(target, _localWavesPreset);
                    referenceWaveHeight += _localWavesPreset.ReferenceWaveHeight;
                }
            }
            else
            {
                target.foamTrailUpdateTime = 1;
                LerpVars lerp = GetLerpVars(windForce01, _maxWindForce, _localWavesPresets);

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


