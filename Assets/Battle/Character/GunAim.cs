using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Character
{
    public class GunAim : MonoBehaviour
    {
        [Header("是否限制在横版角度")]
        public bool sideScrollerMode = true; // 横版模式
        
        public void UpdateRotation(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.001f)
                return;
            
            // 4. 转成角度
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 5. 横版游戏常见修正
            if (sideScrollerMode)
            {
                // 防止枪倒转（只允许指向前方180度内）
                if (angle > 90f || angle < -90f)
                {
                    transform.localScale = new Vector3(1, -1, 1);  // 翻转
                }
                else
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
            }

            // 6. 应用旋转
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
