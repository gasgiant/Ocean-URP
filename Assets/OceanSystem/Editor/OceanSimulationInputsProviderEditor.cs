using EditorExtras.Editor;
using UnityEditor;
using UnityEngine;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(OceanSimulationInputsProvider))]
    public class OceanSimulationInputsProviderEditor : ExtendedEditor
    {
        private WavesScaleEditorWindow _scaleEditorWindow;

        private void OnEnable()
        {
            InitializeExtendedInspector();
        }

        private void OnDisable()
        {
            if (_scaleEditorWindow != null)
                _scaleEditorWindow.Close();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawExtendedInspector();
            if (serializedObject.FindProperty("_mode").enumValueIndex > 0)
            {
                if (GUILayout.Button("Edit Local Waves"))
                {
                    _scaleEditorWindow = WavesScaleEditorWindow.Open(serializedObject.FindProperty("_localWavesPresets"));
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
