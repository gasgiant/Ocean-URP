using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TabScopeAttribute : LayoutGroupAttribute
    {
        public TabScopeAttribute(string path, string tabs)
        {
            this.path = path;
            type = GroupType.TabScope;
            this.tabs = tabs.Split('|');
        }
    }
}
