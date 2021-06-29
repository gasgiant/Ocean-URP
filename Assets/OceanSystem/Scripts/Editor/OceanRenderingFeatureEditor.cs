using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanRenderer))]
    public class OceanRenderingSettingsPropertyDrawer : Editor
    {
        SerializedProperty settings;
        SerializedProperty skyMapResolution;
        SerializedProperty updateSkyMap;
        SerializedProperty transparency;
        SerializedProperty underwater;

        private void OnEnable()
        {
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(skyMapResolution);
            EditorGUILayout.PropertyField(updateSkyMap);
            EditorGUILayout.PropertyField(transparency);
            EditorGUILayout.PropertyField(underwater);

            serializedObject.ApplyModifiedProperties();
        }

        void FindProperties()
        {
            settings = serializedObject.FindProperty("settings");
            skyMapResolution = settings.FindPropertyRelative("skyMapResolution");
            updateSkyMap = settings.FindPropertyRelative("updateSkyMap"); ;
            transparency = settings.FindPropertyRelative("transparency"); ;
            underwater = settings.FindPropertyRelative("underwaterEffect"); ;
        }
    }
}
