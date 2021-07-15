using System;

namespace EditorExtras
{
    public class HideIfAttribute : Attribute
    {
        public string propertyName;
        public bool invert;

        public HideIfAttribute(string propertyName, bool invert = false)
        {
            this.propertyName = propertyName;
            this.invert = invert;
        }
    }
}
