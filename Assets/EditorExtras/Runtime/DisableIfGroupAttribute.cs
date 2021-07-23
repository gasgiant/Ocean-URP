using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DisableIfGroupAttribute : LayoutGroupAttribute
    {
        public DisableIfGroupAttribute(string path, string condition)
        {
            this.path = path;
            type = GroupType.DisableIf;
            this.condition = condition;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfGroupAttribute : DisableIfGroupAttribute
    {
        public EnableIfGroupAttribute(string path, string condition) : base(path, condition)
        {
            conditionInverted = true;
        }
    }
}

