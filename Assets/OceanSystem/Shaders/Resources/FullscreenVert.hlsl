#if !defined(FULLSCREEN_PROCEDURAL_VERT_INCLUDED)
#define FULLSCREEN_PROCEDURAL_VERT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

struct AttributesBasic
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
};

struct AttributesProcedural
{
    uint vertexID       : SV_VertexID;
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
};

Varyings BasicFullscreenVert(AttributesBasic input)
{
    Varyings output;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.uv;
    return output;
}

Varyings ProceduralFullscreenVert(AttributesProcedural input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            //UNITY_NEAR_CLIP_VALUE
    output.positionCS = GetQuadVertexPosition(input.vertexID);
    output.positionCS.xy = output.positionCS.xy * float2(2.0f, -2.0f) + float2(-1.0f, 1.0f); //convert to -1..1
    output.uv = GetQuadTexCoord(input.vertexID);
    return output;
}
#endif