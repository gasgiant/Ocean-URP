using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    public class ConditionWrapper
    {
        private readonly SerializedProperty serializedProperty;
        private readonly object targetObject;
        private readonly FieldInfo fieldInfo;
        private readonly PropertyInfo propertyInfo;
        private readonly MethodInfo methodInfo;
        
        private readonly MaterialProperty materialProperty;
        private readonly Material targetMaterial;
        private readonly string shaderKeyword;
        private readonly bool isShaderKeywordGlobal;
        private readonly bool inverted;

        public bool HasMixedValue
        {
            get
            {
                if (serializedProperty != null && serializedProperty.hasMultipleDifferentValues)
                    return true;
                if (materialProperty != null && materialProperty.hasMixedValue)
                {
                    return true;
                }
                return false;
            }
            
        }

        public ConditionWrapper(string condition, bool inverted,
            SerializedProperty[] serializedProps, object targetObject)
        {
            this.inverted = inverted;
            if (serializedProps != null)
                foreach (var serializedProperty in serializedProps)
                {
                    if (serializedProperty.propertyType == SerializedPropertyType.Boolean
                        && serializedProperty.name == condition)
                    {
                        this.serializedProperty = serializedProperty;
                        return;
                    }
                }

            if (targetObject != null)
            {
                this.targetObject = targetObject;
                Type type = targetObject.GetType();

                FieldInfo fieldInfo = type.GetField(condition, ExtraEditorUtils.DefaultBindingFlags);
                if (fieldInfo != null && fieldInfo.FieldType == typeof(bool))
                {
                    this.fieldInfo = fieldInfo;
                    return;
                }

                PropertyInfo propertyInfo = type.GetProperty(condition, ExtraEditorUtils.DefaultBindingFlags);
                if (propertyInfo != null && propertyInfo.PropertyType == typeof(bool))
                {
                    this.propertyInfo = propertyInfo;
                    return;
                }

                MethodInfo methodInfo = type.GetMethod(condition, ExtraEditorUtils.DefaultBindingFlags);
                if (methodInfo != null && methodInfo.ReturnType == typeof(bool)
                    && methodInfo.GetParameters().Length == 0)
                {
                    this.methodInfo = methodInfo;
                    return;
                }
            }
        }

        public ConditionWrapper(string condition, bool inverted,
            MaterialProperty[] materialProps, Material targetMaterial)
        {
            this.inverted = inverted;
            if (materialProps != null)
                foreach (var materialProperty in materialProps)
                {
                    if ((materialProperty.type == MaterialProperty.PropType.Float 
                        || materialProperty.type == MaterialProperty.PropType.Range) 
                        && materialProperty.name == condition)
                    {
                        this.materialProperty = materialProperty;
                        return;
                    }
                }

            if (targetMaterial != null)
            {
                this.targetMaterial = targetMaterial;
                string[] s = condition.Split(' ');
                if (s.Length < 2)
                {
                    shaderKeyword = condition;
                }
                else
                {
                    isShaderKeywordGlobal = s[0] == "G";
                    shaderKeyword = s[1];
                }
            }
        }

        public bool GetValue()
        {
            if (serializedProperty != null)
                return serializedProperty.boolValue ^ inverted;

            if (targetObject != null && fieldInfo != null)
                return (bool)fieldInfo.GetValue(targetObject) ^ inverted;

            if (targetObject != null && propertyInfo != null)
                return (bool)propertyInfo.GetValue(targetObject) ^ inverted;

            if (targetObject != null && methodInfo != null)
                return (bool)methodInfo.Invoke(targetObject, null) ^ inverted;

            if (materialProperty != null)
                return (materialProperty.floatValue > 0) ^ inverted;

            if (isShaderKeywordGlobal && shaderKeyword != null)
            {
                return Shader.IsKeywordEnabled(shaderKeyword) ^ inverted;
            }

            if (targetMaterial != null && shaderKeyword != null)
            {
                return targetMaterial.IsKeywordEnabled(shaderKeyword) ^ inverted;
            }

            return false;
        }

        public void SetValue(bool b, bool forceIfMixed)
        {
            if (serializedProperty != null)
            {
                if (!forceIfMixed && serializedProperty.hasMultipleDifferentValues)
                    return;
                serializedProperty.boolValue = b ^ inverted;
                return;
            }

            if (materialProperty != null)
            {
                if (!forceIfMixed && materialProperty.hasMixedValue)
                    return;
                materialProperty.floatValue = (b ^ inverted) ? 1 : 0;
                return;
            }

            if (isShaderKeywordGlobal && shaderKeyword != null)
            {
                if (b ^ inverted)
                    Shader.EnableKeyword(shaderKeyword);
                else
                    Shader.DisableKeyword(shaderKeyword);
                return;
            }

            if (targetMaterial != null && shaderKeyword != null)
            {
                if (b ^ inverted)
                    targetMaterial.EnableKeyword(shaderKeyword);
                else
                    targetMaterial.DisableKeyword(shaderKeyword);
                return;
            }
        }
    }
}
