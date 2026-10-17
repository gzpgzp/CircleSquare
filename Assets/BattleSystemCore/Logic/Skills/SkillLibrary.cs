using System;
using System.Collections.Generic;

namespace BattleSystemCore.Logic.Skills
{
    /// <summary>
    /// 技能静态定义注册表：skillId→SkillData（照BuffLibrary模式）。
    /// 配置化链路（Luban Loader→Library→战斗运行时）的缓存层；SkillCaster施放时经此解析。
    /// 同id不同定义视为配置错误（尽早炸）；同引用幂等，供自检双跑、Demo重启安全复用。
    /// </summary>
    public static class SkillLibrary
    {
        private static readonly Dictionary<uint, SkillData> skills = new Dictionary<uint, SkillData>();

        /// <summary>注册技能定义。同id不同定义抛异常，同引用幂等</summary>
        public static void Register(SkillData data)
        {
            if (data == null)
            {
                return;
            }

            if (skills.TryGetValue(data.skillId, out var existing))
            {
                if (!ReferenceEquals(existing, data))
                {
                    throw new ArgumentException(
                        $"SkillLibrary: skillId {data.skillId}（{data.name}）重复注册为不同定义" +
                        $"（已注册：{existing.name}）");
                }

                return;
            }

            skills.Add(data.skillId, data);
        }

        /// <summary>按id取技能定义。未注册返回null（Cast自行空转）</summary>
        public static SkillData Get(uint skillId)
        {
            skills.TryGetValue(skillId, out var data);
            return data;
        }

        /// <summary>清空注册表（重载配置/重置测试环境）</summary>
        public static void Clear()
        {
            skills.Clear();
        }
    }
}
