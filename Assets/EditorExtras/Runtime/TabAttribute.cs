using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TabAttribute : LayoutGroupAttribute
    {
        public TabAttribute(string path)
        {
            this.path = path;
            type = GroupType.Tab;
        }
    }
}
