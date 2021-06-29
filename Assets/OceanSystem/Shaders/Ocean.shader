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
        Ocean_FogColor("Main", Color) = (0, 0, 0, 1)
        Ocean_FogDensity("Fog Density", Float) = 0.1
        [HDR]
        Ocean_SssColor("Subsurface scattering", Color) = (0, 0, 0, 1)
        [HDR]
        Ocean_DiffuseColor("Diffuse", Color) = (0, 0, 0, 1)
        Ocean_TintColor0("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor1("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor2("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor3("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor4("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor5("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor6("", Vector) = (1, 1, 1, 1)
        Ocean_TintColor7("", Vector) = (1, 1, 1, 1)
        Ocean_TintGradientParams("", Vector) = (1, 1, 1, 1)
        Ocean_TintDepthScale("Tint Depth Scale", Float) = 10

        // downward reflections mask
        [HDR]
        Ocean_DownwardReflectionsColor("Downward Reflections", Color) = (0, 0, 0, 0)
        Ocean_DownwardReflectionsRadius("Radius", Range(-0.5, 1.1)) = 0.1
        Ocean_DownwardReflectionsSharpness("Sharpness", Range(0, 30)) = 4

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
        _FoamTexture("Foam Noise", 2D) = "gray" {}
        _ContactFoamTexture("Contact Foam Texture", 2D) = "white" {}
        _FoamNormalsDetail("Normal Strength", Range(0, 1.0)) = 0.5
        _WhitecapsColor("Whitecaps Albedo", Color) = (1, 1, 1, 1)
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

            Varyings OceanMainVert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionWS = ClipMapVertex(IN.positionOS.xyz, IN.uv);
                OUT.worldXZ = OUT.positionWS.xz;

                float viewDist = length(OUT.positionWS - _WorldSpaceCameraPos);

                float4 weights = LodWeights(viewDist, _CascadesFadeDist);
                OUT.positionWS += SampleDisplacement(OUT.worldXZ, weights, 1);

                float3 positionOS = TransformWorldToObject(OUT.positionWS);
                VertexPositionInputs inputs = GetVertexPositionInputs(positionOS);
                OUT.viewDepth = -inputs.positionVS.z - _ProjectionParams.y;
                OUT.positionNDC = inputs.positionNDC;
                OUT.positionHCS = inputs.positionCS;
                #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
                OUT.shadowCoord = GetShadowCoord(inputs);
                #endif
                return OUT;
            }

            half4 OceanMainFrag(Varyings IN, float FACING : VFACE) : SV_Target
            {
                float3 viewDir = _WorldSpaceCameraPos - IN.positionWS;
                float viewDist = length(viewDir);
                viewDir = viewDir / viewDist;

                float4 lodWeights = LodWeights(viewDist, _CascadesFadeDist);
                float4 shoreWeights = 1;// ShoreModulation(i.shore.x);
                float4x4 derivatives = SampleDerivatives(IN.worldXZ, lodWeights * shoreWeights);
                float3 normal = NormalFromDerivatives(derivatives, 1);

                FoamInput fi;
                fi.derivatives = derivatives;
                fi.worldXZ = IN.worldXZ;
                fi.lodWeights = lodWeights;
                fi.shoreWeights = shoreWeights;
                fi.positionNDC = IN.positionNDC;
                fi.viewDepth = IN.viewDepth;
                fi.time = _Time.y;
                fi.viewDir = viewDir;
                fi.normal = normal;
                FoamData foamData = GetFoamData(fi);

                float4 shadowCoord = 0;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    shadowCoord = IN.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                #else
                    shadowCoord = float4(0, 0, 0, 0);
                #endif

                Light mainLight = GetMainLight(shadowCoord);

                LightingInput li;
                li.normal = normal;
                li.viewDir = viewDir;
                li.viewDist = viewDist;
                li.positionWS = IN.positionWS;
                li.shore = 0;
                li.positionNDC = IN.positionNDC;
                li.viewDepth = IN.viewDepth;
                li.cameraPos = _WorldSpaceCameraPos;
                li.mainLight = mainLight;

                bool backface = dot(normal, viewDir) < 0;
                float3 oceanColor;

                #ifdef OCEAN_UNDERWATER_ENABLED
                float submergence = SAMPLE_TEXTURE2D(Ocean_CameraSubmergenceTexture, 
                    samplerOcean_CameraSubmergenceTexture,
                    IN.positionNDC.xy / IN.positionNDC.w).r;
                clip(-(FACING < 0 && submergence > 0.6));

                bool underwater = FACING < 0 || submergence < 0.3;
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