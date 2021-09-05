using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class TextureFromShader : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private int pass;
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;
    [SerializeField] private string savePath = "Texture";

    [ContextMenu("SaveTexture")]
    public void Savetexture()
    {
        RenderTexture target = new RenderTexture(width, height, 0, RenderTextureFormat.DefaultHDR);
        target.Create();

        Graphics.Blit(null, target, material, pass);
        Texture2D tex = new Texture2D(width, height);
        RenderTexture.active = target;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/" + savePath + ".png", bytes);
        AssetDatabase.ImportAsset("Assets/" + savePath + ".png");
    }
}
