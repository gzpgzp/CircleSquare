using System;
using System.Collections.Generic;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Physics;
using BattleSystemCore.Logic.Tracks;
using BattleSystemCore.Unity;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace BattleSystemCore.Editor
{
    /// <summary>
    /// 选中轨道的属性面板：按轨道类型显示对应字段（与 BattleTimelineView 配合使用）。
    /// 通用字段 type/relTick/origin/target；类型切换经 NormalizeForType 清理无关字段并置类型默认值。
    /// 专属字段：ApplyBuff→buffId下拉 / Move→duration+x+y+z / AttackShape→形状按类型条件显示 /
    /// Knockup→vy0+gravity / SpawnBullet→模板下拉（无模板=全内联）+句柄与参数覆盖 /
    /// ControlBullet→duration+句柄+操控模式（追踪走通用target / 固定向量x/y/z） /
    /// TriggerFx→特效键+位置源（durationTicks≥2=持续型，打断即停）。
    /// </summary>
    public static class BattleTrackInspectorPanel
    {
        private static readonly string[] ShapeOptions = { "（未设置）", "Sphere", "Sector", "Box" };
        private static readonly int[] ShapeValues =
        {
            0, (int)AttackShapeType.Sphere, (int)AttackShapeType.Sector, (int)AttackShapeType.Box,
        };

        private static readonly string[] CtrlModeOptions = { "追踪目标(FollowTarget)", "固定向量(FixedDirection)" };

        private static readonly string[] CenterModeOptions = { "跟随origin角色(FollowOrigin)", "弹着点(ImpactPoint)" };

        /// <summary>绘制选中轨道属性。上移/下移/删除会直接修改 tracks 与 index（删除后 index 钳到相邻项，空列表置-1）</summary>
        public static void Draw(List<TrackTemplate> tracks, ref int index, BattleConfigData config, Action markDirty)
        {
            if (index < 0 || index >= tracks.Count)
            {
                return;
            }

            TrackTemplate t = tracks[index];
            int moveDelta = 0;
            bool doDelete = false;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 标题行：序号 + 上移/下移/删除
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("轨道 #" + index, EditorStyles.boldLabel, GUILayout.Width(64f));
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(index == 0);
            if (GUILayout.Button("上移", EditorStyles.miniButtonLeft, GUILayout.Width(40f)))
            {
                moveDelta = -1;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(index >= tracks.Count - 1);
            if (GUILayout.Button("下移", EditorStyles.miniButtonRight, GUILayout.Width(40f)))
            {
                moveDelta = 1;
            }

            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("删除", EditorStyles.miniButton, GUILayout.Width(40f)))
            {
                doDelete = true;
            }

            EditorGUILayout.EndHorizontal();

            // 通用：类型
            EditorGUI.BeginChangeCheck();
            var newType = (TrackType)EditorGUILayout.EnumPopup("类型", t.type);
            if (EditorGUI.EndChangeCheck())
            {
                t.type = newType;
                BattleTimelineView.NormalizeForType(t);
                markDirty();
            }

            // 通用：relTick
            EditorGUI.BeginChangeCheck();
            int relTick = EditorGUILayout.IntField("relTick（相对触发帧）", t.relTick);
            if (EditorGUI.EndChangeCheck())
            {
                t.relTick = Mathf.Max(0, relTick);
                markDirty();
            }

            // 通用：origin（攻击类/弹道类/特效类）/ target（除特效外全类型——特效无target语义）
            bool hasOrigin = t.type == TrackType.DoDamage
                || t.type == TrackType.AttackShape
                || t.type == TrackType.SpawnBullet
                || t.type == TrackType.ControlBullet
                || t.type == TrackType.TriggerFx;
            if (hasOrigin)
            {
                EditorGUI.BeginChangeCheck();
                var origin = (RoleRef)EditorGUILayout.EnumPopup("origin（发起者）", t.origin);
                if (EditorGUI.EndChangeCheck())
                {
                    t.origin = origin;
                    markDirty();
                }
            }

            if (t.type != TrackType.TriggerFx)
            {
                EditorGUI.BeginChangeCheck();
                var target = (RoleRef)EditorGUILayout.EnumPopup("target（承受者）", t.target);
                if (EditorGUI.EndChangeCheck())
                {
                    t.target = target;
                    markDirty();
                }
            }

            // 专属字段
            switch (t.type)
            {
                case TrackType.Move:
                    DrawIntField("durationTicks（持续帧）", ref t.durationTicks, 1, markDirty);
                    DrawIntField("x（千分单位，可负）", ref t.x, int.MinValue, markDirty);
                    DrawIntField("y", ref t.y, int.MinValue, markDirty);
                    DrawIntField("z", ref t.z, int.MinValue, markDirty);
                    break;

                case TrackType.ApplyBuff:
                    DrawBuffIdField(t, config, markDirty);
                    break;

                case TrackType.AttackShape:
                    DrawShapeFields(t, markDirty);
                    break;

                case TrackType.Knockup:
                    DrawIntField("vy0（垂直初速 milli/tick）", ref t.vy0, int.MinValue, markDirty);
                    DrawIntField("gravity（重力 milli/tick²）", ref t.gravity, int.MinValue, markDirty);
                    break;

                case TrackType.SpawnBullet:
                    DrawProjectileIdField(t, config, markDirty);
                    break;

                case TrackType.ControlBullet:
                    DrawControlBulletFields(t, markDirty);
                    break;

                case TrackType.TriggerFx:
                    DrawFxFields(t, markDirty);
                    break;
            }

            EditorGUILayout.EndVertical();

            // 结构性修改放在布局结束之后，避免 IMGUI 配对失衡
            if (doDelete)
            {
                tracks.RemoveAt(index);
                markDirty();
                index = tracks.Count == 0 ? -1 : Mathf.Min(index, tracks.Count - 1);
                return;
            }

            if (moveDelta != 0)
            {
                int other = index + moveDelta;
                TrackTemplate tmp = tracks[index];
                tracks[index] = tracks[other];
                tracks[other] = tmp;
                index = other;
                markDirty();
            }
        }

        // ==================== 专属字段 ====================

        private static void DrawShapeFields(TrackTemplate t, Action markDirty)
        {
            int sel;
            switch ((AttackShapeType)t.shapeType)
            {
                case AttackShapeType.Sphere: sel = 1; break;
                case AttackShapeType.Sector: sel = 2; break;
                case AttackShapeType.Box: sel = 3; break;
                default: sel = 0; break;
            }

            EditorGUI.BeginChangeCheck();
            int newSel = EditorGUILayout.Popup("shapeType", sel, ShapeOptions);
            if (EditorGUI.EndChangeCheck())
            {
                t.shapeType = ShapeValues[newSel];
                markDirty();
            }

            // 判定心：ImpactPoint=弹着点（弹的命中/超程载荷），直放等无弹着点上下文回退origin角色
            EditorGUI.BeginChangeCheck();
            int centerMode = EditorGUILayout.Popup("判定心", t.shapeCenterMode, CenterModeOptions);
            if (EditorGUI.EndChangeCheck())
            {
                t.shapeCenterMode = centerMode;
                markDirty();
            }

            if (t.shapeCenterMode == (int)ShapeCenterMode.ImpactPoint)
            {
                SirenixEditorGUI.InfoMessageBox("弹着点判定心：仅在弹的命中/超程载荷中生效（命中=接触点、超程=落点）；技能直放等无弹着点上下文时回退 origin 角色。");
            }

            switch ((AttackShapeType)t.shapeType)
            {
                case AttackShapeType.Sphere:
                    DrawIntField("shapeRadius（半径 milli）", ref t.shapeRadius, 0, markDirty);
                    break;
                case AttackShapeType.Sector:
                    DrawIntField("shapeRadius（半径 milli）", ref t.shapeRadius, 0, markDirty);
                    DrawIntField("shapeMinDot（万分比，10000=仅正前）", ref t.shapeMinDot, int.MinValue, markDirty);
                    break;
                case AttackShapeType.Box:
                    DrawIntField("shapeHalfX（半宽 milli）", ref t.shapeHalfX, 0, markDirty);
                    DrawIntField("shapeHalfZ（半深 milli）", ref t.shapeHalfZ, 0, markDirty);
                    break;
                default:
                    SirenixEditorGUI.ErrorMessageBox("未指定形状，保存校验将阻断（必须选 Sphere/Sector/Box）", true);
                    break;
            }
        }

        private static void DrawBuffIdField(TrackTemplate t, BattleConfigData config, Action markDirty)
        {
            string[] options = new string[config.buffs.Count + 1];
            options[0] = "（未设置）";
            int sel = 0;
            for (int i = 0; i < config.buffs.Count; i++)
            {
                var b = config.buffs[i];
                options[i + 1] = b.buffId + " " + b.name;
                if (b.buffId == t.buffId)
                {
                    sel = i + 1;
                }
            }

            EditorGUI.BeginChangeCheck();
            int newSel = EditorGUILayout.Popup("buffId", sel, options);
            if (EditorGUI.EndChangeCheck())
            {
                t.buffId = newSel == 0 ? 0u : config.buffs[newSel - 1].buffId;
                markDirty();
            }

            if (sel == 0 && t.buffId != 0)
            {
                SirenixEditorGUI.ErrorMessageBox("buffId " + t.buffId + " 不存在于 Buffs（保存校验将阻断）", true);
            }
        }

        private static void DrawProjectileIdField(TrackTemplate t, BattleConfigData config, Action markDirty)
        {
            // 首项“（无模板·内联参数）”是合法选项：projectileId=0 时全内联（速度必填）
            string[] options = new string[config.projectiles.Count + 1];
            options[0] = "（无模板·内联参数）";
            int sel = 0;
            for (int i = 0; i < config.projectiles.Count; i++)
            {
                var p = config.projectiles[i];
                options[i + 1] = p.projectileId + " " + p.name;
                if (p.projectileId == t.projectileId)
                {
                    sel = i + 1;
                }
            }

            EditorGUI.BeginChangeCheck();
            int newSel = EditorGUILayout.Popup("projectileId", sel, options);
            if (EditorGUI.EndChangeCheck())
            {
                t.projectileId = newSel == 0 ? 0u : config.projectiles[newSel - 1].projectileId;
                markDirty();
            }

            if (sel == 0 && t.projectileId != 0)
            {
                SirenixEditorGUI.ErrorMessageBox("projectileId " + t.projectileId + " 不存在于 Projectiles（保存校验将阻断）", true);
            }

            bool hasTemplate = t.projectileId > 0;
            DrawIntField("bulletLabel（弹句柄，0=不注册操控）", ref t.bulletLabel, 0, markDirty);
            DrawIntField(
                hasTemplate ? "bulletSpeed（0=模板值）" : "bulletSpeed（milli/tick，必填）",
                ref t.bulletSpeed, 0, markDirty);
            DrawIntField(
                hasTemplate ? "bulletRadius（0=模板值）" : "bulletRadius（命中半径 milli）",
                ref t.bulletRadius, 0, markDirty);
            DrawIntField(
                hasTemplate ? "bulletRange（0=模板值）" : "bulletRange（最大射程 milli）",
                ref t.bulletRange, 0, markDirty);

            if (!hasTemplate && t.bulletSpeed <= 0)
            {
                SirenixEditorGUI.ErrorMessageBox("无模板时 bulletSpeed 必须 > 0（保存校验将阻断）", true);
            }
        }

        private static void DrawControlBulletFields(TrackTemplate t, Action markDirty)
        {
            DrawIntField("durationTicks（持续帧）", ref t.durationTicks, 1, markDirty);
            DrawIntField("bulletLabel（须与SpawnBullet的label一致）", ref t.bulletLabel, 1, markDirty);

            EditorGUI.BeginChangeCheck();
            int mode = EditorGUILayout.Popup("操控模式", t.bulletCtrlMode, CtrlModeOptions);
            if (EditorGUI.EndChangeCheck())
            {
                t.bulletCtrlMode = mode;
                markDirty();
            }

            if (t.bulletCtrlMode == (int)BulletCtrlMode.FixedDirection)
            {
                DrawIntField("x（固定方向原始向量，可负）", ref t.x, int.MinValue, markDirty);
                DrawIntField("y", ref t.y, int.MinValue, markDirty);
                DrawIntField("z", ref t.z, int.MinValue, markDirty);
                if (t.x == 0 && t.y == 0 && t.z == 0)
                {
                    SirenixEditorGUI.ErrorMessageBox("固定向量模式下 x/y/z 不能全为 0（保存校验将阻断）", true);
                }
            }

            // FollowTarget 模式用通用 target 字段（弹逐帧朝 target 当前位置追踪）
        }

        private static void DrawFxFields(TrackTemplate t, Action markDirty)
        {
            DrawIntField("durationTicks（1=瞬时，≥2=持续）", ref t.durationTicks, 1, markDirty);

            EditorGUI.BeginChangeCheck();
            string fxId = EditorGUILayout.TextField("fxId（特效库键，短键非路径）", t.fxId);
            if (EditorGUI.EndChangeCheck())
            {
                t.fxId = fxId;
                markDirty();
            }

            EditorGUI.BeginChangeCheck();
            bool useImpact = EditorGUILayout.Toggle("fxUseImpact（位置用弹着点）", t.fxUseImpact);
            if (EditorGUI.EndChangeCheck())
            {
                t.fxUseImpact = useImpact;
                markDirty();
            }

            if (t.durationTicks >= 2)
            {
                SirenixEditorGUI.InfoMessageBox("持续型：Enter广播触发→轨道跑满后Exit广播停止（打断即停）。位置源origin时跟随该角色，弹着点为固定点。");
            }

            if (t.fxUseImpact)
            {
                SirenixEditorGUI.InfoMessageBox("弹着点位置：仅在弹的命中/超程载荷中生效（命中=接触点、超程=落点）；技能直放等无弹着点上下文时回退 origin 角色。");
            }

            if (string.IsNullOrEmpty(t.fxId))
            {
                SirenixEditorGUI.ErrorMessageBox("fxId 不能为空（保存校验将阻断）", true);
            }
        }

        private static void DrawIntField(string label, ref int value, int min, Action markDirty)
        {
            EditorGUI.BeginChangeCheck();
            int v = EditorGUILayout.IntField(label, value);
            if (EditorGUI.EndChangeCheck())
            {
                value = Mathf.Max(min, v);
                markDirty();
            }
        }
    }
}
