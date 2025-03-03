using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EdgeDetectionFeature : ScriptableRendererFeature
{
  [SerializeField] private EdgeDetectionSettings settings;
  [SerializeField] private Shader shader;
  private Material material;
  private EdgeDetectionRenderPass edgeDetectionRenderPass;

  public override void Create()
  {
    if (shader == null)
    {
      return;
    }
    material = new Material(shader);
    edgeDetectionRenderPass = new EdgeDetectionRenderPass(material, settings);

    edgeDetectionRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
  }

  public override void AddRenderPasses(ScriptableRenderer renderer,
      ref RenderingData renderingData)
  {
    if (edgeDetectionRenderPass == null)
    {
      return;
    }
    if (renderingData.cameraData.cameraType == CameraType.Game)
    {
      renderer.EnqueuePass(edgeDetectionRenderPass);
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (Application.isPlaying)
    {
      Destroy(material);
    }
    else
    {
      DestroyImmediate(material);
    }
  }
}

[Serializable]
public class EdgeDetectionSettings
{
  public float _EdgeThreshold;
  public Color _EdgeColor;
}
