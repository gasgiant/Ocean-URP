Shader "Ocean/Ocean"
{
    Properties
    {
        foamEditorExpanded("", Float) = 0
        sssEditorExpanded("", Float) = 0
        horizonEditorExpanded("", Float) = 0

        [Toggle(WAVES_FOAM_ENABLED)]
        _WAVES_FOAM_ENABLED("Waves Foam", Float) = 0

        [Toggle(CONTACT_FOAM_ENABLED)]
        _CONTACT_FOAM_ENABLED("Contact Foam", Float) = 0

        [MaterialToggle] _ReceiveShadows("Receive Shadows", Float) = 0

        // colors
        [HDR]
        _FogColor("Main", Color) = (0, 0, 0, 1)
        _FogDensity("Fog Density", Float) = 0.1
        [HDR]
        _SssColor("Subsurface scattering", Color) = (0, 0, 0, 1)
        [HDR]
        _DiffuseColor("Diffuse", Color) = (0, 0, 0, 1)
        _TintColor0("", Vector) = (1, 1, 1, 1)
        _TintColor1("", Vector) = (1, 1, 1, 1)
        _TintColor2("", Vector) = (1, 1, 1, 1)
        _TintColor3("", Vector) = (1, 1, 1, 1)
        _TintColor4("", Vector) = (1, 1, 1, 1)
        _TintColor5("", Vector) = (1, 1, 1, 1)
        _TintColor6("", Vector) = (1, 1, 1, 1)
        _TintColor7("", Vector) = (1, 1, 1, 1)
        _TintGradientParams("", Vector) = (1, 1, 1, 1)
        _TintDepthScale("Tint Depth Scale", Float) = 10

        // downward reflections mask
        [HDR]
        _DownwardReflectionsColor("Downward Reflections", Color) = (0, 0, 0, 0)
        _DownwardReflectionsRadius("Radius", Range(-0.5, 1.1)) = 0.1
        _DownwardReflectionsSharpness("Sharpness", Range(0, 30)) = 4

        // specular
        _SpecularStrength("Strength", Range(0.0, 10.0)) = 1
        _SpecularMinRoughness("Sun Size", Range(0.0, 1)) = 0.1

        // horizon
        _RoughnessScale("Roughness Scale", Range(0.0, 2.0)) = 1
        _RoughnessDistance("Roughness Distance", Float) = 140
        _HorizonFog("Horizon Fog", Range(0.0, 1.0)) = 0.25
        _CascadesFadeDist("Cascades Fade Scale", Float) = 20

        // planar reflections
        _ReflectionNormalStength("Normal Strength", Range(0.0, 1.0)) = 0.25

        // underwater 
        _RefractionStrength("Strength Air-Water", Float) = 0.25
        _RefractionStrengthUnderwater("Strength Water-Air", Float) = 0.75

        // subsurface scattering
        _SssSunStrength("Sun Strength", Range(0.0, 3.0)) = 0
        _SssEnvironmentStrength("Environment Strength", Range(0.0, 3.0)) = 0
        _SssSpread("Spread", Range(0.0, 1.0)) = 0.2
        _SssNormalStrength("Normal Strength", Range(0.0, 1.0)) = 1
        _SssHeight("Height", Range(-1.0, 2.0)) = 0
        _SssHeightMult("Height Mult", Range(0, 2.0)) = 1
        _SssFadeDistance("Fade Distance", Float) = 30

        // foam
        _SurfaceFoamAlbedo("Albedo", 2D) = "white" {}
        _FoamUnderwaterTexture("Underwater Texture", 2D) = "gray" {}
        [NoScaleOffset]
        _FoamTrailTexture("Trail Texture", 2D) = "white" {}
        _ContactFoamTexture("Contact Foam Texture", 2D) = "white" {}
        _FoamNormalsDetail("Normal Strength", Range(0, 1.0)) = 0.5
        _FoamTint("Tint", Color) = (1, 1, 1, 1)
        _UnderwaterFoamParallax("Underwater Parallax", Range(0, 3.0)) = 1.2
        _ContactFoam("Contact Foam", Range(0, 1.0)) = 0
    }

    CustomEditor "OceanSystem.OceanShaderEditor"

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Tags { "LightMode" = "OceanMain" }
            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma vertex OceanMainVert
            #pragma fragment OceanMainFrag

            #pragma multi_compile _ OCEAN_THREE_CASCADES OCEAN_FOUR_CASCADES
            #pragma multi_compile _ OCEAN_UNDERWATER_ENABLED
            #pragma multi_compile _ OCEAN_TRANSPARENCY_ENABLED
            #pragma shader_feature_local WAVES_FOAM_ENABLED
            #pragma shader_feature_local CONTACT_FOAM_ENABLED

            // URP Keywords
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "GeoClipMap.hlsl"
            #include "OceanGlobals.hlsl"
            #include "OceanSimulationSampling.hlsl"
            #include "OceanFoam.hlsl"
            #include "OceanSurface.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float viewDepth     : TEXCOORD1;
                float4 positionNDC  : TEXCOORD2;
                float2 worldXZ      : TEXCOORD3;
                #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
                float4 shadowCoord  : TEXCOORD4;
                #endif
            };

            Varyings OceanMainVert(Attributes input)
            {
                Varyings output;

                output.positionWS = ClipMapVertex(input.positionOS.xyz, input.uv);
                output.worldXZ = output.positionWS.xz;

                float viewDist = length(output.positionWS - _WorldSpaceCameraPos);

                float4 weights = LodWeights(viewDist, _CascadesFadeDist);
                output.positionWS += SampleDisplacement(output.worldXZ, weights, 1);

                float3 positionOS = TransformWorldToObject(output.positionWS);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                output.viewDepth = -positionInputs.positionVS.z;
                output.positionNDC = positionInputs.positionNDC;
                output.positionHCS = positionInputs.positionCS;
                #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
                output.shadowCoord = GetShadowCoord(positionInputs);
                #endif
                return output;
            }

            half4 OceanMainFrag(Varyings input, float facing : VFACE) : SV_Target
            {
                float3 viewDir = _WorldSpaceCameraPos - input.positionWS;
                float viewDist = length(viewDir);
                viewDir = viewDir / viewDist;

                float4 lodWeights = LodWeights(viewDist, _CascadesFadeDist);
                float4 shoreWeights = 1;// ShoreModulation(i.shore.x);
                float4x4 derivatives = SampleDerivatives(input.worldXZ, lodWeights * shoreWeights);
                float3 normal = NormalFromDerivatives(derivatives, 1);

                FoamInput fi;
                fi.derivatives = derivatives;
                fi.worldXZ = input.worldXZ;
                fi.lodWeights = lodWeights;
                fi.shoreWeights = shoreWeights;
                fi.positionNDC = input.positionNDC;
                fi.viewDepth = input.viewDepth;
                fi.time = _Time.y;
                fi.viewDir = viewDir;
                fi.normal = normal;
                FoamData foamData = GetFoamData(fi);

                float4 shadowCoord = 0;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #else
                    shadowCoord = float4(0, 0, 0, 0);
                #endif

                Light mainLight = GetMainLight(shadowCoord);

                LightingInput li;
                li.normal = normal;
                li.viewDir = viewDir;
                li.viewDist = viewDist;
                li.positionWS = input.positionWS;
                li.shore = 0;
                li.positionNDC = input.positionNDC;
                li.viewDepth = input.viewDepth;
                li.cameraPos = _WorldSpaceCameraPos;
                li.mainLight = mainLight;

                bool backface = dot(normal, viewDir) < 0;
                float3 oceanColor;

                #ifdef OCEAN_UNDERWATER_ENABLED
                float submergence = SAMPLE_TEXTURE2D(Ocean_CameraSubmergenceTexture, 
                    samplerOcean_CameraSubmergenceTexture,
                    input.positionNDC.xy / input.positionNDC.w).r;
                clip(-(facing < 0 && submergence > 0.6));

                bool underwater = facing < 0 || submergence < 0.3;
                if (!underwater && backface)
                {
                    li.normal = reflect(li.normal, li.viewDir);
                }
                else if (underwater && !backface)
                {
                    li.normal = reflect(li.normal, li.viewDir);
                }
                if (underwater)
                    oceanColor = GetOceanColorUnderwater(li);
                else
                    oceanColor = GetOceanColor(li, foamData);
                #else
                if (backface)
                    li.normal = reflect(li.normal, li.viewDir);
                oceanColor = GetOceanColor(li, foamData);
                #endif

                return float4(oceanColor, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "OceanDepthOnly" }
            Cull[_Cull]
            ZWrite On
        
            HLSLPROGRAM
            #pragma vertex OceanDepthOnlyVert
            #pragma fragment OceanDepthOnlyFrag
            #pragma multi_compile _ OCEAN_THREE_CASCADES OCEAN_FOUR_CASCADES
            #include "OceanDepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}