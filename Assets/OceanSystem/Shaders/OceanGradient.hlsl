#if !defined(OCEAN_GRADIENT_INCLUDED)
#define OCEAN_GRADIENT_INCLUDED
#define GRADIENT_MAX_KEYS 8

struct Gradient
{
    float4 colors[GRADIENT_MAX_KEYS];
    int colorsCount;
    bool type;
};

Gradient CreateGradient(float4 colors[GRADIENT_MAX_KEYS], float2 params)
{
    Gradient g;
    g.colors = colors;
    g.colorsCount = params.x;
    g.type = params.y;
    return g;
}

float3 SampleGradient(Gradient grad, float t)
{
    float3 color = grad.colors[0].rgb;
    [unroll]
    for (int i = 1; i < GRADIENT_MAX_KEYS; i++)
    {
        float colorPos = saturate((t - grad.colors[i - 1].w) / (grad.colors[i].w - grad.colors[i - 1].w)) * step(i, grad.colorsCount - 1);
        color = lerp(color, grad.colors[i].rgb, lerp(colorPos, step(0.01, colorPos), grad.type));
    }
    return color;
}
#endif