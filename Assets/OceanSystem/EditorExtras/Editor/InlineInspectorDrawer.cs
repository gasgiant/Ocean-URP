using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    [CustomPropertyDrawer(typeof(InlineInspectorAttribute))]
    public class InlineInspectorDrawer : PropertyDrawer
    {
        private object targetObject;
        private Type targetObjectType;
        private SerializedObject serializedObject;
        private SerializedProperty[] children;
        private FieldInfo[] fields;
        private InlineInspectorAttribute inlineInspectorAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (targetObject != null && property.isExpanded)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    SerializedProperty prop = children[i];
                    if (prop.name == "m_Script") continue;
                    if (AttributesHandler.IsPropertyHidden(fields[i], targetObject)) continue;

                    height += EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            if (targetObject == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (IsMonoBehaviour(targetObjectType) || IsScriptableObject(targetObjectType))
            {
                var indentedPosition = EditorGUI.IndentedRect(position);
                var indentOffset = indentedPosition.x - position.x;
                var propertyRect = new Rect(position.x + (EditorGUIUtility.labelWidth - indentOffset + verticalSpacing),
                    position.y, position.width - (EditorGUIUtility.labelWidth - indentOffset), lineHeight);
                EditorGUI.ObjectField(propertyRect, property, targetObjectType, GUIContent.none);
            }

            if (property.isExpanded)
            {
                if (inlineInspectorAttribute.box)
                    GUI.Box(new Rect(0, position.y + lineHeight + verticalSpacing - 1,
                        Screen.width, position.height - lineHeight - verticalSpacing), "");

                if (inlineInspectorAttribute.indent)
                    EditorGUI.indentLevel += 1;

                float y = position.y + lineHeight + verticalSpacing;
                for (int i = 0; i < children.Length; i++)
                {
                    SerializedProperty prop = children[i];
                    if (prop.name == "m_Script") continue;
                    float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                    DrawChildProperty(new Rect(position.x, y, position.width, height), prop, fields[i]);
                    y += height + EditorGUIUtility.standardVerticalSpacing;
                }

                if (inlineInspectorAttribute.indent)
                    EditorGUI.indentLevel -= 1;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        private void DrawChildProperty(Rect position, SerializedProperty prop, FieldInfo fieldInfo)
        {
            if (!AttributesHandler.IsPropertyHidden(fieldInfo, targetObject))
            {
                EditorGUI.PropertyField(position, prop, true);
            }
        }

        private void Initialize(SerializedProperty property)
        {
            inlineInspectorAttribute = attribute as InlineInspectorAttribute;
            if (targetObject != null) return;

            targetObject = ExtendedEditorUtils.GetTargetObjectOfProperty(property);
            if (targetObject == null) return;

            targetObjectType = targetObject.GetType();

            bool isMonoBehaviour = IsMonoBehaviour(targetObjectType);
            bool isScriptableObject = IsScriptableObject(targetObjectType);

            if (isMonoBehaviour || isScriptableObject)
            {
                serializedObject = new SerializedObject(property.objectReferenceValue);
                children = ExtendedEditorUtils.GetSerializedObjectProps(serializedObject);
            }
            else
            {
                serializedObject = property.serializedObject;
                children = ExtendedEditorUtils.GetChildrenProperties(property);
            }

            fields = new FieldInfo[children.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = targetObjectType.GetField(children[i].name);
            }
        }

        private bool IsMonoBehaviour(Type type) => typeof(MonoBehaviour).IsAssignableFrom(type);
        private bool IsScriptableObject(Type type) => typeof(ScriptableObject).IsAssignableFrom(type);
    }
}
