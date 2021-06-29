#if !defined(OCEAN_DEPTH_ONLY_INCLUDED)
#define OCEAN_DEPTH_ONLY_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "GeoClipMap.hlsl"
#include "OceanSimulationSampling.hlsl"
#include "OceanMaterialProps.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionHCS : SV_POSITION;
};

Varyings OceanDepthOnlyVert(Attributes IN)
{
    Varyings OUT;

    float3 positionWS = ClipMapVertex(IN.positionOS.xyz, IN.uv);

    float viewDist = length(positionWS - _WorldSpaceCameraPos);

    float4 weights = LodWeights(viewDist, _CascadesFadeDist);
    positionWS += SampleDisplacement(positionWS.xz, weights, 1);

    float3 positionOS = TransformWorldToObject(positionWS);
    OUT.positionHCS = GetVertexPositionInputs(positionOS).positionCS;
    return OUT;
}

float4 OceanDepthOnlyFrag(Varyings IN) : SV_TARGET
{
    return 0;
}

#endif