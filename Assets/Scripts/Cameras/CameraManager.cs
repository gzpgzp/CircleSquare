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
    }
}