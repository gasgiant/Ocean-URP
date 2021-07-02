using UnityEngine;

public static class GradientTexture
{
    private const int Resolution = 128;
    private static readonly Color[] _colors = new Color[Resolution];

    public static Texture2D BakeGradient(Gradient gradient)
    {
        Texture2D tex = new Texture2D(Resolution, 1, TextureFormat.RGBAHalf, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        BakeGradient(gradient, ref tex);
        return tex;
    }

    public static void BakeGradient(Gradient gradient, ref Texture2D tex)
    {
        if (tex == null || tex.width != Resolution)
        {
            tex = new Texture2D(Resolution, 1, TextureFormat.RGBAHalf, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
        }

        for (int i = 0; i < Resolution; i++)
        {
            float x = (float)i / Resolution;
            _colors[i] = gradient.Evaluate(x).linear;
        }

        tex.SetPixels(_colors);
        tex.Apply(false);
    }
}
