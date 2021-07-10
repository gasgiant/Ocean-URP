using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
	public class OceanShaderEditor : ShaderGUI
	{
		Material targetMaterial;
		MaterialEditor editor;

		MaterialProperty surfaceEditorExpanded = null;
		MaterialProperty volumeEditorExpanded = null;
		MaterialProperty distantViewEditorExpanded = null;
		MaterialProperty foamEditorExpanded = null;

		// toggles
		MaterialProperty wavesFoamEnabled = null;
		MaterialProperty contactFoamEnabled = null;
		MaterialProperty receiveShadows = null;

		// surface
		MaterialProperty roughnessScale = null;
		MaterialProperty specularStrength = null;
		MaterialProperty specularMinRoughness = null;
		MaterialProperty reflectionNormalStength = null;
		MaterialProperty refractionStrength = null;
		MaterialProperty refractionStrengthUnderwater = null;
		// reflections mask
		MaterialProperty reflectionMaskRadius = null;
		MaterialProperty reflectionMaskSharpness = null;

		// volume
		MaterialProperty fogDensity = null;
		MaterialProperty absorbtionDepthScale = null;
		// subsurface scattering
		MaterialProperty sssSunStrength = null;
		MaterialProperty sssEnvironmentStrength = null;
		MaterialProperty sssSpread = null;
		MaterialProperty sssNormalStrength = null;
		MaterialProperty sssHeightBias = null;
		MaterialProperty sssFadeDistance = null;

		// distant view
		MaterialProperty roughnessDistance = null;
		MaterialProperty horizonFog = null;
		MaterialProperty cascadesFadeDist = null;
		MaterialProperty uvWavrpStrength = null;
		MaterialProperty distantRoughnessMap = null;
		MaterialProperty foamDetailMap = null;

		// foam
		MaterialProperty foamAlbedo = null;
		MaterialProperty foamUnderwaterTexture = null;
		MaterialProperty foamTrailTexture = null;
		MaterialProperty contactFoamTexture = null;
		MaterialProperty foamNormalsDetail = null;
		MaterialProperty surfaceFoamTint = null;
		MaterialProperty underwaterFoamParallax = null;
		MaterialProperty contactFoam = null;

		Material skyMapMaterial;

		public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
		{
			targetMaterial = editor.target as Material;
			this.editor = editor;
			skyMapMaterial = new Material(Shader.Find("Hidden/Ocean/StereographicSky"));
			FindProperties(properties);
			ShaderProperties();
		}

        void FindProperties(MaterialProperty[] properties)
		{
			surfaceEditorExpanded = FindProperty(MaterialProps.Names.SurfaceEditorExpanded, properties);
			volumeEditorExpanded = FindProperty(MaterialProps.Names.VolumeEditorExpanded, properties);
			distantViewEditorExpanded = FindProperty(MaterialProps.Names.DistantViewEditorExpanded, properties);
			foamEditorExpanded = FindProperty(MaterialProps.Names.FoamEditorExpanded, properties);

			// toggles
			wavesFoamEnabled = FindProperty(MaterialProps.Names.WavesFoamEnabled, properties);
			contactFoamEnabled = FindProperty(MaterialProps.Names.ContactFoamEnabled, properties);
			receiveShadows = FindProperty(MaterialProps.Names.ReceiveShadows, properties);

			// surface
			roughnessScale = FindProperty(MaterialProps.Names.RoughnessScale, properties);
			specularStrength = FindProperty(MaterialProps.Names.SpecularStrength, properties);
			specularMinRoughness = FindProperty(MaterialProps.Names.SpecularMinRoughness, properties);
			reflectionNormalStength = FindProperty(MaterialProps.Names.ReflectionNormalStength, properties);
			refractionStrength = FindProperty(MaterialProps.Names.RefractionStrength, properties);
			refractionStrengthUnderwater = FindProperty(MaterialProps.Names.RefractionStrengthUnderwater, properties);
			// reflections mask
			reflectionMaskRadius = FindProperty(MaterialProps.Names.ReflectionMaskRadius, properties);
			reflectionMaskSharpness = FindProperty(MaterialProps.Names.ReflectionMaskSharpness, properties);

			// volume
			fogDensity = FindProperty(MaterialProps.Names.FogDensity, properties);
			absorbtionDepthScale = FindProperty(MaterialProps.Names.AbsorbtionDepthScale, properties);
			// subsurface scattering
			sssSunStrength = FindProperty(MaterialProps.Names.SssSunStrength, properties);
			sssEnvironmentStrength = FindProperty(MaterialProps.Names.SssEnvironmentStrength, properties);
			sssSpread = FindProperty(MaterialProps.Names.SssSpread, properties);
			sssNormalStrength = FindProperty(MaterialProps.Names.SssNormalStrength, properties);
			sssHeightBias = FindProperty(MaterialProps.Names.SssHeightBias, properties);
			sssFadeDistance = FindProperty(MaterialProps.Names.SssFadeDistance, properties);

			// distant view
			roughnessDistance = FindProperty(MaterialProps.Names.RoughnessDistance, properties);
			horizonFog = FindProperty(MaterialProps.Names.HorizonFog, properties);
			cascadesFadeDist = FindProperty(MaterialProps.Names.CascadesFadeDist, properties);
			uvWavrpStrength = FindProperty(MaterialProps.Names.UvWarpStrength, properties);
			distantRoughnessMap = FindProperty(MaterialProps.Names.DistantRoughnessMap, properties);
			foamDetailMap = FindProperty(MaterialProps.Names.FoamDetailMap, properties);

			// foam
			foamAlbedo = FindProperty(MaterialProps.Names.FoamAlbedo, properties);
			foamUnderwaterTexture = FindProperty(MaterialProps.Names.FoamUnderwaterTexture, properties);
			foamTrailTexture = FindProperty(MaterialProps.Names.FoamTrailTexture, properties);
			contactFoamTexture = FindProperty(MaterialProps.Names.ContactFoamTexture, properties);
			foamNormalsDetail = FindProperty(MaterialProps.Names.FoamNormalsDetail, properties);
			surfaceFoamTint = FindProperty(MaterialProps.Names.SurfaceFoamTint, properties);
			underwaterFoamParallax = FindProperty(MaterialProps.Names.UnderwaterFoamParallax, properties);
			contactFoam = FindProperty(MaterialProps.Names.ContactFoam, properties);
		}

		void ShaderProperties()
		{
			surfaceEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(surfaceEditorExpanded.floatValue > 0, "Surface") ? 1 : 0;
			if (surfaceEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(receiveShadows, MakeLabel(receiveShadows));
				if (targetMaterial.GetFloat("_ReceiveShadows") > 0)
					targetMaterial.DisableKeyword("_RECEIVE_SHADOWS_OFF");
				else
					targetMaterial.EnableKeyword("_RECEIVE_SHADOWS_OFF");
				editor.ShaderProperty(roughnessScale, MakeLabel(roughnessScale));
				editor.ShaderProperty(reflectionNormalStength, MakeLabel(reflectionNormalStength));

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Specular", EditorStyles.boldLabel);
				editor.ShaderProperty(specularStrength, MakeLabel(specularStrength));
				editor.ShaderProperty(specularMinRoughness, MakeLabel(specularMinRoughness));

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Refraction", EditorStyles.boldLabel);
				editor.ShaderProperty(refractionStrength, MakeLabel(refractionStrength));
				editor.ShaderProperty(refractionStrengthUnderwater, MakeLabel(refractionStrengthUnderwater));
				
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Reflection Mask", EditorStyles.boldLabel);
				editor.ShaderProperty(reflectionMaskRadius, MakeLabel(reflectionMaskRadius));
				editor.ShaderProperty(reflectionMaskSharpness, MakeLabel(reflectionMaskSharpness));
				Shader.SetGlobalFloat(GlobalShaderVariables.Misc.ReflectionMaskRadius, reflectionMaskRadius.floatValue);
				Shader.SetGlobalFloat(GlobalShaderVariables.Misc.ReflectionMaskSharpness, reflectionMaskSharpness.floatValue);
				//targetMaterial.SetVector(GlobalShaderVariables.Colors.ReflectionMaskColor, Color.red);
				DrawSkyMapPreview();
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			volumeEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(volumeEditorExpanded.floatValue > 0, "Volume") ? 1 : 0;
			if (volumeEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(fogDensity, MakeLabel(fogDensity));
				editor.ShaderProperty(absorbtionDepthScale, MakeLabel(absorbtionDepthScale));
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Subsurface Scattering", EditorStyles.boldLabel);
				editor.ShaderProperty(sssSunStrength, MakeLabel(sssSunStrength));
				editor.ShaderProperty(sssEnvironmentStrength, MakeLabel(sssEnvironmentStrength));
				editor.ShaderProperty(sssSpread, MakeLabel(sssSpread));
				editor.ShaderProperty(sssNormalStrength, MakeLabel(sssNormalStrength));
				editor.ShaderProperty(sssHeightBias, MakeLabel(sssHeightBias));
				editor.ShaderProperty(sssFadeDistance, MakeLabel(sssFadeDistance));
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			foamEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(foamEditorExpanded.floatValue > 0, "Foam") ? 1 : 0;
			if (foamEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(wavesFoamEnabled, MakeLabel(wavesFoamEnabled));
				editor.ShaderProperty(contactFoamEnabled, MakeLabel(contactFoamEnabled));
				if (contactFoamEnabled.floatValue > 0)
				{
					EditorGUILayout.HelpBox("Depth Texture must be enabled in the pipline asset for contact foam to work correctly.", MessageType.Info);
					EditorGUILayout.Space();
				}
				editor.TexturePropertySingleLine(new GUIContent("Albedo"), foamAlbedo, surfaceFoamTint);
				DrawTilingProperty(foamAlbedo);

				editor.TexturePropertySingleLine(MakeLabel(foamUnderwaterTexture), foamUnderwaterTexture);
				DrawTilingProperty(foamUnderwaterTexture);

				editor.TexturePropertySingleLine(new GUIContent("Contact Foam"), contactFoamTexture, contactFoam);
				DrawTilingProperty(contactFoamTexture);

				editor.TexturePropertySingleLine(MakeLabel(foamTrailTexture), foamTrailTexture);

				editor.ShaderProperty(foamNormalsDetail, MakeLabel(foamNormalsDetail));
				editor.ShaderProperty(underwaterFoamParallax, MakeLabel(underwaterFoamParallax));
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			distantViewEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(distantViewEditorExpanded.floatValue > 0, "Distant View") ? 1 : 0;
			if (distantViewEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(roughnessDistance, MakeLabel(roughnessDistance));
				editor.ShaderProperty(horizonFog, MakeLabel(horizonFog));
				editor.ShaderProperty(cascadesFadeDist, MakeLabel(cascadesFadeDist));
				editor.ShaderProperty(uvWavrpStrength, MakeLabel(uvWavrpStrength));
				editor.TexturePropertySingleLine(MakeLabel(distantRoughnessMap), distantRoughnessMap);
				DrawTilingProperty(distantRoughnessMap);
				editor.TexturePropertySingleLine(MakeLabel(foamDetailMap), foamDetailMap);
				DrawTilingProperty(foamDetailMap);
				EditorGUI.indentLevel -= 1;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static void DrawTilingProperty(MaterialProperty prop)
        {
			Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.3f));
			float labelWidth = EditorGUIUtility.labelWidth;
			float controlStartX = rect.x + labelWidth;
			Rect labelRect = new Rect(rect.x + 14, rect.y, labelWidth - 14, EditorGUIUtility.singleLineHeight);
			Rect valueRect = new Rect(controlStartX - 14, rect.y, rect.width - labelWidth + 14, EditorGUIUtility.singleLineHeight);
			EditorGUI.PrefixLabel(labelRect, new GUIContent("Tiling"));
			prop.textureScaleAndOffset = EditorGUI.Vector2Field(valueRect, GUIContent.none, prop.textureScaleAndOffset);
		}

		private void DrawSkyMapPreview()
        {
			Rect skyPreviewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(50));
			Rect skyPreviewLabelRect = skyPreviewRect;
			skyPreviewLabelRect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(skyPreviewLabelRect, "Preview");
			skyPreviewRect.x += EditorGUIUtility.labelWidth;
			skyPreviewRect.width = skyPreviewRect.height;
			EditorGUI.DrawPreviewTexture(skyPreviewRect, EditorGUIUtility.whiteTexture, skyMapMaterial);
		}

		static GUIContent staticLabel = new GUIContent();
		static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
		{
			staticLabel.text = property.displayName;
			staticLabel.tooltip = tooltip;
			return staticLabel;
		}
	}
}
