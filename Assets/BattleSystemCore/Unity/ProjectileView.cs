using BattleSystemCore.Logic.Projectiles;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 投射物表现视图（影子）：逻辑Projectile.pos为唯一权威，本组件只追随不决策。
    /// 追随规则与BattleActorView同款（指数趋平摊开20Hz阶梯位移）；朝向固定为弹道方向（velocity），
    /// 飞行中绕弹道轴自转（搓弹药感）。弹体销毁由宿主订阅ProjectileHitEvent/ExpiredEvent后销毁本视图。
    /// </summary>
    public class ProjectileView : MonoBehaviour
    {
        private const float MilliToUnits = 1f / 1000f;

        [Tooltip("视觉位置追踪速率（越大越贴近逻辑位置）")]
        [SerializeField] private float followSpeed = 25f;

        [Tooltip("飞行自转速度（度/秒）")]
        [SerializeField] private float spinDegPerSec = 720f;

        private Projectile projectile;
        private Vector3 visualPos;

        /// <summary>绑定逻辑弹体并着色（悬浮高度让子弹不插地）</summary>
        public void Bind(Projectile logicProjectile, Color color, float hoverUnits)
        {
            projectile = logicProjectile;
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = SkillCastIndicator.MakeUnlit(color);
            }

            // 首帧直接落位 + 朝向弹道方向（velocity恒定，直线弹一次定死）
            visualPos = new Vector3(
                projectile.pos.x * MilliToUnits,
                projectile.pos.y * MilliToUnits + hoverUnits,
                projectile.pos.z * MilliToUnits);
            transform.position = visualPos;
            Vector3 dir = new Vector3(projectile.velocity.x, 0f, projectile.velocity.z);
            if (dir.sqrMagnitude > 1e-10f)
            {
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }

        private void LateUpdate()
        {
            if (projectile == null || !projectile.alive)
            {
                return;
            }

            Vector3 target = new Vector3(
                projectile.pos.x * MilliToUnits,
                projectile.pos.y * MilliToUnits + transform.localScale.x * 0.5f + 0.1f,
                projectile.pos.z * MilliToUnits);
            float k = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            visualPos = Vector3.Lerp(visualPos, target, k);
            transform.position = visualPos;
            transform.Rotate(Vector3.forward, spinDegPerSec * Time.deltaTime, Space.Self);
        }
    }
}
