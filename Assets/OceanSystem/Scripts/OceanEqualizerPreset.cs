using UnityEngine;

namespace OceanSystem
{
    [CreateAssetMenu(fileName = "New Ocean Equalizer", menuName = "Ocean/Equalizer Preset")]
    public class OceanEqualizerPreset : ScriptableObject
    {
        public const float XMin = -1.5f;
        public const float XMax = 3.5f;
        private const int resolution = 128;
        public Texture2D Ramp => ramp;

        [SerializeField]
        private Filter[] scaleFilters;
        [SerializeField]
        private Filter[] chopFilters;

        private Texture2D ramp;
        private Color[] colors = new Color[resolution];

#if UNITY_EDITOR
        public WavesSettings DisplayWavesSettings;
        public bool showScale;
        public bool showChop;

        private void OnValidate()
        {
            for (int i = 0; i < scaleFilters.Length; i++)
            {
                if (scaleFilters[i].width < Filter.MinWidth)
                    scaleFilters[i].width = 0.5f;
            }

            for (int i = 0; i < chopFilters.Length; i++)
            {
                if (chopFilters[i].width < Filter.MinWidth)
                    chopFilters[i].width = 0.5f;
            }
        }

#endif

        public void BakeRamp()
        {
            if (ramp == null || ramp.width != resolution)
            {
                ramp = new Texture2D(resolution, 1, TextureFormat.RGHalf, false, true);
                ramp.wrapMode = TextureWrapMode.Clamp;
                ramp.filterMode = FilterMode.Bilinear;
            }

            if (colors.Length != resolution)
            {
                colors = new Color[resolution];
            }

            for (int i = 0; i < resolution; i++)
            {
                float x = Mathf.Lerp(XMin, XMax, (float)i / resolution);
                colors[i].r = Mathf.Max(0, EvaluateFiltersArray(scaleFilters, x));
                colors[i].g = Mathf.Max(0, EvaluateFiltersArray(chopFilters, x));
            }

            ramp.SetPixels(colors);
            ramp.Apply(false);
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
