using MarkupAttributes.Editor;
using UnityEditor;
using UnityEngine;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(OceanSimulationInputsProvider))]
    public class OceanSimulationInputsProviderEditor : MarkedUpEditor
    {
        private WavesScaleEditorWindow _scaleEditorWindow;

        protected override void OnInitialize()
        {
            AddCallback(serializedObject.FindProperty("_defaultEqualizer"),
                CallbackEvent.AfterProperty, DrawEditLocalWavesButton);
        }

        protected override void OnCleanup()
        {
            if (_scaleEditorWindow != null)
                _scaleEditorWindow.Close();
        }

        private void DrawEditLocalWavesButton(SerializedProperty property)
        {
            if (serializedObject.FindProperty("_mode").enumValueIndex > 0)
            {
                if (GUILayout.Button("Edit Local Waves"))
                {
                    _scaleEditorWindow = WavesScaleEditorWindow.Open(serializedObject.FindProperty("_localWavesArray"));
                }
            }
        }
    }
}
