using EditorExtras.Editor;
using UnityEditor;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(OceanSimulationSettings))]
    public class OceanSimulationSettingsEditor : ExtendedEditor
    {
        private const string ShowPlotPrefName = "OceanSimulationSettingsShowPlot";

        private OceanSimulationSettings _simulationSettings;
        private OceanSimulationInputs _simulationInputs = new OceanSimulationInputs();

        private void OnEnable()
        {
            InitializeExtendedInspector();
            _simulationSettings = (OceanSimulationSettings)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawExtendedInspector();

            EditorGUILayout.Space();
            bool showPlot = EditorGUILayout.Toggle("Show Plot", EditorPrefs.GetBool(ShowPlotPrefName));
            EditorPrefs.SetBool(ShowPlotPrefName, showPlot);
            if (showPlot)
            {
                SerializedProperty _displaySpectrum = serializedObject.FindProperty("_displaySpectrum");
                EditorGUILayout.PropertyField(_displaySpectrum);
                OceanSimulationInputsProvider inputsProvider = _displaySpectrum.objectReferenceValue as OceanSimulationInputsProvider;
                if (inputsProvider != null)
                {
                    inputsProvider.PopulateInputs(_simulationInputs);
                    SpectrumPlotter.DrawGraphWithCascades(_simulationSettings, _simulationInputs);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
