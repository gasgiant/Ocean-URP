using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    [CustomPropertyDrawer(typeof(ExtendedPropertyAttribute))]
    public class ExtendedDrawer : PropertyDrawer
    {
        private object targetObject;
        private Type targetObjectType;
        private SerializedObject serializedObject;
        private SerializedProperty[] children;
        private FieldInfo[] fields;
        private ExtendedPropertyAttribute settings;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            float height = 0;
            if (!settings.stripped)
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (property.isExpanded || settings.stripped)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    SerializedProperty prop = children[i];
                    if (prop.name == "m_Script") continue;
                    if (!ConditionalAttributesUtility.IsPropertyVisible(fields[i], targetObject)) continue;

                    height += EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize(property);
            EditorGUI.BeginChangeCheck();
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;

            if (!settings.stripped)
            {
                Rect foldoutRect = position;
                foldoutRect.height = lineHeight;
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            }

            if (property.isExpanded || settings.stripped)
            {
                int indent = settings.stripped ? 0 : 1;
                using (new EditorGUI.IndentLevelScope(indent))
                {
                    float y = position.y;
                    if (!settings.stripped)
                        y += lineHeight + verticalSpacing;
                    for (int i = 0; i < children.Length; i++)
                    {
                        SerializedProperty prop = children[i];
                        if (prop.name == "m_Script") continue;
                        float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                        if (DrawChildProperty(new Rect(position.x, y, position.width, height), prop, fields[i]))
                            y += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private bool DrawChildProperty(Rect position, SerializedProperty prop, FieldInfo fieldInfo)
        {
            bool visible = false;

            using (new EditorGUI.DisabledScope(!ConditionalAttributesUtility.IsPropertyEnabled(fieldInfo, targetObject)))
            {
                if (ConditionalAttributesUtility.IsPropertyVisible(fieldInfo, targetObject))
                {
                    EditorGUI.PropertyField(position, prop, true);
                    visible = true;
                }
            }

            return visible;
        }

        private void Initialize(SerializedProperty property)
        {
            targetObject = ExtraEditorUtils.GetTargetObjectOfProperty(property);
            if (targetObjectType != null) return;
            targetObjectType = targetObject.GetType();
            settings = attribute as ExtendedPropertyAttribute;
            serializedObject = property.serializedObject;
            children = ExtraEditorUtils.GetChildrenProperties(property);
            fields = new FieldInfo[children.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = targetObjectType.GetField(children[i].name, ExtraEditorUtils.DefaultBindingFlags);
            }
        }
    }
}
