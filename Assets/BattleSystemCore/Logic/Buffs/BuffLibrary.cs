using System;
using System.Collections.Generic;

namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// Buff静态定义注册表：buffId→BuffData。轨道模板（TrackTemplate.buffId）经它解析成BuffData引用，
    /// 是配置化链路（Luban Loader→Library→战斗运行时）的缓存层。
    /// BuffData定位即全局只读共享，同id重复注册视为配置错误（尽早炸）；同引用幂等放行，
    /// 供自检双跑战斗、Demo重启等场景安全复用。
    /// </summary>
    public static class BuffLibrary
    {
        private static readonly Dictionary<uint, BuffData> buffs = new Dictionary<uint, BuffData>();

        /// <summary>注册Buff定义。同id不同定义抛异常（配置错误尽早暴露），同引用幂等</summary>
        public static void Register(BuffData data)
        {
            if (data == null)
            {
                return;
            }

            if (buffs.TryGetValue(data.buffId, out var existing))
            {
                if (!ReferenceEquals(existing, data))
                {
                    throw new ArgumentException(
                        $"BuffLibrary: buffId {data.buffId}（{data.name}）重复注册为不同定义" +
                        $"（已注册：{existing.name}）");
                }

                return;
            }

            buffs.Add(data.buffId, data);
        }

        /// <summary>按id取Buff定义。未注册返回null（调用方自行处理：ApplyBuff防御空转，工厂则抛错）</summary>
        public static BuffData Get(uint buffId)
        {
            buffs.TryGetValue(buffId, out var data);
            return data;
        }

        /// <summary>清空注册表（重载配置/重置测试环境时用）。运行中的战斗不受影响——它持有的是实例引用</summary>
        public static void Clear()
        {
            buffs.Clear();
        }
    }
}
