using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    [CanEditMultipleObjects]
    public class ExtendedEditor : UnityEditor.Editor
    {
        private Type targetType;
        private SerializedProperty[] props;
        private FieldInfo[] fields;
        private InspectorLayoutController layoutController;
        private Dictionary<SerializedProperty, InlineEditorData> inlineEditors;

        private void OnEnable()
        {
            InitializeExtendedInspector();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawExtendedInspector();
            serializedObject.ApplyModifiedProperties();
        }

        protected void InitializeExtendedInspector()
        {
            targetType = target.GetType();
            CacheProps();
            layoutController = new InspectorLayoutController(targetType.Name,
                GetLayoutData(props, fields, target));
        }

        protected void DrawExtendedInspector(bool drawScriptProperty = true)
        {
            CreateInlineEditors();

            if (drawScriptProperty && ExtraEditorGUI.DrawScriptProperty)
                DrawScriptProp();

            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].name.Equals("m_Script"))
                {
                    continue;
                }
                else
                {
                    DrawProperty(i, props[i], fields[i]);
                }
            }
            layoutController.EndAll();
        }

        private void DrawScriptProp()
        {
            SerializedProperty prop = serializedObject.FindProperty("m_Script");
            if (prop != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(prop);
                }
            }
        }

        private void DrawProperty(int index, SerializedProperty prop, FieldInfo fieldInfo)
        {
            layoutController.BeforeProperty(index);

            if (layoutController.PropertyVisible(index) 
                && ConditionalAttributesUtility.IsPropertyVisible(fieldInfo, target))
            {
                using (new EditorGUI.DisabledScope(!(layoutController.ScopeEnabled
                    && ConditionalAttributesUtility.IsPropertyEnabled(fieldInfo, target))))
                {
                    if (inlineEditors.ContainsKey(prop))
                    {
                        InlineEditorData data = inlineEditors[prop];
                        ExtraEditorGUI.DrawEditorInline(prop, data.editor, data.stripped);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(prop);
                    }
                }
            }
        }

        private void CacheProps()
        {
            props = ExtraEditorUtils.GetSerializedObjectProperties(serializedObject);
            fields = new FieldInfo[props.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = targetType.GetField(props[i].name, ExtraEditorUtils.DefaultBindingFlags);
            }

            inlineEditors = new Dictionary<SerializedProperty, InlineEditorData>();
            for (int i = 0; i < props.Length; i++)
            {
                if (fields[i] == null) continue;
                if (props[i].propertyType == SerializedPropertyType.ObjectReference)
                {
                    var inline = fields[i].GetCustomAttribute<InlineEditorAttribute>();
                    if (inline != null)
                        inlineEditors.Add(props[i], new InlineEditorData(null, inline));
                }
            }
        }

        private PropertyLayoutData[] GetLayoutData(SerializedProperty[] props,
            FieldInfo[] fieldInfos, object targetObject)
        {
            var output = new PropertyLayoutData[props.Length];
            var hiddenProps = new HashSet<string>();

            for (int i = 0; i < fieldInfos.Length; i++)
            {
                if (fieldInfos[i] != null)
                {
                    var groupAttribues = fieldInfos[i].GetCustomAttributes<LayoutGroupAttribute>().ToArray();
                    var groups = new List<ExtraLayoutGroup>();
                    foreach (var groupAttribute in groupAttribues)
                    {
                        ConditionWrapper conditionWrapper = null;
                        if (groupAttribute.HasCondition)
                            conditionWrapper = new ConditionWrapper(groupAttribute.condition,
                                groupAttribute.conditionInverted, props, targetObject);
                        groups.Add(new ExtraLayoutGroup(groupAttribute, conditionWrapper));

                        if (groupAttribute.toggle)
                            hiddenProps.Add(groupAttribute.condition);
                    }

                    var end = fieldInfos[i].GetCustomAttribute<EndGroupAttribute>();
                    output[i] = new PropertyLayoutData(groups, end);
                }
                else
                    output[i] = null;
            }

            for (int i = 0; i < output.Length; i++)
            {
                if (hiddenProps.Contains(props[i].name))
                    output[i].usedByToggle = true;
            }

            return output;
        }

        private void CreateInlineEditors()
        {
            var props = new List<SerializedProperty>(inlineEditors.Keys);
            foreach (var prop in props)
            {
                var editor = inlineEditors[prop].editor;
                CreateCachedEditor(prop.objectReferenceValue,
                    null, ref editor);
                inlineEditors[prop].editor = editor;
            }
        }

        private class InlineEditorData
        {
            public UnityEditor.Editor editor;
            public bool stripped;

            public InlineEditorData(UnityEditor.Editor editor, InlineEditorAttribute attribute)
            {
                this.editor = editor;
                stripped = attribute.stripped;
            }
        }
    }
}
