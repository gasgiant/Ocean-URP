using System.Collections.Generic;
using EditorExtras.Editor;
using UnityEditor;
using UnityEngine;

namespace OceanSystem.Editor
{
    public class WavesScaleEditorWindow : EditorWindow
    {
        private const string WindForceProperty = "_windForce";

        private SerializedProperty _presetsArray;
        private string _assetPath;
        private int _selectedPresetIndex = -1;
        private Dictionary<Object, UnityEditor.Editor> _editors = 
            new Dictionary<Object, UnityEditor.Editor>();
        private Vector2 _scrollPositionSidebar;
        private Vector2 _scrollPositionMain;
        private List<LocalWavesPreset> listForSorting = new List<LocalWavesPreset>();
        private float _maxWindForce = -1;

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

            if (_maxWindForce < 0)
                SortArray();

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
                SerializedProperty property = _presetsArray.GetArrayElementAtIndex(_selectedPresetIndex);
                WindForceField(property);
                EditorGUILayout.Space();
                bool drawScript = ExtraEditorGUI.DrawScriptProperty;
                ExtraEditorGUI.DrawScriptProperty = false;
                GetEditor(property).OnInspectorGUI();
                ExtraEditorGUI.DrawScriptProperty = drawScript;
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndScrollView();

            if (_selectedPresetIndex >= 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(150)))
                {
                    RemovePreset();
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
                string label = CreateLabel(GetPresetAtIndex(i).WindForce);
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
                AddPreset();
            }
        }

        private void WindForceField(SerializedProperty property)
        {
            var preset = property.objectReferenceValue as LocalWavesPreset;
            EditorGUI.BeginChangeCheck();
            float value = EditorGUILayout.DelayedFloatField("Wind Force", preset.WindForce);
            value = Mathf.Max(0, value);
            if (EditorGUI.EndChangeCheck())
            {
                SetWindForce(preset, value);
                SortArray();
                AssetDatabase.SaveAssets();
            }
        }

        private UnityEditor.Editor GetEditor(SerializedProperty property)
        {
            UnityEditor.Editor editor = null;
            if (_editors.ContainsKey(property.objectReferenceValue))
                editor = _editors[property.objectReferenceValue];

            UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, 
                null, ref editor);
            return editor;
        }

        private void SortArray()
        {
            if (_presetsArray.arraySize <= 0)
            {
                _maxWindForce = -1;
            }

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

            _maxWindForce = -1;
            for (int i = 0; i < _presetsArray.arraySize; i++)
            {
                if (listForSorting[i].WindForce > _maxWindForce)
                    _maxWindForce = listForSorting[i].WindForce;
                if (listForSorting[i] == selected)
                    _selectedPresetIndex = i;
                listForSorting[i].name = CreateName(listForSorting[i].WindForce);
                var prop = _presetsArray.GetArrayElementAtIndex(i);
                prop.objectReferenceValue = listForSorting[i];
            }

            _presetsArray.serializedObject.ApplyModifiedProperties();
        }

        private LocalWavesPreset GetPresetAtIndex(int i)
        {
            return _presetsArray.GetArrayElementAtIndex(i).objectReferenceValue
                    as LocalWavesPreset;
        }

        private void AddPreset()
        {
            LocalWavesPreset localWaves = CreateInstance<LocalWavesPreset>();
            _maxWindForce += 1;
            SetWindForce(localWaves, _maxWindForce);
            
            Undo.RegisterCreatedObjectUndo(localWaves, $"Create {typeof(LocalWavesPreset).Name} Asset");
            AddObjectToAsset(localWaves, _assetPath, _presetsArray);
        }

        private void RemovePreset()
        {
            var preset = GetPresetAtIndex(_selectedPresetIndex);
            _editors.Remove(preset);
            RemoveObjectFromAsset(_selectedPresetIndex, _assetPath, _presetsArray);
            SortArray();
        }

        private void SetWindForce(LocalWavesPreset localWaves, float windForce)
        {
            SerializedObject so = new SerializedObject(localWaves);
            so.FindProperty(WindForceProperty).floatValue = windForce;
            so.ApplyModifiedProperties();
            localWaves.name = CreateName(windForce);
        }

        private string CreateName(float windForce)
        {
            return _presetsArray.serializedObject.targetObject.name + " | Wind Force " 
                + windForce.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private string CreateLabel(float windForce)
        {
            return "Wind Force "
                + windForce.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
