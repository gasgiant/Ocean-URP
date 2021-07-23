using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EditorExtras.Editor
{
    public enum CompactTextureMode { Default, UniformScale, Scale, ScaleOffset }

    [CanEditMultipleObjects()]
    public class ExtendedShaderEditor : ShaderGUI
    {
        private static MaterialEditor currentEditor;
        private static string[][] allAttributes;
        private static InspectorLayoutController layoutController;
        private static Shader shader;
        private static Material material;
        private static bool drawSystemProperties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Initialize(materialEditor, properties);

            for (int i = 0; i < properties.Length; i++)
            {
                layoutController.BeforeProperty(i);
                if (layoutController.PropertyVisible(i))
                {
                    using (new EditorGUI.DisabledScope(!layoutController.ScopeEnabled))
                    {
                        DrawField(materialEditor, properties[i], allAttributes[i]);
                    }
                }
                    
            }
            layoutController.EndAll();

            if (drawSystemProperties)
            {
                EditorGUILayout.Space();
                materialEditor.RenderQueueField();
                materialEditor.DoubleSidedGIField();
            }
        }

        private void Initialize(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (currentEditor == materialEditor && allAttributes != null)
                return;
            material = (Material)materialEditor.target;
            shader = material.shader;
            currentEditor = materialEditor;
            allAttributes = GetAttributes(shader, properties.Length);
            layoutController = new InspectorLayoutController(shader.name,
                ShaderAttributesParser.GetLayoutData(allAttributes, properties, material));
            drawSystemProperties = ShaderAttributesParser.GetDrawSystemProperties(allAttributes);
        }

        private string[][] GetAttributes(Shader shader, int propsCount)
        {
            var output = new string[propsCount][];
            for (int i = 0; i < propsCount; i++)
            {
                output[i] = shader.GetPropertyAttributes(i);
            }
            return output;
        }

        private void DrawField(MaterialEditor editor, MaterialProperty prop, string[] attributes)
        {
            if (prop.flags.HasFlag(MaterialProperty.PropFlags.HideInInspector))
                return;

            if (prop.type == MaterialProperty.PropType.Texture)
            {
                var mode = ShaderAttributesParser.GetCompactTextureAttribute(attributes);
                if (mode.HasValue)
                    CompactTextureField(editor, prop, mode.Value);
                else
                    editor.ShaderProperty(prop, MakeLabel(prop));
            }
            else
                editor.ShaderProperty(prop, MakeLabel(prop));
        }

        private void CompactTextureField(MaterialEditor editor, 
            MaterialProperty prop, CompactTextureMode mode)
        {

            Rect fullRect = EditorGUILayout.GetControlRect(false);
            editor.TexturePropertyMiniThumbnail(fullRect, prop, prop.displayName, null);
            if (prop.flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset))
                return;

            Vector4 currentScaleOffset = prop.textureScaleAndOffset;
            float[] scale = new float[] { currentScaleOffset.x, currentScaleOffset.y };
            float[] offset = new float[] { currentScaleOffset.z, currentScaleOffset.w };
            GUIContent scaleLabel = new GUIContent("Tiling");
            GUIContent offsetLabel = new GUIContent("Offset");
            Rect scaleRect = fullRect;
            scaleRect.x += EditorGUIUtility.labelWidth;
            scaleRect.width -= EditorGUIUtility.labelWidth;
            float labelWidth = EditorStyles.label.CalcSize(offsetLabel).x;
            labelWidth += EditorGUIUtility.standardVerticalSpacing * 5;
            Rect scaleLabelRect = scaleRect;
            scaleLabelRect.width = labelWidth;
            Rect scaleFieldRect = scaleRect;
            scaleFieldRect.x += labelWidth;
            scaleFieldRect.width -= labelWidth;

            if (mode == CompactTextureMode.UniformScale)
            {
                float previousLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;
                scale[0] = EditorGUI.FloatField(scaleRect, scaleLabel, scale[0]);
                EditorGUIUtility.labelWidth = previousLabelWidth;
                scale[1] = scale[0];
            }
            else
            {
                EditorGUI.LabelField(scaleLabelRect, scaleLabel);
                FloatArrayFields(scaleFieldRect, scale);
            }

            if (mode == CompactTextureMode.Default || mode == CompactTextureMode.ScaleOffset)
            {
                EditorGUILayout.GetControlRect(false);
                Rect offsetLabelRect = scaleLabelRect;
                Rect offsetFieldRect = scaleFieldRect;
                offsetLabelRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                offsetFieldRect.y = offsetLabelRect.y;
                EditorGUI.LabelField(offsetLabelRect, offsetLabel);
                FloatArrayFields(offsetFieldRect, offset);
            }

            prop.textureScaleAndOffset = new Vector4(scale[0], scale[1], offset[0], offset[1]);
        }

        private void FloatArrayFields(Rect rect, float[] floats)
        {
            GUIContent[] labels = { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z") };
            EditorGUI.MultiFloatField(rect, labels, floats);
        }

        private static readonly GUIContent EmptyGUIContent = new GUIContent();
        private static GUIContent Label = new GUIContent();
        private static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
        {
            Label.text = property.displayName;
            Label.tooltip = tooltip;
            return Label;
        }
    }
}

