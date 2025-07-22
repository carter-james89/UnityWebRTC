using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StereoSBSFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class StereoSBSSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Material sbsMaterial;
    }

    public StereoSBSSettings settings = new StereoSBSSettings();
    private StereoSBSPass _pass;

    public override void Create()
    {
        _pass = new StereoSBSPass(settings.renderPassEvent, settings.sbsMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}
