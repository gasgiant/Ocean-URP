using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;


public class MyRenderObjectsPass : ScriptableRenderPass
{
    RenderQueueType renderQueueType;
    FilteringSettings m_FilteringSettings;
    string m_ProfilerTag;
    ProfilingSampler m_ProfilingSampler;

    List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

    RenderStateBlock m_RenderStateBlock;

    public MyRenderObjectsPass(string profilerTag, RenderPassEvent renderPassEvent, string[] shaderTags, RenderQueueType renderQueueType, int layerMask)
    {
        base.profilingSampler = new ProfilingSampler(nameof(MyRenderObjectsPass));

        m_ProfilerTag = profilerTag;
        m_ProfilingSampler = new ProfilingSampler(profilerTag);
        this.renderPassEvent = renderPassEvent;
        this.renderQueueType = renderQueueType;
        RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
            ? RenderQueueRange.transparent
            : RenderQueueRange.opaque;
        m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);

        if (shaderTags != null && shaderTags.Length > 0)
        {
            foreach (var passName in shaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(passName));
        }
        else
        {
            m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
        }

        m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        SortingCriteria sortingCriteria = (renderQueueType == RenderQueueType.Transparent)
            ? SortingCriteria.CommonTransparent
            : renderingData.cameraData.defaultOpaqueSortFlags;

        DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);

        ref CameraData cameraData = ref renderingData.cameraData;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings,
                ref m_RenderStateBlock);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}

