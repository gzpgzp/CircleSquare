using GameFramework;
using UnityEngine;

namespace Adventure.Location
{
    /// <summary>
    /// 地点建筑组件 - 挂载到场景中的3D建筑物上
    /// 通过 locationId 从Luban表读取配置
    /// 需要有Collider才能被射线检测到
    /// </summary>
    public class LocationPoint : MonoBehaviour
    {
        [Header("地点配置")]
        [SerializeField] private ELocationId locationId;

        [Header("表现")]
        [SerializeField] private GameObject highlightEffect;
        [SerializeField] private GameObject nameLabel;

        private cfg.Adventure.LocationInfo cachedInfo;
        private string cachedStringId;

        /// <summary>枚举对应的字符串ID</summary>
        private string StringId => cachedStringId ?? (cachedStringId = locationId.ToStringId());

        /// <summary>从Luban表加载的配置数据</summary>
        public cfg.Adventure.LocationInfo Info
        {
            get
            {
                if (cachedInfo == null)
                {
                    cachedInfo = ConfigManager.Instance.GetLocationInfo(StringId);
                }
                return cachedInfo;
            }
        }

        public string LocationId => StringId;
        public string LocationName => Info?.Name ?? StringId;
        public string Description => Info?.Desc ?? "";
        public int LocationType => Info?.LocationType ?? 0;
        public int UnlockLevel => Info?.UnlockLevel ?? 1;

        /// <summary>
        /// 左键点击 - 打开功能
        /// </summary>
        public void OnClicked()
        {
            Debug.Log($"[LocationPoint] Clicked: {LocationName} (Type:{LocationType})");
            LocationManager.Instance.OnLocationClicked(this);
        }

        /// <summary>
        /// 右键点击 - 触发额外功能（如查看信息、快捷操作等）
        /// </summary>
        public void OnRightClicked()
        {
            Debug.Log($"[LocationPoint] Right Clicked: {LocationName} (Type:{LocationType})");
            LocationManager.Instance.OnLocationRightClicked(this);
        }

        /// <summary>
        /// 鼠标悬停高亮
        /// </summary>
        public void SetHighlight(bool active)
        {
            if (highlightEffect != null)
                highlightEffect.SetActive(active);
        }

        /// <summary>
        /// 获取建筑世界坐标（角色走到这里）
        /// </summary>
        public Vector3 GetPosition()
        {
            return transform.position;
        }
    }
}
