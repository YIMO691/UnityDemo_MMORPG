using UnityEngine;

public static class MiniMapAssembler
{
    public static void TryInitObjects(ref Camera miniMapCamera, ref MiniMapCameraController miniMapController)
    {
        if (miniMapCamera == null)
        {
            GameObject go = GameObject.Find(ObjectNames.MiniMapCamera);
            if (go != null)
            {
                miniMapCamera = go.GetComponent<Camera>();
            }
        }

        if (miniMapCamera == null)
        {
            Debug.LogWarning("[MiniMapAssembler] miniMapCamera 未找到。");
            return;
        }

        if (miniMapController == null)
        {
            miniMapController = miniMapCamera.GetComponent<MiniMapCameraController>();
        }

        if (miniMapController == null)
        {
            Debug.LogWarning("[MiniMapAssembler] MiniMapCameraController 缺失。");
            return;
        }

        RenderTexture rt = ResourceManager.Instance.Load<RenderTexture>(AssetPaths.MiniMapRenderTexture);
        if (rt == null)
        {
            Debug.LogWarning("[MiniMapAssembler] RenderTexture 加载失败: " + AssetPaths.MiniMapRenderTexture);
            return;
        }

        miniMapCamera.targetTexture = rt;
        MiniMapService.Instance.Register(miniMapCamera, rt, miniMapController);
    }

    public static void BindTarget(Camera miniMapCamera, MiniMapCameraController miniMapController, Transform playerTransform)
    {
        if (miniMapCamera == null || miniMapController == null || playerTransform == null)
        {
            Debug.LogWarning("[MiniMapAssembler] BindTarget 失败，缺少必要对象。");
            return;
        }

        Transform target = playerTransform.Find(ObjectNames.PlayerCameraRoot);
        if (target == null) target = playerTransform;

        miniMapController.SetTarget(target);
        MiniMapService.Instance.BindTarget(target);
        Debug.Log("[MiniMapAssembler] 绑定小地图目标 = " + target.name);
    }
}
