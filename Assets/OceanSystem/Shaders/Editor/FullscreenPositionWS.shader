Shader "Hidden/Ocean/FullscreenPositionWS"
{
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Fullscreen Position WS"

            HLSLPROGRAM
            #pragma vertex BasicFullscreenVert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../Resources/FullscreenVert.hlsl"


            float3 PositionWsFromDepth(float rawDepth, float2 screenUV, float4x4 inverseProj, float4x4 inverseView)
            {
                float4 positionCS = float4(screenUV * 2 - 1, rawDepth, 1);
                float4 positionVS = mul(inverseProj, positionCS);
                positionVS /= positionVS.w;
                return mul(inverseView, positionVS).xyz;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return 1;
                //return float4(PositionWsFromDepth(rawDepth, input.uv, 1, 1), 1);
            }
            ENDHLSL
        }
    }
}