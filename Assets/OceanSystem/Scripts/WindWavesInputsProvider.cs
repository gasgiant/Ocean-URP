using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Waves Inputs Provider", menuName = "Ocean/Inputs Provider")]
    public class WindWavesInputsProvider : ScriptableObject
    {
        public enum Mode { Fixed, Scale }

        [SerializeField] private Mode _mode;
        [SerializeField] private float _timeScale = 1;
        [SerializeField] private float _depth = 1000;

        [SerializeField] private WindWavesPreset[] _localPresets;
        [SerializeField] private WindWavesPreset _localPreset;
        [SerializeField] private WindWavesPreset _swellPreset;

        public void PopulateInputs(WindWavesSimulationInputs target, float windDirection, float swellDirection, float mixValue)
        {
            target.timeScale = _timeScale;
            target.depth = _depth;
            target.localWindDirection = windDirection;
            target.swellDirection = swellDirection;
            if (_swellPreset != null)
                target.swell = _swellPreset.Spectrum;

            if (_mode == Mode.Fixed || _localPresets == null || _localPresets.Length < 2)
            {
                if (_localPreset == null) return;
                target.chop = _localPreset.Chop;
                target.local = _localPreset.Spectrum;
                target.foam = _localPreset.Foam;
                target.equalizerRamp = _localPreset.Equalizer.GetRamp();
            }
            else
            {
                LerpVars lerp = GetLerpVars(mixValue, _localPresets.Length);
                WindWavesPreset start = _localPresets[lerp.startInd];
                WindWavesPreset end = _localPresets[lerp.startInd];
                target.chop = Mathf.Lerp(start.Chop, end.Chop, lerp.t);
                target.local = SpectrumSettings.Lerp(start.Spectrum, end.Spectrum, lerp.t);
                target.foam = WindWavesFoamSettings.Lerp(start.Foam, end.Foam, lerp.t);
            }    
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


