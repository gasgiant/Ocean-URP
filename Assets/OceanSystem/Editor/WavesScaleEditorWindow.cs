using System.Collections.Generic;
using EditorExtras.Editor;
using UnityEditor;
using UnityEngine;

namespace OceanSystem.Editor
{
    public class WavesScaleEditorWindow : EditorWindow
    {
        private SerializedProperty _presetsArray;
        private string _assetPath;
        private int _selectedPresetIndex = -1;
        private Dictionary<SerializedProperty, UnityEditor.Editor> _editors = 
            new Dictionary<SerializedProperty, UnityEditor.Editor>();
        private Vector2 _scrollPositionSidebar;
        private Vector2 _scrollPositionMain;
        private List<LocalWavesPreset> listForSorting = new List<LocalWavesPreset>();

        public static WavesScaleEditorWindow Open(SerializedProperty presetsArray)
        {
            var window = GetWindow<WavesScaleEditorWindow>("Waves Scale");
            window._presetsArray = presetsArray;
            window._assetPath = AssetDatabase.GetAssetPath(
                presetsArray.serializedObject.targetObject);
            return window;
        }

        private void OnGUI()
        {
            if (_presetsArray.serializedObject == null)
            {
                Close();
                return;
            }

            //if (_presetsArray.arraySize > 0)
            //    SortArray();
            if (_selectedPresetIndex >= _presetsArray.arraySize)
                _selectedPresetIndex = -1;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), 
                GUILayout.ExpandHeight(true));
            DrawSidebar();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            _scrollPositionMain = EditorGUILayout.BeginScrollView(_scrollPositionMain);
            if (_selectedPresetIndex >= 0)
            {
                bool drawScript = ExtraEditorGUI.DrawScriptProperty;
                ExtraEditorGUI.DrawScriptProperty = false;
                GetEditor(_presetsArray.GetArrayElementAtIndex(_selectedPresetIndex)).OnInspectorGUI();
                ExtraEditorGUI.DrawScriptProperty = drawScript;
            }
            EditorGUILayout.EndScrollView();

            if (_selectedPresetIndex >= 0)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(150)))
                {
                    RemoveObjectFromAsset(_selectedPresetIndex, _assetPath, _presetsArray);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            _scrollPositionSidebar = EditorGUILayout.BeginScrollView(_scrollPositionSidebar);
            for (int i = 0; i < _presetsArray.arraySize; i++)
            {
                var preset = GetPresetAtIndex(i);
                string label = "Wind Force " + preset.WindForce;
                if (i == _selectedPresetIndex)
                {
                    GUILayout.Toggle(true, label, GUI.skin.button);
                }
                else
                {
                    if (GUILayout.Button(label))
                    {
                        _selectedPresetIndex = i;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                AddItem();
            }
        }

        private UnityEditor.Editor GetEditor(SerializedProperty property)
        {
            if (_editors.ContainsKey(property))
                return _editors[property];
            return UnityEditor.Editor.CreateEditor(property.objectReferenceValue);
        }

        private void SortArray()
        {
            listForSorting.Clear();
            LocalWavesPreset selected = null;
            for (int i = 0; i < _presetsArray.arraySize; i++)
            {
                var preset = GetPresetAtIndex(i);
                listForSorting.Add(preset);
                if (i == _selectedPresetIndex)
                    selected = preset;
            }
            listForSorting.Sort((p0, p1) => p0.WindForce.CompareTo(p1.WindForce));

            for (int i = 0; i < _presetsArray.arraySize; i++)
            {
                if (listForSorting[i] == selected)
                    _selectedPresetIndex = i;
                _presetsArray.GetArrayElementAtIndex(i).objectReferenceValue = listForSorting[i];
            }

            _presetsArray.serializedObject.ApplyModifiedProperties();
        }

        private LocalWavesPreset GetPresetAtIndex(int i)
        {
            return _presetsArray.GetArrayElementAtIndex(i).objectReferenceValue
                    as LocalWavesPreset;
        }

        private void AddItem()
        {
            LocalWavesPreset localWaves = CreateInstance<LocalWavesPreset>();
            localWaves.name = $"Child {_presetsArray.arraySize}";
            Undo.RegisterCreatedObjectUndo(localWaves, $"Create {typeof(LocalWavesPreset).Name} Asset");
            AddObjectToAsset(localWaves, _assetPath, _presetsArray);
        }

        private static void AddObjectToAsset(Object asset, string path, SerializedProperty list)
        {
            AssetDatabase.AddObjectToAsset(asset, path);
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = asset;
            list.serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        public static void RemoveObjectFromAsset(int index, string path, SerializedProperty list)
        {
            SerializedProperty property = list.GetArrayElementAtIndex(index);
            Object asset = property.objectReferenceValue;

            if (asset)
            {
                AssetDatabase.RemoveObjectFromAsset(asset);
                property.objectReferenceValue = default;
                list.DeleteArrayElementAtIndex(index);
                list.serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}
