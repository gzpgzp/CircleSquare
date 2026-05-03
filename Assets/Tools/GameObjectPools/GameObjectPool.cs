using Tools.ResourcesTool;
using Tools.Singletons;
using UnityEngine;

namespace Tools.GameObjectPools
{
    public class GameObjectPool : Singleton<GameObjectPool>
    {
        /// <summary>
        /// 直接注册一个预制体到对象池（key 使用 prefab.name）
        /// </summary>
        public void Register(GameObject prefab)
        {
            SpawningPool.AddPrefab(prefab.name, prefab);
        }

        /// <summary>
        /// 从 Resources 路径加载并注册到对象池（key 使用 prefab.name）
        /// </summary>
        public void Load(string objectPath)
        {
            var obj = MyResourcesManager.Instance.Load<GameObject>(objectPath);
            SpawningPool.AddPrefab(obj.name, obj);
        }

        /// <summary>
        /// 从 Resources 路径加载并注册到对象池（自定义 key）
        /// </summary>
        public void Load(string key, string objectPath)
        {
            var obj = MyResourcesManager.Instance.Load<GameObject>(objectPath);
            SpawningPool.AddPrefab(key, obj);
        }

        public GameObject CreateGameObject(string objectName)
        {
            if (!SpawningPool.ContainsPrefab(objectName))
            {
                Debug.LogError($"[GameObjectPool] 预制体 '{objectName}' 未注册，请在使用前调用 Register() 注册。");
                return null;
            }
            return SpawningPool.CreateFromCache(objectName);
        }

        public void RemoveGameObject(GameObject obj)
        {
            SpawningPool.ReturnToCache(obj);
        }

        public void ClearAll()
        {
            SpawningPool.RemoveAllPrefabs();
        }
    }
}