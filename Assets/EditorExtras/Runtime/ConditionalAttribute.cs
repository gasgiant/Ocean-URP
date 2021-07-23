using System;

namespace EditorExtras
{
    public abstract class ConditionalAttribute : Attribute
    {
        public string condition;
        public bool invert;

        public ConditionalAttribute(string condition, bool invert = false)
        {
            this.condition = condition;
            this.invert = invert;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HideIfAttribute : ConditionalAttribute
    {
        public HideIfAttribute(string condition) : base(condition)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ShowIfAttribute : ConditionalAttribute
    {
        public ShowIfAttribute(string condition) : base(condition)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DisableIfAttribute : ConditionalAttribute
    {
        public DisableIfAttribute(string condition) : base(condition)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableIfAttribute : ConditionalAttribute
    {
        public EnableIfAttribute(string condition) : base(condition)
        {
        }
    }
}
