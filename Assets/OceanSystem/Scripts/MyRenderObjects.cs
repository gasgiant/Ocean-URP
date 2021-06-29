using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom("UnityEngine.Experimental.Rendering.LWRP")]
public enum RenderQueueType
{
    Opaque,
    Transparent,
}

public class MyRenderObjects : ScriptableRendererFeature
{
    [System.Serializable]
    public class MyRenderObjectsSettings
    {
        public string passTag = "RenderObjectsFeature";
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        public MyFilterSettings filterSettings = new MyFilterSettings();
    }

    [System.Serializable]
    public class MyFilterSettings
    {
        // TODO: expose opaque, transparent, all ranges as drop down
        public RenderQueueType RenderQueueType;
        public LayerMask LayerMask;
        public string[] PassNames;

        public MyFilterSettings()
        {
            RenderQueueType = RenderQueueType.Opaque;
            LayerMask = 0;
        }
    }

    public MyRenderObjectsSettings settings = new MyRenderObjectsSettings();

    MyRenderObjectsPass renderObjectsPass;

    public override void Create()
    {
        MyFilterSettings filter = settings.filterSettings;

        renderObjectsPass = new MyRenderObjectsPass(settings.passTag, settings.Event, filter.PassNames,
            filter.RenderQueueType, filter.LayerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderObjectsPass);
    }
}

