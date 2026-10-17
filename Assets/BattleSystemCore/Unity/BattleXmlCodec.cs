using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Physics;
using BattleSystemCore.Logic.Projectiles;
using BattleSystemCore.Logic.Skills;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 战斗配置编辑/传输容器：XML 与运行时对象树之间的中间形态
    /// （等价于旧五表装载后未注册进Library的状态，编辑器以它为编辑模型）。
    /// </summary>
    public sealed class BattleConfigData
    {
        public readonly List<BuffData> buffs = new List<BuffData>();
        public readonly List<SkillData> skills = new List<SkillData>();
        public readonly List<ProjectileData> projectiles = new List<ProjectileData>();
    }

    /// <summary>
    /// battlecore.xml 编解码器：运行时装载与编辑器共用唯一实现（不依赖UnityEngine，可脱离引擎单测）。
    /// 解析（Parse）：宽容读——属性缺省取默认值；但未知子元素、非法数值/枚举、缺失必填type一律抛异常（配置错误尽早炸）。
    /// 写出（Write）：规范化格式——枚举属性总是写出、数值属性仅非默认值写出、属性顺序固定、LF换行两空格缩进，
    /// 保证 git diff 稳定与"加载→原样保存→文件不变"的 round-trip 一致性。手工注释不保留（头部说明注释为固定输出）。
    /// 校验（Validate）：主键唯一、引用完整性（buffId/projectileId）、字段合法性；返回阻断级错误列表。
    /// </summary>
    public static class BattleXmlCodec
    {
        private const string HeaderComment =
            "<!-- BattleSystemCore 战斗配置唯一数据源（由战斗配置编辑器读写，也可手工编辑）。\n" +
            "     缺省值（写出时省略）：relTick=0 durationTicks=1 maxStacks=1 isStun=false fxUseImpact=false 其余数值=0；origin/target等枚举属性总是写出 -->\n";

        // ==================== Parse：XML → 运行时对象树 ====================

        /// <summary>解析 battlecore.xml 全文。语法/语义错误抛 ArgumentException（含元素与属性定位）</summary>
        public static BattleConfigData Parse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentException("BattleXmlCodec.Parse: xml 内容为空");
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch (XmlException ex)
            {
                throw new ArgumentException(
                    $"BattleXmlCodec.Parse: XML 语法错误（行 {ex.LineNumber} 列 {ex.LinePosition}）：{ex.Message}", ex);
            }

            var root = doc.Root;
            if (root == null || root.Name.LocalName != "BattleConfig")
            {
                throw new ArgumentException("BattleXmlCodec.Parse: 根元素必须是 <BattleConfig>");
            }

            var data = new BattleConfigData();

            var buffsEl = root.Element("Buffs");
            if (buffsEl != null)
            {
                foreach (var e in buffsEl.Elements("Buff"))
                {
                    data.buffs.Add(ParseBuff(e));
                }
            }

            var projectilesEl = root.Element("Projectiles");
            if (projectilesEl != null)
            {
                foreach (var e in projectilesEl.Elements("Projectile"))
                {
                    data.projectiles.Add(ParseProjectile(e));
                }
            }

            var skillsEl = root.Element("Skills");
            if (skillsEl != null)
            {
                foreach (var e in skillsEl.Elements("Skill"))
                {
                    data.skills.Add(ParseSkill(e));
                }
            }

            return data;
        }

        private static BuffData ParseBuff(XElement e)
        {
            var data = new BuffData
            {
                buffId = AttrUInt(e, "id", 0),
                name = AttrStr(e, "name"),
                durationTicks = AttrInt(e, "durationTicks", 0),
                maxStacks = AttrInt(e, "maxStacks", 1),
                isStun = AttrBool(e, "isStun", false),
            };

            foreach (var child in e.Elements())
            {
                switch (child.Name.LocalName)
                {
                    case "StatModifier":
                        data.effects.Add(new StatModifierEffect
                        {
                            statType = AttrEnum(child, "statType", StatType.Atk),
                            modType = AttrEnum(child, "modType", ModifierType.Flat),
                            value = AttrInt(child, "value", 0),
                        });
                        break;

                    case "AttackTrigger":
                    {
                        var trigger = new AttackTriggerEffect
                        {
                            everyN = AttrInt(child, "everyN", 1),
                        };
                        foreach (var trackEl in child.Elements("Track"))
                        {
                            trigger.payload.Add(ParseTrack(trackEl));
                        }

                        EnsureOnlyTrackChildren(child);
                        data.effects.Add(trigger);
                        break;
                    }

                    default:
                        throw new ArgumentException(
                            $"BattleXmlCodec.Parse: <Buff id=\"{data.buffId}\"> 下存在未知子元素 <{child.Name.LocalName}>（支持 StatModifier/AttackTrigger）");
                }
            }

            return data;
        }

        private static ProjectileData ParseProjectile(XElement e)
        {
            var data = new ProjectileData
            {
                projectileId = AttrUInt(e, "id", 0),
                name = AttrStr(e, "name"),
                speedMilliPerTick = AttrInt(e, "speed", 0),
                hitRadiusMilli = AttrInt(e, "hitRadius", 0),
                maxRangeMilli = AttrInt(e, "maxRange", 0),
            };

            foreach (var trackEl in e.Elements("Track"))
            {
                data.payload.Add(ParseTrack(trackEl));
            }

            EnsureOnlyTrackChildren(e);
            return data;
        }

        private static SkillData ParseSkill(XElement e)
        {
            var data = new SkillData
            {
                skillId = AttrUInt(e, "id", 0),
                name = AttrStr(e, "name"),
            };

            foreach (var trackEl in e.Elements("Track"))
            {
                data.payload.Add(ParseTrack(trackEl));
            }

            EnsureOnlyTrackChildren(e);
            return data;
        }

        private static TrackTemplate ParseTrack(XElement e)
        {
            if (e.Attribute("type") == null)
            {
                throw new ArgumentException("BattleXmlCodec.Parse: <Track> 缺少必填属性 type");
            }

            return new TrackTemplate
            {
                type = AttrEnum(e, "type", TrackType.DoDamage),
                relTick = AttrInt(e, "relTick", 0),
                durationTicks = AttrInt(e, "durationTicks", 1),
                origin = AttrEnum(e, "origin", RoleRef.Caster),
                target = AttrEnum(e, "target", RoleRef.Target),
                buffId = AttrUInt(e, "buffId", 0),
                x = AttrInt(e, "x", 0),
                y = AttrInt(e, "y", 0),
                z = AttrInt(e, "z", 0),
                shapeType = AttrEnumAsInt<AttackShapeType>(e, "shapeType"),
                shapeRadius = AttrInt(e, "shapeRadius", 0),
                shapeMinDot = AttrInt(e, "shapeMinDot", 0),
                shapeHalfX = AttrInt(e, "shapeHalfX", 0),
                shapeHalfZ = AttrInt(e, "shapeHalfZ", 0),
                shapeCenterMode = AttrInt(e, "shapeCenterMode", 0),
                vy0 = AttrInt(e, "vy0", 0),
                gravity = AttrInt(e, "gravity", 0),
                projectileId = AttrUInt(e, "projectileId", 0),
                bulletLabel = AttrInt(e, "bulletLabel", 0),
                bulletSpeed = AttrInt(e, "bulletSpeed", 0),
                bulletRadius = AttrInt(e, "bulletRadius", 0),
                bulletRange = AttrInt(e, "bulletRange", 0),
                bulletCtrlMode = AttrInt(e, "bulletCtrlMode", 0),
                fxId = AttrStr(e, "fxId"),
                fxUseImpact = AttrBool(e, "fxUseImpact", false),
            };
        }

        private static void EnsureOnlyTrackChildren(XElement e)
        {
            foreach (var child in e.Elements())
            {
                if (child.Name.LocalName != "Track")
                {
                    throw new ArgumentException(
                        $"BattleXmlCodec.Parse: <{e.Name.LocalName}> 下存在未知子元素 <{child.Name.LocalName}>（此处只允许 Track）");
                }
            }
        }

        // ==================== Write：对象树 → 规范化 XML ====================

        /// <summary>
        /// 序列化为规范格式文本（UTF-8 落盘由调用方负责；输出末尾含换行）。
        /// 运行时对象的 onTrigger 委托为程序侧逃生口，不参与序列化。
        /// </summary>
        public static string Write(BattleConfigData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var sb = new StringBuilder(4096);
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            sb.Append(HeaderComment);
            sb.Append("<BattleConfig>\n");

            if (data.buffs.Count > 0)
            {
                sb.Append("  <Buffs>\n");
                foreach (var buff in data.buffs)
                {
                    WriteBuff(sb, buff, "    ");
                }

                sb.Append("  </Buffs>\n");
            }

            if (data.projectiles.Count > 0)
            {
                sb.Append("  <Projectiles>\n");
                foreach (var projectile in data.projectiles)
                {
                    WriteProjectile(sb, projectile, "    ");
                }

                sb.Append("  </Projectiles>\n");
            }

            if (data.skills.Count > 0)
            {
                sb.Append("  <Skills>\n");
                foreach (var skill in data.skills)
                {
                    WriteSkill(sb, skill, "    ");
                }

                sb.Append("  </Skills>\n");
            }

            sb.Append("</BattleConfig>\n");
            return sb.ToString();
        }

        private static void WriteBuff(StringBuilder sb, BuffData buff, string indent)
        {
            sb.Append(indent).Append("<Buff");
            AppendAttr(sb, "id", buff.buffId);
            AppendAttrOpt(sb, "name", buff.name);
            AppendAttrInt(sb, "durationTicks", buff.durationTicks, 0);
            AppendAttrInt(sb, "maxStacks", buff.maxStacks, 1);
            AppendAttrBool(sb, "isStun", buff.isStun);
            if (buff.effects.Count == 0)
            {
                sb.Append("/>\n");
                return;
            }

            sb.Append(">\n");
            string childIndent = indent + "  ";
            foreach (var effect in buff.effects)
            {
                switch (effect)
                {
                    case StatModifierEffect stat:
                        sb.Append(childIndent).Append("<StatModifier");
                        AppendAttr(sb, "statType", stat.statType);
                        AppendAttr(sb, "modType", stat.modType);
                        AppendAttrInt(sb, "value", stat.value, 0);
                        sb.Append("/>\n");
                        break;

                    case AttackTriggerEffect trigger:
                        sb.Append(childIndent).Append("<AttackTrigger");
                        AppendAttrInt(sb, "everyN", trigger.everyN, 1);
                        if (trigger.payload.Count == 0)
                        {
                            sb.Append("/>\n");
                            break;
                        }

                        sb.Append(">\n");
                        foreach (var track in trigger.payload)
                        {
                            WriteTrack(sb, track, childIndent + "  ");
                        }

                        sb.Append(childIndent).Append("</AttackTrigger>\n");
                        break;

                    default:
                        throw new ArgumentException(
                            $"BattleXmlCodec.Write: 未支持的效果类型 {effect.GetType().Name}（buffId {buff.buffId}）");
                }
            }

            sb.Append(indent).Append("</Buff>\n");
        }

        private static void WriteProjectile(StringBuilder sb, ProjectileData projectile, string indent)
        {
            sb.Append(indent).Append("<Projectile");
            AppendAttr(sb, "id", projectile.projectileId);
            AppendAttrOpt(sb, "name", projectile.name);
            AppendAttrInt(sb, "speed", projectile.speedMilliPerTick, 0);
            AppendAttrInt(sb, "hitRadius", projectile.hitRadiusMilli, 0);
            AppendAttrInt(sb, "maxRange", projectile.maxRangeMilli, 0);
            WriteTrackListBody(sb, projectile.payload, indent, "Projectile");
        }

        private static void WriteSkill(StringBuilder sb, SkillData skill, string indent)
        {
            sb.Append(indent).Append("<Skill");
            AppendAttr(sb, "id", skill.skillId);
            AppendAttrOpt(sb, "name", skill.name);
            WriteTrackListBody(sb, skill.payload, indent, "Skill");
        }

        /// <summary>轨道列表容器元素的收尾写出（空列表自闭合；否则逐轨道缩进一层）</summary>
        private static void WriteTrackListBody(StringBuilder sb, List<TrackTemplate> tracks, string indent, string elementName)
        {
            if (tracks.Count == 0)
            {
                sb.Append("/>\n");
                return;
            }

            sb.Append(">\n");
            foreach (var track in tracks)
            {
                WriteTrack(sb, track, indent + "  ");
            }

            sb.Append(indent).Append("</").Append(elementName).Append(">\n");
        }

        private static void WriteTrack(StringBuilder sb, TrackTemplate t, string indent)
        {
            sb.Append(indent).Append("<Track");
            AppendAttr(sb, "type", t.type);
            AppendAttrInt(sb, "relTick", t.relTick, 0);
            AppendAttrInt(sb, "durationTicks", t.durationTicks, 1);
            AppendAttr(sb, "origin", t.origin);
            AppendAttr(sb, "target", t.target);
            AppendAttrUInt(sb, "buffId", t.buffId);
            AppendAttrInt(sb, "x", t.x, 0);
            AppendAttrInt(sb, "y", t.y, 0);
            AppendAttrInt(sb, "z", t.z, 0);
            if (t.shapeType != 0)
            {
                AppendAttr(sb, "shapeType", (AttackShapeType)t.shapeType);
            }

            AppendAttrInt(sb, "shapeRadius", t.shapeRadius, 0);
            AppendAttrInt(sb, "shapeMinDot", t.shapeMinDot, 0);
            AppendAttrInt(sb, "shapeHalfX", t.shapeHalfX, 0);
            AppendAttrInt(sb, "shapeHalfZ", t.shapeHalfZ, 0);
            AppendAttrInt(sb, "shapeCenterMode", t.shapeCenterMode, 0);
            AppendAttrInt(sb, "vy0", t.vy0, 0);
            AppendAttrInt(sb, "gravity", t.gravity, 0);
            AppendAttrUInt(sb, "projectileId", t.projectileId);
            AppendAttrInt(sb, "bulletLabel", t.bulletLabel, 0);
            AppendAttrInt(sb, "bulletSpeed", t.bulletSpeed, 0);
            AppendAttrInt(sb, "bulletRadius", t.bulletRadius, 0);
            AppendAttrInt(sb, "bulletRange", t.bulletRange, 0);
            AppendAttrInt(sb, "bulletCtrlMode", t.bulletCtrlMode, 0);
            AppendAttrOpt(sb, "fxId", t.fxId);
            AppendAttrBool(sb, "fxUseImpact", t.fxUseImpact);
            sb.Append("/>\n");
        }

        // ==================== Validate：引用完整性与字段合法性 ====================

        /// <summary>校验配置。返回阻断级错误（空列表=通过）；空技能等提示性警告由调用方自行处理</summary>
        public static List<string> Validate(BattleConfigData data)
        {
            var errors = new List<string>();
            if (data == null)
            {
                errors.Add("配置数据为空");
                return errors;
            }

            CollectIdErrors(data.buffs, b => b.buffId, "Buff", errors);
            CollectIdErrors(data.skills, s => s.skillId, "技能", errors);
            CollectIdErrors(data.projectiles, p => p.projectileId, "投射物", errors);

            var buffIds = new HashSet<uint>();
            foreach (var b in data.buffs)
            {
                buffIds.Add(b.buffId);
            }

            var projectileIds = new HashSet<uint>();
            foreach (var p in data.projectiles)
            {
                projectileIds.Add(p.projectileId);
            }

            foreach (var buff in data.buffs)
            {
                if (buff.durationTicks <= 0)
                {
                    errors.Add($"Buff {buff.buffId}（{buff.name}）: durationTicks 必须 > 0（当前 {buff.durationTicks}）");
                }

                for (int i = 0; i < buff.effects.Count; i++)
                {
                    if (buff.effects[i] is AttackTriggerEffect trigger)
                    {
                        if (trigger.everyN < 1)
                        {
                            errors.Add($"Buff {buff.buffId} 的攻击触发效果: everyN 必须 >= 1（当前 {trigger.everyN}）");
                        }

                        ValidateTracks(trigger.payload, $"Buff {buff.buffId} 触发载荷", buffIds, projectileIds, errors);
                    }
                }
            }

            foreach (var skill in data.skills)
            {
                ValidateTracks(skill.payload, $"技能 {skill.skillId}（{skill.name}）", buffIds, projectileIds, errors);
            }

            foreach (var projectile in data.projectiles)
            {
                ValidateTracks(projectile.payload, $"投射物 {projectile.projectileId}（{projectile.name}）命中载荷", buffIds, projectileIds, errors);
            }

            return errors;
        }

        private static void ValidateTracks(
            List<TrackTemplate> tracks, string owner,
            HashSet<uint> buffIds, HashSet<uint> projectileIds, List<string> errors)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                var t = tracks[i];
                string where = $"{owner} 轨道#{i}（{t.type}）";
                if (t.relTick < 0)
                {
                    errors.Add($"{where}: relTick 不能为负（{t.relTick}）");
                }

                if (t.durationTicks < 1)
                {
                    errors.Add($"{where}: durationTicks 必须 >= 1（{t.durationTicks}）");
                }

                switch (t.type)
                {
                    case TrackType.ApplyBuff:
                        if (!buffIds.Contains(t.buffId))
                        {
                            errors.Add($"{where}: buffId {t.buffId} 不存在于 Buffs（或被引用的 Buff 尚未定义）");
                        }

                        break;

                    case TrackType.SpawnBullet:
                        if (t.projectileId > 0)
                        {
                            if (!projectileIds.Contains(t.projectileId))
                            {
                                errors.Add($"{where}: projectileId {t.projectileId} 不存在于 Projectiles");
                            }
                        }
                        else if (t.bulletSpeed <= 0)
                        {
                            errors.Add($"{where}: projectileId=0（无模板）时必须内联 bulletSpeed > 0（当前 {t.bulletSpeed}）");
                        }

                        if (t.bulletSpeed < 0 || t.bulletRadius < 0 || t.bulletRange < 0)
                        {
                            errors.Add($"{where}: 覆盖值 bulletSpeed/bulletRadius/bulletRange 不能为负");
                        }

                        break;

                    case TrackType.ControlBullet:
                        if (t.bulletLabel <= 0)
                        {
                            errors.Add($"{where}: bulletLabel 必须 > 0（操控轨道须指定弹句柄，与 SpawnBullet 的 bulletLabel 对应）");
                        }

                        if (t.bulletCtrlMode != (int)BulletCtrlMode.FollowTarget
                            && t.bulletCtrlMode != (int)BulletCtrlMode.FixedDirection)
                        {
                            errors.Add($"{where}: bulletCtrlMode 只能为 0（追踪目标）/1（固定向量），当前 {t.bulletCtrlMode}");
                        }
                        else if (t.bulletCtrlMode == (int)BulletCtrlMode.FixedDirection
                            && t.x == 0 && t.y == 0 && t.z == 0)
                        {
                            errors.Add($"{where}: FixedDirection 模式下 x/y/z 不能全为 0（零向量无方向）");
                        }

                        break;

                    case TrackType.AttackShape:
                        if (t.shapeType != (int)AttackShapeType.Sphere
                            && t.shapeType != (int)AttackShapeType.Sector
                            && t.shapeType != (int)AttackShapeType.Box)
                        {
                            errors.Add($"{where}: AttackShape 必须指定 shapeType（Sphere/Sector/Box）");
                        }

                        if (t.shapeCenterMode != (int)ShapeCenterMode.FollowOrigin
                            && t.shapeCenterMode != (int)ShapeCenterMode.ImpactPoint)
                        {
                            errors.Add($"{where}: shapeCenterMode 只能为 0（跟随origin角色）/1（弹着点），当前 {t.shapeCenterMode}");
                        }

                        break;

                    case TrackType.TriggerFx:
                        if (string.IsNullOrEmpty(t.fxId))
                        {
                            errors.Add($"{where}: TriggerFx 必须填 fxId（特效库键，短键非路径）");
                        }

                        break;
                }
            }
        }

        private static void CollectIdErrors<T>(List<T> list, Func<T, uint> idOf, string kind, List<string> errors)
        {
            var seen = new HashSet<uint>();
            foreach (var item in list)
            {
                uint id = idOf(item);
                if (id == 0)
                {
                    errors.Add($"{kind}: id 必须 > 0（存在未分配 id 的条目）");
                }
                else if (!seen.Add(id))
                {
                    errors.Add($"{kind}: id {id} 重复");
                }
            }
        }

        // ==================== 属性读写小工具 ====================

        private static string AttrStr(XElement e, string name, string def = "")
        {
            var a = e.Attribute(name);
            return a == null ? def : a.Value;
        }

        private static int AttrInt(XElement e, string name, int def)
        {
            var a = e.Attribute(name);
            if (a == null)
            {
                return def;
            }

            if (int.TryParse(a.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
            {
                return v;
            }

            throw new ArgumentException(
                $"BattleXmlCodec.Parse: 元素 <{e.Name.LocalName}> 属性 {name}=\"{a.Value}\" 不是合法整数");
        }

        private static uint AttrUInt(XElement e, string name, uint def)
        {
            var a = e.Attribute(name);
            if (a == null)
            {
                return def;
            }

            if (uint.TryParse(a.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint v))
            {
                return v;
            }

            throw new ArgumentException(
                $"BattleXmlCodec.Parse: 元素 <{e.Name.LocalName}> 属性 {name}=\"{a.Value}\" 不是合法非负整数");
        }

        private static bool AttrBool(XElement e, string name, bool def)
        {
            var a = e.Attribute(name);
            if (a == null)
            {
                return def;
            }

            if (bool.TryParse(a.Value, out bool v))
            {
                return v;
            }

            throw new ArgumentException(
                $"BattleXmlCodec.Parse: 元素 <{e.Name.LocalName}> 属性 {name}=\"{a.Value}\" 不是合法布尔（true/false）");
        }

        private static T AttrEnum<T>(XElement e, string name, T def) where T : struct, Enum
        {
            var a = e.Attribute(name);
            if (a == null)
            {
                return def;
            }

            if (Enum.TryParse(a.Value, ignoreCase: true, out T v) && Enum.IsDefined(typeof(T), v))
            {
                return v;
            }

            throw new ArgumentException(
                $"BattleXmlCodec.Parse: 元素 <{e.Name.LocalName}> 属性 {name}=\"{a.Value}\" 不是合法的 {typeof(T).Name} 枚举值");
        }

        /// <summary>枚举名→int存储字段（如TrackTemplate.shapeType）；属性缺失返回0（未设置）</summary>
        private static int AttrEnumAsInt<T>(XElement e, string name) where T : struct, Enum
        {
            var a = e.Attribute(name);
            if (a == null)
            {
                return 0;
            }

            if (Enum.TryParse(a.Value, ignoreCase: true, out T v) && Enum.IsDefined(typeof(T), v))
            {
                return Convert.ToInt32(v, CultureInfo.InvariantCulture);
            }

            throw new ArgumentException(
                $"BattleXmlCodec.Parse: 元素 <{e.Name.LocalName}> 属性 {name}=\"{a.Value}\" 不是合法的 {typeof(T).Name} 枚举值");
        }

        private static void AppendAttr(StringBuilder sb, string name, string value)
        {
            sb.Append(' ').Append(name).Append("=\"").Append(EscapeAttr(value)).Append('"');
        }

        private static void AppendAttr(StringBuilder sb, string name, uint value)
        {
            AppendAttr(sb, name, value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendAttr(StringBuilder sb, string name, Enum value)
        {
            AppendAttr(sb, name, value.ToString());
        }

        private static void AppendAttrOpt(StringBuilder sb, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                AppendAttr(sb, name, value);
            }
        }

        private static void AppendAttrInt(StringBuilder sb, string name, int value, int def)
        {
            if (value != def)
            {
                AppendAttr(sb, name, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void AppendAttrUInt(StringBuilder sb, string name, uint value)
        {
            if (value != 0)
            {
                AppendAttr(sb, name, value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void AppendAttrBool(StringBuilder sb, string name, bool value)
        {
            if (value)
            {
                AppendAttr(sb, name, "true");
            }
        }

        private static string EscapeAttr(string s)
        {
            if (s.IndexOf('&') < 0 && s.IndexOf('<') < 0 && s.IndexOf('>') < 0 && s.IndexOf('"') < 0)
            {
                return s;
            }

            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
