using System;
using UnityEngine;

namespace Mask
{
    public class WallCollider : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var dir = transform.position - other.transform.position;
                other.gameObject.GetComponent<Rigidbody2D>().AddForce(dir.normalized, ForceMode2D.Impulse);
            }
        }
    }
}