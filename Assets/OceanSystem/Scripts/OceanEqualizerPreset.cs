using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Ocean Equalizer", menuName = "Ocean/Equalizer Preset")]
    public class OceanEqualizerPreset : ScriptableObject
    {
        public const float XMin = -1.5f;
        public const float XMax = 3.5f;
        private const int Resolution = 128;

        [SerializeField] private Filter[] _scaleFilters;
        [SerializeField] private Filter[] _chopFilters;

        private Texture2D _ramp;
        private Color[] _colors = new Color[Resolution];

#if UNITY_EDITOR
        [SerializeField] private OceanWavesSettings _displayWavesSettings;

        private void OnValidate()
        {
            for (int i = 0; i < _scaleFilters.Length; i++)
            {
                if (_scaleFilters[i].width < Filter.MinWidth)
                    _scaleFilters[i].width = 0.5f;
            }

            for (int i = 0; i < _chopFilters.Length; i++)
            {
                if (_chopFilters[i].width < Filter.MinWidth)
                    _chopFilters[i].width = 0.5f;
            }

            BakeRamp();
        }
#endif

        public Texture2D GetRamp()
        {
            if (_ramp == null)
                BakeRamp();
            return _ramp;
        }

        private void BakeRamp()
        {
            if (_ramp == null || _ramp.width != Resolution)
            {
                _ramp = new Texture2D(Resolution, 1, TextureFormat.RGHalf, false, true);
                _ramp.wrapMode = TextureWrapMode.Clamp;
                _ramp.filterMode = FilterMode.Bilinear;
            }

            if (_colors.Length != Resolution)
            {
                _colors = new Color[Resolution];
            }

            for (int i = 0; i < Resolution; i++)
            {
                float x = Mathf.Lerp(XMin, XMax, (float)i / Resolution);
                _colors[i].r = Mathf.Max(0, EvaluateFiltersArray(_scaleFilters, x));
                _colors[i].g = Mathf.Max(0, EvaluateFiltersArray(_chopFilters, x));
            }

            _ramp.SetPixels(_colors);
            _ramp.Apply(false);
        }

        float EvaluateFiltersArray(Filter[] filters, float x)
        {
            if (filters == null) return 1;
            float v = 1;
            for (int j = 0; j < filters.Length; j++)
            {
                v += filters[j].Evaluate(x);
            }
            return v;
        }

        private enum FilterType
        {
            Bell,
            [InspectorName("Hight Shelf")]
            Highshelf,
            [InspectorName("Low Shelf")]
            Lowshelf
        }

        [System.Serializable]
        private struct Filter
        {
            public const float MinWidth = 0.1f;

            public FilterType type;
            [Range(XMin, XMax)]
            public float center;
            [Range(-1, 2)]
            public float value;
            [Range(MinWidth, 3)]
            public float width;

            public float Evaluate(float x)
            {
                float arg;
                float w = Mathf.Max(MinWidth, width);
                switch (type)
                {
                    case FilterType.Bell:
                        arg = (x - center);
                        return value * Mathf.Exp(-arg * arg / w / w);
                    case FilterType.Highshelf:
                        arg = Mathf.Min((x - center), 0);
                        return value * Mathf.Exp(-arg * arg / w / w);
                    case FilterType.Lowshelf:
                        arg = Mathf.Max((x - center), 0);
                        return value * Mathf.Exp(-arg * arg / w / w);
                    default:
                        return 0;
                }

            }
        }
    }
}
