using System;
using System.Collections.Generic;
using BattleSystemCore.Logic.Physics;
using BattleSystemCore.Logic.Tracks;
using BattleSystemCore.Unity;
using UnityEditor;
using UnityEngine;

namespace BattleSystemCore.Editor
{
    /// <summary>
    /// 时间轴交互的跨帧状态（宿主窗口以成员变量持有；拖拽为简单状态机，v1 不使用 Undo 系统）。
    /// </summary>
    public sealed class TimelineInteractionState
    {
        /// <summary>缩放：每 tick 像素数（Ctrl+滚轮调整，钳制 [4,120]）</summary>
        public float pxPerTick = 22f;

        /// <summary>水平滚动偏移（像素，钳制 >=0）</summary>
        public float scrollX;

        /// <summary>拖拽模式：0=无 / 1=拖条身(relTick) / 2=拖右缘(durationTicks)</summary>
        public int dragMode;

        public List<TrackTemplate> dragList;
        public int dragIndex = -1;
        public int dragStartRelTick;
        public int dragStartDuration;
        public float dragStartMouseX;
    }

    /// <summary>
    /// 时间轴轨道视图（可复用组件：技能载荷 / 投射物载荷 / Buff 攻击触发载荷共用）。
    /// 行=轨道（行头=序号+类型色块+类型短名），横轴=tick 刻度尺；条块 x=relTick*pxPerTick、宽=durationTicks*pxPerTick（最小可视宽6px）。
    /// 交互：左键拖条身改 relTick（吸附整tick、钳>=0）、拖右缘改 durationTicks（钳>=1，仅持续型 Move）；
    /// 单击选中（黄框）；右键=菜单（添加6类轨道/上移/下移/删除）；空白区右键=添加轨道；
    /// Ctrl+滚轮缩放（锚点tick保持）、滚轮水平滚动。元素顺序=运行时payload顺序（同帧瞬时轨道按序结算，顺序有意义）。
    /// </summary>
    public static class BattleTimelineView
    {
        public const float RulerHeight = 18f;
        public const float RowHeight = 26f;
        public const float HeadWidth = 72f;
        public const float MinBlockWidth = 6f;

        private const float ResizeEdgePx = 5f;
        private const float DefaultPxPerTick = 22f;

        private static readonly TrackType[] AllTrackTypes =
        {
            TrackType.DoDamage,
            TrackType.ApplyBuff,
            TrackType.Move,
            TrackType.AttackShape,
            TrackType.Knockup,
            TrackType.SpawnBullet,
            TrackType.ControlBullet,
        };

        private static GUIStyle s_blockLabel;
        private static GUIStyle s_tickLabel;
        private static GUIStyle s_headLabel;
        private static GUIStyle s_typeLabel;
        private static GUIStyle s_hintLabel;

        // ==================== 类型元信息 ====================

        /// <summary>仅持续型轨道开放右缘拉伸</summary>
        public static bool IsDurationTrack(TrackType type)
        {
            return type == TrackType.Move || type == TrackType.ControlBullet || type == TrackType.TriggerFx;
        }

        public static Color TypeColor(TrackType type)
        {
            switch (type)
            {
                case TrackType.DoDamage: return new Color(0.82f, 0.32f, 0.28f);
                case TrackType.ApplyBuff: return new Color(0.26f, 0.52f, 0.82f);
                case TrackType.Move: return new Color(0.33f, 0.65f, 0.38f);
                case TrackType.AttackShape: return new Color(0.88f, 0.58f, 0.23f);
                case TrackType.Knockup: return new Color(0.60f, 0.42f, 0.83f);
                case TrackType.SpawnBullet: return new Color(0.25f, 0.70f, 0.75f);
                case TrackType.ControlBullet: return new Color(0.80f, 0.55f, 0.28f);
                case TrackType.TriggerFx: return new Color(0.95f, 0.78f, 0.25f);
                default: return new Color(0.6f, 0.6f, 0.6f);
            }
        }

        public static string TypeLabel(TrackType type)
        {
            switch (type)
            {
                case TrackType.DoDamage: return "伤害(DoDamage)";
                case TrackType.ApplyBuff: return "上Buff(ApplyBuff)";
                case TrackType.Move: return "位移(Move)";
                case TrackType.AttackShape: return "形状攻击(AttackShape)";
                case TrackType.Knockup: return "击飞(Knockup)";
                case TrackType.SpawnBullet: return "生成子弹(SpawnBullet)";
                case TrackType.ControlBullet: return "操控子弹(ControlBullet)";
                case TrackType.TriggerFx: return "触发特效(TriggerFx)";
                default: return type.ToString();
            }
        }

        /// <summary>行头类型短名（窄列用；完整名见 TypeLabel，右键菜单/检查面板用完整名）</summary>
        public static string TypeShortLabel(TrackType type)
        {
            switch (type)
            {
                case TrackType.DoDamage: return "伤害";
                case TrackType.ApplyBuff: return "Buff";
                case TrackType.Move: return "位移";
                case TrackType.AttackShape: return "形状";
                case TrackType.Knockup: return "击飞";
                case TrackType.SpawnBullet: return "子弹";
                case TrackType.ControlBullet: return "操控";
                case TrackType.TriggerFx: return "特效";
                default: return type.ToString();
            }
        }

        /// <summary>类型切换后的字段规范化：清理类型无关字段并初始化类型默认值（公共字段 type/relTick/origin/target 保留）</summary>
        public static void NormalizeForType(TrackTemplate t)
        {
            t.durationTicks = 1;
            t.buffId = 0;
            t.x = 0;
            t.y = 0;
            t.z = 0;
            t.shapeType = 0;
            t.shapeRadius = 0;
            t.shapeMinDot = 0;
            t.shapeHalfX = 0;
            t.shapeHalfZ = 0;
            t.shapeCenterMode = 0;
            t.vy0 = 0;
            t.gravity = 0;
            t.projectileId = 0;
            t.bulletLabel = 0;
            t.bulletSpeed = 0;
            t.bulletRadius = 0;
            t.bulletRange = 0;
            t.bulletCtrlMode = 0;
            t.fxId = "";
            t.fxUseImpact = false;

            switch (t.type)
            {
                case TrackType.Move:
                    t.durationTicks = 5;
                    break;
                case TrackType.AttackShape:
                    t.shapeType = (int)AttackShapeType.Sphere;
                    t.shapeRadius = 3000;
                    break;
                case TrackType.ControlBullet:
                    t.durationTicks = 5;
                    break;
                case TrackType.TriggerFx:
                    // 默认非空键：新建即可通过保存校验（fxId空串会被Validate阻断）
                    t.fxId = "new_fx";
                    break;
            }
        }

        /// <summary>新建轨道的默认值（尽量让新轨道即刻可通过校验）</summary>
        public static TrackTemplate CreateDefaultTrack(TrackType type)
        {
            var t = new TrackTemplate { type = type };
            NormalizeForType(t);
            return t;
        }

        /// <summary>条块摘要里的 buff 名称（"101 破甲"；找不到时明确标"（不存在）"）</summary>
        public static string BuffLabel(uint buffId, BattleConfigData config)
        {
            for (int i = 0; i < config.buffs.Count; i++)
            {
                var b = config.buffs[i];
                if (b.buffId == buffId)
                {
                    return b.buffId + " " + b.name;
                }
            }

            return buffId + "（不存在）";
        }

        /// <summary>条块上的摘要文本（类型名 + 关键参数）</summary>
        public static string TrackSummary(TrackTemplate t, BattleConfigData config)
        {
            switch (t.type)
            {
                case TrackType.DoDamage:
                    return "DoDamage";
                case TrackType.ApplyBuff:
                    return "ApplyBuff " + BuffLabel(t.buffId, config);
                case TrackType.Move:
                    return "Move x" + t.x + " z" + t.z;
                case TrackType.AttackShape:
                    return "AttackShape " + (AttackShapeType)t.shapeType + " r" + t.shapeRadius
                        + (t.shapeCenterMode == (int)ShapeCenterMode.ImpactPoint ? " @弹着点" : "");
                case TrackType.Knockup:
                    return "Knockup vy" + t.vy0 + " g" + t.gravity;
                case TrackType.SpawnBullet:
                    return t.projectileId > 0
                        ? "SpawnBullet " + t.projectileId
                        : "SpawnBullet 内联 s" + t.bulletSpeed;
                case TrackType.ControlBullet:
                    return "ControlBullet #" + t.bulletLabel + (t.bulletCtrlMode == 0 ? " 追踪" : " 固定向");
                case TrackType.TriggerFx:
                    return "TriggerFx " + t.fxId
                        + (t.durationTicks >= 2 ? " 持续" + t.durationTicks + "t" : "")
                        + (t.fxUseImpact ? " @弹着点" : "");
                default:
                    return t.type.ToString();
            }
        }

        // ==================== 主绘制 ====================

        /// <summary>绘制一段时间轴。返回是否发生数据修改（发生修改时已调用 markDirty）</summary>
        public static bool Draw(
            List<TrackTemplate> tracks,
            BattleConfigData config,
            TimelineInteractionState state,
            ref List<TrackTemplate> selectedList,
            ref int selectedIndex,
            Action markDirty)
        {
            float height = RulerHeight + Mathf.Max(tracks.Count, 1) * RowHeight + 4f;
            Rect area = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            if (area.width < HeadWidth + 8f)
            {
                return false;
            }

            bool modified = false;
            float contentX0 = area.x + HeadWidth;
            float visibleWidth = area.width - HeadWidth;

            float maxEndTick = 16f;
            for (int i = 0; i < tracks.Count; i++)
            {
                maxEndTick = Mathf.Max(maxEndTick, tracks[i].relTick + Mathf.Max(1, tracks[i].durationTicks));
            }

            float contentWidth = maxEndTick * state.pxPerTick + 40f;
            float maxScroll = Mathf.Max(0f, contentWidth - visibleWidth);

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && area.Contains(e.mousePosition))
                    {
                        int row = HitTestRow(area, e.mousePosition.y);
                        if (row >= 0 && row < tracks.Count)
                        {
                            Rect blockRect = BlockRect(area, state, row, tracks[row]);
                            bool onBlock = e.mousePosition.x >= blockRect.xMin && e.mousePosition.x <= blockRect.xMax;
                            if (onBlock)
                            {
                                TrackTemplate t = tracks[row];
                                selectedList = tracks;
                                selectedIndex = row;
                                if (IsDurationTrack(t.type) && e.mousePosition.x >= blockRect.xMax - ResizeEdgePx)
                                {
                                    state.dragMode = 2;
                                    state.dragStartDuration = t.durationTicks;
                                }
                                else
                                {
                                    state.dragMode = 1;
                                    state.dragStartRelTick = t.relTick;
                                }

                                state.dragList = tracks;
                                state.dragIndex = row;
                                state.dragStartMouseX = e.mousePosition.x;
                                e.Use();
                            }
                            else
                            {
                                selectedList = null;
                                selectedIndex = -1;
                            }
                        }
                        else
                        {
                            selectedList = null;
                            selectedIndex = -1;
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (state.dragMode != 0 && state.dragList != null
                        && state.dragIndex >= 0 && state.dragIndex < state.dragList.Count)
                    {
                        TrackTemplate t = state.dragList[state.dragIndex];
                        float px = Mathf.Max(4f, state.pxPerTick);
                        int deltaTicks = Mathf.RoundToInt((e.mousePosition.x - state.dragStartMouseX) / px);
                        if (state.dragMode == 1)
                        {
                            int newRelTick = Mathf.Max(0, state.dragStartRelTick + deltaTicks);
                            if (newRelTick != t.relTick)
                            {
                                t.relTick = newRelTick;
                                modified = true;
                                markDirty();
                            }
                        }
                        else
                        {
                            int newDuration = Mathf.Max(1, state.dragStartDuration + deltaTicks);
                            if (newDuration != t.durationTicks)
                            {
                                t.durationTicks = newDuration;
                                modified = true;
                                markDirty();
                            }
                        }

                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    if (e.button == 0)
                    {
                        state.dragMode = 0;
                        state.dragList = null;
                        state.dragIndex = -1;
                    }

                    break;

                case EventType.ContextClick:
                    if (area.Contains(e.mousePosition))
                    {
                        int row = HitTestRow(area, e.mousePosition.y);
                        int blockIndex = -1;
                        if (row >= 0 && row < tracks.Count)
                        {
                            Rect blockRect = BlockRect(area, state, row, tracks[row]);
                            if (e.mousePosition.x >= blockRect.xMin && e.mousePosition.x <= blockRect.xMax)
                            {
                                blockIndex = row;
                            }
                        }

                        ShowContextMenu(tracks, blockIndex, e.mousePosition, ref selectedList, ref selectedIndex, markDirty);
                        e.Use();
                    }

                    break;

                case EventType.ScrollWheel:
                    if (area.Contains(e.mousePosition))
                    {
                        if (e.control || e.command)
                        {
                            float mouseX = e.mousePosition.x;
                            float tickAtMouse = (mouseX - contentX0 + state.scrollX) / Mathf.Max(4f, state.pxPerTick);
                            float factor = e.delta.y > 0f ? 1.1f : 1f / 1.1f;
                            state.pxPerTick = Mathf.Clamp(state.pxPerTick * factor, 4f, 120f);
                            state.scrollX = tickAtMouse * state.pxPerTick - (mouseX - contentX0);
                        }
                        else
                        {
                            state.scrollX -= (e.delta.y + e.delta.x) * 24f;
                        }

                        e.Use();
                    }

                    break;
            }

            state.scrollX = Mathf.Clamp(state.scrollX, 0f, maxScroll);
            DrawContents(area, tracks, config, state, selectedList, selectedIndex);
            return modified;
        }

        // ==================== 绘制细节 ====================

        private static void DrawContents(
            Rect area,
            List<TrackTemplate> tracks,
            BattleConfigData config,
            TimelineInteractionState state,
            List<TrackTemplate> selectedList,
            int selectedIndex)
        {
            bool pro = EditorGUIUtility.isProSkin;
            Color bg = pro ? new Color(0.21f, 0.21f, 0.21f) : new Color(0.76f, 0.76f, 0.76f);
            Color rulerBg = pro ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.66f, 0.66f, 0.66f);
            Color headBg = pro ? new Color(0.17f, 0.17f, 0.17f) : new Color(0.70f, 0.70f, 0.70f);
            Color stripe = pro ? new Color(1f, 1f, 1f, 0.045f) : new Color(0f, 0f, 0f, 0.05f);
            Color grid = pro ? new Color(1f, 1f, 1f, 0.08f) : new Color(0f, 0f, 0f, 0.10f);
            Color tickText = pro ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.18f, 0.18f, 0.18f);

            EditorGUI.DrawRect(area, bg);
            EditorGUI.DrawRect(new Rect(area.x, area.y, area.width, RulerHeight), rulerBg);

            float contentX0 = area.x + HeadWidth;
            float contentWidth = area.width - HeadWidth;

            // 行条纹与行头底色
            int rows = Mathf.Max(tracks.Count, 1);
            for (int i = 0; i < rows; i++)
            {
                float ry = area.y + RulerHeight + i * RowHeight;
                if ((i & 1) == 1)
                {
                    EditorGUI.DrawRect(new Rect(area.x, ry, area.width, RowHeight), stripe);
                }

                EditorGUI.DrawRect(new Rect(area.x, ry, HeadWidth, RowHeight), headBg);
            }

            // 刻度尺与网格线（自适应步长：放大时每5tick主刻度，缩小时每10tick）
            float px = Mathf.Max(4f, state.pxPerTick);
            int minorStep = px >= 10f ? 5 : 10;
            int majorStep = minorStep * 2;
            int startTick = Mathf.Max(0, Mathf.FloorToInt(state.scrollX / px) - 1);
            int endTick = Mathf.CeilToInt((state.scrollX + contentWidth) / px) + 1;
            TickLabelStyle.normal.textColor = tickText;
            for (int tick = startTick; tick <= endTick; tick++)
            {
                if (tick % minorStep != 0)
                {
                    continue;
                }

                float x = contentX0 + tick * px - state.scrollX;
                bool major = tick % majorStep == 0;
                float tickH = major ? 6f : 4f;
                EditorGUI.DrawRect(new Rect(x, area.y + RulerHeight - tickH, 1f, tickH), grid);
                if (major)
                {
                    EditorGUI.DrawRect(new Rect(x, area.y + RulerHeight, 1f, area.height - RulerHeight), grid);
                    GUI.Label(new Rect(x + 2f, area.y, 52f, RulerHeight - 3f), tick.ToString(), TickLabelStyle);
                }
            }

            // 条块
            Rect clip = new Rect(contentX0, area.y + RulerHeight, contentWidth, area.height - RulerHeight);
            for (int i = 0; i < tracks.Count; i++)
            {
                TrackTemplate t = tracks[i];
                Rect blockRect = BlockRect(area, state, i, t);
                Rect visible = Intersect(blockRect, clip);
                if (visible.width < 1f || visible.height < 1f)
                {
                    continue;
                }

                bool selected = ReferenceEquals(selectedList, tracks) && selectedIndex == i;
                Color color = TypeColor(t.type);
                if (selected)
                {
                    EditorGUI.DrawRect(visible, new Color(1f, 0.85f, 0.2f));
                    Rect inner = new Rect(visible.x + 1f, visible.y + 1f, visible.width - 2f, visible.height - 2f);
                    if (inner.width > 0f && inner.height > 0f)
                    {
                        EditorGUI.DrawRect(inner, color);
                    }
                }
                else
                {
                    EditorGUI.DrawRect(visible, color);
                }

                if (visible.width > 36f)
                {
                    GUI.Label(
                        new Rect(visible.x + 4f, visible.y, visible.width - 8f, visible.height),
                        TrackSummary(t, config), BlockLabelStyle);
                }

                // 行头：类型色块 + 序号 + 类型短名（文字与色块同色，浅色皮肤下压暗保对比）
                float ry = area.y + RulerHeight + i * RowHeight;
                EditorGUI.DrawRect(new Rect(area.x + 3f, ry + RowHeight * 0.5f - 4f, 8f, 8f), color);
                GUI.Label(new Rect(area.x + 13f, ry, 12f, RowHeight), i.ToString(), HeadLabelStyle);
                TypeLabelStyle.normal.textColor = pro ? color : color * 0.6f;
                GUI.Label(new Rect(area.x + 27f, ry, HeadWidth - 30f, RowHeight), TypeShortLabel(t.type), TypeLabelStyle);
            }

            if (tracks.Count == 0)
            {
                GUI.Label(
                    new Rect(contentX0 + 8f, area.y + RulerHeight + 2f, contentWidth - 16f, RowHeight),
                    "右键空白处添加轨道（6 种类型）", HintLabelStyle);
            }
        }

        // ==================== 交互辅助 ====================

        /// <summary>鼠标Y命中：-2=刻度尺区 / -1=区域外 / >=0=行号（可能超出轨道数，调用方自行判界）</summary>
        private static int HitTestRow(Rect area, float mouseY)
        {
            float localY = mouseY - area.y;
            if (localY < 0f)
            {
                return -1;
            }

            if (localY < RulerHeight)
            {
                return -2;
            }

            return Mathf.FloorToInt((localY - RulerHeight) / RowHeight);
        }

        private static Rect BlockRect(Rect area, TimelineInteractionState state, int row, TrackTemplate t)
        {
            float x = area.x + HeadWidth + t.relTick * state.pxPerTick - state.scrollX;
            float w = Mathf.Max(MinBlockWidth, Mathf.Max(1, t.durationTicks) * state.pxPerTick);
            float y = area.y + RulerHeight + row * RowHeight + 3f;
            return new Rect(x, y, w, RowHeight - 6f);
        }

        private static Rect Intersect(Rect a, Rect b)
        {
            float x1 = Mathf.Max(a.xMin, b.xMin);
            float y1 = Mathf.Max(a.yMin, b.yMin);
            float x2 = Mathf.Min(a.xMax, b.xMax);
            float y2 = Mathf.Min(a.yMax, b.yMax);
            return new Rect(x1, y1, Mathf.Max(0f, x2 - x1), Mathf.Max(0f, y2 - y1));
        }

        /// <summary>
        /// 右键菜单：命中条块时=上移/下移/删除 + 添加（插入到其后）；空白处=添加（追加到末尾）。
        /// 回调在 DropDown 内同步执行，结束后回写选中项。
        /// </summary>
        private static void ShowContextMenu(
            List<TrackTemplate> tracks,
            int blockIndex,
            Vector2 mousePosition,
            ref List<TrackTemplate> selectedList,
            ref int selectedIndex,
            Action markDirty)
        {
            var menu = new GenericMenu();
            int sel = selectedIndex;
            List<TrackTemplate> selList = selectedList;
            bool onBlock = blockIndex >= 0 && blockIndex < tracks.Count;
            int insertAt = onBlock ? blockIndex + 1 : tracks.Count;

            if (onBlock)
            {
                if (blockIndex > 0)
                {
                    menu.AddItem(new GUIContent("上移"), false, () =>
                    {
                        Swap(tracks, blockIndex, blockIndex - 1);
                        if (sel == blockIndex) sel = blockIndex - 1;
                        else if (sel == blockIndex - 1) sel = blockIndex;
                        markDirty();
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("上移"));
                }

                if (blockIndex < tracks.Count - 1)
                {
                    menu.AddItem(new GUIContent("下移"), false, () =>
                    {
                        Swap(tracks, blockIndex, blockIndex + 1);
                        if (sel == blockIndex) sel = blockIndex + 1;
                        else if (sel == blockIndex + 1) sel = blockIndex;
                        markDirty();
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("下移"));
                }

                menu.AddItem(new GUIContent("删除"), false, () =>
                {
                    tracks.RemoveAt(blockIndex);
                    if (sel == blockIndex)
                    {
                        sel = -1;
                        selList = null;
                    }
                    else if (sel > blockIndex)
                    {
                        sel--;
                    }

                    markDirty();
                });

                menu.AddSeparator(string.Empty);
            }

            for (int i = 0; i < AllTrackTypes.Length; i++)
            {
                TrackType type = AllTrackTypes[i];
                menu.AddItem(new GUIContent("添加轨道/" + TypeLabel(type)), false, () =>
                {
                    int at = Mathf.Clamp(insertAt, 0, tracks.Count);
                    tracks.Insert(at, CreateDefaultTrack(type));
                    selList = tracks;
                    sel = at;
                    markDirty();
                });
            }

            menu.DropDown(new Rect(mousePosition.x, mousePosition.y, 0f, 0f));
            selectedList = selList;
            selectedIndex = sel;
        }

        private static void Swap(List<TrackTemplate> list, int a, int b)
        {
            TrackTemplate tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
        }

        // ==================== 文本样式（懒初始化） ====================

        private static GUIStyle BlockLabelStyle
        {
            get
            {
                if (s_blockLabel == null)
                {
                    s_blockLabel = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 10,
                        alignment = TextAnchor.MiddleLeft,
                        clipping = TextClipping.Clip,
                    };
                    s_blockLabel.normal.textColor = new Color(1f, 1f, 1f, 0.95f);
                }

                return s_blockLabel;
            }
        }

        private static GUIStyle TickLabelStyle
        {
            get
            {
                if (s_tickLabel == null)
                {
                    s_tickLabel = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 9,
                        alignment = TextAnchor.MiddleLeft,
                        clipping = TextClipping.Clip,
                    };
                }

                return s_tickLabel;
            }
        }

        private static GUIStyle HeadLabelStyle
        {
            get
            {
                if (s_headLabel == null)
                {
                    s_headLabel = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 9,
                        alignment = TextAnchor.MiddleLeft,
                        clipping = TextClipping.Clip,
                    };
                    s_headLabel.normal.textColor = new Color(0.68f, 0.68f, 0.68f);
                }

                return s_headLabel;
            }
        }

        private static GUIStyle TypeLabelStyle
        {
            get
            {
                if (s_typeLabel == null)
                {
                    s_typeLabel = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 9,
                        alignment = TextAnchor.MiddleLeft,
                        clipping = TextClipping.Clip,
                    };
                }

                return s_typeLabel;
            }
        }

        private static GUIStyle HintLabelStyle
        {
            get
            {
                if (s_hintLabel == null)
                {
                    s_hintLabel = new GUIStyle(EditorStyles.miniLabel)
                    {
                        fontSize = 10,
                        alignment = TextAnchor.MiddleLeft,
                        clipping = TextClipping.Clip,
                    };
                    s_hintLabel.normal.textColor = new Color(0.65f, 0.65f, 0.65f);
                }

                return s_hintLabel;
            }
        }
    }
}
