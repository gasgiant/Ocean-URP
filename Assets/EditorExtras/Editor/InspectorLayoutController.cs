using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    public class InspectorLayoutController
    {
        public bool PropertyVisible(int index) => ScopeVisible && (layoutData == null
            || !layoutData[index].usedByToggle);

        public bool ScopeEnabled => groupsStack.Count == 0 || groupsStack.Peek().isEnabled;
        private bool ScopeVisible => groupsStack.Count == 0 || groupsStack.Peek().isVisible;
        
        private string prefsPrefix;
        private PropertyLayoutData[] layoutData;
        private Stack<ExtraLayoutGroup> groupsStack = new Stack<ExtraLayoutGroup>();
        private List<string> currentPath = new List<string>();
        private string activeTabName;

        public InspectorLayoutController(string prefsPrefix, PropertyLayoutData[] layoutData)
        {
            this.prefsPrefix = prefsPrefix;
            this.layoutData = layoutData;
        }

        public void BeforeProperty(int index)
        {
            if (layoutData[index].end != null)
            {
                EndGroupsUntill(layoutData[index].end.name);
            }

            if (layoutData[index].groups == null)
                return;

            foreach (var group in layoutData[index].groups)
            {
                HandleScope(group.pathArray);
                string fullName = string.Join("/", currentPath);
                bool isVisible = ScopeVisible;
                bool isEnabled = ScopeEnabled;

                if (ScopeVisible)
                {
                    if (group.data.Type == GroupType.DisableIf)
                    {
                        isEnabled = ScopeEnabled && !group.data.conditionWrapper.GetValue();
                    }

                    if (group.data.Type == GroupType.HideIf)
                    {
                        isVisible = ScopeVisible && !group.data.conditionWrapper.GetValue();
                    }

                    if (group.data.Type == GroupType.TabScope)
                    {
                        string prefsName = prefsPrefix + fullName;
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUI.indentLevel * ExtraEditorGUI.IndentWidth);
                        int activeTab = GUILayout.Toolbar(EditorPrefs.GetInt(prefsName), group.data.Tabs);
                        GUILayout.EndHorizontal();
                        EditorPrefs.SetInt(prefsName, activeTab);
                        group.savedActiveTab = activeTabName;
                        activeTabName = group.data.Tabs[activeTab];
                    }

                    if (group.data.Type == GroupType.Tab)
                    {
                        isVisible = isVisible
                            && (activeTabName == null || activeTabName == group.name);
                    }

                    GUIStyle style = GUIStyle.none;
                    if (group.data.BoxMode == BoxMode.Box)
                    {
                        style = ExtraEditorStyles.Box;
                        group.savedHierarchyMode = EditorGUIUtility.hierarchyMode;
                        float labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.hierarchyMode = false;
                        EditorGUIUtility.labelWidth = labelWidth;
                    }

                    if (group.data.Type == GroupType.Vertical)
                        EditorGUILayout.BeginVertical(style);
                    if (group.data.Type == GroupType.Horizontal)
                    {
                        group.savedLabelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = group.data.LabelWidth;
                        EditorGUILayout.BeginHorizontal(style);
                    }

                    if (group.data.HeaderMode != HeaderMode.None)
                    {
                        string prefsName = prefsPrefix + fullName;
                        bool isExpanded = group.data.HeaderMode == HeaderMode.Foldout ?
                            EditorPrefs.GetBool(prefsName) : true;
                        Rect headerRect = ExtraEditorGUI.HeaderBase(group.data.BoxMode);

                        if (group.data.Toggle)
                        {
                            bool hasMixedValue = group.data.conditionWrapper.HasMixedValue;
                            bool value = group.data.conditionWrapper.GetValue();
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.showMixedValue = hasMixedValue;
                            value = EditorGUI.ToggleLeft(headerRect, group.name, value, EditorStyles.boldLabel);
                            EditorGUI.showMixedValue = false;
                            if (EditorGUI.EndChangeCheck())
                                group.data.conditionWrapper.SetValue(value, true);
                            isEnabled = value && !hasMixedValue && isEnabled;
                        }
                        else
                        {
                            if (group.data.HeaderMode == HeaderMode.Foldout)
                            {
                                isExpanded = EditorGUI.Foldout(headerRect, isExpanded, group.name, true, ExtraEditorStyles.BoldFoldout);
                                EditorPrefs.SetBool(prefsName, isExpanded);
                                isVisible = isExpanded && isVisible;
                            }
                            if (group.data.HeaderMode == HeaderMode.Label)
                            {
                                using (new EditorGUI.DisabledScope(!ScopeEnabled))
                                {
                                    EditorGUI.LabelField(headerRect, group.name, EditorStyles.boldLabel);
                                }
                            } 
                        }
                        if (isExpanded)
                        {
                            if (group.data.BoxMode == BoxMode.Line)
                                ExtraEditorGUI.HorizontalLine();
                            if (group.data.BoxMode == BoxMode.Box)
                                SmallSpace();
                        }
                    }
                }

                group.isVisible = isVisible;
                group.isEnabled = isEnabled;
                groupsStack.Push(group);
            }
        }

        public void EndAll()
        {
            while (groupsStack.Count > 0)
            {
                EndGroup();
            }
        }

        private void HandleScope(string[] path)
        {
            if (path == null || path.Length < 1)
            {
                EndAll();
                currentPath.Add("");
                return;
            }

            if (path.Length > 1 && (path[0] == "." || path[0] == ".."))
            {
                if (currentPath.Count > 0 && path[0] == "..")
                {
                    EndGroup();
                }
                currentPath.Add(path.Last());
                return;
            }

            var newPath = new List<string>();
            int i = 0;
            while (i < path.Length && i < currentPath.Count)
            {
                if (path[i] != currentPath[i])
                {
                    break;
                }
                newPath.Add(path[i]);
                i++;
            }

            if (i < path.Length)
                newPath.Add(path.Last());

            int groupsToRemove = currentPath.Count - i;
            for (int j = 0; j < groupsToRemove; j++)
            {
                EndGroup();
            }

            currentPath = newPath;
        }

        private void EndGroupsUntill(string name)
        {
            if (name == null)
            {
                EndGroup();
                return;
            }
            int index = currentPath.Count - 1;
            while (index > 0)
            {
                if (currentPath[index] != name)
                {
                    EndGroup();
                    index -= 1;
                }
                else
                {
                    EndGroup();
                    break;
                }     
            }
        }

        private void EndGroup()
        {
            if (groupsStack.Count > 0)
            {
                ExtraLayoutGroup group = groupsStack.Pop();
                if (currentPath.Count > 0)
                    currentPath.RemoveAt(currentPath.Count - 1);
                if (ScopeVisible)
                {
                    if (group.savedIndent > -100)
                        EditorGUI.indentLevel = group.savedIndent;
                    if (group.savedLabelWidth > -1)
                        EditorGUIUtility.labelWidth = group.savedLabelWidth;
                    if (group.savedActiveTab != null)
                        activeTabName = group.savedActiveTab;

                    if (group.savedHierarchyMode.HasValue)
                        EditorGUIUtility.hierarchyMode = group.savedHierarchyMode.Value;

                    if (group.data.Type == GroupType.Vertical)
                    {
                        if (group.data.BoxMode == BoxMode.Line && group.isVisible)
                            SmallSpace();
                        EditorGUILayout.EndVertical();
                    }

                    if (group.data.Type == GroupType.Horizontal)
                        EditorGUILayout.EndHorizontal();

                    group.savedIndent = -100;
                    group.savedLabelWidth = -1;
                    group.savedActiveTab = null;
                }
            }
        }

        private static void SmallSpace()
        {
            EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 0.1f);
        }
    }
}

