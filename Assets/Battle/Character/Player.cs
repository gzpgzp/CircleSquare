using System;
using Battle.Character.Ability;
using Battle.Character.Ability.A2D;
using UnityEngine;

namespace Battle.Character
{
    public class Player : MonoBehaviour
    {
        [Header("是否限制在横版角度")]
        public bool sideScrollerMode = true; // 横版模式

        private Vector3 mouseWorldPos = Vector3.zero;
        private Vector3 posToMousePosDir = Vector3.zero;

        public Vector3 PosToMousePosDir
        {
            get => posToMousePosDir;
        }

        public GunAim gunAim;
        public DashAbility2D DashAbility2D;

        private void Update()
        {
            if (Camera.main == null) return;

            // 2. 转成世界坐标
            mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            // 3. 方向向量（从枪指向鼠标）
            posToMousePosDir = mouseWorldPos - transform.position;
            
            gunAim.UpdateRotation(posToMousePosDir);
            // DashAbility2D.Dash();
        }
    }
}