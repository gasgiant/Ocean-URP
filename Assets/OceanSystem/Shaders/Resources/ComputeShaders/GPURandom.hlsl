//Source: https://www.shadertoy.com/view/Xt3cDn

//Trivial modifications made to the code to translate it to HLSL by Huw Bowles

//Quality hashes collection
//by nimitz 2018 (twitter: @stormoid)

//The MIT License
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#ifndef GPU_RANDOM_INCLUDED
#define GPU_RANDOM_INCLUDED

uint baseHash(uint3 p)
{
	p = 1103515245U * ((p.xyz >> 1U) ^ (p.yzx));
	uint h32 = 1103515245U * ((p.x ^ p.z) ^ (p.y >> 3U));
	return h32 ^ (h32 >> 16);
}

float hash13(uint3 x)
{
	uint n = baseHash(x);
	return float(n) * (1.0 / float(0xffffffffU));
}

float2 hash23(float3 x)
{
	uint n = baseHash(x);
	uint2 rz = uint2(n, n * 48271U); //see: http://random.mat.sbg.ac.at/results/karl/server/node4.html
	return float2(rz.xy & (uint2) 0x7fffffffU) / float(0x7fffffff);
}

float3 hash33(uint3 x)
{
	uint n = baseHash(x);
	uint3 rz = uint3(n, n * 16807U, n * 48271U); //see: http://random.mat.sbg.ac.at/results/karl/server/node4.html
	return float3(rz & (uint3) 0x7fffffffU) / float(0x7fffffff);
}
#endif
