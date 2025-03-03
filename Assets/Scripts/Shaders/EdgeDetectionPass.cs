using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class EdgeDetectionRenderPass : ScriptableRenderPass
{    private EdgeDetectionSettings defaultSettings;
    private Material material;

    private RenderTextureDescriptor edgeDetectionDescriptor;

    public EdgeDetectionRenderPass(Material material, EdgeDetectionSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;

        edgeDetectionDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
    }

    private void UpdateSettings()
    {
        if (material == null) return;

Debug.Log(defaultSettings._EdgeThreshold);

        material.SetFloat("_Threshold", defaultSettings._EdgeThreshold);
        material.SetColor("_EdgeColor", defaultSettings._EdgeColor);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph,
    ContextContainer frameData)
    {
        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Set the blur texture size to be the same as the camera target size.
        edgeDetectionDescriptor.width = cameraData.cameraTargetDescriptor.width;
        edgeDetectionDescriptor.height = cameraData.cameraTargetDescriptor.height;
        edgeDetectionDescriptor.depthBufferBits = 0;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
            edgeDetectionDescriptor, "_EdgeDetectionTexture", false);

        // Update the blur settings in the material
        UpdateSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid() || !dst.IsValid())
            return;

        // The AddBlitPass method adds a vertical blur render graph pass that blits from the source texture (camera color in this case) to the destination texture using the first shader pass (the shader pass is defined in the last parameter).
        RenderGraphUtils.BlitMaterialParameters paraDetect = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraDetect, "EdgeDetectionPass");
    }
}
