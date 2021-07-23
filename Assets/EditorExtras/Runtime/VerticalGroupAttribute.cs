using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class VerticalGroupAttribute : LayoutGroupAttribute
    {
        public VerticalGroupAttribute(string path, 
            HeaderMode header = HeaderMode.None, BoxMode boxMode = BoxMode.None)
        {
            this.path = path;
            type = GroupType.Vertical;
            headerMode = header;
            this.boxMode = boxMode;
        }
    }
}
