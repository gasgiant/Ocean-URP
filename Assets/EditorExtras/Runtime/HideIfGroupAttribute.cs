using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HideIfGroupAttribute : LayoutGroupAttribute
    {
        public HideIfGroupAttribute(string path, string condition)
        {
            this.path = path;
            type = GroupType.HideIf;
            this.condition = condition;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfGroupAttribute : HideIfGroupAttribute
    {
        public ShowIfGroupAttribute(string path, string condition) : base(path, condition)
        {
            conditionInverted = true;
        }
    }
}

