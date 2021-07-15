using System.Reflection;

namespace EditorExtras.Editor
{
    public static class AttributesHandler
    {
        public static bool IsPropertyHidden(FieldInfo fieldInfo, object obj)
        {
            bool isHidden = false;
            HideIfAttribute hideIf = fieldInfo.GetCustomAttribute<HideIfAttribute>();
            if (hideIf != null)
            {
                PropertyInfo checkPropInfo = obj.GetType().GetProperty(hideIf.propertyName);
                if (checkPropInfo.PropertyType == typeof(bool))
                {
                    isHidden = (bool)checkPropInfo.GetValue(obj);
                    if (hideIf.invert) isHidden = !isHidden;
                }
            }
            return isHidden;
        }
    }
}
