using UnityEngine;

namespace Mask
{
    public class WearMask : MonoBehaviour
    {
        public SpriteRenderer mask;
        
        public void Wear(Color targetColor)
        {
            mask.gameObject.SetActive(true);
            mask.color = targetColor;
        }

        public void Restart()
        {
            mask.gameObject.SetActive(false);
        }
    }
}