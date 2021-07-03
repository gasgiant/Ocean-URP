using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    //[CustomEditor(typeof(Ocean))]
    public class OceanEditor : Editor
    {
        private SerializedProperty _reflectionsMode;
        private SerializedProperty _probe;
        private SerializedProperty _cubemap;
        private SerializedProperty _material;

        private SerializedProperty _viewer;
        private SerializedProperty _minMeshScale;
        private SerializedProperty _clipMapLevels;
        private SerializedProperty _vertexDensity;

        private SerializedProperty _simulationSettings;

        private void OnEnable()
        {
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((Ocean)target), typeof(Ocean), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_material);
            EditorGUILayout.PropertyField(_reflectionsMode);
            switch ((Ocean.OceanReflectionsMode)_reflectionsMode.enumValueIndex)
            {
                case Ocean.OceanReflectionsMode.RealtimeProbe:
                    EditorGUILayout.PropertyField(_probe);
                    break;
                case Ocean.OceanReflectionsMode.Custom:
                    EditorGUILayout.PropertyField(_cubemap);
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(_simulationSettings);

            EditorGUILayout.PropertyField(_viewer);
            EditorGUILayout.PropertyField(_minMeshScale);
            EditorGUILayout.PropertyField(_clipMapLevels);
            EditorGUILayout.PropertyField(_vertexDensity);

            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            _reflectionsMode = serializedObject.FindProperty("_reflectionsMode");
            _probe = serializedObject.FindProperty("_probe");
            _cubemap = serializedObject.FindProperty("cubemap");
            _material = serializedObject.FindProperty("_material");

            _viewer = serializedObject.FindProperty("_viewer");
            _minMeshScale = serializedObject.FindProperty("_minMeshScale");
            _clipMapLevels = serializedObject.FindProperty("_clipMapLevels");
            _vertexDensity = serializedObject.FindProperty("_vertexDensity");

            _simulationSettings = serializedObject.FindProperty("_simulationSettings");
        }
    }
}


