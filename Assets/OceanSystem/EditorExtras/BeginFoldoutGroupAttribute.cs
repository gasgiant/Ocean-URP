using System;

namespace EditorExtras
{
    public class BeginFoldoutGroupAttribute : Attribute
    {
        public string name;
        public bool indent;
        public bool endSpace;

        public BeginFoldoutGroupAttribute(string name, bool indent = false, bool endSpace = false)
        {
            this.name = name;
            this.indent = indent;
            this.endSpace = endSpace;
        }
    }
}
