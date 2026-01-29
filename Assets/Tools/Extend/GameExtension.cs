using UnityEngine;

namespace Tools.Extend
{
    public static class GameExtension
    {
        public static T TryGetOrAddComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            if (obj == null)
            {
                Debug.LogError("Get Component Object is null");
                return null;
            }

            if (obj.TryGetComponent<T>(out var co))
            {
                return co;
            }

            return obj.AddComponent<T>();
        }
    }
}