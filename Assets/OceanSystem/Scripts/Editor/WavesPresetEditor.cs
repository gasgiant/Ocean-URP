using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(WavesPreset))]
    public class WavesPresetEditor : Editor
    {
        private SerializedProperty _type;
        private SerializedProperty _referenceWaveHeight;
        private SerializedProperty _spectrum;
        private SerializedProperty _foam;
        private SerializedProperty _chop;
        private SerializedProperty _equalizer;
        private SpectrumParamsDrawer.SpectrumProperties spectrumProps;

        private void OnEnable()
        {
            FindProperties();
            spectrumProps = SpectrumParamsDrawer.FindSpectrumProperties(_spectrum);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((WavesPreset)target), typeof(WavesPreset), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_type);
            EditorGUILayout.Space();
            SpectrumParamsDrawer.DrawSpectrumParams(_spectrum, spectrumProps, SpectrumParamsDrawer.SpectrumParamsDrawerMode.Foldout, true);
            EditorGUILayout.PropertyField(_referenceWaveHeight);
            if (_type.enumValueIndex == (int)WavesPreset.PresetType.Local)
            {
                EditorGUILayout.PropertyField(_chop);
                EditorGUILayout.PropertyField(_equalizer);
                EditorGUILayout.PropertyField(_foam);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            _type = serializedObject.FindProperty("_type");
            _referenceWaveHeight = serializedObject.FindProperty("_referenceWaveHeight");
            _spectrum = serializedObject.FindProperty("_spectrum");
            _foam = serializedObject.FindProperty("_foam");
            _chop = serializedObject.FindProperty("_chop");
            _equalizer = serializedObject.FindProperty("_equalizer");
        }
    }
}
