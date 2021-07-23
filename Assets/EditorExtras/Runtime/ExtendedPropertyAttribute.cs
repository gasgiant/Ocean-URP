using System;
using UnityEngine;

namespace EditorExtras
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ExtendedPropertyAttribute : PropertyAttribute
    {
        public bool stripped;

        public ExtendedPropertyAttribute(bool stripped = false)
        {
            this.stripped = stripped;
        }
    }
}
