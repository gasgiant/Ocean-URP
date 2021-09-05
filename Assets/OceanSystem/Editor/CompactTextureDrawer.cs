using UnityEditor;
using UnityEngine;

namespace OceanSystem.Editor
{
    public class CompactTextureDrawer : MaterialPropertyDrawer
    {
        internal enum CompactTextureMode { Default, UniformScaleOnly, ScaleOnly }
        private static readonly GUIContent ScaleLabel = new GUIContent("Tiling");
        private static readonly GUIContent OffsetLabel = new GUIContent("Offset");
        private static readonly GUIContent[] AxesLabels = { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") };

        private readonly CompactTextureMode mode;
        private readonly float[] scale = new float[2];
        private readonly float[] offset = new float[2];

        private static CompactTextureMode GetMode(string arg)
        {
            if (arg == CompactTextureMode.ScaleOnly.ToString())
                return CompactTextureMode.ScaleOnly;
            if (arg == CompactTextureMode.UniformScaleOnly.ToString())
                return CompactTextureMode.UniformScaleOnly;

            return CompactTextureMode.Default;
        }

        public CompactTextureDrawer()
        {
            mode = CompactTextureMode.Default;
        }

        public CompactTextureDrawer(string arg)
        {
            mode = GetMode(arg);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (prop.type != MaterialProperty.PropType.Texture)
                return EditorGUIUtility.singleLineHeight * 1.5f;
            float height = EditorGUIUtility.singleLineHeight;
            if (mode == CompactTextureMode.Default &&
                !prop.flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset))
                height += EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (prop.type == MaterialProperty.PropType.Texture)
                CompactTextureField(position, prop, editor, mode);
            else
                EditorGUI.HelpBox(position, "Compact texture drawer only works for textures.",
                    MessageType.Warning);
        }

        private void CompactTextureField(Rect position, MaterialProperty prop,
            MaterialEditor editor, CompactTextureMode mode)
        {
            Rect firstLine = position;
            firstLine.height = EditorGUIUtility.singleLineHeight;

            editor.TexturePropertyMiniThumbnail(firstLine, prop, prop.displayName, null);
            if (prop.flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset))
                return;

            Vector4 scaleOffset = prop.textureScaleAndOffset;
            (scale[0], scale[1]) = (scaleOffset.x, scaleOffset.y);
            (offset[0], offset[1]) = (scaleOffset.z, scaleOffset.w);

            Rect scaleRect = firstLine;
            scaleRect.x += EditorGUIUtility.labelWidth;
            scaleRect.width -= EditorGUIUtility.labelWidth;
            float labelWidth = EditorStyles.label.CalcSize(OffsetLabel).x;
            labelWidth += EditorGUIUtility.standardVerticalSpacing * 5;
            Rect scaleLabelRect = scaleRect;
            scaleLabelRect.width = labelWidth;
            Rect scaleFieldRect = scaleRect;
            scaleFieldRect.x += labelWidth;
            scaleFieldRect.width -= labelWidth;

            if (mode == CompactTextureMode.UniformScaleOnly)
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;
                scale[0] = EditorGUI.FloatField(scaleRect, ScaleLabel, scale[0]);
                EditorGUIUtility.labelWidth = previousLabelWidth;
                scale[1] = scale[0];
            }
            else
            {
                EditorGUI.LabelField(scaleLabelRect, ScaleLabel);
                EditorGUI.MultiFloatField(scaleFieldRect, AxesLabels, scale);
            }

            if (mode == CompactTextureMode.Default)
            {
                Rect offsetLabelRect = scaleLabelRect;
                Rect offsetFieldRect = scaleFieldRect;
                offsetLabelRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                offsetFieldRect.y = offsetLabelRect.y;
                EditorGUI.LabelField(offsetLabelRect, OffsetLabel);
                EditorGUI.MultiFloatField(offsetFieldRect, AxesLabels, offset);
            }

            prop.textureScaleAndOffset = new Vector4(scale[0], scale[1], offset[0], offset[1]);
        }
    }
}

