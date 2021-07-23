using System;
using System.Linq;

namespace EditorExtras
{
    public abstract class LayoutGroupAttribute : Attribute
    {
        public string path;
        public GroupType type;
        public HeaderMode headerMode;
        public BoxMode boxMode;
        public bool toggle;
        public float labelWidth;
        public string[] tabs;
        public string condition;
        public bool conditionInverted;

        public bool HasCondition => type == GroupType.DisableIf
            || type == GroupType.HideIf || toggle;
    }

    public enum GroupType
    {
        Vertical,
        Horizontal,
        Tab,
        TabScope,
        DisableIf,
        HideIf
    }

    public enum HeaderMode
    {
        None,
        Label,
        Foldout
    }

    public enum BoxMode
    {
        None,
        Line,
        Box
    }
}



