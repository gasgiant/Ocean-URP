using System;
using System.Linq;
using System.Reflection;

namespace EditorExtras.Editor
{
    public static class ConditionalAttributesUtility
    {
        public enum Operation { And, Or }

        public static bool IsPropertyVisible(FieldInfo fieldInfo, object obj)
        {
            return !ConditionalAttributeValue<HideIfAttribute>(fieldInfo, obj, Operation.Or) &&
                ConditionalAttributeValue<ShowIfAttribute>(fieldInfo, obj, Operation.And);
        }

        public static bool IsPropertyEnabled(FieldInfo fieldInfo, object obj)
        {
            return !ConditionalAttributeValue<DisableIfAttribute>(fieldInfo, obj, Operation.Or)
                && ConditionalAttributeValue<EnableIfAttribute>(fieldInfo, obj, Operation.And);
        }

        private static bool ConditionalAttributeValue<T>(FieldInfo fieldInfo, object obj, Operation op) 
            where T : ConditionalAttribute
        {
            bool output = op == Operation.And;
            Type objType = obj.GetType();
            T[] attributes = fieldInfo.GetCustomAttributes<T>().ToArray();
            for (int i = 0; i < attributes.Length; i++)
            {
                bool value = false;

                PropertyInfo propInfo = objType.GetProperty(attributes[i].condition, ExtraEditorUtils.DefaultBindingFlags);
                if (propInfo != null)
                {
                    if (propInfo.PropertyType == typeof(bool))
                    {
                        value = (bool)propInfo.GetValue(obj) ^ attributes[i].invert;
                    }
                }
                else
                {
                    MethodInfo methodInfo = objType.GetMethod(attributes[i].condition, ExtraEditorUtils.DefaultBindingFlags);
                    if (methodInfo != null && methodInfo.ReturnType == typeof(bool) && methodInfo.GetParameters().Length == 0)
                    {
                        value = (bool)methodInfo.Invoke(obj, null) ^ attributes[i].invert;
                    }
                }

                if (op == Operation.Or)
                {
                    output |= value;
                    if (output) return true;
                }
                if (op == Operation.And)
                {
                    output &= value;
                    if (!output) return false;
                }
            }
            return output;
        }
    }
}
