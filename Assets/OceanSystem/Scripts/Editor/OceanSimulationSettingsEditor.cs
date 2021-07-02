using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanSimulationSettings))]
    public class OceanSimulationSettingsEditor : Editor
    {
        private const string ShowPlot = "OceanSimulationSettingsShowPlot";

        private SerializedProperty _resolution;
        private SerializedProperty _cascadesNumber;
        private SerializedProperty _simulateFoam;
        private SerializedProperty _updateSpectrum;
        private SerializedProperty _domainsMode;
        private SerializedProperty _simulationScale;
        private SerializedProperty _allowOverlap;
        private SerializedProperty _c0Scale;
        private SerializedProperty _c1Scale;
        private SerializedProperty _c2Scale;
        private SerializedProperty _c3Scale;
        private SerializedProperty _minWavesInCascade;
        private SerializedProperty _anisoLevel;
        private SerializedProperty _readbackCascades;
        private SerializedProperty _samplingIterations;
        private SerializedProperty _displayWavesSettings;

        private OceanSimulationSettings _simulationSettings;

        private void OnEnable()
        {
            _simulationSettings = (OceanSimulationSettings)target;

            _resolution = serializedObject.FindProperty("_resolution");
            _cascadesNumber = serializedObject.FindProperty("_cascadesNumber");
            _simulateFoam = serializedObject.FindProperty("_simulateFoam");
            _updateSpectrum = serializedObject.FindProperty("_updateSpectrum");
            _domainsMode = serializedObject.FindProperty("_domainsMode");
            _simulationScale = serializedObject.FindProperty("_simulationScale");
            _allowOverlap = serializedObject.FindProperty("_allowOverlap");
            _c0Scale = serializedObject.FindProperty("_c0Scale");
            _c1Scale = serializedObject.FindProperty("_c1Scale");
            _c2Scale = serializedObject.FindProperty("_c2Scale");
            _c3Scale = serializedObject.FindProperty("_c3Scale");
            _minWavesInCascade = serializedObject.FindProperty("_minWavesInCascade");
            _anisoLevel = serializedObject.FindProperty("_anisoLevel");
            _readbackCascades = serializedObject.FindProperty("_readbackCascades");
            _samplingIterations = serializedObject.FindProperty("_samplingIterations");

            _displayWavesSettings = serializedObject.FindProperty("_displayWavesSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(_simulationSettings), typeof(OceanSimulationSettings), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_resolution);
            EditorGUILayout.PropertyField(_cascadesNumber);
            EditorGUILayout.PropertyField(_anisoLevel);
            EditorGUILayout.PropertyField(_updateSpectrum);
            EditorGUILayout.PropertyField(_simulateFoam);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Physics Readback", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(_readbackCascades);
            if (_readbackCascades.enumValueIndex == (int)OceanSimulationSettings.ReadbackCascadesMode.None)
                EditorGUILayout.PropertyField(_samplingIterations);
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Cascade Domains", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(_domainsMode);
            if (_domainsMode.enumValueIndex == (int)OceanSimulationSettings.CascadeDomainsMode.Auto)
            {
                EditorGUILayout.PropertyField(_simulationScale);
            }
            else
            {
                EditorGUILayout.PropertyField(_allowOverlap);
                EditorGUILayout.PropertyField(_minWavesInCascade);
                EditorGUILayout.PropertyField(_c0Scale);
                EditorGUILayout.PropertyField(_c1Scale);
                if (_simulationSettings.CascadesNumber > 2)
                    EditorGUILayout.PropertyField(_c2Scale);
                if (_simulationSettings.CascadesNumber > 3)
                    EditorGUILayout.PropertyField(_c3Scale);
            }
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Space();
            EditorGUILayout.EndFoldoutHeaderGroup();
            bool showPlot = EditorGUILayout.Toggle("Show Plot", EditorPrefs.GetBool(ShowPlot));
            EditorPrefs.SetBool(ShowPlot, showPlot);
            if (showPlot)
            {
                EditorGUILayout.PropertyField(_displayWavesSettings);
                OceanWavesSettings wavesSettings = _displayWavesSettings.objectReferenceValue as OceanWavesSettings;
                if (wavesSettings != null)
                    SpectrumPlotter.DrawGraphWithCascades(_simulationSettings, wavesSettings);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
