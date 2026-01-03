using UnityEngine;

namespace Tools.Extend
{
    public static class ComponentExtension
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var comp)) return comp;
            return go.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this Component c) where T : Component
        {
            return c.gameObject.GetOrAddComponent<T>();
        }
    }
}