using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
    [CustomEditor(typeof(EqualizerPreset))]
    public class OceanEqualizerPresetEditor : Editor
    {
        private const string ShowScale = "OceanEqualizerShowScale";
        private const string ShowChop = "OceanEqualizerShowChop";

        private EqualizerPreset _oceanEqualizerPreset;

        private SerializedProperty _scaleFilters;
        private SerializedProperty _chopFilters;
        private SerializedProperty _displaySpectrum;

        private OceanSimulationInputs _simulationInputs = new OceanSimulationInputs();

        private static readonly Color _scaleFill = new Color32(54, 98, 160, 255);
        private static readonly Color _scaleLine = new Color32(129, 180, 254, 255);

        private static readonly Color _chopFill = (Color)(new Color32(252, 109, 64, 255)) * 0.8f;
        private static readonly Color _chopLine = new Color32(252, 109, 64, 255);

        private static readonly GUIContent _duplicateButtonContent = new GUIContent("+", "duplicate");
        private static readonly GUIContent _deleteButtonContent = new GUIContent("-", "delete");
        private static readonly GUILayoutOption _miniButtonWidth = GUILayout.Width(20f);


        private void OnEnable()
        {
            _oceanEqualizerPreset = (EqualizerPreset)target;
            _scaleFilters = serializedObject.FindProperty("_scaleFilters");
            _chopFilters = serializedObject.FindProperty("_chopFilters");
            _displaySpectrum = serializedObject.FindProperty("_displaySpectrum");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(_oceanEqualizerPreset), typeof(EqualizerPreset), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_displaySpectrum);

            OceanSimulationInputsProvider inputsProvider = _displaySpectrum.objectReferenceValue as OceanSimulationInputsProvider;
            OceanSimulationInputs inputs = null;
            if (inputsProvider != null)
            {
                inputsProvider.PopulateInputs(_simulationInputs);
                inputs = _simulationInputs;
            }

            bool showScale = EditorGUILayout.BeginFoldoutHeaderGroup(EditorPrefs.GetBool(ShowScale), "Scale");
            EditorPrefs.SetBool(ShowScale, showScale);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (showScale)
            {
                EditorGUILayout.Space();
                
                SpectrumPlotter.DrawSpectrumWithEqualizer(inputs,
                    _oceanEqualizerPreset.GetRamp(), 0, _scaleFill, _scaleLine);
                ShowFiltersArray(_scaleFilters);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            bool showChop = EditorGUILayout.BeginFoldoutHeaderGroup(EditorPrefs.GetBool(ShowChop), "Chop");
            EditorPrefs.SetBool(ShowChop, showChop);
            if (showChop)
            {
                EditorGUILayout.Space();
                SpectrumPlotter.DrawSpectrumWithEqualizer(inputs,
                   _oceanEqualizerPreset.GetRamp(), 1, _chopFill, _chopLine);
                ShowFiltersArray(_chopFilters);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowFiltersArray(SerializedProperty filters)
        {
            for (int i = 0; i < filters.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Filter " + i.ToString(), EditorStyles.boldLabel);
                ShowMiniButtons(filters, i);
                EditorGUILayout.EndHorizontal();
                if (i < filters.arraySize)
                {
                    var enumerator = filters.GetArrayElementAtIndex(i).GetEnumerator();

                    EditorGUI.indentLevel += 1;
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current as SerializedProperty;
                        EditorGUILayout.PropertyField(current);
                    }
                    EditorGUI.indentLevel -= 1;
                    EditorGUILayout.Space();
                }
            }

            if (GUILayout.Button("Add filter", EditorStyles.miniButton))
            {
                filters.arraySize += 1;
            }
        }

        private void ShowMiniButtons(SerializedProperty list, int index)
        {
            if (GUILayout.Button(_duplicateButtonContent, EditorStyles.miniButtonLeft, _miniButtonWidth))
            {
                list.InsertArrayElementAtIndex(index);
            }
            if (GUILayout.Button(_deleteButtonContent, EditorStyles.miniButtonRight, _miniButtonWidth))
            {
                int oldSize = list.arraySize;
                list.DeleteArrayElementAtIndex(index);
                if (list.arraySize == oldSize)
                {
                    list.DeleteArrayElementAtIndex(index);
                }
            }
        }
    }
}
