using UnityEngine;
using UnityEngine.Rendering;

namespace OceanSystem
{
    public static class RenderingUtils
    {
        public static void DrawPreceduralFullscreenQuad(CommandBuffer cmd, RenderTargetIdentifier target,
            RenderBufferLoadAction loadAction, Material material, int pass)
        {
            cmd.SetRenderTarget(target, loadAction, RenderBufferStoreAction.Store);
            cmd.DrawProcedural(Matrix4x4.identity, material, pass, MeshTopology.Quads, 4, 1, null);
        }

        public static RenderTexture CreateRenderTexture(
            RenderTextureDescriptor descriptor, 
            TextureWrapMode wrapMode, FilterMode filterMode, 
            int anisoLevel)
        {
            RenderTexture rt = new RenderTexture(descriptor)
            {
                anisoLevel = filterMode == FilterMode.Trilinear ? anisoLevel : 0,
                wrapMode = wrapMode,
                filterMode = filterMode
            };
            rt.Create();
            return rt;
        }
    }
}
