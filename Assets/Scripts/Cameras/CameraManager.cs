using System;
using Tools.Singletons;
using UnityEngine;

namespace Cameras
{
    public enum MyCameraType
    {
        None,
        Main,
        UI,
    }

    public class CameraManager : MMSingleton<CameraManager>
    {
        [SerializeField] private Camera uiCamera;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CameraFollow cameraFollow;

        private Vector3 startPos = new Vector3(0,0,-10);
        
        public Camera GetCamera(MyCameraType cameraType)
        {
            switch (cameraType)
            {
                case MyCameraType.Main:
                    return mainCamera;
                case MyCameraType.UI:
                    return uiCamera;
                default:
                    return null;
            }
        }

        public void ResetMainCameraPos()
        {
            mainCamera.transform.position = startPos;
        }

        public void MainCameraFollowTarget(Transform target)
        {
            cameraFollow.SetFollow(mainCamera.transform, target);
        }

        public void ChangeCameraFollowLimit(int index)
        {
            cameraFollow.ChangeLimits(index);
        }
    }
}