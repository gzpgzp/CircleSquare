using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Tracks;
using UnityEngine;

namespace BattleSystemCore.Unity.VTracks
{
    /// <summary>
    /// 形状攻击表现轨道（AttackShapeTick的影子）：读AttackShapeStaticData绘制AoE贴地
    /// 指示圈（SkillCastIndicator自灭组件：hold后向判定心收缩消散）。
    /// 判定心/朝向按逻辑同帧快照重算（同AttackShapeTick.Enter公式，事件帧末派发时
    /// 同帧结算已完毕，重算值一致；表现只读不回写）：
    /// useImpactCenter=弹着点固定心；否则origin当前位置；forward=判定心→target水平方向。
    /// 瞬时轨道——建了即拆，圈对象自带生命周期自灭，OnExit无需清理。
    /// </summary>
    public class AttackShapeVTrack : VBaseTrack
    {
        private const float HoldSeconds = 0.15f;
        private const float FadeSeconds = 0.35f;

        public override void OnEnter()
        {
            var st = (AttackShapeStaticData)logicTrack.SharedStaticData;

            // 判定心：弹着点固定；否则origin当前位置（origin死亡/移除时跳过——无处可画）
            Vector2 center;
            if (st.useImpactCenter)
            {
                center = new Vector2(st.impactCenter.x, st.impactCenter.z);
            }
            else
            {
                var origin = battle.actorManager.GetActor(st.originActorId);
                if (origin == null)
                {
                    return;
                }

                center = new Vector2(origin.pos.x, origin.pos.z);
            }

            // 朝向：判定心→target水平方向（target无效=zero，指示圈侧不转朝向；
            // 传未归一化差值即可，SkillCastIndicator.Init内部自行normalize）
            var target = battle.actorManager.GetActor(st.targetActorId);
            var forward = Vector2.zero;
            if (target != null)
            {
                float fx = target.pos.x - center.x;
                float fz = target.pos.z - center.y;
                forward = new Vector2(fx, fz);
            }

            var go = new GameObject("VTrack_AttackShape");
            // origin/forward直传milli（SkillCastIndicator.Init内部自行换算/归一化）
            go.AddComponent<SkillCastIndicator>().Init(
                (int)st.shape.type, st.shape.radius, st.shape.minDot,
                st.shape.halfExtents.x, st.shape.halfExtents.z,
                center, forward,
                new Color(1f, 0.35f, 0.2f, 0.55f), HoldSeconds, FadeSeconds);
        }
    }
}
