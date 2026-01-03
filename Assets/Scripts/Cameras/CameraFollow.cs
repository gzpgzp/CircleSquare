using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cameras
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("要跟随的目标（玩家）")]
        public Transform target;

        [Header("平滑度（越大越平滑越慢）")]
        public float smoothSpeed = 5f;

        [Header("跟随偏移（通常 Z 设为 -10）")]
        public Vector3 offset = new Vector3(0, 0, -10);

        [Header("是否锁住Y轴（横版游戏常用）")]
        public bool lockY = true;

        private Vector3 velocity = Vector3.zero;

        void LateUpdate()
        {
            if (target == null) return;

            // 1. 计算目标位置
            Vector3 targetPos = target.position + offset;

            if (lockY)
            {
                // 保持相机的原来Y
                targetPos.y = transform.position.y;
            }

            // 2. 平滑插值
            Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

            transform.position = smoothPos;
        }
    }
}
