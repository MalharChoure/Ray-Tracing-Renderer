using UnityEngine;

public class CameraRaycaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    private Camera _camera;
    private RenderTexture _target;

    public Texture SkyBoxTexture;

    // anti aliasing
    private uint _currentSample = 0;
    private Material _addMaterial;

    // no of bounces for the light ray
    [SerializeField] private int _maxNumberOfBounces = 8;

    // The directional light
    [SerializeField] private Light _directionalLight;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyBoxTexture);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        Vector3 l = _directionalLight.transform.forward;
        RayTracingShader.SetVector("_directionalLight", new Vector4(l.x, l.y, l.z, _directionalLight.intensity));
        RayTracingShader.SetVector("_directionalLightColor", new Vector4(_directionalLight.color.r, _directionalLight.color.g, _directionalLight.color.b, _directionalLight.color.a));
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetFloat("_maxNumberOfBounces", _maxNumberOfBounces);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
    }
}
