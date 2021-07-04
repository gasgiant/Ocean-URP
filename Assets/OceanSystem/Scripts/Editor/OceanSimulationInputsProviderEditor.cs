using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanSimulationInputsProvider))]
    public class OceanSimulationInputsProviderEditor : Editor
    {
        private SerializedProperty _mode;
        private SerializedProperty _timeScale;
        private SerializedProperty _depth;
        private SerializedProperty _defaultEqualizer;
        private SerializedProperty _localPresets;
        private SerializedProperty _localPreset;
        private SerializedProperty _swellPreset;
        private SerializedProperty _windForce;

        private void OnEnable()
        {
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject((OceanSimulationInputsProvider)target), typeof(OceanSimulationInputsProvider), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_mode);
            EditorGUILayout.PropertyField(_timeScale);
            EditorGUILayout.PropertyField(_depth);
            if (_mode.enumValueIndex == (int)OceanSimulationInputsProvider.InputsProviderMode.Fixed)
            {
                EditorGUILayout.PropertyField(_swellPreset);
                EditorGUILayout.PropertyField(_localPreset);
            }
            else
            {
                EditorGUILayout.PropertyField(_swellPreset);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_windForce);
                EditorGUILayout.PropertyField(_defaultEqualizer);
                EditorGUILayout.PropertyField(_localPresets, new GUIContent("Local Wind"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            _mode = serializedObject.FindProperty("_mode");
            _timeScale = serializedObject.FindProperty("_timeScale");
            _depth = serializedObject.FindProperty("_depth");
            _defaultEqualizer = serializedObject.FindProperty("_defaultEqualizer");
            _localPresets = serializedObject.FindProperty("_localWinds");
            _localPreset = serializedObject.FindProperty("_localWind");
            _swellPreset = serializedObject.FindProperty("_swell");
            _windForce = serializedObject.FindProperty("_defaultWindForce");
        }
    }
}
