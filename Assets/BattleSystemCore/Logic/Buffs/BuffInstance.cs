using System.Collections.Generic;

namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// Buff运行时实例：挂在目标BuffComponent中的一份生效记录。
    /// effectStates[i]对应data.effects[i]的运行时计数状态（触发计数等），参与状态哈希。
    /// </summary>
    public class BuffInstance
    {
        public BuffData data;

        /// <summary>剩余tick数，每帧-1，归零时移除（效果随之失效）</summary>
        public int remainTicks;

        /// <summary>当前层数（每层各贡献一份属性类效果）</summary>
        public int stacks;

        /// <summary>各效果运行时状态槽（计数器等），与data.effects按索引对应，创建时全0</summary>
        public List<int> effectStates = new List<int>();
    }
}
