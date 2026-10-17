using System.Collections.Generic;

namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// Buff静态定义（配置数据，运行期只读，可全局共享）。
    /// 效果为组合模型：effects由BuffEffect子类组成（属性修改/攻击触发/后续周期等），
    /// 一个Buff可同时携带多种效果。同buffId重复施加：刷新剩余时长，层数+1至上限
    /// （层数只放大属性类效果，触发类效果不随层数复制）。
    /// </summary>
    public class BuffData
    {
        public uint buffId;
        public string name;

        /// <summary>持续tick数（每次施加/刷新时重置）</summary>
        public int durationTicks;

        /// <summary>最大叠加层数，默认1（不可叠）</summary>
        public int maxStacks = 1;

        /// <summary>眩晕（控制类标志）：持有期间该Actor意图移动停止，位移效果轨道不受影响。
        /// 控制类保留裸字段（高频短路径查询），不走效果模型</summary>
        public bool isStun;

        /// <summary>效果列表（组合模式），索引与BuffInstance.effectStates一一对应</summary>
        public List<BuffEffect> effects = new List<BuffEffect>();
    }
}
