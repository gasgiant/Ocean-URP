using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
    public static class SpectrumPlotter
    {
        static float padding = 2;
        static float tickLabelsHeight = 10;
        static Color cascade0Color = new Color(241, 88, 84, 255) / 255;
        static Color cascade1Color = new Color(250, 164, 58, 255) / 255;
        static Color cascade2Color = new Color(96, 189, 104, 255) / 255;
        static Color cascade3Color = new Color(93, 165, 218, 255) / 255;

        static Vector2 graphSize = new Vector2(OceanEqualizerPreset.XMax - OceanEqualizerPreset.XMin, 1);
        static Vector2 leftBottom = new Vector2(OceanEqualizerPreset.XMin, 0.0f);

        static Material materialCached;
        static Material material
        {
            get
            {
                if (materialCached == null)
                    materialCached = new Material(Shader.Find("Hidden/SpectrumPlot"));
                return materialCached;
            }
        }

        static Texture2D backgroundTextureCached;
        static Texture2D backgroundTexture
        {
            get
            {
                if (backgroundTextureCached == null)
                {
                    backgroundTextureCached = new Texture2D(1, 1);
                    backgroundTextureCached.SetPixel(0, 0, new Color32(56, 56, 56, 255));
                    backgroundTextureCached.Apply();
                }
                return backgroundTextureCached;
            }
        }

        public static void DrawSpectrumOnly(WavesSettings wavesSettings)
        {
            DrawSpectrum(wavesSettings);
        }

        public static void DrawGraphWithCascades(OceanSimulationSettings simulationSettings)
        {
            DrawSpectrum(simulationSettings.DisplayWavesSettings, simulationSettings);
        }

        public static void DrawSpectrumWithEqualizer(WavesSettings wavesSettings, Texture2D ramp, int channel, Color fill, Color line)
        {
            material.SetInt("equalizerChannel", channel);
            material.SetVector("rampFill", fill);
            material.SetVector("rampLine", line);
            DrawSpectrum(wavesSettings, null, ramp);
        }

        static void DrawSpectrum(WavesSettings wavesSettings, OceanSimulationSettings simulationSettings = null, Texture2D ramp = null)
        {
            Rect space = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(150));
            Rect graphSpace = space;
            graphSpace.height -= 2 * tickLabelsHeight + 2 * padding;

            material.SetFloat("width", graphSize.x);
            material.SetFloat("height", graphSize.y);
            material.SetFloat("windowAspectRatio", graphSpace.width / graphSpace.height);
            material.SetVector("leftBottom", leftBottom);
            material.SetColor("backgroundColor", new Color32(56, 56, 56, 255));

            material.SetFloat("drawSpectrum", 0);
            material.SetFloat("showCascades", 0);
            material.SetFloat("drawRamp", 0);

            if (wavesSettings != null)
            {
                SetSpectrumSettingsToMaterial(material, wavesSettings);
                material.SetFloat("drawSpectrum", 1);
            }


            if (simulationSettings != null)
            {
                SetCascadeMaterialProps(material, simulationSettings);
                material.SetFloat("showCascades", 1);
            }

            if (ramp != null)
            {
                SetRampMaterialProps(material, ramp);
                material.SetFloat("drawRamp", 1);
            }

            EditorGUI.DrawPreviewTexture(graphSpace, EditorGUIUtility.whiteTexture, material);
            AddXLabels(graphSpace);

            if (simulationSettings != null)
                AddCascadesLegend(graphSpace, simulationSettings);
        }



        static float GraphXToPositionX(float graphX, Rect rect)
        {
            graphX -= leftBottom.x;
            graphX /= graphSize.x;
            return rect.position.x + rect.width * graphX;
        }

        static void SetSpectrumSettingsToMaterial(Material material, WavesSettings settings)
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

        static void SetCascadeMaterialProps(Material material, OceanSimulationSettings simulationSettings)
        {
            Vector4 cutoffsLow;
            Vector4 cutoffsHigh;
            simulationSettings.CalculateCascadeDomains(out cutoffsLow, out cutoffsHigh);

            material.SetVector("cutoffsLow", cutoffsLow);
            material.SetVector("cutoffsHigh", cutoffsHigh);
            material.SetVector("cascade0Color", cascade0Color);
            material.SetVector("cascade1Color", cascade1Color);
            material.SetVector("cascade2Color", cascade2Color);
            material.SetVector("cascade3Color", cascade3Color);
        }

        static void SetRampMaterialProps(Material material, Texture2D ramp)
        {
            material.SetTexture("equalizerRamp", ramp);
        }

        public static void AddXLabels(Rect graphSpace)
        {
            float tickLabelsWidth = tickLabelsHeight * 4;
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            boldLabelStyle.alignment = TextAnchor.MiddleCenter;

            float xTickLabelsY = graphSpace.position.y + graphSpace.height + padding + tickLabelsHeight * 0.5f;
            Rect xTickLabelRect = new Rect(Vector2.zero, new Vector2(tickLabelsWidth, tickLabelsHeight));
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

            Rect yLabelRect = new Rect(new Vector2(graphSpace.position.x, xTickLabelsY + tickLabelsHeight * 1.5f + padding),
                new Vector2(graphSpace.width, tickLabelsHeight * 1.5f));
            EditorGUI.LabelField(yLabelRect, "Wavelength", boldLabelStyle);
            GUILayout.Space(tickLabelsHeight + padding);
            EditorGUILayout.Space();
        }

        public static void AddCascadesLegend(Rect graphSpace, OceanSimulationSettings simulationSettings)
        {
            GUIStyle legendStyle = new GUIStyle(EditorStyles.boldLabel);
            legendStyle.alignment = TextAnchor.MiddleLeft;
            legendStyle.normal.textColor = cascade0Color;
            GUIStyle legendBackgroundStyle = new GUIStyle();
            legendBackgroundStyle.normal.background = backgroundTexture;
            Vector2 legendSize = GUI.skin.label.CalcSize(new GUIContent("Cascade 0"));
            Rect legendRectangle = new Rect(
                new Vector2(graphSpace.position.x + tickLabelsHeight, graphSpace.position.y + tickLabelsHeight),
                legendSize);
            EditorGUI.LabelField(new Rect(legendRectangle.position - Vector2.right * tickLabelsHeight * 0.5f,
                new Vector2(legendRectangle.width + tickLabelsHeight,
                            legendRectangle.height * (int)simulationSettings.cascadesNumber)), "", legendBackgroundStyle);
            EditorGUI.LabelField(legendRectangle, "Cascade 0", legendStyle);
            legendRectangle.position += Vector2.up * legendSize.y;
            legendStyle.normal.textColor = cascade1Color;
            EditorGUI.LabelField(legendRectangle, "Cascade 1", legendStyle);
            if (simulationSettings.CascadesNumber > 2)
            {
                legendRectangle.position += Vector2.up * legendSize.y;
                legendStyle.normal.textColor = cascade2Color;
                EditorGUI.LabelField(legendRectangle, "Cascade 2", legendStyle);
            }
            if (simulationSettings.CascadesNumber > 3)
            {
                legendRectangle.position += Vector2.up * legendSize.y;
                legendStyle.normal.textColor = cascade3Color;
                EditorGUI.LabelField(legendRectangle, "Cascade 3", legendStyle);
            }
        }
    }
}
