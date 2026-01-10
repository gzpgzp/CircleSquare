using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Character
{
    public class PlayerGun : MonoBehaviour
    {
        [Header("引用")]
        public Rigidbody2D rb;              // 玩家刚体
        public Transform muzzle;            // 枪口
        public GameObject rocketPrefab;     // 火箭子弹预制体

        [Header("参数")]
        public float fireCooldown = 0.3f;   // 开火间隔
        public float rocketSpeed = 20f;     // 火箭初速度
        public float recoilForce = 8f;      // 开火后坐力（作用在玩家身上）

        private float fireTimer = 0f;

        void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (Camera.main == null) return;

            fireTimer -= Time.deltaTime;

            // 左键开火
            if (Input.GetMouseButton(0) && fireTimer <= 0f)
            {
                Fire();
            }
        }

        void Fire()
        {
            if (rocketPrefab == null || muzzle == null) return;

            fireTimer = fireCooldown;

            // 1. 计算发射方向
            // 方案 A：用枪口的右方向（前提：枪 sprite 默认朝右）
            Vector2 dir = muzzle.right.normalized;

            // 如果你更想精确对准鼠标，也可以用下面这个：
            // Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // mouseWorld.z = 0f;
            // dir = (mouseWorld - muzzle.position).normalized;

            // 2. 生成火箭
            GameObject rocket = Instantiate(rocketPrefab, muzzle.position, Quaternion.identity);

            // 让火箭的朝向对齐方向
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rocket.transform.rotation = Quaternion.Euler(0, 0, angle + 90);

            // 给火箭速度
            Rigidbody2D rocketRb = rocket.GetComponent<Rigidbody2D>();
            if (rocketRb != null)
            {
                rocketRb.velocity = dir * rocketSpeed;
            }

            // 3. 给玩家后坐力（反方向）
            rb.AddForce(-dir * recoilForce, ForceMode2D.Impulse);
        }
    }
}