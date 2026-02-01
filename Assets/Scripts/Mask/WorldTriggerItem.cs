using System;
using UnityEngine;

namespace Mask
{
    public abstract class WorldTriggerItem :  MonoBehaviour
    {
        protected abstract void OnTrigger(GameObject go);

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnTrigger(other.gameObject);    
            }
        }
    }
}