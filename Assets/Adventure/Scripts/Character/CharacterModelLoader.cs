using System;
using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 角色模型加载器 - 动态加载/切换角色模型
    /// 预制体只挂载逻辑脚本，模型通过此组件动态加载
    /// </summary>
    public class CharacterModelLoader : MonoBehaviour
    {
        [Header("模型容器")]
        [Tooltip("模型实例化到此位置，不设置则自动创建子物体")]
        [SerializeField] private Transform modelContainer;

        [Header("模型配置")]
        [Tooltip("默认模型预制体路径（Resources文件夹下，不需要扩展名）")]
        [SerializeField] private string defaultModelPath = "Prefabs/Adventure/Models/Robot";

        /// <summary>当前加载的模型</summary>
        public GameObject CurrentModel { get; private set; }

        /// <summary>当前模型ID</summary>
        public string CurrentModelId { get; private set; }

        /// <summary>模型切换事件</summary>
        public event Action<string> onModelChanged;

        /// <summary>模型加载完成事件</summary>
        public event Action<GameObject> onModelLoaded;

        /// <summary>模型容器Transform</summary>
        public Transform ModelContainer => modelContainer;

        private void Awake()
        {
            // 如果没有设置容器，自动创建一个子物体
            if (modelContainer == null)
            {
                GameObject containerObj = new GameObject("ModelContainer");
                containerObj.transform.SetParent(transform, false);
                containerObj.transform.localPosition = Vector3.zero;
                containerObj.transform.localRotation = Quaternion.identity;
                containerObj.transform.localScale = Vector3.one;
                modelContainer = containerObj.transform;
            }
        }

        private void Start()
        {
            // 加载默认模型
            if (!string.IsNullOrEmpty(defaultModelPath))
            {
                LoadModel(defaultModelPath);
            }
        }

        /// <summary>
        /// 加载角色模型（同步）
        /// </summary>
        /// <param name="modelPath">Resources文件夹下的路径</param>
        public void LoadModel(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogWarning("[ModelLoader] 模型路径为空！");
                return;
            }

            // 从Resources加载预制体
            GameObject prefab = Resources.Load<GameObject>(modelPath);
            if (prefab == null)
            {
                Debug.LogError($"[ModelLoader] 找不到模型预制体: {modelPath}");
                return;
            }

            // 卸载旧模型
            UnloadModel();

            // 实例化新模型
            CurrentModel = Instantiate(prefab, modelContainer.position, Quaternion.identity, modelContainer);
            CurrentModelId = modelPath;

            // 重置位置和旋转
            CurrentModel.transform.localPosition = Vector3.zero;
            CurrentModel.transform.localRotation = Quaternion.identity;

            Debug.Log($"[ModelLoader] 加载模型: {modelPath}");

            // 触发事件
            onModelLoaded?.Invoke(CurrentModel);
            onModelChanged?.Invoke(CurrentModelId);
        }

        /// <summary>
        /// 加载角色模型（异步）
        /// </summary>
        public void LoadModelAsync(string modelPath, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(modelPath))
            {
                Debug.LogWarning("[ModelLoader] 模型路径为空！");
                return;
            }

            // 异步加载
            var request = Resources.LoadAsync<GameObject>(modelPath);
            request.completed += operation =>
            {
                GameObject prefab = request.asset as GameObject;
                if (prefab == null)
                {
                    Debug.LogError($"[ModelLoader] 异步加载失败: {modelPath}");
                    return;
                }

                // 卸载旧模型
                UnloadModel();

                // 实例化新模型
                CurrentModel = Instantiate(prefab, modelContainer.position, Quaternion.identity, modelContainer);
                CurrentModelId = modelPath;

                // 重置位置和旋转
                CurrentModel.transform.localPosition = Vector3.zero;
                CurrentModel.transform.localRotation = Quaternion.identity;

                Debug.Log($"[ModelLoader] 异步加载模型: {modelPath}");

                // 触发事件
                onModelLoaded?.Invoke(CurrentModel);
                onModelChanged?.Invoke(CurrentModelId);
                onComplete?.Invoke();
            };
        }

        /// <summary>
        /// 卸载当前模型
        /// </summary>
        public void UnloadModel()
        {
            if (CurrentModel != null)
            {
                Destroy(CurrentModel);
                CurrentModel = null;
                CurrentModelId = null;
                Debug.Log("[ModelLoader] 卸载模型");
            }
        }

        /// <summary>
        /// 切换模型（如果已加载相同模型则跳过）
        /// </summary>
        public void SwitchModel(string modelPath)
        {
            if (CurrentModelId == modelPath)
            {
                Debug.Log($"[ModelLoader] 模型已加载: {modelPath}，跳过");
                return;
            }
            LoadModel(modelPath);
        }

        /// <summary>
        /// 获取当前模型上的Animator
        /// </summary>
        public Animator GetAnimator()
        {
            if (CurrentModel == null) return null;
            return CurrentModel.GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// 设置模型层（用于渲染排序）
        /// </summary>
        public void SetModelLayer(int layer)
        {
            if (CurrentModel == null) return;
            SetLayerRecursive(CurrentModel, layer);
        }

        private void SetLayerRecursive(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}
