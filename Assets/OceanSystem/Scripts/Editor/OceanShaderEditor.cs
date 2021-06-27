using UnityEngine;
using UnityEditor;

namespace OceanSystem
{
	public class OceanShaderEditor : ShaderGUI
	{
		Material targetMaterial;
		MaterialEditor editor;

		// toggles
		MaterialProperty wavesFoamEnabled = null;
		MaterialProperty contactFoamEnabled = null;

		// colors
		MaterialProperty fogColor = null;
		MaterialProperty sssColor = null;
		MaterialProperty diffuseColor = null;

		// specular
		MaterialProperty specularStrength = null;
		MaterialProperty specularMinRoughness = null;

		// horizon
		MaterialProperty horizonEditorExpanded = null;
		MaterialProperty roughnessScale = null;
		MaterialProperty roughnessDistance = null;
		MaterialProperty horizonFog = null;
		MaterialProperty cascadesFadeDist = null;

		// planar reflections
		MaterialProperty reflectionNormalStength = null;

		// refractions
		MaterialProperty refractionStrength = null;
		MaterialProperty refractionStrengthUnderwater = null;

		// subsurface scattering
		MaterialProperty sssEditorExpanded = null;
		MaterialProperty sssSunStrength = null;
		MaterialProperty sssEnvironmentStrength = null;
		MaterialProperty sssSpread = null;
		MaterialProperty sssNormalStrength = null;
		MaterialProperty sssHeight = null;
		MaterialProperty sssHeightMult = null;
		MaterialProperty sssFadeDistance = null;

		// foam
		MaterialProperty foamEditorExpanded = null;
		MaterialProperty foamTexture = null;
		MaterialProperty contactFoamTexture = null;
		MaterialProperty foamCoverage = null;
		MaterialProperty foamDensity = null;
		MaterialProperty foamPersistence = null;
		MaterialProperty foamNormalsDetail = null;
		MaterialProperty foamCascadesWeights = null;
		MaterialProperty whitecapsColor = null;
		MaterialProperty underwaterFoam = null;
		MaterialProperty underwaterFoamParallax = null;
		MaterialProperty contactFoam = null;

		public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
		{
			this.targetMaterial = editor.target as Material;
			this.editor = editor;
			FindProperties(properties);
			ShaderProperties();
		}

		void FindProperties(MaterialProperty[] properties)
		{
			// keywords
			wavesFoamEnabled = FindProperty("_WAVES_FOAM_ENABLED", properties);
			contactFoamEnabled = FindProperty("_CONTACT_FOAM_ENABLED", properties);

			// specular
			fogColor = FindProperty("Ocean_FogColor", properties);
			sssColor = FindProperty("Ocean_SssColor", properties);
			diffuseColor = FindProperty("Ocean_DiffuseColor", properties);

			// specular
			specularStrength = FindProperty("_SpecularStrength", properties);
			specularMinRoughness = FindProperty("_SpecularMinRoughness", properties);

			// horizon
			horizonEditorExpanded = FindProperty("horizonEditorExpanded", properties);
			roughnessScale = FindProperty("_RoughnessScale", properties);
			roughnessDistance = FindProperty("_RoughnessDistance", properties);
			horizonFog = FindProperty("_HorizonFog", properties);
			cascadesFadeDist = FindProperty("_CascadesFadeDist", properties);

			// planar reflections
			reflectionNormalStength = FindProperty("_ReflectionNormalStength", properties);

			// refractions
			refractionStrength = FindProperty("_RefractionStrength", properties);
			refractionStrengthUnderwater = FindProperty("_RefractionStrengthUnderwater", properties);

			// subsurface scattering
			sssEditorExpanded = FindProperty("sssEditorExpanded", properties);
			sssSunStrength = FindProperty("_SssSunStrength", properties);
			sssEnvironmentStrength = FindProperty("_SssEnvironmentStrength", properties);
			sssSpread = FindProperty("_SssSpread", properties);
			sssNormalStrength = FindProperty("_SssNormalStrength", properties);
			sssHeight = FindProperty("_SssHeight", properties);
			sssHeightMult = FindProperty("_SssHeightMult", properties);
			sssFadeDistance = FindProperty("_SssFadeDistance", properties);

			// foam
			foamEditorExpanded = FindProperty("foamEditorExpanded", properties);
			foamCoverage = FindProperty("_FoamCoverage", properties);
			foamDensity = FindProperty("_FoamDensity", properties);
			foamPersistence = FindProperty("_FoamPersistence", properties);
			foamNormalsDetail = FindProperty("_FoamNormalsDetail", properties);
			foamCascadesWeights = FindProperty("_FoamCascadesWeights", properties);
			whitecapsColor = FindProperty("_WhitecapsColor", properties);
			foamTexture = FindProperty("_FoamTexture", properties);
			underwaterFoam = FindProperty("_UnderwaterFoam", properties);
			underwaterFoamParallax = FindProperty("_UnderwaterFoamParallax", properties);
			contactFoam = FindProperty("_ContactFoam", properties);
			contactFoamTexture = FindProperty("_ContactFoamTexture", properties);
		}

		void ShaderProperties()
		{
			EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
			EditorGUI.indentLevel += 1;
			editor.ShaderProperty(fogColor, MakeLabel(fogColor));
			editor.ShaderProperty(sssColor, MakeLabel(sssColor));
			editor.ShaderProperty(diffuseColor, MakeLabel(diffuseColor));
			Gradient tint = EditorGUILayout.GradientField(new GUIContent("Tint Gradient"), GetGradient(Ocean.TintGradientIDs), false);
			EditorGUILayout.LabelField("Ocean gradient supports up to 4 color keys. Keys above the limit are ignored.", EditorStyles.helpBox);
			SetGradient(Ocean.TintGradientIDs, tint);
			

			EditorGUI.indentLevel -= 1;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Specular", EditorStyles.boldLabel);
			EditorGUI.indentLevel += 1;
			editor.ShaderProperty(specularStrength, MakeLabel(specularStrength));
			editor.ShaderProperty(specularMinRoughness, MakeLabel(specularMinRoughness));
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Planar Reflections", EditorStyles.boldLabel);
			EditorGUI.indentLevel += 1;
			editor.ShaderProperty(reflectionNormalStength, MakeLabel(reflectionNormalStength));
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Refraction", EditorStyles.boldLabel);
			EditorGUI.indentLevel += 1;
			editor.ShaderProperty(refractionStrength, MakeLabel(refractionStrength));
			editor.ShaderProperty(refractionStrengthUnderwater, MakeLabel(refractionStrengthUnderwater));
			EditorGUI.indentLevel -= 1;
			EditorGUILayout.Separator();

			horizonEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(horizonEditorExpanded.floatValue > 0, "Horizon") ? 1 : 0;
			if (horizonEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(roughnessScale, MakeLabel(roughnessScale));
				editor.ShaderProperty(roughnessDistance, MakeLabel(roughnessDistance));
				editor.ShaderProperty(horizonFog, MakeLabel(horizonFog));
				editor.ShaderProperty(cascadesFadeDist, MakeLabel(cascadesFadeDist));
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			sssEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(sssEditorExpanded.floatValue > 0, "Subsurface Scattering") ? 1 : 0;
			if (sssEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(sssSunStrength, MakeLabel(sssSunStrength));
				editor.ShaderProperty(sssEnvironmentStrength, MakeLabel(sssEnvironmentStrength));
				editor.ShaderProperty(sssSpread, MakeLabel(sssSpread));
				editor.ShaderProperty(sssNormalStrength, MakeLabel(sssNormalStrength));
				editor.ShaderProperty(sssHeight, MakeLabel(sssHeight));
				editor.ShaderProperty(sssHeightMult, MakeLabel(sssHeightMult));
				editor.ShaderProperty(sssFadeDistance, MakeLabel(sssFadeDistance));
				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Separator();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			foamEditorExpanded.floatValue = EditorGUILayout.BeginFoldoutHeaderGroup(foamEditorExpanded.floatValue > 0, "Foam") ? 1 : 0;
			if (foamEditorExpanded.floatValue > 0)
			{
				EditorGUI.indentLevel += 1;
				editor.ShaderProperty(wavesFoamEnabled, MakeLabel(wavesFoamEnabled));
				editor.ShaderProperty(contactFoamEnabled, MakeLabel(contactFoamEnabled));
				editor.TexturePropertySingleLine(new GUIContent("Whitecaps"), foamTexture,
						whitecapsColor);
				editor.TexturePropertySingleLine(new GUIContent("Contact Foam"), contactFoamTexture,
						contactFoam);
				editor.ShaderProperty(foamCoverage, MakeLabel(foamCoverage));
				editor.ShaderProperty(foamDensity, MakeLabel(foamDensity));
				editor.ShaderProperty(foamPersistence, MakeLabel(foamPersistence));
				editor.ShaderProperty(foamNormalsDetail, MakeLabel(foamNormalsDetail));
				editor.ShaderProperty(underwaterFoam, MakeLabel(underwaterFoam));
				editor.ShaderProperty(underwaterFoamParallax, MakeLabel(underwaterFoamParallax));
				editor.ShaderProperty(foamCascadesWeights, MakeLabel(foamCascadesWeights));
				EditorGUI.indentLevel -= 1;
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		Gradient GetGradient(int[] propIDs)
        {
			Gradient grad = new Gradient();
			Vector2 pars = targetMaterial.GetVector(propIDs[0]);
			grad.mode = (GradientMode)pars.y;
			int count = (int)pars.x;
			var keys = new GradientColorKey[count];
            for (int i = 0; i < count; i++)
            {
				Vector4 value = targetMaterial.GetVector(propIDs[i + 1]);
				keys[i].color = new Color(Mathf.LinearToGammaSpace(value.x),
					Mathf.LinearToGammaSpace(value.y),
					Mathf.LinearToGammaSpace(value.z));
				keys[i].time = value.w;
			}
			grad.colorKeys = keys;
			return grad;
		}

		void SetGradient(int[] propIDs, Gradient grad)
		{
			int keysCount = Mathf.Min(grad.colorKeys.Length, 4);
			for (int i = 0; i < keysCount; i++)
			{
				Vector4 v = grad.colorKeys[i].color.linear;
				v.w = grad.colorKeys[i].time;
				targetMaterial.SetVector(propIDs[i + 1], v);
			}
			targetMaterial.SetVector(propIDs[0],
				new Vector2(keysCount, (float)grad.mode));
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
