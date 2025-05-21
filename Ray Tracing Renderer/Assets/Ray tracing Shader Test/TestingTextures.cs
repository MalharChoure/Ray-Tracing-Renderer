using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTextures : MonoBehaviour
{
    public ComputeShader Shader;
    public RenderTexture RenderTexture;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (RenderTexture == null)
        {
            RenderTexture = new RenderTexture(256, 256, 24);
            RenderTexture.enableRandomWrite = true;
            RenderTexture.Create();
        }
        Shader.SetTexture(0, "InputTexture", source);
        Shader.SetTexture(0, "Result", RenderTexture);
        Shader.SetFloat("ResolutionWidth", Camera.main.pixelWidth);
        Shader.SetFloat("ResolutionHeight", Camera.main.pixelHeight);
        Shader.Dispatch(0, RenderTexture.width / 8, RenderTexture.height / 8, 1);

        Graphics.Blit(RenderTexture, destination);
    }
}
