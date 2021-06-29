Shader "Ocean/UnderwaterEffect"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #pragma exclude_renderers gles

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
        #include "../OceanSimulationSampling.hlsl"
        #include "../OceanGlobals.hlsl"
        //#include "../OceanShoreMap.hlsl"
        #include "../OceanVolume.hlsl"

        struct Attributes
        {
            uint vertexID       : SV_VertexID;
        };

        struct Varyings
        {
            float4 positionCS  : SV_POSITION;
            float2 uv           : TEXCOORD0;
        };

        //Varyings Vert(Attributes IN)
        //{
        //    Varyings OUT;
        //    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
        //    OUT.uv = IN.uv;
        //    return OUT;
        //}

        Varyings FullscreenVert(Attributes IN)
        {
            Varyings OUT;
            UNITY_SETUP_INSTANCE_ID(IN);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            //UNITY_NEAR_CLIP_VALUE
            OUT.positionCS = GetQuadVertexPosition(IN.vertexID);
            OUT.positionCS.xy = OUT.positionCS.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f); //convert to -1..1
            OUT.uv = GetQuadTexCoord(IN.vertexID);
            return OUT;
        }

        float SubmergenceFrag(Varyings IN) : SV_Target
        {
            float4 pos = mul(Ocean_CameraToWorld,
                float4((IN.uv - 0.5) * Ocean_CameraNearPlaneParams.xy, -Ocean_CameraNearPlaneParams.z, 1));
            float waterHeight = SampleHeight(pos.xz, 1, 1);//ShoreModulation(SampleShore(pos.xz).r));
            return pos.y - waterHeight + 0.5;
        }

        half4 UnderwaterPostEffectFrag(Varyings IN, float FACING : VFACE) : SV_Target
        {
            float3 backgroundColor = SampleSceneColor(IN.uv);
            float rawDepth = SampleSceneDepth(IN.uv);
            float4 positionCS = float4(IN.uv * 2 - 1, rawDepth, 1);
            float4 positionVS = mul(Ocean_CameraInverseProjection, positionCS);
            positionVS /= positionVS.w;
            float3 viewDir = -mul(Ocean_CameraToWorld, float4(positionVS.xyz, 0)).xyz;
            float viewDist = length(positionVS);
            viewDir /= viewDist;
            float4 positionWS = mul(Ocean_CameraToWorld, positionVS);

            float submergence = -SAMPLE_TEXTURE2D(Ocean_CameraSubmergenceTexture,
                samplerOcean_CameraSubmergenceTexture, IN.uv).r;
            float safetyMargin = 0.05 * saturate((viewDir.y * 1.3 + 1) * 0.5);
            submergence = saturate((submergence + 0.5 + safetyMargin) * 200);

            Light mainLight = GetMainLight();

            float3 volume = UnderwaterFogColor(viewDir, mainLight.direction, _WorldSpaceCameraPos.y);
            float3 color = ColorThroughWater(backgroundColor, volume,
                viewDist - _ProjectionParams.y, -positionWS.y);

            return float4(lerp(backgroundColor, color, submergence), 1);
        }

        ENDHLSL

        Pass
        {
            Name "Camera Submergence"
            Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment SubmergenceFrag
            #pragma target 3.5
            #pragma multi_compile _ OCEAN_THREE_CASCADES OCEAN_FOUR_CASCADES
            ENDHLSL
        }

        Pass
        {
            Name "Underwater Post Effect"
            Cull Off ZWrite Off ZTest Always

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment UnderwaterPostEffectFrag
            #pragma target 3.5
            ENDHLSL
        }
    }
}