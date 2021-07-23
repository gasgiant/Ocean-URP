using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    public static class ShaderAttributesParser
    {
        public static bool GetDrawSystemProperties(string[][] allAttributes)
        {
            for (int i = 0; i < allAttributes.Length; i++)
            {
                var attributes = allAttributes[i];
                foreach (var attribute in attributes)
                {
                    if (ParseAttribute(attribute, "DrawSystemProperties"))
                        return true;
                }
            }
            return false;
        }
        public static CompactTextureMode? GetCompactTextureAttribute(string[] attributes)
        {
            CompactTextureMode? mode = null;
            for (int i = 0; i < attributes.Length; i++)
            {
                mode = GetCompactTextureAttribute(attributes[i]);
                if (mode != null)
                    break;
            }
            return mode;
        }

        private static CompactTextureMode? GetCompactTextureAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "CompactTexture", 0, out string[] args);
            if (!valid)
                return null;
            if (args != null && args.Length > 0)
            {
                if (args[0] == "UniformScale")
                    return CompactTextureMode.UniformScale;
                if (args[0] == "Scale")
                    return CompactTextureMode.Scale;
                if (args[0] == "ScaleOffset")
                    return CompactTextureMode.ScaleOffset;
            }
            return CompactTextureMode.Default;
        }

        public static PropertyLayoutData[] GetLayoutData(string[][] allAttributes,
            MaterialProperty[] props, Material targetMaterial)
        {
            var output = new PropertyLayoutData[props.Length];
            var hiddenProps = new HashSet<string>();
            for (int i = 0; i < props.Length; i++)
            {
                var groupAttributes = GetLayoutGroupAttributes(allAttributes[i]);
                var groups = new List<ExtraLayoutGroup>();
                foreach (var groupAttribute in groupAttributes)
                {
                    ConditionWrapper conditionWrapper = null;
                    if (groupAttribute.HasCondition)
                        conditionWrapper = new ConditionWrapper(groupAttribute.condition,
                            groupAttribute.conditionInverted, props, targetMaterial);
                    groups.Add(new ExtraLayoutGroup(groupAttribute, conditionWrapper));

                    if (groupAttribute.toggle)
                        hiddenProps.Add(groupAttribute.condition);
                }
                output[i] = new PropertyLayoutData(
                    groups, GetEndGroupAttribute(allAttributes[i]));
            }

            for (int i = 0; i < output.Length; i++)
            {
                if (hiddenProps.Contains(props[i].name))
                    output[i].usedByToggle = true;
            }
            return output;
        }

        private static EndGroupAttribute GetEndGroupAttribute(string[] attributes)
        {
            foreach (var attribute in attributes)
            {
                if (ParseAttribute(attribute, "EndGroup", 0, out string[] args))
                {
                    EndGroupAttribute a;
                    if (args != null && args.Length > 0)
                        a = new EndGroupAttribute(args[0]);
                    else
                        a = new EndGroupAttribute();
                    return a;
                }
            }
            return null;
        }

        private static LayoutGroupAttribute[] GetLayoutGroupAttributes(string[] attributes)
        {
            var groups = new List<LayoutGroupAttribute>();
            var temp = new List<LayoutGroupAttribute>();
            for (int i = 0; i < attributes.Length; i++)
            {
                temp.Clear();
                temp.Add(GetHideIfGroupAttribute(attributes[i]));
                temp.Add(GetDisableIfGroupAttribute(attributes[i]));
                temp.Add(GetTabScopeAttribute(attributes[i]));
                temp.Add(GetTabAttribute(attributes[i]));
                temp.Add(GetVerticalGroupAttribute(attributes[i]));
                temp.Add(GetHorizontalGroupAttribute(attributes[i]));
                temp.Add(GetFoldoutGroupAttribute(attributes[i]));
                temp.Add(GetToggleGroupAttribute(attributes[i]));
                temp.Add(GetBoxAttribute(attributes[i]));

                foreach (var g in temp)
                {
                    if (g != null)
                        groups.Add(g);
                }
            }
            return groups.ToArray();
        }

        private static HideIfGroupAttribute GetHideIfGroupAttribute(string attribute)
        {
            string[] args;
            bool valid = ParseAttribute(attribute, "HideIfGroup", 2, out args);
            if (valid)
                return new HideIfGroupAttribute(GetPath(args[0]), args[1]);
            valid = ParseAttribute(attribute, "ShowIfGroup", 2, out args);
            if (valid)
                return new ShowIfGroupAttribute(GetPath(args[0]), args[1]);
            return null;
        }

        private static DisableIfGroupAttribute GetDisableIfGroupAttribute(string attribute)
        {
            string[] args;
            bool valid = ParseAttribute(attribute, "DisableIfGroup", 2, out args);
            if (valid)
                return new DisableIfGroupAttribute(GetPath(args[0]), args[1]);
            valid = ParseAttribute(attribute, "EnableIfGroup", 2, out args);
            if (valid)
                return new EnableIfGroupAttribute(GetPath(args[0]), args[1]);
            return null;
        }

        private static TabScopeAttribute GetTabScopeAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "TabScope", 2, out string[] args);
            if (valid)
                return new TabScopeAttribute(GetPath(args[0]), GetTabs(args[1]));
            return null;
        }

        private static TabAttribute GetTabAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "Tab", 1, out string[] args);
            if (valid)
                return new TabAttribute(GetPath(args[0]));
            return null;
        }

        private static VerticalGroupAttribute GetVerticalGroupAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "VerticalGroup", 1, out string[] args);
            if (valid)
            {
                if (args.Length < 2)
                    return new VerticalGroupAttribute(GetPath(args[0]));
                if (args.Length < 3)
                    return new VerticalGroupAttribute(GetPath(args[0]), GetHeaderMode(args[1]));
                return new VerticalGroupAttribute(GetPath(args[0]), GetHeaderMode(args[1]),
                    GetBoxMode(args[2]));
            }  
            return null;
        }

        private static HorizontalGroupAttribute GetHorizontalGroupAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "HorizontalGroup", 2, out string[] args);
            if (valid)
                return new HorizontalGroupAttribute(GetPath(args[0]), GetFloat(args[1]));
            return null;
        }

        private static FoldoutAttribute GetFoldoutGroupAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "Foldout", 1, out string[] args);
            if (valid)
            {
                if (args.Length < 2)
                    return new FoldoutAttribute(GetPath(args[0]));
                return new FoldoutAttribute(GetPath(args[0]), GetBoxMode(args[1]));
            }
            return null;
        }

        private static ToggleGroupAttribute GetToggleGroupAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "ToggleGroup", 2, out string[] args);
            if (valid)
                return new ToggleGroupAttribute(GetPath(args[0]), args[1]);
            return null;
        }

        private static BoxAttribute GetBoxAttribute(string attribute)
        {
            bool valid = ParseAttribute(attribute, "Box", 1, out string[] args);
            if (valid)
            {
                if (args.Length < 2)
                    return new BoxAttribute(GetPath(args[0]));
                return new BoxAttribute(GetPath(args[0]), GetBoxMode(args[1]));
            }
            return null;
        }

        private static string GetPath(string arg)
        {
            arg = arg.Replace(' ', '/');
            arg = arg.Replace('_', ' ');
            return arg;
        }

        private static string GetTabs(string arg)
        {
            arg = arg.Replace(' ', '|');
            arg = arg.Replace('_', ' ');
            return arg;
        }

        private static HeaderMode GetHeaderMode(string arg)
        {
            if (arg == "Foldout")
                return HeaderMode.Foldout;
            if (arg == "Label")
                return HeaderMode.Label;
            return HeaderMode.None;
        }

        private static BoxMode GetBoxMode(string arg)
        {
            if (arg == "Box")
                return BoxMode.Box;
            if (arg == "Line")
                return BoxMode.Line;
            return BoxMode.None;
        }

        private static float GetFloat(string arg) => float.Parse(arg, CultureInfo.InvariantCulture.NumberFormat);

        private static bool GetBool(string arg) => arg == "true";

        private static bool ParseAttribute(string attribute, string attributeName)
        {
            return ParseAttribute(attribute, attributeName, 0, out _);
        }

        private static bool ParseAttribute(string attribute, string attributeName, int minArgsCount, out string[] args)
        {
            args = null;
            int argStartIndex = attribute.IndexOf('(');
            if (argStartIndex < 0 && minArgsCount > 0)
                return false;
            if (!(argStartIndex < 0 ? attribute : attribute.Substring(0, argStartIndex)).Equals(attributeName))
                return false;

            if (argStartIndex < 0)
                return true;

            string argument = attribute.Substring(argStartIndex + 1, attribute.Length - argStartIndex - 1);
            argument = argument.Trim(')');
            if (argument.Length < 1)
                return !(minArgsCount > 0);

            args = argument.Split(',');
            if (args.Length < minArgsCount)
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = args[i].Trim();
            }
            return true;
        }
    }
}
