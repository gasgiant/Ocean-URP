Shader "Ocean/StereographicSky"
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
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../OceanGlobals.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag(Varyings IN, float FACING : VFACE) : SV_Target
            {
                float2 xy = (IN.uv - 0.5) * 2 * 1.1;
                float sqrs = dot(xy, xy);
                float3 dir = float3(2 * xy.x, 1 - sqrs, 2 * xy.y) / (1 + sqrs);
                float3 col = SampleOceanSpecCube(dir);
                float t = saturate(-dir.y * Ocean_BottomHemisphereStrength + Ocean_BottomHemisphereRadius);
                col = lerp(col, Ocean_BottomHemisphereColor.rgb, t * Ocean_BottomHemisphereColor.w);
                return float4(col, 1);
            }
    ENDHLSL
}
    }
}