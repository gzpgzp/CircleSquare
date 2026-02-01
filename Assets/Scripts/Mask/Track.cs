using UnityEngine;

namespace Mask
{
    public class Track : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.gameObject.GetComponent<PlayerController2D>();
                player.Die();
            }
        }
    }
}