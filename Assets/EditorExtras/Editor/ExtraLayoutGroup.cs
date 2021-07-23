using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EditorExtras.Editor
{
    public class ExtraLayoutGroup
    {
        public readonly Params data;
        public readonly string[] pathArray;
        public readonly string name;
        public bool isVisible;
        public bool isEnabled;
        public bool? savedHierarchyMode = null;
        public int savedIndent = -100;
        public float savedLabelWidth = -1;
        public string savedActiveTab = null;

        public int Order()
        {
            if (pathArray.Length > 0 && (pathArray[0] == "." || pathArray[0] == ".."))
                return pathArray.Length + 1000;
            return pathArray.Length;
        }

        public ExtraLayoutGroup(LayoutGroupAttribute attribute, ConditionWrapper conditionWrapper = null)
        {
            data = new Params(attribute, conditionWrapper);
            if (attribute.path != null)
            {
                pathArray = attribute.path.Split('/');
                if (pathArray.Length > 0)
                    name = pathArray.Last();
                else
                    name = "";
            }
            else
            {
                pathArray = new string[0];
                name = "";
            }
        }

        public class Params
        {
            public string Path => attribute.path;
            public GroupType Type => attribute.type;
            public HeaderMode HeaderMode => attribute.headerMode;
            public bool Toggle => attribute.toggle;
            public BoxMode BoxMode => attribute.boxMode;
            public float LabelWidth => attribute.labelWidth;
            public string[] Tabs => attribute.tabs;

            public readonly ConditionWrapper conditionWrapper;
            private readonly LayoutGroupAttribute attribute;

            public Params(LayoutGroupAttribute attribute, ConditionWrapper conditionWrapper)
            {
                this.attribute = attribute;
                this.conditionWrapper = conditionWrapper;
            }
        }
    }
}
