using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Tracks;
using UnityEngine;

namespace BattleSystemCore.Unity.VTracks
{
    /// <summary>
    /// 特效表现轨道（TriggerFxTrack/TriggerFxTick的影子）：读TriggerFxStaticData
    /// 计算落位并按fxId建特效（BattleFxLibrary键→实例）。
    /// 位置源（工厂已裁决弹着点回退）：useImpactPos=固定点不跟随；否则origin当前位置。
    /// 瞬时（!isDuration）落位即走——特效自带生命周期自灭，OnExit只清句柄；
    /// 持续（isDuration）持特效对象，OnSync每帧贴地跟随origin（指数趋平照影子惯例，
    /// origin死亡/移除停在最后位置），OnExit销毁——打断即停。
    /// </summary>
    public class TriggerFxVTrack : VBaseTrack
    {
        private const float MilliToUnits = 1f / 1000f;
        private const float GroundY = 0.02f;
        private const float FollowSpeed = 15f;

        private GameObject fxObject;
        private uint followActorId;
        private Vector3 visualPos;

        public override void OnEnter()
        {
            var st = (TriggerFxStaticData)logicTrack.SharedStaticData;
            fxObject = BattleFxLibrary.Create(st.fxId);
            if (fxObject == null)
            {
                return;
            }

            Vector3 pos;
            if (st.useImpactPos)
            {
                // 弹着点特效：固定点不跟随
                pos = new Vector3(st.impactPos.x * MilliToUnits, st.impactPos.y * MilliToUnits,
                    st.impactPos.z * MilliToUnits);
            }
            else
            {
                // origin特效：落触发时刻origin位置，持续型随后每帧跟随
                var origin = battle.actorManager.GetActor(st.originActorId);
                var op = origin != null ? origin.pos : Vec3.zero;
                pos = new Vector3(op.x * MilliToUnits, op.y * MilliToUnits, op.z * MilliToUnits);
                followActorId = st.originActorId;
            }

            if (logicTrack.isDuration)
            {
                // 持续型：贴地落位（光环类不随击飞抬升），OnSync驱动跟随
                visualPos = new Vector3(pos.x, GroundY, pos.z);
                fxObject.transform.position = visualPos;
            }
            else
            {
                // 瞬时型：落位即走（SkillCastIndicator等自带Update收缩自毁）
                fxObject.transform.position = pos;
            }
        }

        public override void OnSync()
        {
            if (fxObject == null || followActorId == 0)
            {
                return;
            }

            var actor = battle.actorManager.GetActor(followActorId);
            if (actor == null)
            {
                return; // 目标死亡/移除：停在最后位置
            }

            var target = new Vector3(actor.pos.x * MilliToUnits, GroundY, actor.pos.z * MilliToUnits);
            float k = 1f - Mathf.Exp(-FollowSpeed * Time.deltaTime);
            visualPos = Vector3.Lerp(visualPos, target, k);
            fxObject.transform.position = visualPos;
        }

        public override void OnExit()
        {
            if (fxObject != null)
            {
                Object.Destroy(fxObject);
                fxObject = null;
            }
        }
    }
}
