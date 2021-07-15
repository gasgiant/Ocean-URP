using UnityEngine;

namespace EditorExtras
{
    public class InlineInspectorAttribute : PropertyAttribute
    {
        public bool indent;
        public bool box;

        public InlineInspectorAttribute(bool indent = true, bool box = true)
        {
            this.indent = indent;
            this.box = box;
        }
    }
}
