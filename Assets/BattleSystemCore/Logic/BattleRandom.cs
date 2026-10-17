namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 确定性随机数（xorshift32）。所有战斗逻辑判定（暴击、闪避等）必须经由本类，
    /// 禁止使用System.Random、UnityEngine.Random等无共享种子的随机源。
    /// State可导出，用于状态哈希与断点调试。
    /// </summary>
    public sealed class BattleRandom
    {
        private uint state;

        public BattleRandom(uint seed)
        {
            // xorshift要求state非0
            state = seed == 0 ? 0x9E3779B9u : seed;
        }

        /// <summary>当前内部状态（只读）。参与BattleSystem.GetStateHash计算</summary>
        public uint State => state;

        /// <summary>下一个随机uint</summary>
        public uint NextUInt()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        /// <summary>[minInclusive, maxExclusive) 区间整数</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            return minInclusive + (int)(NextUInt() % (uint)(maxExclusive - minInclusive));
        }

        /// <summary>万分比判定：rate=10000必中，rate=3000约30%概率为true。越界值不消耗随机数</summary>
        public bool Roll(int ratePermyriad)
        {
            if (ratePermyriad <= 0)
            {
                return false;
            }

            if (ratePermyriad >= 10000)
            {
                return true;
            }

            return NextUInt() % 10000u < (uint)ratePermyriad;
        }
    }
}
