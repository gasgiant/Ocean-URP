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

        [SerializeField] private EqualizerPreset _defaultEqualizer;
        [SerializeField] private WavesPreset[] _localWinds;
        [SerializeField] private WavesPreset _localWind;
        [SerializeField] private WavesPreset _swell;

        [Range(0, 1)]
        [SerializeField] private float _defaultWindForce;

        public void PopulateInputs(OceanSimulationInputs target)
        {
            PopulateInputs(target, _defaultWindForce);
        }

        public void PopulateInputs(OceanSimulationInputs target, float windForce01)
        {
            target.timeScale = _timeScale;
            target.depth = _depth;
            if (_swell)
                target.swell = _swell.Spectrum;

            if (_mode == InputsProviderMode.Fixed || _localWinds == null || _localWinds.Length < 2)
            {
                if (_localWind == null) return;
                SetValues(target, _localWind);
            }
            else
            {
                LerpVars lerp = GetLerpVars(windForce01, _localWinds.Length);
                WavesPreset start = _localWinds[lerp.startInd];
                WavesPreset end = _localWinds[lerp.endInd];

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


