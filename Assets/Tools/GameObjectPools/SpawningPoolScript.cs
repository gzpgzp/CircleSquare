    //
// Spawning Pool for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// http://www.digitalruby.com/unity-plugins/
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace Tools.GameObjectPools
{
    /// <summary>
    /// This script only needs to be in the first object in your scene and is an alternative way to setup prefabs in the inspector, rather than code.
    /// </summary>
    public class SpawningPoolScript : MonoBehaviour
    {
        private static SpawningPoolScript instance;
        public static SpawningPoolScript Instance { get { return instance; } }

        private bool awake;

        [System.Serializable]
        public struct SpawningPoolEntry
        {
            public string Key;
            public GameObject Prefab;
        }

        [Tooltip("List of prefabs to add to the spawning pool upon start")]
        public SpawningPoolEntry[] Prefabs;

        [Tooltip("Whether to return spawned objects to cache on level load.")]
        public bool ReturnToCacheOnLevelLoad = true;

        private void SceneWasLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (ReturnToCacheOnLevelLoad)
            {
                SpawningPool.RecycleActiveObjects();
            }
        }

        private void Awake()
        {
            if (awake)
            {
                return;
            }
            else if (instance == null)
            {
                awake = true;
                instance = this;
                DontDestroyOnLoad(this);
                if (Prefabs != null)
                {
                    foreach (var prefab in Prefabs)
                    {
                        SpawningPool.AddPrefab(prefab.Key, prefab.Prefab);
                    }
                }
				UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneWasLoaded;
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Contains methods to assist in object caching / pooling
    /// </summary>
    public static class SpawningPool
    {
        private static readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        private static readonly Dictionary<GameObject, string> activeGameObjectsAndKeys = new Dictionary<GameObject, string>();
        private static readonly Dictionary<string, List<GameObject>> cachedKeysAndGameObjects = new Dictionary<string, List<GameObject>>();
        private static readonly Dictionary<string, List<GameObject>> activeKeysAndGameObjects = new Dictionary<string, List<GameObject>>();
        private static readonly Dictionary<GameObject, IPooledObject> interfaces = new Dictionary<GameObject, IPooledObject>();
        private static int cacheCount;

        /// <summary>
        /// The hide flags to apply to newly created objects
        /// </summary>
        //public static HideFlags DefaultHideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
        public static HideFlags DefaultHideFlags = HideFlags.None;
        private static List<GameObject> GetOrCreateCacheList(string key)
        {
            List<GameObject> list;
            if (!cachedKeysAndGameObjects.TryGetValue(key, out list))
            {
                list = new List<GameObject>();
                cachedKeysAndGameObjects[key] = list;
            }

            return list;
        }

        private static void ActivateObject(GameObject obj, string key)
        {
            obj.SetActive(true);
            activeGameObjectsAndKeys[obj] = key;
            List<GameObject> list;
            if (!activeKeysAndGameObjects.TryGetValue(key, out list))
            {
                list = new List<GameObject>();
                activeKeysAndGameObjects[key] = list;
            }
            list.Add(obj);
        }

        /// <summary>
        /// Add (or replace) a prefab with a key and source object
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <param name="prefab">Source object</param>
        public static void AddPrefab(string key, GameObject prefab)
        {
            GameObject.DontDestroyOnLoad(prefab);
            prefabs[key] = prefab;
        }

        /// <summary>
        /// Remove a prefab, all cached objects and active objects
        /// </summary>
        /// <param name="key">Unique key to remove</param>
        public static void RemovePrefab(string key)
        {
            if (prefabs.Remove(key))
            {
                List<GameObject> list = GetOrCreateCacheList(key);
                foreach (GameObject obj in list)
                {
                    Object.Destroy(obj);
                }
                cacheCount -= list.Count;
                cachedKeysAndGameObjects.Remove(key);
                if (activeKeysAndGameObjects.TryGetValue(key, out list))
                {
                    activeKeysAndGameObjects.Remove(key);
                    foreach (GameObject obj in list)
                    {
                        activeGameObjectsAndKeys.Remove(obj);
                        Object.Destroy(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the spawning pool contains a prefab with the key
        /// </summary>
        /// <param name="key">Key to check for.</param>
        /// <returns>True if a prefab with the key exists, false otherwise</returns>
        public static bool ContainsPrefab(string key)
        {
            return prefabs.ContainsKey(key);
        }

        /// <summary>
        /// Removes all prefabs and cached objects from memory
        /// </summary>
        public static void RemoveAllPrefabs()
        {
            // make copy of keys so we can remove during iteration
            string[] keys = new string[prefabs.Keys.Count];
            prefabs.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
            {
                RemovePrefab(key);
            }
            prefabs.Clear();
        }

        /// <summary>
        /// Pull an object from the cache, or create new if not found in cache
        /// </summary>
        /// <param name="key">Unique key</param>
        /// <returns>Instance of the prefab for the specified key, or null if error creating</returns>
        public static GameObject CreateFromCache(string key)
        {
            List<GameObject> list = GetOrCreateCacheList(key);
            GameObject pooledObject;
            if (list.Count == 0)
            {
                GameObject prefab;
                if (!prefabs.TryGetValue(key, out prefab))
                {
                    return null;
                }
                pooledObject = GameObject.Instantiate(prefab);
                GameObject.DontDestroyOnLoad(pooledObject);
                pooledObject.hideFlags = DefaultHideFlags;
                IPooledObject pooledInterface = pooledObject.GetComponent<IPooledObject>();
                if (pooledInterface != null)
                    interfaces[pooledObject] = pooledInterface;
            }
            else
            {
                int index = list.Count - 1;
                pooledObject = list[index];
                list.RemoveAt(index);
                cacheCount--;
                IPooledObject pooledInterface;
                if (interfaces.TryGetValue(pooledObject, out pooledInterface))
                    pooledInterface.OnSpawned();
            }

            ActivateObject(pooledObject, key);
            return pooledObject;
        }

        /// <summary>
        /// Returns an object to the cache
        /// </summary>
        /// <param name="pooledObject">The pooled object that was created</param>
        /// <returns>True if recycled back into the pool, false if error</returns>
        public static bool ReturnToCache(this GameObject pooledObject)
        {
            string key;
            if (!activeGameObjectsAndKeys.TryGetValue(pooledObject, out key))
            {
                return false;
            }
            return ReturnToCache(pooledObject, key);
        }

        /// <summary>
        /// Returns and object to the cache
        /// </summary>
        /// <param name="pooledObject">The pooled object that was created</param>
        /// <param name="key">The key for the object</param>
        /// <returns>True if recycled back into the pool, false if error</returns>
        public static bool ReturnToCache(this GameObject pooledObject, string key)
        {
            if (pooledObject == null)
            {
                // destroyed, do not re-use
                return false;
            }

            List<GameObject> list;
            if (!cachedKeysAndGameObjects.TryGetValue(key, out list))
            {
                return false;
            }
            list.Add(pooledObject);
            activeGameObjectsAndKeys.Remove(pooledObject);
            if (activeKeysAndGameObjects.TryGetValue(key, out list))
            {
                list.Remove(pooledObject);
            }
            pooledObject.SetActive(false);
            cacheCount++;
            IPooledObject pooledInterface;
            if (interfaces.TryGetValue(pooledObject, out pooledInterface))
                pooledInterface.OnDespawned();
            return true;
        }

        /// <summary>
        /// Enumerate all active objects that have been pulled from cache or created new using the CreateFromCache method
        /// </summary>
        /// <returns>Enumerable of active objects</returns>
        public static IEnumerable<KeyValuePair<GameObject, string>> EnumerateActiveObjects()
        {
            foreach (var v in activeGameObjectsAndKeys)
            {
                yield return v;
            }
        }

        /// <summary>
        /// Recycle all active objects and return them to the cache
        /// </summary>
        public static void RecycleActiveObjects()
        {
            foreach (var v in activeGameObjectsAndKeys)
            {
                List<GameObject> list;
                if (cachedKeysAndGameObjects.TryGetValue(v.Value, out list))
                {
                    list.Add(v.Key);
                    v.Key.SetActive(false);
                    cacheCount++;
                }
            }
            activeGameObjectsAndKeys.Clear();
            activeKeysAndGameObjects.Clear();
        }

        /// <summary>
        /// Clear out all caches and optionally all active objects. Does not remove prefabs.
        /// </summary>
        public static void Clear(bool clearActiveObjects)
        {
            if (clearActiveObjects)
            {
                foreach (var v in activeGameObjectsAndKeys)
                {
                    Object.Destroy(v.Key);
                }
                activeGameObjectsAndKeys.Clear();
                activeKeysAndGameObjects.Clear();
            }
            foreach (var keyValue in cachedKeysAndGameObjects)
            {
                foreach (var gameObjectToDestroy in keyValue.Value)
                {
                    Object.Destroy(gameObjectToDestroy);
                }
            }
            cachedKeysAndGameObjects.Clear();
            cacheCount = 0;
        }

        /// <summary>
        /// Get the number of active objects for a specific key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Number of active objects for that key</returns>
        public static int ActiveCountForKey(string key)
        {
            List<GameObject> list;
            if (activeKeysAndGameObjects.TryGetValue(key, out list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// Gets the number of cached objects. Does not include active objects.
        /// </summary>
        public static int CacheCount
        {
            get { return cacheCount; }
        }

        /// <summary>
        /// Gets the number of active objects. Does not include cached objects.
        /// </summary>
        public static int ActiveCount
        {
            get { return activeGameObjectsAndKeys.Count; }
        }
    }

    /// <summary>
    /// 对象池生命周期回调接口（可选）。
    /// 业务脚本实现此接口以接收池化事件，禁止在接口中添加主动操作方法。
    /// OnSpawned：对象从池中取出时回调，用于重置运行时状态。
    /// OnDespawned：对象归还池时回调，用于清理引用。
    /// </summary>
    public interface IPooledObject
    {
        void OnSpawned();
        void OnDespawned();
    }

}