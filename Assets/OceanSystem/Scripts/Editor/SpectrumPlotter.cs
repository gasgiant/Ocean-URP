using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
    public static class SpectrumPlotter
    {
        private const float Padding = 2;
        private const float TickLabelsHeight = 10;
        private static readonly Color _cascade0Color = new Color(241, 88, 84, 255) / 255;
        private static readonly Color _cascade1Color = new Color(250, 164, 58, 255) / 255;
        private static readonly Color _cascade2Color = new Color(96, 189, 104, 255) / 255;
        private static readonly Color _cascade3Color = new Color(93, 165, 218, 255) / 255;
        private static readonly Vector2 _graphSize = new Vector2(OceanEqualizerPreset.XMax - OceanEqualizerPreset.XMin, 1);
        private static readonly Vector2 _leftBottom = new Vector2(OceanEqualizerPreset.XMin, 0.0f);

        private static Material _cachedMaterial;
        private static Material PlotMaterial
        {
            get
            {
                if (_cachedMaterial == null)
                    _cachedMaterial = new Material(Shader.Find("Hidden/SpectrumPlot"));
                return _cachedMaterial;
            }
        }

        private static Texture2D _cachedBackgroundTexture;
        private static Texture2D BackgroundTexture
        {
            get
            {
                if (_cachedBackgroundTexture == null)
                {
                    _cachedBackgroundTexture = new Texture2D(1, 1);
                    _cachedBackgroundTexture.SetPixel(0, 0, new Color32(56, 56, 56, 255));
                    _cachedBackgroundTexture.Apply();
                }
                return _cachedBackgroundTexture;
            }
        }

        public static void DrawSpectrumOnly(OceanWavesSettings wavesSettings)
        {
            DrawSpectrum(wavesSettings);
        }

        public static void DrawGraphWithCascades(OceanSimulationSettings simulationSettings, OceanWavesSettings wavesSettings)
        {
            DrawSpectrum(wavesSettings, simulationSettings);
        }

        public static void DrawSpectrumWithEqualizer(OceanWavesSettings wavesSettings, Texture2D ramp, int channel, Color fill, Color line)
        {
            PlotMaterial.SetInt("equalizerChannel", channel);
            PlotMaterial.SetVector("rampFill", fill);
            PlotMaterial.SetVector("rampLine", line);
            DrawSpectrum(wavesSettings, null, ramp);
        }

        static void DrawSpectrum(OceanWavesSettings wavesSettings, OceanSimulationSettings simulationSettings = null, Texture2D ramp = null)
        {
            Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(150));
            Rect graphSpace = space;
            graphSpace.height -= 2 * TickLabelsHeight + 2 * Padding;

            PlotMaterial.SetFloat("width", _graphSize.x);
            PlotMaterial.SetFloat("height", _graphSize.y);
            PlotMaterial.SetFloat("windowAspectRatio", graphSpace.width / graphSpace.height);
            PlotMaterial.SetVector("leftBottom", _leftBottom);
            PlotMaterial.SetColor("backgroundColor", new Color32(56, 56, 56, 255));

            PlotMaterial.SetFloat("drawSpectrum", 0);
            PlotMaterial.SetFloat("showCascades", 0);
            PlotMaterial.SetFloat("drawRamp", 0);

            if (wavesSettings != null)
            {
                SetSpectrumSettingsToMaterial(PlotMaterial, wavesSettings);
                PlotMaterial.SetFloat("drawSpectrum", 1);
            }


            if (simulationSettings != null)
            {
                SetCascadeMaterialProps(PlotMaterial, simulationSettings);
                PlotMaterial.SetFloat("showCascades", 1);
            }

            if (ramp != null)
            {
                SetRampMaterialProps(PlotMaterial, ramp);
                PlotMaterial.SetFloat("drawRamp", 1);
            }

            EditorGUI.DrawPreviewTexture(graphSpace, EditorGUIUtility.whiteTexture, PlotMaterial);
            AddXLabels(graphSpace);

            if (simulationSettings != null)
                AddCascadesLegend(graphSpace, simulationSettings);
        }

        private static float GraphXToPositionX(float graphX, Rect rect)
        {
            graphX -= _leftBottom.x;
            graphX /= _graphSize.x;
            return rect.position.x + rect.width * graphX;
        }

        private static void SetSpectrumSettingsToMaterial(Material material, OceanWavesSettings settings)
        {
            material.SetFloat("local_scale", settings.local.scale);
            material.SetInt("local_energySpectrum", (int)settings.local.energySpectrum);
            material.SetFloat("local_windSpeed", settings.local.windSpeed);
            material.SetFloat("local_fetch", settings.local.fetch);
            material.SetFloat("local_peaking", settings.local.peaking);
            material.SetFloat("local_shortWaves", settings.local.cutoffWavelength);

            material.SetFloat("swell_scale", settings.swell.scale);
            material.SetInt("swell_energySpectrum", (int)settings.swell.energySpectrum);
            material.SetFloat("swell_windSpeed", settings.swell.windSpeed);
            material.SetFloat("swell_fetch", settings.swell.fetch);
            material.SetFloat("swell_peaking", settings.swell.peaking);
            material.SetFloat("swell_shortWaves", settings.swell.cutoffWavelength);

            material.SetFloat("Depth", settings.depth);
        }

        private static void SetCascadeMaterialProps(Material material, OceanSimulationSettings simulationSettings)
        {
            Vector4 cutoffsLow;
            Vector4 cutoffsHigh;
            simulationSettings.CalculateCascadeDomains(out cutoffsLow, out cutoffsHigh);

            material.SetVector("cutoffsLow", cutoffsLow);
            material.SetVector("cutoffsHigh", cutoffsHigh);
            material.SetVector("cascade0Color", _cascade0Color);
            material.SetVector("cascade1Color", _cascade1Color);
            material.SetVector("cascade2Color", _cascade2Color);
            material.SetVector("cascade3Color", _cascade3Color);
        }

        private static void SetRampMaterialProps(Material material, Texture2D ramp)
        {
            material.SetTexture("equalizerRamp", ramp);
        }

        private static void AddXLabels(Rect graphSpace)
        {
            float tickLabelsWidth = TickLabelsHeight * 4;
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            boldLabelStyle.alignment = TextAnchor.MiddleCenter;

            float xTickLabelsY = graphSpace.position.y + graphSpace.height + Padding + TickLabelsHeight * 0.5f;
            Rect xTickLabelRect = new Rect(Vector2.zero, new Vector2(tickLabelsWidth, TickLabelsHeight));
            xTickLabelRect.position = new Vector2(GraphXToPositionX(-1, graphSpace) - tickLabelsWidth * 0.5f, xTickLabelsY);
            EditorGUI.LabelField(xTickLabelRect, "10 cm", labelStyle);
            xTickLabelRect.position = new Vector2(GraphXToPositionX(0, graphSpace) - tickLabelsWidth * 0.5f, xTickLabelsY);
            EditorGUI.LabelField(xTickLabelRect, "1 m", labelStyle);
            xTickLabelRect.position = new Vector2(GraphXToPositionX(1, graphSpace) - tickLabelsWidth * 0.5f, xTickLabelsY);
            EditorGUI.LabelField(xTickLabelRect, "10 m", labelStyle);
            xTickLabelRect.position = new Vector2(GraphXToPositionX(2, graphSpace) - tickLabelsWidth * 0.5f, xTickLabelsY);
            EditorGUI.LabelField(xTickLabelRect, "100 m", labelStyle);
            xTickLabelRect.position = new Vector2(GraphXToPositionX(3, graphSpace) - tickLabelsWidth * 0.5f, xTickLabelsY);
            EditorGUI.LabelField(xTickLabelRect, "1 km", labelStyle);

            Rect yLabelRect = new Rect(new Vector2(graphSpace.position.x, xTickLabelsY + TickLabelsHeight * 1.5f + Padding),
                new Vector2(graphSpace.width, TickLabelsHeight * 1.5f));
            EditorGUI.LabelField(yLabelRect, "Wavelength", boldLabelStyle);
            GUILayout.Space(TickLabelsHeight + Padding);
            EditorGUILayout.Space();
        }

        private static void AddCascadesLegend(Rect graphSpace, OceanSimulationSettings simulationSettings)
        {
            GUIStyle legendStyle = new GUIStyle(EditorStyles.boldLabel);
            legendStyle.alignment = TextAnchor.MiddleLeft;
            legendStyle.normal.textColor = _cascade0Color;
            GUIStyle legendBackgroundStyle = new GUIStyle();
            legendBackgroundStyle.normal.background = BackgroundTexture;
            Vector2 legendSize = GUI.skin.label.CalcSize(new GUIContent("Cascade 0"));
            Rect legendRectangle = new Rect(
                new Vector2(graphSpace.position.x + TickLabelsHeight, graphSpace.position.y + TickLabelsHeight),
                legendSize);
            EditorGUI.LabelField(new Rect(legendRectangle.position - Vector2.right * TickLabelsHeight * 0.5f,
                new Vector2(legendRectangle.width + TickLabelsHeight,
                            legendRectangle.height * simulationSettings.CascadesNumber)), "", legendBackgroundStyle);
            EditorGUI.LabelField(legendRectangle, "Cascade 0", legendStyle);
            legendRectangle.position += Vector2.up * legendSize.y;
            legendStyle.normal.textColor = _cascade1Color;
            EditorGUI.LabelField(legendRectangle, "Cascade 1", legendStyle);
            if (simulationSettings.CascadesNumber > 2)
            {
                legendRectangle.position += Vector2.up * legendSize.y;
                legendStyle.normal.textColor = _cascade2Color;
                EditorGUI.LabelField(legendRectangle, "Cascade 2", legendStyle);
            }
            if (simulationSettings.CascadesNumber > 3)
            {
                legendRectangle.position += Vector2.up * legendSize.y;
                legendStyle.normal.textColor = _cascade3Color;
                EditorGUI.LabelField(legendRectangle, "Cascade 3", legendStyle);
            }
        }
    }
}
