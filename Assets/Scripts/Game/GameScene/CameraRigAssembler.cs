using Cinemachine;
using StarterAssets;
using UnityEngine;

public static class CameraRigAssembler
{
    public static bool TryCreate(out GameObject mainCameraInstance, out GameObject followCameraInstance)
    {
        mainCameraInstance = null;
        followCameraInstance = null;

        GameObject mainCameraPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.MainCamera);
        GameObject followCameraPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PlayerFollowCamera);
        if (mainCameraPrefab == null || followCameraPrefab == null) return false;

        mainCameraInstance = Object.Instantiate(mainCameraPrefab);
        followCameraInstance = Object.Instantiate(followCameraPrefab);
        return true;
    }

    public static bool TryBind(ThirdPersonController controller, GameObject followCameraInstance)
    {
        if (controller == null || followCameraInstance == null) return false;

        CinemachineVirtualCamera vcam = followCameraInstance.GetComponent<CinemachineVirtualCamera>();
        if (vcam == null) return false;
        if (controller.CinemachineCameraTarget == null) return false;

        vcam.Follow = controller.CinemachineCameraTarget.transform;
        vcam.LookAt = controller.CinemachineCameraTarget.transform;
        return true;
    }
}
