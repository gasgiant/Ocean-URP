#if !defined(FULLSCREEN_PROCEDURAL_VERT_INCLUDED)
#define FULLSCREEN_PROCEDURAL_VERT_INCLUDED
struct Attributes
{
    uint vertexID : SV_VertexID;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
};

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
#endif