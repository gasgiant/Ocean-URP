using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanRenderer))]
    public class OceanRenderingSettingsPropertyDrawer : Editor
    {
        private const string RequirementTextures = "Depth Texture and Opaque Texture must " +
            "be enabled in the pipeline asset.";
        private const string RequirementDownsampling = "Opaque downsampling must be None.";

        private SerializedProperty _settings;
        private SerializedProperty _skyMapResolution;
        private SerializedProperty _updateSkyMap;
        private SerializedProperty _transparency;
        private SerializedProperty _underwater;


        private void OnEnable()
        {
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_skyMapResolution);
            EditorGUILayout.PropertyField(_updateSkyMap);
            EditorGUILayout.PropertyField(_transparency);
            EditorGUILayout.PropertyField(_underwater);

            if (_transparency.boolValue || _underwater.boolValue)
            {
                string message = RequirementTextures;
                if (_underwater.boolValue)
                    message += " " + RequirementDownsampling;
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
            _settings = serializedObject.FindProperty("_settings");
            _skyMapResolution = _settings.FindPropertyRelative("skyMapResolution");
            _updateSkyMap = _settings.FindPropertyRelative("updateSkyMap"); ;
            _transparency = _settings.FindPropertyRelative("transparency"); ;
            _underwater = _settings.FindPropertyRelative("underwaterEffect"); ;
        }
    }
}
