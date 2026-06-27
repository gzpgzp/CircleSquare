using Adventure.Location;
using UnityEngine;

namespace Adventure.Interaction
{
    /// <summary>
    /// 建筑交互处理器 - 继承自 InteractiveObjectHandler
    /// 功能：悬停高亮、短按打开功能、长按拖动、右键打开菜单
    /// </summary>
    public class LocationInteractionHandler : InteractiveObjectHandler
    {
        // ========== 实现抽象方法 ==========

        protected override MonoBehaviour GetInteractiveObjectFromHit(RaycastHit hit)
        {
            return hit.collider.GetComponent<LocationPoint>();
        }

        protected override Vector3 GetObjectPosition(MonoBehaviour obj)
        {
            if (obj is LocationPoint location)
                return location.transform.position;
            return Vector3.zero;
        }

        // ========== 重写虚拟方法 ==========

        protected override void OnHoverEnter(MonoBehaviour obj)
        {
            if (obj is LocationPoint location)
            {
                location.SetHighlight(true);
            }
        }

        protected override void OnHoverExit(MonoBehaviour obj)
        {
            if (obj is LocationPoint location)
            {
                location.SetHighlight(false);
            }
        }

        protected override void OnShortClick(MonoBehaviour obj)
        {
            if (obj is LocationPoint location)
            {
                location.OnClicked();
            }
        }

        protected override void OnDragging(MonoBehaviour obj, Vector3 newPosition)
        {
            if (obj is LocationPoint location)
            {
                newPosition.z = location.transform.position.z; // 保持Z不变
                location.transform.position = newPosition;
            }
        }

        protected override void OnRightClick(MonoBehaviour obj)
        {
            if (obj is LocationPoint location)
            {
                location.OnRightClicked();
            }
        }
    }
}
