using Tools.Singletons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tools.ResourcesTool
{
    public class MyResourcesManager : Singleton<MyResourcesManager>
    {
        public void Init()
        {
            
        }

        public T Load<T>(string objName) where T : Object
        {
            return Resources.Load<T>(objName);
        }

        public GameObject LoadAndInstantiate(GameObject prefab)
        {
            var obj = GameObject.Instantiate(prefab);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        public GameObject LoadAndInstantiate(GameObject prefab,Transform parent)
        {
            var obj = GameObject.Instantiate(prefab,parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }
    
        public GameObject LoadAndInstantiate(string objName) 
        {
            return LoadAndInstantiate(Load<GameObject>(objName));
        }

        public GameObject LoadAndInstantiate(string objName,Transform parent)
        {
            return LoadAndInstantiate(Load<GameObject>(objName), parent);
        }
    }
}
