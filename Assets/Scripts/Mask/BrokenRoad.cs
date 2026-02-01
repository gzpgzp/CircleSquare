using System;
using UnityEngine;

namespace Mask
{
    public class BrokenRoad : MonoBehaviour
    {
        [SerializeField] private GameObject afx;

        public void Restart()
        {
            gameObject.SetActive(true);
            if (afx != null)
            {
                afx.SetActive(false);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                 var player = other.gameObject.GetComponent<PlayerController2D>();
                if (player!= null && player.isMaxAirTime)
                {
                    if (afx != null)
                    {
                        afx.SetActive(true);
                    }
                    gameObject.SetActive(false);
                }
            }
        }
    }
}