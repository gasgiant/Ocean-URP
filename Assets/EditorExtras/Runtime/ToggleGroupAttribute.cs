using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ToggleGroupAttribute : LayoutGroupAttribute
    {
        public ToggleGroupAttribute(string path, string property)
        {
            this.path = path;
            type = GroupType.Vertical;
            condition = property;
            toggle = true;
            headerMode = HeaderMode.Label;
            boxMode = BoxMode.Box;
        }
    }
}

