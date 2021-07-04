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
        [SerializeField] private WavesLevel[] _localWinds;
        [SerializeField] private WavesPreset _localWind;
        [SerializeField] private WavesPreset _swell;

        [Range(0, 1)]
        [SerializeField] private float _defaultWindForce;

        [SerializeField, HideInInspector] private float _maxWindForce;

        private void OnValidate()
        {
            _maxWindForce = 0;
            for (int i = 0; i < _localWinds.Length; i++)
            {
                if (_localWinds[i].windForce < 0)
                    _localWinds[i].windForce = 0;
                if (_localWinds[i].windForce > _maxWindForce)
                    _maxWindForce = _localWinds[i].windForce;
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
            if (_swell)
                target.swell = _swell.Spectrum;

            if (_mode == InputsProviderMode.Fixed || _localWinds == null || _localWinds.Length < 2)
            {
                if (_localWind == null) return;
                SetValues(target, _localWind);
            }
            else
            {
                LerpVars lerp = GetLerpVars(windForce01, _maxWindForce, _localWinds);

                if (lerp.start == null || lerp.end == null)
                    return;
                SetValues(target, lerp.start, lerp.end, lerp.t);
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

        private static LerpVars GetLerpVars(float windForce01, float maxWindForce, WavesLevel[] wavesLevels)
        {
            LerpVars res = new LerpVars();
            if (wavesLevels.Length < 2) return res;
            float windForce = Mathf.Clamp01(windForce01) * maxWindForce;
            int i;
            for (i = 0; i < wavesLevels.Length; i++)
            {
                if (windForce < wavesLevels[i].windForce)
                    break;
            }
            if (i == 0) return res;
            if (i == wavesLevels.Length) i -= 1;
            res.start = wavesLevels[i - 1].preset;
            res.end = wavesLevels[i].preset;

            res.t = Mathf.InverseLerp(wavesLevels[i - 1].windForce, wavesLevels[i].windForce, windForce);
            return res;
        }

        private struct LerpVars
        {
            public float t;
            public WavesPreset start;
            public WavesPreset end;
        }

        [System.Serializable]
        private struct WavesLevel
        {
            public WavesPreset preset;
            public float windForce;
        }
    }
}


