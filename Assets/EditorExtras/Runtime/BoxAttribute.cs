using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class BoxAttribute : LayoutGroupAttribute
    {
        public BoxAttribute(string path, BoxMode boxMode = BoxMode.Box)
        {
            this.path = path;
            type = GroupType.Vertical;
            headerMode = HeaderMode.Label;
            this.boxMode = boxMode;
        }
    }
}
