using UnityEditor;
using UnityEngine;

namespace EditorExtras
{
    public static class ExtraEditorStyles
    {
        private static GUIStyle boldFoldout;
        public static GUIStyle BoldFoldout
        {
            get
            {
                if (boldFoldout == null)
                {
                    boldFoldout = new GUIStyle(EditorStyles.foldout);
                    boldFoldout.font = EditorStyles.boldFont;
                }
                return boldFoldout;
            }
        }

        private static GUIStyle box;
        public static GUIStyle Box
        {
            get
            {
                if (box == null)
                {
                    box = new GUIStyle(GUI.skin.box);
                    box.normal.background = Texture2D.whiteTexture;

                    string[] results = AssetDatabase.FindAssets("EditorExtrasBoxBorderTexture");
                    Texture2D tex = null;
                    if (results != null && results.Length > 0)
                        tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(results[0]));
                    if (tex != null)
                        box.normal.scaledBackgrounds = new Texture2D[] { tex };
                }
                return box;
            }
        }

        private static GUIStyle fillBoxDark;
        public static GUIStyle FillBoxDark
        {
            get
            {
                if (fillBoxDark == null)
                {
                    fillBoxDark = new GUIStyle(GUI.skin.box);
                    fillBoxDark.normal.background = Texture2D.whiteTexture;
                    string[] results = AssetDatabase.FindAssets("EditorExtrasHeaderFillTexture_Dark");
                    Texture2D tex = null;
                    if (results != null && results.Length > 0)
                        tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(results[0]));
                    if (tex != null)
                        fillBoxDark.normal.scaledBackgrounds = new Texture2D[] { tex };
                }
                return fillBoxDark;
            }
        }

        private static GUIStyle fillBoxLight;
        public static GUIStyle FillBoxLight
        {
            get
            {
                if (fillBoxLight == null)
                {
                    fillBoxLight = new GUIStyle(GUI.skin.box);
                    fillBoxLight.normal.background = Texture2D.whiteTexture;
                    string[] results = AssetDatabase.FindAssets("EditorExtrasHeaderFillTexture_Light");
                    Texture2D tex = null;
                    if (results != null && results.Length > 0)
                        tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(results[0]));
                    if (tex != null)
                        fillBoxLight.normal.scaledBackgrounds = new Texture2D[] { tex };
                }
                return fillBoxLight;
            }
        }

        public static GUIStyle FillBox => EditorGUIUtility.isProSkin ? FillBoxDark : FillBoxLight;
    }
}
