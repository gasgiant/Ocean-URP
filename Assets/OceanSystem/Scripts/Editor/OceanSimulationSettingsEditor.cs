using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanSimulationSettings))]
    public class OceanSimulationSettingsEditor : Editor
    {
        SerializedProperty resolution;
        SerializedProperty cascadesNumber;
        SerializedProperty simulateFoam;
        SerializedProperty domainsMode;
        SerializedProperty simulationScale;
        SerializedProperty allowOverlap;
        SerializedProperty c0Scale;
        SerializedProperty c1Scale;
        SerializedProperty c2Scale;
        SerializedProperty c3Scale;
        SerializedProperty minWavesInCascade;
        SerializedProperty anisoLevel;
        SerializedProperty readbackCascades;
        SerializedProperty samplingIterations;

        SerializedProperty showSpectrumGraph;

        OceanSimulationSettings simulationSettings;

        private void OnEnable()
        {
            simulationSettings = (OceanSimulationSettings)target;

            resolution = serializedObject.FindProperty("resolution");
            cascadesNumber = serializedObject.FindProperty("cascadesNumber");
            simulateFoam = serializedObject.FindProperty("simulateFoam");
            domainsMode = serializedObject.FindProperty("domainsMode");
            simulationScale = serializedObject.FindProperty("simulationScale");
            allowOverlap = serializedObject.FindProperty("allowOverlap");
            c0Scale = serializedObject.FindProperty("c0Scale");
            c1Scale = serializedObject.FindProperty("c1Scale");
            c2Scale = serializedObject.FindProperty("c2Scale");
            c3Scale = serializedObject.FindProperty("c3Scale");
            minWavesInCascade = serializedObject.FindProperty("minWavesInCascade");
            anisoLevel = serializedObject.FindProperty("anisoLevel");

            readbackCascades = serializedObject.FindProperty("readbackCascades");
            samplingIterations = serializedObject.FindProperty("samplingIterations");

            showSpectrumGraph = serializedObject.FindProperty("showSpectrumGraph");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(simulationSettings), typeof(OceanSimulationSettings), false);
            GUI.enabled = true;

            if (Application.isPlaying)
                GUI.enabled = false;
            EditorGUILayout.PropertyField(resolution);
            EditorGUILayout.PropertyField(cascadesNumber);
            EditorGUILayout.PropertyField(anisoLevel);
            GUI.enabled = true;
            EditorGUILayout.PropertyField(simulateFoam);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Physics Readback", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(readbackCascades);
            if (simulationSettings.readbackCascades != OceanSimulationSettings.ReadbackCascadesValue.None)
                EditorGUILayout.PropertyField(samplingIterations);
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Cascade Domains", EditorStyles.boldLabel);
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(domainsMode);
            if (simulationSettings.domainsMode == OceanSimulationSettings.CascadeDomainsMode.Auto)
            {
                EditorGUILayout.PropertyField(simulationScale);
            }
            else
            {
                EditorGUILayout.PropertyField(allowOverlap);
                EditorGUILayout.PropertyField(minWavesInCascade);
                EditorGUILayout.PropertyField(c0Scale);
                EditorGUILayout.PropertyField(c1Scale);
                if (simulationSettings.CascadesNumber > 2)
                    EditorGUILayout.PropertyField(c2Scale);
                if (simulationSettings.CascadesNumber > 3)
                    EditorGUILayout.PropertyField(c3Scale);
            }
            EditorGUI.indentLevel -= 1;

            EditorGUILayout.Separator();
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.PropertyField(showSpectrumGraph);
            if (simulationSettings.showSpectrumGraph)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DisplayWavesSettings"));
                if (simulationSettings.DisplayWavesSettings != null)
                    SpectrumGraphPlotter.DrawGraphWithCascades(simulationSettings);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
