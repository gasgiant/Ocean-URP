using System;
using UnityEngine;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class InlineEditorAttribute : PropertyAttribute
    {
        public bool stripped;

        public InlineEditorAttribute(bool stripped = false)
        {
            this.stripped = stripped;
        }
    }
}
