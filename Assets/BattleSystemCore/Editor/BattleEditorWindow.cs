using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Projectiles;
using BattleSystemCore.Logic.Skills;
using BattleSystemCore.Logic.Tracks;
using BattleSystemCore.Unity;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace BattleSystemCore.Editor
{
    /// <summary>
    /// 战斗配置编辑器（时间轴蓝图）主窗口：菜单 Tools/BattleSystemCore/战斗配置编辑器。
    /// Odin 菜单窗布局：顶部工具栏（保存/重载/校验/新建 + 文件路径）；
    /// 左侧 OdinMenuTree（搜索过滤、分组图标，菜单项绑定实体对象）；
    /// 右侧：实体属性 + 时间轴（BattleTimelineView 拖拽调时序）+ 选中轨道属性（BattleTrackInspectorPanel）。
    /// 数据唯一源：StreamingAssets/BattleCore/battlecore.xml（经 BattleXmlCodec 读写，与运行时装载同一文件）。
    /// v1 边界：不使用 Undo 系统（git 兜底数据安全）、无试跑预览；
    /// 运行时数据类不加 Odin attribute（保持 noEngineReferences 纯净），右侧编辑区完全自绘。
    /// </summary>
    public class BattleEditorWindow : OdinMenuEditorWindow
    {
        private const int ErrorDialogLimit = 10;
        private const float LeftMenuWidth = 240f;

        [MenuItem("Tools/BattleSystemCore/战斗配置编辑器")]
        private static void ShowWindow()
        {
            var window = GetWindow<BattleEditorWindow>();
            window.titleContent = new GUIContent("战斗配置编辑器");
            window.minSize = new Vector2(900f, 500f);
        }

        private BattleConfigData m_data;
        private bool m_dirty;
        private string m_loadError;

        private Vector2 m_detailScroll;

        /// <summary>菜单树延迟重建标记（增删实体/改名后下一帧重建，避免绘制中途改树）</summary>
        private bool m_treeRebuildPending;
        private object m_treeRebuildKeepSelected;

        // 时间轴选中（m_selList 指向当前选中的轨道列表，用于校验属性面板对应关系）
        private List<TrackTemplate> m_selList;
        private int m_selTrack = -1;
        private readonly TimelineInteractionState m_timeline = new TimelineInteractionState();

        /// <summary>Buff 效果卡片的折叠状态（默认全部展开）</summary>
        private readonly HashSet<BuffEffect> m_effectClosed = new HashSet<BuffEffect>();

        // ==================== Odin 菜单窗框架 ====================

        protected override OdinMenuTree BuildMenuTree()
        {
            MenuWidth = LeftMenuWidth;
            ResizableMenuWidth = true;

            var tree = new OdinMenuTree(false);
            tree.Config.DrawSearchToolbar = true;
            tree.Config.AutoFocusSearchBar = false;

            if (m_data == null)
            {
                LoadFromDisk();
            }

            if (m_data != null)
            {
                tree.Add("技能", null, SdfIconType.Lightning);
                foreach (SkillData s in m_data.skills)
                {
                    tree.Add("技能/" + s.skillId + " " + s.name, s, SdfIconType.LightningCharge);
                }

                tree.Add("Buff", null, SdfIconType.ShieldFill);
                foreach (BuffData b in m_data.buffs)
                {
                    tree.Add("Buff/" + b.buffId + " " + b.name, b, SdfIconType.Shield);
                }

                tree.Add("投射物", null, SdfIconType.SendFill);
                foreach (ProjectileData p in m_data.projectiles)
                {
                    tree.Add("投射物/" + p.projectileId + " " + p.name, p, SdfIconType.Send);
                }
            }

            return tree;
        }

        protected override void OnImGUI()
        {
            HandleShortcuts();
            DrawToolbar();

            if (m_treeRebuildPending)
            {
                m_treeRebuildPending = false;
                ForceMenuTreeRebuild();
                if (m_treeRebuildKeepSelected != null)
                {
                    TrySelectMenuItemWithObject(m_treeRebuildKeepSelected);
                }
            }

            base.OnImGUI();
        }

        private void HandleShortcuts()
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown
                && (e.control || e.command) && e.keyCode == KeyCode.S)
            {
                e.Use();
                Save();
            }
        }

        // ==================== 工具栏 ====================

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (SirenixEditorGUI.ToolbarButton(m_dirty ? "保存 *" : "保存", false))
            {
                Save();
            }

            if (SirenixEditorGUI.ToolbarButton("重新加载", false))
            {
                ReloadFromDisk();
            }

            if (SirenixEditorGUI.ToolbarButton("校验", false))
            {
                ValidateAndReport();
            }

            if (m_data != null)
            {
                if (SirenixEditorGUI.ToolbarButton("+ 技能", false))
                {
                    NewSkill();
                }

                if (SirenixEditorGUI.ToolbarButton("+ Buff", false))
                {
                    NewBuff();
                }

                if (SirenixEditorGUI.ToolbarButton("+ 投射物", false))
                {
                    NewProjectile();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label(BattleXmlConfigLoader.FilePath, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        // ==================== 右侧编辑区 ====================

        protected override void DrawEditor(int index)
        {
            if (m_data == null)
            {
                DrawNoDataPane();
                return;
            }

            m_detailScroll = EditorGUILayout.BeginScrollView(m_detailScroll);

            object selected = MenuTree != null && MenuTree.Selection != null
                ? MenuTree.Selection.SelectedValue
                : null;

            if (selected is SkillData skill)
            {
                DrawSkillPane(skill);
            }
            else if (selected is BuffData buff)
            {
                DrawBuffPane(buff);
            }
            else if (selected is ProjectileData projectile)
            {
                DrawProjectilePane(projectile);
            }
            else
            {
                SirenixEditorGUI.InfoMessageBox(
                    "在左侧选择或新建一个实体。\n时间轴：左键拖条身改 relTick、拖右缘改持续时长（仅 Move）、右键空白处添加轨道、Ctrl+滚轮缩放。",
                    true);
            }

            EditorGUILayout.EndScrollView();
        }

        // ==================== 无数据视图（文件缺失/解析失败） ====================

        private void DrawNoDataPane()
        {
            EditorGUILayout.Space(16f);
            if (!string.IsNullOrEmpty(m_loadError))
            {
                SirenixEditorGUI.ErrorMessageBox(m_loadError, true);
            }

            EditorGUILayout.LabelField("可「重新加载」重试，或从零开始新建一份空白配置（保存时写入配置文件）。", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重新加载", GUILayout.Height(24f)))
            {
                LoadFromDisk();
                ScheduleTreeRebuild(null);
            }

            if (GUILayout.Button("新建空白配置", GUILayout.Height(24f)))
            {
                m_data = new BattleConfigData();
                m_loadError = null;
                m_dirty = true;
                ClearSelection();
                ScheduleTreeRebuild(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSkillPane(SkillData s)
        {
            SirenixEditorGUI.Title("技能 " + s.skillId, s.name, TextAlignment.Left, false, true);

            if (DrawEntityDeleteRow("技能 " + s.skillId))
            {
                m_data.skills.Remove(s);
                ClearSelection();
                MarkDirty();
                ScheduleTreeRebuild(null);
                return;
            }

            EditorGUILayout.LabelField("id", s.skillId.ToString());
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField("name", s.name);
            if (EditorGUI.EndChangeCheck())
            {
                s.name = name;
                MarkDirty();
                ScheduleTreeRebuild(s);
            }

            EditorGUILayout.Space(6f);
            DrawTrackSection(s.payload, "时间轴（载荷轨道：从上到下 = 同帧结算顺序）");
        }

        private void DrawBuffPane(BuffData b)
        {
            SirenixEditorGUI.Title("Buff " + b.buffId, b.name, TextAlignment.Left, false, true);

            if (DrawEntityDeleteRow("Buff " + b.buffId, FindBuffReferences(b.buffId)))
            {
                m_data.buffs.Remove(b);
                ClearSelection();
                MarkDirty();
                ScheduleTreeRebuild(null);
                return;
            }

            EditorGUILayout.LabelField("id", b.buffId.ToString());
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField("name", b.name);
            if (EditorGUI.EndChangeCheck())
            {
                b.name = name;
                MarkDirty();
                ScheduleTreeRebuild(b);
            }

            DrawIntField("durationTicks（持续帧）", ref b.durationTicks, 1);
            DrawIntField("maxStacks（最大层数）", ref b.maxStacks, 1);

            EditorGUI.BeginChangeCheck();
            bool isStun = EditorGUILayout.Toggle("isStun（眩晕控制）", b.isStun);
            if (EditorGUI.EndChangeCheck())
            {
                b.isStun = isStun;
                MarkDirty();
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("效果（" + b.effects.Count + "）", EditorStyles.boldLabel);

            int removeAt = -1;
            for (int i = 0; i < b.effects.Count; i++)
            {
                if (DrawEffectCard(b.effects[i], i))
                {
                    removeAt = i;
                    break;
                }
            }

            if (removeAt >= 0)
            {
                m_effectClosed.Remove(b.effects[removeAt]);
                b.effects.RemoveAt(removeAt);
                MarkDirty();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ 属性修改效果"))
            {
                b.effects.Add(new StatModifierEffect());
                MarkDirty();
            }

            if (GUILayout.Button("+ 攻击触发效果"))
            {
                b.effects.Add(new AttackTriggerEffect());
                MarkDirty();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawProjectilePane(ProjectileData p)
        {
            SirenixEditorGUI.Title("投射物 " + p.projectileId, p.name, TextAlignment.Left, false, true);

            if (DrawEntityDeleteRow("投射物 " + p.projectileId, FindProjectileReferences(p.projectileId)))
            {
                m_data.projectiles.Remove(p);
                ClearSelection();
                MarkDirty();
                ScheduleTreeRebuild(null);
                return;
            }

            EditorGUILayout.LabelField("id", p.projectileId.ToString());
            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField("name", p.name);
            if (EditorGUI.EndChangeCheck())
            {
                p.name = name;
                MarkDirty();
                ScheduleTreeRebuild(p);
            }

            DrawIntField("speed（milli/tick）", ref p.speedMilliPerTick, 0);
            DrawIntField("hitRadius（命中半径 milli）", ref p.hitRadiusMilli, 0);
            DrawIntField("maxRange（最大射程 milli）", ref p.maxRangeMilli, 0);

            EditorGUILayout.Space(6f);
            DrawTrackSection(p.payload, "命中载荷时间轴");
        }

        /// <summary>实体删除按钮行（含引用情况确认）。返回 true 表示用户确认删除</summary>
        private bool DrawEntityDeleteRow(string title, string referenceInfo = null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool del = GUILayout.Button("删除", EditorStyles.miniButton, GUILayout.Width(44f));
            EditorGUILayout.EndHorizontal();

            if (!del)
            {
                return false;
            }

            string message = string.IsNullOrEmpty(referenceInfo)
                ? "确认删除 " + title + "？\n\n此操作不可撤销（保存后写盘生效）。"
                : "确认删除 " + title + "？\n\n以下位置仍在引用它（删除将造成配置引用错误）：\n" + referenceInfo;
            return EditorUtility.DisplayDialog("删除确认", message, "删除", "取消");
        }

        /// <summary>时间轴区段：轨道视图 + 选中轨道的属性面板（选中列表必须与当前区段一致）</summary>
        private void DrawTrackSection(List<TrackTemplate> tracks, string header)
        {
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            if (tracks.Count == 0)
            {
                EditorGUILayout.LabelField("（空——在下方时间轴空白处右键添加轨道）", EditorStyles.miniLabel);
            }

            SirenixEditorGUI.BeginBox();
            BattleTimelineView.Draw(tracks, m_data, m_timeline, ref m_selList, ref m_selTrack, MarkDirty);

            if (m_selList == tracks && m_selTrack >= 0)
            {
                if (m_selTrack >= tracks.Count)
                {
                    m_selTrack = -1;
                }
                else
                {
                    BattleTrackInspectorPanel.Draw(tracks, ref m_selTrack, m_data, MarkDirty);
                }
            }

            SirenixEditorGUI.EndBox();
        }

        // ==================== Buff 效果卡片 ====================

        /// <summary>绘制单个效果卡片。返回 true 表示用户请求删除该效果</summary>
        private bool DrawEffectCard(BuffEffect effect, int index)
        {
            bool open = !m_effectClosed.Contains(effect);
            bool deleted = false;

            SirenixEditorGUI.BeginBox();

            EditorGUILayout.BeginHorizontal();
            string title;
            if (effect is StatModifierEffect)
            {
                title = "属性修改效果 #" + index;
            }
            else if (effect is AttackTriggerEffect)
            {
                title = "攻击触发效果 #" + index;
            }
            else
            {
                title = "效果 #" + index + "（未知类型）";
            }

            bool newOpen = EditorGUILayout.Foldout(open, title, true);
            if (newOpen != open)
            {
                if (newOpen)
                {
                    m_effectClosed.Remove(effect);
                }
                else
                {
                    m_effectClosed.Add(effect);
                }

                open = newOpen;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除", EditorStyles.miniButton, GUILayout.Width(44f)))
            {
                deleted = true;
            }

            EditorGUILayout.EndHorizontal();

            if (open && !deleted)
            {
                if (effect is StatModifierEffect stat)
                {
                    DrawStatModifier(stat);
                }
                else if (effect is AttackTriggerEffect trigger)
                {
                    DrawAttackTrigger(trigger);
                }
                else
                {
                    EditorGUILayout.LabelField("该效果子类不在编辑器支持范围内（不会被序列化，保存前请移除）", EditorStyles.miniLabel);
                }
            }

            SirenixEditorGUI.EndBox();
            return deleted;
        }

        private void DrawStatModifier(StatModifierEffect stat)
        {
            EditorGUI.BeginChangeCheck();
            var statType = (StatType)EditorGUILayout.EnumPopup("statType", stat.statType);
            var modType = (ModifierType)EditorGUILayout.EnumPopup("modType", stat.modType);
            int value = EditorGUILayout.IntField("value", stat.value);
            if (EditorGUI.EndChangeCheck())
            {
                stat.statType = statType;
                stat.modType = modType;
                stat.value = value;
                MarkDirty();
            }
        }

        private void DrawAttackTrigger(AttackTriggerEffect trigger)
        {
            EditorGUI.BeginChangeCheck();
            int everyN = EditorGUILayout.IntField("everyN（每N次普攻命中触发）", trigger.everyN);
            if (EditorGUI.EndChangeCheck())
            {
                trigger.everyN = Mathf.Max(1, everyN);
                MarkDirty();
            }

            EditorGUILayout.Space(4f);
            DrawTrackSection(trigger.payload, "触发载荷时间轴");
        }

        // ==================== 新建与删除辅助 ====================

        private void NewSkill()
        {
            var s = new SkillData
            {
                skillId = NextId(m_data.skills, x => x.skillId),
                name = "新技能",
            };
            m_data.skills.Add(s);
            ScheduleTreeRebuild(s);
            MarkDirty();
        }

        private void NewBuff()
        {
            var b = new BuffData
            {
                buffId = NextId(m_data.buffs, x => x.buffId),
                name = "新Buff",
                durationTicks = 60,
                maxStacks = 1,
            };
            m_data.buffs.Add(b);
            ScheduleTreeRebuild(b);
            MarkDirty();
        }

        private void NewProjectile()
        {
            var p = new ProjectileData
            {
                projectileId = NextId(m_data.projectiles, x => x.projectileId),
                name = "新投射物",
                speedMilliPerTick = 500,
                hitRadiusMilli = 300,
                maxRangeMilli = 6000,
            };
            m_data.projectiles.Add(p);
            ScheduleTreeRebuild(p);
            MarkDirty();
        }

        private static uint NextId<T>(List<T> list, Func<T, uint> idOf)
        {
            uint max = 0;
            for (int i = 0; i < list.Count; i++)
            {
                uint id = idOf(list[i]);
                if (id > max)
                {
                    max = id;
                }
            }

            return max + 1;
        }

        /// <summary>扫描 ApplyBuff 轨道对指定 buff 的引用</summary>
        private string FindBuffReferences(uint buffId)
        {
            var refs = new List<string>();
            foreach (SkillData s in m_data.skills)
            {
                ScanTracksForBuff(s.payload, buffId, "技能 " + s.skillId + "（" + s.name + "）", refs);
            }

            foreach (ProjectileData p in m_data.projectiles)
            {
                ScanTracksForBuff(p.payload, buffId, "投射物 " + p.projectileId + "（" + p.name + "）", refs);
            }

            foreach (BuffData b in m_data.buffs)
            {
                foreach (BuffEffect effect in b.effects)
                {
                    if (effect is AttackTriggerEffect trigger)
                    {
                        ScanTracksForBuff(trigger.payload, buffId, "Buff " + b.buffId + " 触发载荷", refs);
                    }
                }
            }

            return string.Join("\n", refs);
        }

        private static void ScanTracksForBuff(List<TrackTemplate> tracks, uint buffId, string owner, List<string> refs)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].type == TrackType.ApplyBuff && tracks[i].buffId == buffId)
                {
                    refs.Add(owner + " 轨道#" + i);
                }
            }
        }

        /// <summary>扫描 Projectile 轨道对指定投射物的引用</summary>
        private string FindProjectileReferences(uint projectileId)
        {
            var refs = new List<string>();
            foreach (SkillData s in m_data.skills)
            {
                ScanTracksForProjectile(s.payload, projectileId, "技能 " + s.skillId + "（" + s.name + "）", refs);
            }

            foreach (ProjectileData p in m_data.projectiles)
            {
                ScanTracksForProjectile(p.payload, projectileId, "投射物 " + p.projectileId + "（" + p.name + "）", refs);
            }

            foreach (BuffData b in m_data.buffs)
            {
                foreach (BuffEffect effect in b.effects)
                {
                    if (effect is AttackTriggerEffect trigger)
                    {
                        ScanTracksForProjectile(trigger.payload, projectileId, "Buff " + b.buffId + " 触发载荷", refs);
                    }
                }
            }

            return string.Join("\n", refs);
        }

        private static void ScanTracksForProjectile(List<TrackTemplate> tracks, uint projectileId, string owner, List<string> refs)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].type == TrackType.SpawnBullet && tracks[i].projectileId == projectileId)
                {
                    refs.Add(owner + " 轨道#" + i);
                }
            }
        }

        // ==================== 文件读写 ====================

        private void LoadFromDisk()
        {
            string path = BattleXmlConfigLoader.FilePath;
            try
            {
                if (!File.Exists(path))
                {
                    m_data = null;
                    m_loadError = "配置文件不存在：\n" + path;
                }
                else
                {
                    m_data = BattleXmlCodec.Parse(File.ReadAllText(path));
                    m_loadError = null;
                    m_dirty = false;
                }
            }
            catch (Exception ex)
            {
                m_data = null;
                m_loadError = "加载失败：" + ex.Message;
                m_dirty = false;
            }

            ClearSelection();
            m_timeline.scrollX = 0f;
            m_effectClosed.Clear();
        }

        private void ReloadFromDisk()
        {
            if (m_data != null && m_dirty)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "未保存的修改",
                    "当前配置有未保存的修改。重新加载将丢弃这些修改。",
                    "保存并重载", "放弃修改", "取消");

                if (choice == 2)
                {
                    return;
                }

                if (choice == 0)
                {
                    Save();
                    if (m_dirty)
                    {
                        // 保存失败（校验未过/IO错误）：不重载，避免丢数据
                        return;
                    }
                }
            }

            LoadFromDisk();
            ScheduleTreeRebuild(null);
        }

        private void Save()
        {
            if (m_data == null)
            {
                return;
            }

            List<string> errors = BattleXmlCodec.Validate(m_data);
            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "校验失败，未保存",
                    "共 " + errors.Count + " 项错误（须全部修复后才能保存）：\n\n" + JoinFirst(errors, ErrorDialogLimit),
                    "确定");
                return;
            }

            foreach (string warning in CollectWarnings())
            {
                Debug.LogWarning("[BattleEditor] " + warning);
            }

            try
            {
                File.WriteAllText(BattleXmlConfigLoader.FilePath, BattleXmlCodec.Write(m_data));
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("保存失败", ex.Message, "确定");
                return;
            }

            AssetDatabase.Refresh();
            m_dirty = false;
            Debug.Log("[BattleEditor] 已保存 " + BattleXmlConfigLoader.FilePath);
        }

        private void ValidateAndReport()
        {
            if (m_data == null)
            {
                return;
            }

            List<string> errors = BattleXmlCodec.Validate(m_data);
            List<string> warnings = CollectWarnings();
            if (errors.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "校验发现问题",
                    "错误 " + errors.Count + " 项（保存将被阻断）：\n\n" + JoinFirst(errors, ErrorDialogLimit),
                    "确定");
                return;
            }

            foreach (string warning in warnings)
            {
                Debug.LogWarning("[BattleEditor] " + warning);
            }

            EditorUtility.DisplayDialog(
                "校验通过",
                warnings.Count == 0 ? "配置合法，无错误。" : "配置合法（" + warnings.Count + " 条警告，见 Console）。",
                "确定");
        }

        /// <summary>提示性警告（不阻断保存）：空技能载荷等</summary>
        private List<string> CollectWarnings()
        {
            var warnings = new List<string>();
            foreach (SkillData s in m_data.skills)
            {
                if (s.payload.Count == 0)
                {
                    warnings.Add("技能 " + s.skillId + "（" + s.name + "）载荷为空，施放不会产生任何效果");
                }
            }

            return warnings;
        }

        private static string JoinFirst(List<string> items, int max)
        {
            var sb = new StringBuilder();
            int n = Mathf.Min(items.Count, max);
            for (int i = 0; i < n; i++)
            {
                sb.Append("• ").Append(items[i]).Append('\n');
            }

            if (items.Count > max)
            {
                sb.Append("…（共 ").Append(items.Count).Append(" 项）");
            }

            return sb.ToString();
        }

        // ==================== 选择、菜单重建与脏标记 ====================

        private void MarkDirty()
        {
            m_dirty = true;
            Repaint();
        }

        /// <summary>清空轨道级选择（实体级选择由 Odin 菜单树管理）</summary>
        private void ClearSelection()
        {
            m_selList = null;
            m_selTrack = -1;
        }

        /// <summary>
        /// 下一帧重建菜单树并恢复选中。增删实体/改名后调用（不在绘制中途改树）；
        /// keepSelected 为 null 时尝试保留当前选中项，找不到则清空选择。
        /// </summary>
        private void ScheduleTreeRebuild(object keepSelected)
        {
            m_treeRebuildPending = true;
            m_treeRebuildKeepSelected = keepSelected
                ?? (MenuTree != null && MenuTree.Selection != null ? MenuTree.Selection.SelectedValue : null);
        }

        private void DrawIntField(string label, ref int value, int min)
        {
            EditorGUI.BeginChangeCheck();
            int v = EditorGUILayout.IntField(label, value);
            if (EditorGUI.EndChangeCheck())
            {
                value = Mathf.Max(min, v);
                MarkDirty();
            }
        }
    }
}
