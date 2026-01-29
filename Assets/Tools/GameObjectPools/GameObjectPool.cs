using Tools.ResourcesTool;
using Tools.Singletons;
using UnityEngine;

namespace Tools.GameObjectPools
{
    public class GameObjectPool : Singleton<GameObjectPool> 
    {
        public void Load(string objectPath)
        {
            var obj = MyResourcesManager.Instance.Load<GameObject>(objectPath);
            SpawningPool.AddPrefab(obj.name, obj);
        }

        public GameObject CreateGameObject(string objectName)
        {
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