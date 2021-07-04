using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    public static class SpectrumParamsDrawer
    {
        public static void DrawSpectrumParams(SerializedProperty spectrumParamsProperty, SpectrumProperties props, 
            SpectrumParamsDrawerMode mode, bool initializationButtons)
        {
            if (mode == SpectrumParamsDrawerMode.Foldout)
                spectrumParamsProperty.isExpanded = EditorGUILayout.Foldout(spectrumParamsProperty.isExpanded, spectrumParamsProperty.displayName);
            if (mode == SpectrumParamsDrawerMode.FoldoutGroup)
                spectrumParamsProperty.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(spectrumParamsProperty.isExpanded, spectrumParamsProperty.displayName);
            if (mode == SpectrumParamsDrawerMode.Raw || spectrumParamsProperty.isExpanded)
            {
                if (mode != SpectrumParamsDrawerMode.Raw)
                    EditorGUI.indentLevel += 1;
                EditorGUILayout.LabelField("Energy", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(props.energySpectrum);
                EditorGUILayout.PropertyField(props.windSpeed);
                if (props.energySpectrum.enumValueIndex != (int)SpectrumParams.EnergySpectrumModel.PM)
                {
                    EditorGUILayout.PropertyField(props.fetch);
                    EditorGUILayout.PropertyField(props.peaking);
                }
                EditorGUILayout.PropertyField(props.scale);
                EditorGUILayout.PropertyField(props.cutoffWavelength);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Spread", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(props.alignment);
                EditorGUILayout.PropertyField(props.extraAlignment);
                if (initializationButtons)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 15);
                    SpectrumParams? pars = null;
                    if (GUILayout.Button("Set Default Local"))
                    {
                        pars = SpectrumParams.GetDefaultLocal();
                    }
                    if (GUILayout.Button("Set Default Swell"))
                    {
                        pars = SpectrumParams.GetDefaultSwell();
                    }
                    if (pars != null)
                    {
                        SetValueToProperty(props, pars.Value);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (mode != SpectrumParamsDrawerMode.Raw)
                {
                    EditorGUI.indentLevel -= 1;
                    if (mode == SpectrumParamsDrawerMode.FoldoutGroup)
                        EditorGUILayout.EndFoldoutHeaderGroup();
                    EditorGUILayout.Space();
                }
            }
        }

        public static SpectrumProperties FindSpectrumProperties(SerializedProperty spectrum)
        {
            SpectrumProperties props = new SpectrumProperties();
            props.energySpectrum = spectrum.FindPropertyRelative("energySpectrum");
            props.windSpeed = spectrum.FindPropertyRelative("windSpeed");
            props.fetch = spectrum.FindPropertyRelative("fetch");
            props.peaking = spectrum.FindPropertyRelative("peaking");
            props.scale = spectrum.FindPropertyRelative("scale");
            props.cutoffWavelength = spectrum.FindPropertyRelative("cutoffWavelength");
            props.alignment = spectrum.FindPropertyRelative("alignment");
            props.extraAlignment = spectrum.FindPropertyRelative("extraAlignment");
            return props;
        }

        private static void SetValueToProperty(SpectrumProperties props, SpectrumParams value)
        {
            props.energySpectrum.enumValueIndex = (int)value.energySpectrum;
            props.windSpeed.floatValue = value.windSpeed;
            props.fetch.floatValue = value.fetch;
            props.peaking.floatValue = value.peaking;
            props.scale.floatValue = value.scale;
            props.cutoffWavelength.floatValue = value.cutoffWavelength;
            props.alignment.floatValue = value.alignment;
            props.extraAlignment.floatValue = value.extraAlignment;
        }

        public enum SpectrumParamsDrawerMode { Raw, Foldout, FoldoutGroup }

        public class SpectrumProperties
        {
            public SerializedProperty energySpectrum;
            public SerializedProperty windSpeed;
            public SerializedProperty fetch;
            public SerializedProperty peaking;
            public SerializedProperty scale;
            public SerializedProperty cutoffWavelength;
            public SerializedProperty alignment;
            public SerializedProperty extraAlignment;
        }
    }
}

