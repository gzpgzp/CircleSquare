using System;
using UnityEngine;
using UnityTimer;
using Random = UnityEngine.Random;

namespace Mask
{
    public class WorldCircleWall : MonoBehaviour
    {
        [SerializeField] private Material material;
        private float x;
        private float y;

        private Vector3 center = Vector3.zero;
        private float waitTime = 4f;
        private Timer effectTimer;

        private Color lastColor = Color.black;
        
        private void Start()
        {
            // lastColor = material.GetColor("_InnerColor");
            CircleWallEffect.Instance.GetCurrentColors(out var edg, out lastColor, out var outside);
            Show();
            effectTimer = Timer.Register(waitTime, ShowCircleEffect);
        }

        private void ShowCircleEffect()
        {
            waitTime = Random.Range(1, 10);
            Show();
            effectTimer = Timer.Register(waitTime, ShowCircleEffect);
        }

        private void Show()
        {
            var targetColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            center.x = Random.Range(-10f,10f);
            center.y = Random.Range(-10f,10f);
            CircleWallEffect.Instance.StartEffect(center, Color.black,targetColor,lastColor, false);    
            lastColor = targetColor;
        }

        private void OnDestroy()
        {
            effectTimer.Pause();
        }
    }
}