/**
 * Real-time Realistic Ocean Lighting using Seamless Transitions from Geometry to BRDF
 * Copyright (c) 2009 INRIA
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/**
 * Author: Eric Bruneton
 */

#if !defined(BRUNETON_LIGHTING_INCLUDED)

#define BRUNETON_LIGHTING_INCLUDED

#define BRUNETON_PI 3.1415926

float meanFresnel(float cosThetaV, float sigmaV)
{
    return pow(abs(1.0 - cosThetaV), 5.0 * exp(-2.69 * sigmaV)) / (1.0 + 22.7 * pow(abs(sigmaV), 1.5));
}

// V, N in wind space
float MeanFresnel(float3 V, float3 N, float2 sigmaSq)
{
	float2 v = V.xz; // view direction in wind space
	float2 t = v * v / (1.0 - V.y * V.y); // cos^2 and sin^2 of view direction
	float sigmaV2 = dot(t, sigmaSq); // slope variance in view direction
	return meanFresnel(dot(V, N), sqrt(sigmaV2));
}

// assumes x>0
float erfc(float x)
{
	return 2.0 * exp(-x * x) / (2.319 * x + sqrt(4.0 + 1.52 * x * x));
}

float Lambda(float cosTheta, float sigmaSq)
{
	float v = cosTheta / sqrt((1.0 - cosTheta * cosTheta) * (2.0 * sigmaSq));
	return max(0.0, (exp(-v * v) - v * sqrt(BRUNETON_PI) * erfc(v)) / (2.0 * v * sqrt(BRUNETON_PI)));
	//return (exp(-v * v)) / (2.0 * v * sqrt(BRUNETON_PI)); // approximate, faster formula
}

// L, V, N, Tx, Ty in wind space
float ReflectedSunRadiance(float3 L, float3 V, float3 N, float3 Tx, float3 Ty, float2 sigmaSq)
{
	float3 H = normalize(L + V);

	float zetax = dot(H, Tx) / dot(H, N);
	float zetay = dot(H, Ty) / dot(H, N);

	float zL = dot(L, N); // cos of source zenith angle
	float zV = dot(V, N); // cos of receiver zenith angle
	float zH = dot(H, N); // cos of facet normal zenith angle
	float zH2 = zH * zH;

	float p = exp(-0.5 * (zetax * zetax / sigmaSq.x + zetay * zetay / sigmaSq.y)) / (2.0 * BRUNETON_PI * sqrt(sigmaSq.x * sigmaSq.y));

	float tanV = atan2(dot(V, Ty), dot(V, Tx));
	float cosV2 = 1.0 / (1.0 + tanV * tanV);
	float sigmaV2 = sigmaSq.x * cosV2 + sigmaSq.y * (1.0 - cosV2);

	float tanL = atan2(dot(L, Ty), dot(L, Tx));
	float cosL2 = 1.0 / (1.0 + tanL * tanL);
	float sigmaL2 = sigmaSq.x * cosL2 + sigmaSq.y * (1.0 - cosL2);

	float fresnel = 0.02 + 0.98 * pow(1.0 - dot(V, H), 5.0);

	zL = max(zL, 0.01);
	zV = max(zV, 0.01);

	return fresnel * p / ((1.0 + Lambda(zL, sigmaL2) + Lambda(zV, sigmaV2)) * zV * zH2 * zH2 * 4.0);

}

// V, N, Tx, Ty in wind space
float2 U(float4x4 windToWorld, float2 zeta, float3 V, float3 N, float3 Tx, float3 Ty)
{
	float3 f = normalize(float3(-zeta, 1.0)); // tangent space
	float3 F = f.x * Tx + f.y * Ty + f.z * N; // wind space
	float3 R = 2.0 * dot(F, V) * F - V;
	R = mul(windToWorld, float4(R, 0)).xyz;
	return R.xz / (1.0 + R.y);
}

// V, N, Tx, Ty in wind space;
float3 MeanSkyRadiance(Texture2D skyMap, SamplerState skyMapSampler, float4x4 windToWorld, float3 V, float3 N, float3 Tx, float3 Ty, float2 sigmaSq)
{
	float4 result;
	const float eps = 0.001;
	float2 u0 = U(windToWorld, float2(0, 0), V, N, Tx, Ty);
	float2 dux = 2.0 * (U(windToWorld, float2(eps, 0.0), V, N, Tx, Ty) - u0) / eps * sqrt(sigmaSq.x);
	float2 duy = 2.0 * (U(windToWorld, float2(0.0, eps), V, N, Tx, Ty) - u0) / eps * sqrt(sigmaSq.y);

    result = skyMap.SampleGrad(skyMapSampler, u0 * (0.5 / 1.1) + 0.5, dux * (0.5 / 1.1), duy * (0.5 / 1.1));
	//result = tex2D(_SkyMap, u0 * (0.5 / 1.1) + 0.5);

	return result.rgb;
}

#endif