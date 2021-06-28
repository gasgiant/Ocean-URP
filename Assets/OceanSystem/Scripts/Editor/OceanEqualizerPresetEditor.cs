using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanEqualizerPreset))]
    public class OceanEqualizerPresetEditor : Editor
    {
        OceanEqualizerPreset oceanEqualizerPreset;

        SerializedProperty scaleFilters;
        SerializedProperty chopFilters;
        SerializedProperty DisplayWavesSettings;

        static Color scaleFill = new Color32(54, 98, 160, 255);
        static Color scaleLine = new Color32(129, 180, 254, 255);

        static Color chopFill = (Color)(new Color32(252, 109, 64, 255)) * 0.8f;
        static Color chopLine = new Color32(252, 109, 64, 255);

        private static GUIContent
            duplicateButtonContent = new GUIContent("+", "duplicate"),
            deleteButtonContent = new GUIContent("-", "delete");
        private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

        private void OnEnable()
        {
            oceanEqualizerPreset = (OceanEqualizerPreset)target;
            scaleFilters = serializedObject.FindProperty("scaleFilters");
            chopFilters = serializedObject.FindProperty("chopFilters");
            DisplayWavesSettings = serializedObject.FindProperty("DisplayWavesSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromScriptableObject(oceanEqualizerPreset), typeof(OceanEqualizerPreset), false);
            GUI.enabled = true;

            oceanEqualizerPreset.BakeRamp();

            EditorGUILayout.PropertyField(DisplayWavesSettings);
            oceanEqualizerPreset.showScale = EditorGUILayout.BeginFoldoutHeaderGroup(oceanEqualizerPreset.showScale, "Scale");
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (oceanEqualizerPreset.showScale)
            {
                EditorGUILayout.Space();
                SpectrumPlotter.DrawSpectrumWithEqualizer(
                    oceanEqualizerPreset.DisplayWavesSettings,
                    oceanEqualizerPreset.Ramp, 0, scaleFill, scaleLine);
                ShowFiltersArray(scaleFilters);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            oceanEqualizerPreset.showChop = EditorGUILayout.BeginFoldoutHeaderGroup(oceanEqualizerPreset.showChop, "Chop");
            if (oceanEqualizerPreset.showChop)
            {
                EditorGUILayout.Space();
                SpectrumPlotter.DrawSpectrumWithEqualizer(
                   oceanEqualizerPreset.DisplayWavesSettings,
                   oceanEqualizerPreset.Ramp, 1, chopFill, chopLine);
                ShowFiltersArray(chopFilters);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        void ShowFiltersArray(SerializedProperty filters)
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

        void ShowMiniButtons(SerializedProperty list, int index)
        {
            if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth))
            {
                list.InsertArrayElementAtIndex(index);
            }
            if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
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
