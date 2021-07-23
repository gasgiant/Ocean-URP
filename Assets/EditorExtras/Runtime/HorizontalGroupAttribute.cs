using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HorizontalGroupAttribute : LayoutGroupAttribute
    {
        public HorizontalGroupAttribute(string path, float labelWidth)
        {
            this.path = path;
            type = GroupType.Horizontal;
            this.labelWidth = labelWidth;
            headerMode = HeaderMode.None;
            boxMode = BoxMode.None;
        }
    }
}
