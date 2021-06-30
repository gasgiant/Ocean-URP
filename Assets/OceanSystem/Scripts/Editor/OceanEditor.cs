using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(Ocean))]
    public class OceanEditor : Editor
    {
        private SerializedProperty reflectionsMode;
        private SerializedProperty probe;
        private SerializedProperty cubemap;
        private SerializedProperty material;

        private SerializedProperty viewer;
        private SerializedProperty minMeshScale;
        private SerializedProperty clipMapLevels;
        private SerializedProperty vertexDensity;

        private SerializedProperty simulationSettings;
        private SerializedProperty wavesSettings;
        private SerializedProperty equalizerPreset;

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

            bool prefValue = EditorPrefs.GetBool("RenderOceanInEditMode");
            prefValue = EditorGUILayout.Toggle("Render In Edit Mode", prefValue);
            if (prefValue != Ocean.RenderInEditMode)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
            Ocean.RenderInEditMode = prefValue;
            EditorPrefs.SetBool("RenderOceanInEditMode", prefValue);

            EditorGUILayout.PropertyField(material);
            EditorGUILayout.PropertyField(reflectionsMode);
            switch ((Ocean.OceanReflectionsMode)reflectionsMode.enumValueIndex)
            {
                case Ocean.OceanReflectionsMode.RealtimeProbe:
                    EditorGUILayout.PropertyField(probe);
                    break;
                case Ocean.OceanReflectionsMode.Custom:
                    EditorGUILayout.PropertyField(cubemap);
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(simulationSettings);
            EditorGUILayout.PropertyField(wavesSettings);
            EditorGUILayout.PropertyField(equalizerPreset);

            EditorGUILayout.PropertyField(viewer);
            EditorGUILayout.PropertyField(minMeshScale);
            EditorGUILayout.PropertyField(clipMapLevels);
            EditorGUILayout.PropertyField(vertexDensity);

            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            reflectionsMode = serializedObject.FindProperty("reflectionsMode");
            probe = serializedObject.FindProperty("probe");
            cubemap = serializedObject.FindProperty("cubemap");
            material = serializedObject.FindProperty("material");

            viewer = serializedObject.FindProperty("viewer");
            minMeshScale = serializedObject.FindProperty("minMeshScale");
            clipMapLevels = serializedObject.FindProperty("clipMapLevels");
            vertexDensity = serializedObject.FindProperty("vertexDensity");

            simulationSettings = serializedObject.FindProperty("simulationSettings");
            wavesSettings = serializedObject.FindProperty("wavesSettings");
            equalizerPreset = serializedObject.FindProperty("equalizerPreset");
        }
    }
}


