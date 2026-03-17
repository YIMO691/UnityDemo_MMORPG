using UnityEngine;
using Game;
using Cinemachine;

public static class CameraRigAssembler
{
    public static bool TryCreate(out GameObject mainCameraInstance, out GameObject followCameraInstance)
    {
        mainCameraInstance = null;
        followCameraInstance = null;

        GameObject mainCameraPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.MainCamera);
        GameObject followCameraPrefab = ResourceManager.Instance.Load<GameObject>(AssetPaths.PlayerFollowCamera);

        if (mainCameraPrefab == null)
        {
            var go = new GameObject("MainCamera");
            go.tag = "MainCamera";
            go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            // 尝试挂载 CinemachineBrain（若存在包）
            var brainType = System.Type.GetType("Cinemachine.CinemachineBrain, Cinemachine");
            if (brainType != null) go.AddComponent(brainType);
            mainCameraInstance = go;
        }
        else
        {
            mainCameraInstance = Object.Instantiate(mainCameraPrefab);
        }

        if (followCameraPrefab != null)
        {
            followCameraInstance = Object.Instantiate(followCameraPrefab);
        }

        return mainCameraInstance != null;
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

    public static void FallbackBind(ThirdPersonController controller, GameObject mainCameraInstance)
    {
        if (controller == null || mainCameraInstance == null) return;
        Transform target = controller.CinemachineCameraTarget != null
            ? controller.CinemachineCameraTarget.transform
            : controller.transform;

        Vector3 back = -target.forward.normalized;
        Vector3 camPos = target.position + back * 4f + Vector3.up * 2f;
        mainCameraInstance.transform.position = camPos;
        mainCameraInstance.transform.rotation = Quaternion.LookRotation((target.position - camPos).normalized, Vector3.up);
    }
}
