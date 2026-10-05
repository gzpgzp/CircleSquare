using Battle3D.Character;
using UnityEngine;

namespace Battle3D.Combat
{
    /// <summary>
    /// 攻击判定器 - 处理HitBox/HurtBox碰撞检测
    /// 支持球形、盒形、胶囊形检测区域
    /// 可挂载到角色或武器上
    /// </summary>
    public class HitDetector : MonoBehaviour
    {
        public enum DetectorShape
        {
            Sphere,
            Box,
            Capsule
        }

        [Header("检测配置")]
        [SerializeField] private DetectorShape shape = DetectorShape.Sphere;
        [SerializeField] private Vector3 center = Vector3.forward;
        [SerializeField] private float size = 1.5f;
        [SerializeField] private Vector3 boxSize = Vector3.one;
        [SerializeField] private float capsuleHeight = 2f;
        [SerializeField] private LayerMask hitLayer = ~0;

        [Header("归属")]
        [SerializeField] private BaseCharacter owner;

        // 已命中列表（防止同一次攻击多次命中）
        private System.Collections.Generic.HashSet<BaseCharacter> hitTargets =
            new System.Collections.Generic.HashSet<BaseCharacter>();

        private bool isActive;

        public BaseCharacter Owner
        {
            get => owner;
            set => owner = value;
        }

        /// <summary>
        /// 激活判定（攻击开始时调用）
        /// </summary>
        public void Activate()
        {
            isActive = true;
            hitTargets.Clear();
        }

        /// <summary>
        /// 停用判定（攻击结束时调用）
        /// </summary>
        public void Deactivate()
        {
            isActive = false;
            hitTargets.Clear();
        }

        /// <summary>
        /// 执行一次检测并返回所有命中的角色
        /// </summary>
        public BaseCharacter[] Detect()
        {
            if (!isActive || owner == null) return System.Array.Empty<BaseCharacter>();

            Vector3 worldCenter = transform.TransformPoint(center);
            Collider[] colliders;

            switch (shape)
            {
                case DetectorShape.Sphere:
                    colliders = Physics.OverlapSphere(worldCenter, size, hitLayer);
                    break;
                case DetectorShape.Box:
                    colliders = Physics.OverlapBox(worldCenter, boxSize * 0.5f, transform.rotation, hitLayer);
                    break;
                case DetectorShape.Capsule:
                    Vector3 top = worldCenter + Vector3.up * capsuleHeight * 0.5f;
                    Vector3 bottom = worldCenter - Vector3.up * capsuleHeight * 0.5f;
                    colliders = Physics.OverlapCapsule(top, bottom, size, hitLayer);
                    break;
                default:
                    colliders = System.Array.Empty<Collider>();
                    break;
            }

            var results = new System.Collections.Generic.List<BaseCharacter>();
            foreach (var col in colliders)
            {
                var targetChar = col.GetComponent<BaseCharacter>();
                if (targetChar == null || targetChar == owner) continue;
                if (hitTargets.Contains(targetChar)) continue; // 已命中过

                hitTargets.Add(targetChar);
                results.Add(targetChar);
            }

            return results.ToArray();
        }

        /// <summary>
        /// 检测并直接造成伤害
        /// </summary>
        public int DetectAndDamage(DamageInfo damageInfo)
        {
            var targets = Detect();
            int hitCount = 0;

            foreach (var target in targets)
            {
                target.TakeDamage(damageInfo);
                hitCount++;
            }

            return hitCount;
        }

        /// <summary>
        /// 重置已命中列表（连招每段开始时调用）
        /// </summary>
        public void ResetHitList()
        {
            hitTargets.Clear();
        }

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!isActive) return;

            Gizmos.color = Color.red;
            Vector3 worldCenter = transform.TransformPoint(center);

            switch (shape)
            {
                case DetectorShape.Sphere:
                    Gizmos.DrawWireSphere(worldCenter, size);
                    break;
                case DetectorShape.Box:
                    Gizmos.DrawWireCube(worldCenter, boxSize);
                    break;
                case DetectorShape.Capsule:
                    Gizmos.DrawWireSphere(worldCenter, size);
                    break;
            }
        }

        #endregion
    }
}
