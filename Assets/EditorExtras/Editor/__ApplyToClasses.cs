using UnityEditor;
using UnityEngine;

namespace EditorExtras.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ExtendedMonoBehaviourEditor : ExtendedEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    public class ExtendedScriptableObjectEditor : ExtendedEditor
    {
    }
}
