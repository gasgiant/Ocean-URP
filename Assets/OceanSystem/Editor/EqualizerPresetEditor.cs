using UnityEngine;
using UnityEditor;
using MarkupAttributes.Editor;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(EqualizerPreset))]
    public class EqualizerPresetEditor : UnityEditor.Editor
    {
        private const string ActiveTab = "OceanEqualizerActiveTab";
        private readonly string[] Tabs = new string[] { "Scale", "Chop" };
        private static readonly string[] _filterTypeNames = { "Bell ", "Hight Shelf ", "Low Shelf "};

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
        private static readonly GUILayoutOption _miniButtonHeight = GUILayout.Height(20);

        private MarkupGUI.GroupsStack _groupsStack = new MarkupGUI.GroupsStack();


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
            _groupsStack.Clear();

            if (!MarkupGUI.IsInsideInlineEditor)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                }

                EditorGUILayout.PropertyField(_displaySpectrum);
                EditorGUILayout.Space();
            }

            

            OceanSimulationInputsProvider inputsProvider = _displaySpectrum.objectReferenceValue as OceanSimulationInputsProvider;
            OceanSimulationInputs inputs = null;
            if (inputsProvider != null)
            {
                inputsProvider.PopulateInputs(_simulationInputs);
                inputs = _simulationInputs;
            }

            int activeTab = EditorPrefs.GetInt(ActiveTab);
            _groupsStack += MarkupGUI.BeginTabsGroup(ref activeTab, Tabs);
            EditorPrefs.SetInt(ActiveTab, activeTab);

            if (activeTab == 0)
            {
                SpectrumPlotter.DrawSpectrumWithEqualizer(inputs,
                    _oceanEqualizerPreset.GetRamp(), 0, _scaleFill, _scaleLine);
                ShowFiltersArray(_scaleFilters);
            }

            if (activeTab == 1)
            {
                SpectrumPlotter.DrawSpectrumWithEqualizer(inputs,
                   _oceanEqualizerPreset.GetRamp(), 1, _chopFill, _chopLine);
                ShowFiltersArray(_chopFilters);
            }

            _groupsStack.EndAll();

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowFiltersArray(SerializedProperty filters)
        {
            for (int i = 0; i < filters.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                SerializedProperty prop = filters.GetArrayElementAtIndex(i);
                int type = prop.FindPropertyRelative("type").enumValueIndex;

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.IndentLevelScope(EditorGUIUtility.hierarchyMode ? 1 : 0))
                {
                    prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, _filterTypeNames[type], true);
                }
                ShowMiniButtons(filters, i);
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel += 1;
                if (i < filters.arraySize)
                {
                    if (prop.isExpanded)
                    {
                        var enumerator = prop.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            var current = enumerator.Current as SerializedProperty;
                            EditorGUILayout.PropertyField(current);
                        }
                    }
                }
                EditorGUI.indentLevel -= 1;


                EditorGUILayout.Space(0.5f);
                EditorGUILayout.EndVertical();
                
            }

            if (GUILayout.Button("Add filter", EditorStyles.miniButton))
            {
                filters.arraySize += 1;
            }
        }

        private void ShowMiniButtons(SerializedProperty list, int index)
        {
            if (GUILayout.Button(_duplicateButtonContent, EditorStyles.miniButton, _miniButtonWidth, _miniButtonHeight))
            {
                list.InsertArrayElementAtIndex(index);
            }
            if (GUILayout.Button(_deleteButtonContent, EditorStyles.miniButton, _miniButtonWidth, _miniButtonHeight))
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
