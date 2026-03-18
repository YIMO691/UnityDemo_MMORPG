using UnityEngine;

public class MiniMapService
{
    private static readonly MiniMapService instance = new MiniMapService();
    public static MiniMapService Instance => instance;

    private Camera miniMapCamera;
    private RenderTexture miniMapTexture;
    private MiniMapCameraController miniMapController;

    private MiniMapService() { }

    public void Register(Camera cam, RenderTexture rt, MiniMapCameraController controller)
    {
        miniMapCamera = cam;
        miniMapTexture = rt;
        miniMapController = controller;
    }

    public void BindTarget(Transform target)
    {
        if (miniMapController != null)
        {
            miniMapController.SetTarget(target);
        }
    }

    public Texture GetMiniMapTexture()
    {
        return miniMapTexture;
    }

    public bool IsReady()
    {
        return miniMapCamera != null && miniMapTexture != null && miniMapController != null;
    }

    public void Clear()
    {
        miniMapCamera = null;
        miniMapTexture = null;
        miniMapController = null;
    }
}
