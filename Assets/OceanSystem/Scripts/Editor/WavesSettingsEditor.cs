using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(WavesSettings))]
    [CanEditMultipleObjects]
    public class WavesSettingsEditor : Editor
    {
        SerializedProperty timeScale;
        SerializedProperty depth;
        SerializedProperty chop;
        SerializedProperty foam;
        SerializedProperty local;
        SerializedProperty swell;

        SerializedProperty spectrumPlot;

        SpectrumProperties localSpectrumProperties;
        SpectrumProperties swellSpectrumProperties;

        WavesSettings wavesSettings;

        private void OnEnable()
        {
            wavesSettings = (WavesSettings)target;
            ValidateSpectrums();
            FindProperties();
        }

        public void ValidateSpectrums()
        {
            if (wavesSettings.local.windSpeed < 0.1f)
                wavesSettings.local = SpectrumSettings.GetDefaultLocal();
            if (wavesSettings.swell.windSpeed < 0.1f)
                wavesSettings.swell = SpectrumSettings.GetDefaultSwell();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((WavesSettings)target), typeof(WavesSettings), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(spectrumPlot);
            if (wavesSettings.spectrumPlot)
            {
                SpectrumPlotter.DrawSpectrumOnly(wavesSettings);
            }

            EditorGUILayout.LabelField("Common", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(timeScale);
            EditorGUILayout.PropertyField(depth);
            EditorGUILayout.PropertyField(chop);
            EditorGUILayout.PropertyField(foam);
            EditorGUI.indentLevel -= 1;

            DrawSpectrumSettings(local, localSpectrumProperties, wavesSettings.local);
            DrawSpectrumSettings(swell, swellSpectrumProperties, wavesSettings.swell);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSpectrumSettings(SerializedProperty spectrumSettingsProperty, SpectrumProperties props, SpectrumSettings settings)
        {
            spectrumSettingsProperty.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(spectrumSettingsProperty.isExpanded, spectrumSettingsProperty.displayName);
            if (spectrumSettingsProperty.isExpanded)
            {
                EditorGUI.indentLevel += 1;
                EditorGUILayout.LabelField("Energy", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(props.energySpectrum);
                EditorGUILayout.PropertyField(props.windSpeed);
                if (settings.energySpectrum != EnergySpectrum.PM)
                {
                    EditorGUILayout.PropertyField(props.fetch);
                    EditorGUILayout.PropertyField(props.peaking);
                }
                EditorGUILayout.PropertyField(props.scale);
                EditorGUILayout.PropertyField(props.cutoffWavelength);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Spread", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(props.windDirection);
                EditorGUILayout.PropertyField(props.alignment);
                EditorGUILayout.PropertyField(props.extraAlignment);
                EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void FindProperties()
        {
            timeScale = serializedObject.FindProperty("timeScale");
            depth = serializedObject.FindProperty("depth");
            chop = serializedObject.FindProperty("chop");
            foam = serializedObject.FindProperty("foam");
            local = serializedObject.FindProperty("local");
            swell = serializedObject.FindProperty("swell");

            localSpectrumProperties = FindSpectrumProperties(local);
            swellSpectrumProperties = FindSpectrumProperties(swell);

            spectrumPlot = serializedObject.FindProperty("spectrumPlot");
        }

        SpectrumProperties FindSpectrumProperties(SerializedProperty spectrum)
        {
            SpectrumProperties props = new SpectrumProperties();
            props.energySpectrum = spectrum.FindPropertyRelative("energySpectrum");
            props.windSpeed = spectrum.FindPropertyRelative("windSpeed");
            props.fetch = spectrum.FindPropertyRelative("fetch");
            props.peaking = spectrum.FindPropertyRelative("peaking");
            props.scale = spectrum.FindPropertyRelative("scale");
            props.cutoffWavelength = spectrum.FindPropertyRelative("cutoffWavelength");
            props.windDirection = spectrum.FindPropertyRelative("windDirection");
            props.alignment = spectrum.FindPropertyRelative("alignment");
            props.extraAlignment = spectrum.FindPropertyRelative("extraAlignment");
            return props;
        }

        class SpectrumProperties
        {
            public SerializedProperty energySpectrum;
            public SerializedProperty windSpeed;
            public SerializedProperty fetch;
            public SerializedProperty peaking;
            public SerializedProperty scale;
            public SerializedProperty cutoffWavelength;
            public SerializedProperty windDirection;
            public SerializedProperty alignment;
            public SerializedProperty extraAlignment;
        }

    }
}
