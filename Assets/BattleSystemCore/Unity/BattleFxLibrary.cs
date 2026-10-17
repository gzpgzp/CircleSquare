using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 特效库（表现层）：fxId（短键）→特效构建器的注册表。逻辑层TriggerFx轨道只透传键，
    /// 本库按键查构建器程序化生成特效实例（零资产依赖，风格与SkillCastIndicator一致；
    /// 接美术资产时替换为键→Prefab映射即可，接口不变）。
    /// 查不到的键返回null（调用方跳过并告警）——配置错键不炸运行时。
    /// </summary>
    public static class BattleFxLibrary
    {
        private static readonly Dictionary<string, Func<GameObject>> builders = new Dictionary<string, Func<GameObject>>();

        /// <summary>注册特效构建器（同键覆盖，幂等——Demo每次StartBattle可重复调用）</summary>
        public static void Register(string fxId, Func<GameObject> builder)
        {
            if (string.IsNullOrEmpty(fxId) || builder == null)
            {
                return;
            }

            builders[fxId] = builder;
        }

        /// <summary>按键生成特效实例（未着位置，由调用方摆放）。未知键返回null</summary>
        public static GameObject Create(string fxId)
        {
            if (!builders.TryGetValue(fxId, out var builder))
            {
                Debug.LogWarning($"[BattleFxLibrary] 未注册的特效键 \"{fxId}\"（配置与表现层键表需对齐）");
                return null;
            }

            var go = builder();
            go.name = "Fx_" + fxId;
            return go;
        }

        public static void Clear()
        {
            builders.Clear();
        }

        /// <summary>注册内置程序化特效（Demo启动调用；资产管线接入后由装载器注册替代）</summary>
        public static void RegisterDefaults()
        {
            // 瞬时·爆炸收缩圈（橙）：命中/爆炸点
            Register("burst", () =>
            {
                var go = new GameObject();
                go.AddComponent<SkillCastIndicator>().Init(1, 1200, 0, 0, 0, Vector2.zero, Vector2.zero,
                    new Color(1f, 0.55f, 0.15f), 0.05f, 0.3f);
                return go;
            });

            // 瞬时·弹着点冲击环（青白）：弹命中/超程落点
            Register("impact_ring", () =>
            {
                var go = new GameObject();
                go.AddComponent<SkillCastIndicator>().Init(1, 1600, 0, 0, 0, Vector2.zero, Vector2.zero,
                    new Color(0.6f, 0.9f, 1f), 0.06f, 0.45f);
                return go;
            });

            // 持续·光环（蓝紫，脉动）：持续型特效跟随角色
            Register("aura", () => CreatePulseRing(900, new Color(0.55f, 0.45f, 0.95f)));

            // 持续·施法光效（金色，脉动）：施法期间跟随施法者
            Register("cast_glow", () => CreatePulseRing(1100, new Color(1f, 0.8f, 0.3f)));
        }

        /// <summary>持续型脉动圆环（组件自渲染，位置由TriggerFxVTrack驱动，销毁由其OnExit负责）</summary>
        private static GameObject CreatePulseRing(int radiusMilli, Color tint)
        {
            var go = new GameObject();
            var pulse = go.AddComponent<FxPulseRing>();
            pulse.Init(radiusMilli / 1000f, tint);
            return go;
        }
    }

    /// <summary>
    /// 持续型特效：贴地脉动圆环（LineRenderer，URP Unlit）。
    /// 无自毁逻辑——生命周期由TriggerFxVTrack按TrackViewEnded销毁（打断即停）。
    /// </summary>
    public class FxPulseRing : MonoBehaviour
    {
        private float baseScale;
        private float phase;

        /// <summary>构建圆环（radius为units）。Init后位置由warper每帧驱动</summary>
        public void Init(float radiusUnits, Color tint)
        {
            baseScale = 1f;
            phase = 0f;
            transform.position = new Vector3(0f, 0.02f, 0f);

            var ring = gameObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 48;
            ring.startWidth = 0.07f;
            ring.endWidth = 0.07f;
            ring.material = SkillCastIndicator.MakeUnlit(tint);
            for (int i = 0; i < 48; i++)
            {
                float angle = i / 48f * Mathf.PI * 2f;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radiusUnits, 0f, Mathf.Sin(angle) * radiusUnits));
            }
        }

        private void Update()
        {
            // 轻微呼吸脉动（纯表现，不影响逻辑位置）
            phase += Time.deltaTime;
            transform.localScale = Vector3.one * (baseScale + 0.1f * Mathf.Sin(phase * 5f));
        }
    }
}
