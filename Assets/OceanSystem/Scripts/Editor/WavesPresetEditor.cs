using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(WavesPreset))]
    public class WavesPresetEditor : Editor
    {
        private SerializedProperty _type;
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
            if (_type.enumValueIndex == (int)WavesPreset.PresetType.Swell)
            {
                SpectrumParamsDrawer.DrawSpectrumParams(_spectrum, spectrumProps, SpectrumParamsDrawer.SpectrumParamsDrawerMode.Raw, true);
            }
            else
            {
                EditorGUILayout.PropertyField(_chop);
                EditorGUILayout.PropertyField(_equalizer);
                EditorGUILayout.PropertyField(_foam);
                SpectrumParamsDrawer.DrawSpectrumParams(_spectrum, spectrumProps, SpectrumParamsDrawer.SpectrumParamsDrawerMode.FoldoutGroup, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            _type = serializedObject.FindProperty("_type");
            _spectrum = serializedObject.FindProperty("_spectrum");
            _foam = serializedObject.FindProperty("_foam");
            _chop = serializedObject.FindProperty("_chop");
            _equalizer = serializedObject.FindProperty("_equalizer");
        }
    }
}
