using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Inputs Provider", menuName = "Ocean/Inputs Provider")]
    public class OceanSimulationInputsProvider : ScriptableObject
    {
        public enum InputsProviderMode { Fixed, Scale }

        public InputsProviderMode Mode => _mode;

        [SerializeField] private InputsProviderMode _mode;
        [SerializeField] private float _timeScale = 1;
        [SerializeField] private float _depth = 1000;

        [SerializeField] private EqualizerPreset _defaultEqualizer;
        [SerializeField] private WavesPreset[] _localPresets;
        [SerializeField] private WavesPreset _localPreset;
        [SerializeField] private WavesPreset _swellPreset;

        public void PopulateInputs(OceanSimulationInputs target, float windForce01)
        {
            target.timeScale = _timeScale;
            target.depth = _depth;
            if (_swellPreset != null)
                target.swell = _swellPreset.Spectrum;

            if (_mode == InputsProviderMode.Fixed || _localPresets == null || _localPresets.Length < 2)
            {
                if (_localPreset == null) return;
                SetValues(target, _localPreset);
            }
            else
            {
                LerpVars lerp = GetLerpVars(windForce01, _localPresets.Length);
                WavesPreset start = _localPresets[lerp.startInd];
                WavesPreset end = _localPresets[lerp.endInd];

                if (start == null || end == null)
                    return;
                SetValues(target, start, end, lerp.t);
            }    
        }

        private void SetValues(OceanSimulationInputs target, WavesPreset preset)
        {
            target.chop = preset.Chop;
            target.local = preset.Spectrum;
            target.foam = preset.Foam;
            target.equalizerLerpValue = 0;
            target.equalizerRamp0 = preset.Equalizer.GetRamp();
            target.equalizerRamp1 = EqualizerPreset.GetDefaultRamp();
        }

        private void SetValues(OceanSimulationInputs target, WavesPreset start, WavesPreset end, float t)
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

        private static LerpVars GetLerpVars(float t, int count)
        {
            LerpVars res = new LerpVars();
            if (count < 2) return res;
            float v = Mathf.Clamp01(t) * (count - 1);
            res.startInd = Mathf.FloorToInt(v);
            res.endInd = Mathf.CeilToInt(v);
            res.t = Mathf.InverseLerp(res.startInd, res.endInd, v);
            return res;
        }

        private struct LerpVars
        {
            public float t;
            public int startInd;
            public int endInd;
        }
    }
}


