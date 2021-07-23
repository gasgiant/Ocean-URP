using System;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class FoldoutAttribute : LayoutGroupAttribute
    {
        public FoldoutAttribute(string path, BoxMode boxMode = BoxMode.Box)
        {
            this.path = path;
            type = GroupType.Vertical;
            headerMode = HeaderMode.Foldout;
            this.boxMode = boxMode;
        }
    }
}
