using UnityEditor;
using UnityEngine;
using MarkupAttributes.Editor;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class MarkedUpMonoBehaviourEditor : MarkedUpEditor
    {
    }

    [CustomEditor(typeof(ScriptableObject), true), CanEditMultipleObjects]
    public class MarkedUpScriptableObjectEditor : MarkedUpEditor
    {
    }
}
