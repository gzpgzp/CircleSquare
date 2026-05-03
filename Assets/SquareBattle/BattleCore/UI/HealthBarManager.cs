using System.Collections.Generic;
using Cameras;
using UnityEngine;

namespace SquareBattle.BattleCore.UI
{
    /// <summary>
    /// 血条管理器，挂在 Screen Space Canvas 上
    /// 统一管理所有角色的血条创建、更新、回收
    /// </summary>
    public class HealthBarManager : MonoBehaviour
    {
        public static HealthBarManager Instance { get; private set; }

        [Tooltip("血条 Item 预制体（需包含 HealthBarItem 组件）")]
        [SerializeField] private HealthBarItem healthBarPrefab;

        [Tooltip("世界坐标头顶偏移（世界单位）")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.8f, 0f);

        [Tooltip("用于世界→屏幕坐标转换的摄像机，留空则使用 Camera.main")]
        [SerializeField] private Camera targetCamera;

        // 角色 → 血条 Item 的映射
        private readonly Dictionary<CharacterBase, HealthBarItem> _bars = new();

        // 对象池（回收复用）
        private readonly Queue<HealthBarItem> _pool = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Awake 阶段就确定相机，避免和角色 Start 的时序问题
            if (targetCamera == null)
            {
                targetCamera = CameraManager.Instance != null
                    ? CameraManager.Instance.GetCamera(MyCameraType.Main)
                    : Camera.main;
            }

            // 同步把相机设置到 Canvas（Screen Space - Camera 模式下必须）
            var canvas = GetComponent<Canvas>();
            if (canvas != null && targetCamera != null)
                canvas.worldCamera = targetCamera;
        }

        private void Start()
        {
            // 如果 Awake 时 CameraManager 还未就绪，在 Start 再尝试一次
            if (targetCamera == null)
            {
                targetCamera = CameraManager.Instance != null
                    ? CameraManager.Instance.GetCamera(MyCameraType.Main)
                    : Camera.main;

                // 补一次 Canvas 绑定
                var canvas = GetComponent<Canvas>();
                if (canvas != null && targetCamera != null)
                    canvas.worldCamera = targetCamera;
            }

            if (targetCamera == null)
                Debug.LogWarning("[HealthBarManager] 未找到渲染相机，血条坐标转换将失效！");
        }

        /// <summary>
        /// 为角色注册一个血条（角色 Awake/OnEnable 时调用）
        /// </summary>
        public void Register(CharacterBase character, int currentHp, int maxHp)
        {
            if (_bars.ContainsKey(character)) return;

            HealthBarItem item = GetFromPool();
            item.Init(character.transform, worldOffset, targetCamera);
            item.SetHealth(currentHp, maxHp);
            item.gameObject.SetActive(true);

            _bars[character] = item;
        }

        /// <summary>
        /// 注销并回收角色的血条（角色 OnDisable/Die 时调用）
        /// </summary>
        public void Unregister(CharacterBase character)
        {
            if (!_bars.TryGetValue(character, out HealthBarItem item)) return;

            _bars.Remove(character);

            // item 可能已被销毁（场景切换等），需要先判空
            if (item == null) return;

            item.SetTarget(null);
            item.gameObject.SetActive(false);
            _pool.Enqueue(item);
        }

        /// <summary>
        /// 更新指定角色的血条数值
        /// </summary>
        public void UpdateHealth(CharacterBase character, int currentHp, int maxHp)
        {
            if (_bars.TryGetValue(character, out HealthBarItem item))
            {
                item.SetHealth(currentHp, maxHp);
            }
        }

        private HealthBarItem GetFromPool()
        {
            // 跳过已被销毁的对象
            while (_pool.Count > 0)
            {
                var item = _pool.Dequeue();
                if (item != null) return item;
            }

            return Instantiate(healthBarPrefab, transform);
        }
    }
}
