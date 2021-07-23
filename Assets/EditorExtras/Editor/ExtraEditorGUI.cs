using System;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    public static class ExtraEditorGUI
    {
        public const float IndentWidth = 14;
        public const float FoldoutArrowWidth = 13;
        public static bool DrawScriptProperty { get; set; } = true;

        public static void DrawEditorInline(SerializedProperty property, 
            UnityEditor.Editor editor, bool stripped = false)
        {
            bool expanded = editor != null && !property.hasMultipleDifferentValues;
            bool hierarchyMode = EditorGUIUtility.hierarchyMode;

            if (!stripped)
            {
                EditorGUILayout.BeginVertical(ExtraEditorStyles.Box);
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.hierarchyMode = false;
                EditorGUIUtility.labelWidth = labelWidth;
                Rect headerRect = HeaderBase(BoxMode.Box);
                property.isExpanded = FoldoutWithObjectField(headerRect, property);
                expanded &= property.isExpanded;
                if (expanded)
                {
                    EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.1f);
                }
            }

            if (expanded)
            {
                bool drawScriptPrevious = DrawScriptProperty;
                DrawScriptProperty = false;
                editor.OnInspectorGUI();
                DrawScriptProperty = drawScriptPrevious;
            }

            if (!stripped)
            {
                EditorGUIUtility.hierarchyMode = hierarchyMode;
                EditorGUILayout.EndVertical();
                if (expanded)
                {
                    EditorGUILayout.Space();
                }
            }
        }

        public static Rect HeaderBase(BoxMode boxMode)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            if (boxMode == BoxMode.Box)
            {
                var padding = ExtraEditorStyles.Box.padding;
                Rect boxRect = padding.Add(rect);
                GUI.Box(boxRect, GUIContent.none, ExtraEditorStyles.FillBox);
            }
            return rect;
        }

        public static void HorizontalLine(float height = 1f)
        {
            HorizontalLine(height, EditorStyles.boldLabel.normal.textColor * 0.8f);
        }

        public static void HorizontalLine(float height, Color color)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(rect, color);
            EditorGUILayout.GetControlRect(false, height);
        }

        public static bool FoldoutWithObjectField(SerializedProperty property, float heightMultiplier = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(true, 
                heightMultiplier * EditorGUIUtility.singleLineHeight);
            bool res = FoldoutWithObjectField(rect, property, null, heightMultiplier);
            return res;
        }

        public static bool FoldoutWithObjectField(Rect position, 
            SerializedProperty property, GUIContent label = null, float heightMultiplier = 1)
        {
            if (label == null)
                label = new GUIContent(property.displayName, property.tooltip);
            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight * heightMultiplier);
            bool result = false;
            if (property.objectReferenceValue != null)
                result = property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true, ExtraEditorStyles.BoldFoldout);
            else
            {
                Rect rect = position;
                rect.x += FoldoutArrowWidth;
                rect.width -= FoldoutArrowWidth;
                EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
            }

            float xOffset = EditorGUIUtility.labelWidth - 1;
            var propertyRect = new Rect(position.x + xOffset,
                position.y, position.width - xOffset, EditorGUIUtility.singleLineHeight * heightMultiplier);
            EditorGUI.ObjectField(propertyRect, property, GUIContent.none);
            return result;
        }

        public struct SetIndentLevelScope : IDisposable
        {
            private readonly int storedIndentLevel;

            public SetIndentLevelScope(int indentLevel)
            {
                storedIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = indentLevel;
            }

            public void Dispose()
            {
                EditorGUI.indentLevel = storedIndentLevel;
            }
        }
    }
}
