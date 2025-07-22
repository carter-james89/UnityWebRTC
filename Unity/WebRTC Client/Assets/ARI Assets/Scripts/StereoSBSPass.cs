using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StereoSBSPass : ScriptableRenderPass
{
    private Material _blitMaterial;
    private RTHandle _source;
    private RTHandle _tempRT;

    public StereoSBSPass(RenderPassEvent renderPassEvent, Material blitMaterial)
    {
        this.renderPassEvent = renderPassEvent;
        _blitMaterial = blitMaterial;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        _source = renderingData.cameraData.renderer.cameraColorTargetHandle;

        var desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref _tempRT, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_StereoSBSTempRT");

        ConfigureInput(ScriptableRenderPassInput.Color);
        ConfigureTarget(_source);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        Debug.Log("[StereoSBSPass] Execute is running.");

        if (_blitMaterial == null || _source == null || _tempRT == null)
        {
            Debug.LogWarning("StereoSBSPass: Missing material or RTHandles.");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Stereo SBS XR Blit");

        Blitter.BlitCameraTexture(cmd, _source, _tempRT, _blitMaterial, 0);
        Blitter.BlitCameraTexture(cmd, _tempRT, _source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        _tempRT?.Release();
    }
}
