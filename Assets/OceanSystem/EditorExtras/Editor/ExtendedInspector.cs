using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    public class ExtendedInspector : UnityEditor.Editor
    {
        private Type targetType;
        private SerializedProperty[] props;
        private FieldInfo[] fields;

        private bool foldoutOpen;
        private BeginFoldoutGroupAttribute currentFoldout;

        private void OnEnable()
        {
            targetType = target.GetType();
            CacheProps();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].name.Equals("m_Script", StringComparison.Ordinal))
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(props[i]);
                    GUI.enabled = true;
                }
                else
                {
                    HandleProperty(props[i], fields[i]);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void CacheProps()
        {
            props = ExtendedEditorUtils.GetSerializedObjectProps(serializedObject);
            fields = new FieldInfo[props.Length];

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = targetType.GetField(props[i].name);
            }
        }

        private void HandleProperty(SerializedProperty prop, FieldInfo fieldInfo)
        {
            BeginFoldoutGroupAttribute beginFoldoutGroup = fieldInfo.GetCustomAttribute<BeginFoldoutGroupAttribute>();
            EndFoldoutGroupAttribute endFoldoutGroup = fieldInfo.GetCustomAttribute<EndFoldoutGroupAttribute>();
            HideIfAttribute hideIf = fieldInfo.GetCustomAttribute<HideIfAttribute>();

            if (beginFoldoutGroup != null)
            {
                string prefName = targetType.Name + beginFoldoutGroup.name;
                foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(
                    EditorPrefs.GetBool(prefName), beginFoldoutGroup.name);
                EditorPrefs.SetBool(prefName, foldoutOpen);
                if (beginFoldoutGroup.indent) EditorGUI.indentLevel += 1;
                currentFoldout = beginFoldoutGroup;

            }

            if (currentFoldout == null || foldoutOpen)
            {
                if (!AttributesHandler.IsPropertyHidden(fieldInfo, target))
                    EditorGUILayout.PropertyField(prop);
            }

            if (currentFoldout != null && endFoldoutGroup != null)
            {
                if (currentFoldout.indent) EditorGUI.indentLevel -= 1;
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (currentFoldout.endSpace && foldoutOpen) EditorGUILayout.Space();
                currentFoldout = null;
            }
        }
    }
}
