using BattleSystemCore.Logic.Actors;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 战斗单位的表现视图（影子）：逻辑Actor.pos为唯一权威，本组件只做"追随"不做决策。
    /// 同步规则：逻辑milli坐标换算为units（/1000，float换算在表现层合法），视觉位置以帧率无关的
    /// 指数趋近平滑追踪（20Hz逻辑帧的阶梯位移被渲染帧摊开）；y额外垫半高使方块站立于地面。
    /// 朝向跟随水平位移方向；落地表现（scale弹跳）由宿主订阅ActorLandEvent后调用PlayLandBounce触发。
    /// 额外表观：受击闪白（PlayHitFlash）、离地拖尾、随高度缩放的地面投影（腾空高度可读性）。
    /// 逻辑Actor死亡移除由宿主负责销毁本视图（OnDestroy同步清理独立影子物体）。
    /// </summary>
    public class BattleActorView : MonoBehaviour
    {
        private const float MilliToUnits = 1f / 1000f;

        [Tooltip("视觉位置追踪速率（越大越贴近逻辑位置，越小越柔滑）")]
        [SerializeField] private float followSpeed = 15f;

        [Tooltip("落地弹跳时长（秒）")]
        [SerializeField] private float landBounceDuration = 0.25f;

        private Actor actor;
        private Vector3 visualPos;
        private Vector3 lastFlatPos;
        private Quaternion visualRot = Quaternion.identity;
        private float bounceTimer = -1f;
        private float halfHeight = 0.5f;

        // 受击闪白 / 拖尾 / 地面投影（纯表现状态）
        private Renderer viewRenderer;
        private Color baseColor = Color.white;
        private float flashTimer = -1f;
        private TrailRenderer trail;
        private Transform shadow;
        private float diameterUnits = 1f;

        /// <summary>绑定逻辑单位并着色。halfHeightUnits为碰撞体半高（表现垫高，让方块立于地面）</summary>
        public void Bind(Actor logicActor, Color color, float halfHeightUnits)
        {
            actor = logicActor;
            halfHeight = halfHeightUnits;
            diameterUnits = halfHeightUnits * 2f;
            viewRenderer = GetComponent<Renderer>();
            if (viewRenderer != null)
            {
                viewRenderer.material.color = color;
            }

            baseColor = color;
            BuildTrail(color);
            BuildShadow();

            // 首帧直接落位（避免从原点飞入）
            visualPos = LogicToVisual(actor.pos);
            lastFlatPos = new Vector3(visualPos.x, 0f, visualPos.z);
            transform.position = visualPos;
        }

        private void LateUpdate()
        {
            if (actor == null)
            {
                return;
            }

            Vector3 target = LogicToVisual(actor.pos);
            float k = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            visualPos = Vector3.Lerp(visualPos, target, k);
            transform.position = visualPos;

            UpdateFacing();
            UpdateLandBounce();
            UpdateFlash();
            UpdateAirborneVisuals();
        }

        /// <summary>落地弹跳：scale纵压回弹（纯表现，不回写逻辑）</summary>
        public void PlayLandBounce()
        {
            bounceTimer = 0f;
        }

        private void UpdateFacing()
        {
            Vector3 flat = new Vector3(visualPos.x, 0f, visualPos.z);
            Vector3 delta = flat - lastFlatPos;
            lastFlatPos = flat;
            if (delta.sqrMagnitude > 1e-10f)
            {
                Quaternion look = Quaternion.LookRotation(delta.normalized, Vector3.up);
                visualRot = Quaternion.Slerp(visualRot, look, 1f - Mathf.Exp(-10f * Time.deltaTime));
                transform.rotation = visualRot;
            }
        }

        private void UpdateLandBounce()
        {
            if (bounceTimer < 0f)
            {
                return;
            }

            bounceTimer += Time.deltaTime;
            if (bounceTimer >= landBounceDuration)
            {
                bounceTimer = -1f;
                transform.localScale = Vector3.one;
                return;
            }

            // 正弦半波：纵压到0.7再回1
            float phase = Mathf.Sin(bounceTimer / landBounceDuration * Mathf.PI);
            transform.localScale = new Vector3(1f + 0.15f * phase, 1f - 0.3f * phase, 1f + 0.15f * phase);
        }

        /// <summary>受击闪白：材质瞬白后回落原色（宿主订阅DamageDealtEvent后转发）</summary>
        public void PlayHitFlash()
        {
            flashTimer = 0f;
        }

        private void BuildTrail(Color color)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0f;
            trail.material = SkillCastIndicator.MakeUnlit(color);
            trail.startColor = new Color(color.r, color.g, color.b, 0.6f);
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.emitting = false;
        }

        private void BuildShadow()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(quad.GetComponent<Collider>());
            quad.name = "Shadow";
            shadow = quad.transform;
            shadow.rotation = Quaternion.Euler(90f, 0f, 0f);
            shadow.GetComponent<Renderer>().material = SkillCastIndicator.MakeUnlit(new Color(0.09f, 0.10f, 0.12f));
        }

        private void UpdateFlash()
        {
            const float duration = 0.12f;
            if (flashTimer < 0f)
            {
                return;
            }

            flashTimer += Time.deltaTime;
            if (flashTimer >= duration)
            {
                flashTimer = -1f;
                if (viewRenderer != null)
                {
                    viewRenderer.material.color = baseColor;
                }

                return;
            }

            // 1→0衰减：受击瞬间最白，随后融回本色
            float remain = 1f - flashTimer / duration;
            if (viewRenderer != null)
            {
                viewRenderer.material.color = Color.Lerp(baseColor, Color.white, remain * 0.85f);
            }
        }

        private void UpdateAirborneVisuals()
        {
            float height = actor.pos.y * (1f / 1000f);
            if (trail != null)
            {
                trail.emitting = height > 0.05f;
            }

            if (shadow != null)
            {
                // 投影钉在地面正下方，随高度缩小（腾空高度的可读性）
                float shrink = 1f / (1f + height * 1.2f);
                shadow.position = new Vector3(transform.position.x, 0.011f, transform.position.z);
                shadow.localScale = new Vector3(diameterUnits * shrink, diameterUnits * shrink, 1f);
            }
        }

        private void OnDestroy()
        {
            if (shadow != null)
            {
                Destroy(shadow.gameObject);
            }
        }

        private Vector3 LogicToVisual(Vec3 logicPos)
        {
            return new Vector3(
                logicPos.x * MilliToUnits,
                logicPos.y * MilliToUnits + halfHeight,
                logicPos.z * MilliToUnits);
        }
    }
}
