using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Tracks;
using UnityEngine;

namespace BattleSystemCore.Unity.VTracks
{
    /// <summary>
    /// 操控子弹表现轨道（ControlBulletTrack的影子）：轨道激活期间（=操控生效期）为
    /// owner名下bulletLabel的每颗存活弹维持一枚导向光标（悬浮弹上方、朝弹速度方向
    /// 拉伸），OnSync每帧按逻辑弹列表对齐（多退少补）。操控语义全在逻辑侧
    /// （本类零结算），纯只读跟随；OnExit清全部光标——打断即消失。
    /// </summary>
    public class ControlBulletVTrack : VBaseTrack
    {
        private const float MilliToUnits = 1f / 1000f;
        private const float HoverY = 0.3f;

        private readonly List<GameObject> markers = new List<GameObject>();

        public override void OnEnter()
        {
            OnSync(); // 进入即对齐一版（无弹时为空操作）
        }

        public override void OnSync()
        {
            var st = (ControlBulletStaticData)logicTrack.SharedStaticData;
            if (st.bulletLabel <= 0)
            {
                return;
            }

            var bullets = battle.projectileManager.GetByLabel(st.ownerActorId, st.bulletLabel);

            // 数量对齐：多退少补（弹命中消失→光标同帧回收；新弹生成→补光标）
            while (markers.Count < bullets.Count)
            {
                markers.Add(CreateMarker());
            }
            while (markers.Count > bullets.Count)
            {
                var last = markers[markers.Count - 1];
                markers.RemoveAt(markers.Count - 1);
                if (last != null)
                {
                    Object.Destroy(last);
                }
            }

            for (int i = 0; i < markers.Count; i++)
            {
                var p = bullets[i];
                var marker = markers[i];
                if (marker == null || p == null)
                {
                    continue;
                }

                marker.transform.position = new Vector3(p.pos.x * MilliToUnits,
                    p.pos.y * MilliToUnits + HoverY, p.pos.z * MilliToUnits);

                // 朝速度方向（milli比例即方向比例，无需换算）；静止弹保持现姿
                if (p.velocity != Vec3.zero)
                {
                    marker.transform.rotation = Quaternion.LookRotation(new Vector3(
                        p.velocity.x, p.velocity.y, p.velocity.z));
                }
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < markers.Count; i++)
            {
                if (markers[i] != null)
                {
                    Object.Destroy(markers[i]);
                }
            }

            markers.Clear();
        }

        /// <summary>导向光标：小拉伸方块（Unlit亮黄，无碰撞），纯程序化构建照BattleFxLibrary惯例</summary>
        private static GameObject CreateMarker()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "ControlBulletMarker";
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.localScale = new Vector3(0.06f, 0.06f, 0.22f);
            go.GetComponent<Renderer>().material = SkillCastIndicator.MakeUnlit(new Color(1f, 0.92f, 0.3f, 0.9f));
            return go;
        }
    }
}
