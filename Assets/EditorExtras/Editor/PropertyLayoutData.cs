using System.Collections.Generic;

namespace EditorExtras.Editor
{
    public class PropertyLayoutData
    {
        public readonly ExtraLayoutGroup[] groups;
        public readonly EndGroupAttribute end;
        public bool usedByToggle;

        public PropertyLayoutData(List<ExtraLayoutGroup> groups, EndGroupAttribute end)
        {
            groups.Sort((g0, g1) => g0.Order().CompareTo(g1.Order()));

            this.groups = groups.ToArray();
            this.end = end;
        }
    }
}
