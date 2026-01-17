using System;
using Cameras;
using Unity.Collections;
using UnityEngine;

namespace Tools.Dialogs
{
    public enum GameUIType
    {
        None= 0,
        Window = 1,
        Top = 2,
        Dialogs = 3,
        Tips = 4,
    }
    
    public class BaseUI : MonoBehaviour
    {
        [SerializeField, ReadOnly] private GameUIType UIType = GameUIType.None;
        [SerializeField] private int order;
        [SerializeField] private Camera renderCamera;

        private void Start()
        {
            if (renderCamera == null)
            {
                renderCamera = CameraManager.Instance.GetCamera(MyCameraType.UI);
            }
        }
    }
}