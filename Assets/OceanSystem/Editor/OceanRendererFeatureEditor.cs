using EditorExtras.Editor;
using System.Threading.Tasks;
using UnityEditor;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(OceanRendererFeature))]
    public class OceanRendererFeatureEditor : ExtendedEditor
    {
        private const string RequirementTextures = "Depth Texture and Opaque Texture must " +
            "be enabled in the pipeline asset.";
        private const string RequirementDownsampling = "Opaque downsampling must be None.";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawExtendedInspector(false);

            SerializedProperty _settings = serializedObject.FindProperty("_settings");
            SerializedProperty _transparency = _settings.FindPropertyRelative("transparency");
            SerializedProperty _underwater = _settings.FindPropertyRelative("underwaterEffect");

            if (_transparency.boolValue || _underwater.boolValue)
            {
                string message = RequirementTextures;
                if (_underwater.boolValue)
                    message += " " + RequirementDownsampling;
                EditorGUILayout.HelpBox(message, MessageType.Info, true);
            }

            EditorGUILayout.Space();
            bool newValue = EditorGUILayout.Toggle("Render In Edit Mode",
                EditorPrefs.GetBool(OceanRendererFeature.RenderInEditModePrefName));
            OceanRendererFeature.RenderInEditMode = newValue;
            EditorPrefs.SetBool(OceanRendererFeature.RenderInEditModePrefName, newValue);

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
    }
}