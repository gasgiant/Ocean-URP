using UnityEngine;

public static class GradientTexture
{
    const int resolution = 128;
    static readonly Color[] colors = new Color[resolution];

    public static Texture2D BakeGradient(Gradient gradient)
    {
        Texture2D tex = new Texture2D(resolution, 1, TextureFormat.RGBAHalf, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        BakeGradient(gradient, ref tex);
        return tex;
    }

    public static void BakeGradient(Gradient gradient, ref Texture2D tex)
    {
        if (tex == null || tex.width != resolution)
        {
            tex = new Texture2D(resolution, 1, TextureFormat.RGBAHalf, false, true);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
        }

        for (int i = 0; i < resolution; i++)
        {
            float x = (float)i / resolution;
            colors[i] = gradient.Evaluate(x).linear;
        }

        tex.SetPixels(colors);
        tex.Apply(false);
    }
}
