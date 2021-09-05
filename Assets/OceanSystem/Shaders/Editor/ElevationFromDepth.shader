Shader "Ocean/Elevation" 
{
	Properties
	{
	}
	SubShader
	{
		Tags 
		{
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
		}

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		ENDHLSL

		Pass 
		{
			Name "Unlit"
			Tags { "LightMode"="SRPDefaultUnlit" }

			HLSLPROGRAM
			#pragma vertex ElevationVertex
			#pragma fragment ElevationFragment

			struct Attributes 
			{
				float4 positionOS	: POSITION;
			};

			struct Varyings 
			{
				float4 positionCS 	: SV_POSITION;
				float3 positionWS 	: TEXCOORD0;
			};

			Varyings ElevationVertex(Attributes input) 
			{
				Varyings output;

				VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
				output.positionCS = positionInputs.positionCS;
				output.positionWS = positionInputs.positionWS;
				return output;
			}

			half4 ElevationFragment(Varyings input) : SV_Target
			{
				return input.positionWS.y;
			}
			ENDHLSL
		}
	}
}