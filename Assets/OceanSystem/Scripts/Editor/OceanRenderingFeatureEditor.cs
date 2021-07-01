using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanRenderer))]
    public class OceanRenderingSettingsPropertyDrawer : Editor
    {
        private const string requirementTextures = "Depth Texture and Opaque Texture must " +
            "be enabled in the pipeline asset.";
        private const string requirementDownsampling = "Opaque downsampling must be None.";

        private SerializedProperty settings;
        private SerializedProperty skyMapResolution;
        private SerializedProperty updateSkyMap;
        private SerializedProperty transparency;
        private SerializedProperty underwater;


        private void OnEnable()
        {
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(skyMapResolution);
            EditorGUILayout.PropertyField(updateSkyMap);
            EditorGUILayout.PropertyField(transparency);
            EditorGUILayout.PropertyField(underwater);

            if (transparency.boolValue || underwater.boolValue)
            {
                string message = requirementTextures;
                if (underwater.boolValue)
                    message += " " + requirementDownsampling;
                EditorGUILayout.HelpBox(message, MessageType.Info, true);
            }

            EditorGUILayout.Space();
            bool newValue = EditorGUILayout.Toggle("Render In Edit Mode", 
                EditorPrefs.GetBool(OceanRenderer.RenderInEditModePrefName));
            OceanRenderer.RenderInEditMode = newValue;
            EditorPrefs.SetBool(OceanRenderer.RenderInEditModePrefName, newValue);

            if (EditorGUI.EndChangeCheck())
            {
                EditorApplication.QueuePlayerLoopUpdate();
                QueueDelayedPlayerLoopUpdate();
            }

            serializedObject.ApplyModifiedProperties();
        }

        async private void QueueDelayedPlayerLoopUpdate()
        {
            await Task.Delay(100);
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private void FindProperties()
        {
            settings = serializedObject.FindProperty("settings");
            skyMapResolution = settings.FindPropertyRelative("skyMapResolution");
            updateSkyMap = settings.FindPropertyRelative("updateSkyMap"); ;
            transparency = settings.FindPropertyRelative("transparency"); ;
            underwater = settings.FindPropertyRelative("underwaterEffect"); ;
        }
    }
}
