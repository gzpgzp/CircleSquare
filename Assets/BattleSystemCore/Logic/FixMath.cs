namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 定点数数学工具。空间数值统一为千分单位（milli）：1逻辑单位 = 1000 milli，表现层换算 值/1000。
    /// 距离比较优先用DistanceSquare（整数平方比较，无开方误差也更快）；确需长度时用ISqrt。
    /// 全部整数运算（中间量long防溢出），保证跨平台确定性。
    /// </summary>
    public static class FixMath
    {
        /// <summary>千分单位换算基数：1逻辑单位=1000 milli</summary>
        public const int MilliPerUnit = 1000;

        /// <summary>两点距离的平方（milli²）。长整中间量，入参差值过大时仍安全</summary>
        public static long DistanceSquare(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            long dx = x2 - x1;
            long dy = y2 - y1;
            long dz = z2 - z1;
            return dx * dx + dy * dy + dz * dz;
        }

        /// <summary>
        /// 整数平方根（向下取整），牛顿迭代法。确定性：同输入同输出。
        /// </summary>
        public static int ISqrt(long value)
        {
            if (value <= 0)
            {
                return 0;
            }

            long x = value;
            long r = (x + 1) / 2;
            while (r * r > x)
            {
                r = (r + x / r) / 2;
            }

            return (int)r;
        }

        /// <summary>
        /// from到to的方向单位向量（千分单位，即长度约1000）。两点重合时输出(0,0,0)。
        /// </summary>
        public static void GetDirection(
            int fromX, int fromY, int fromZ,
            int toX, int toY, int toZ,
            out int unitX, out int unitY, out int unitZ)
        {
            long dx = toX - fromX;
            long dy = toY - fromY;
            long dz = toZ - fromZ;
            long distSq = dx * dx + dy * dy + dz * dz;
            if (distSq == 0)
            {
                unitX = 0;
                unitY = 0;
                unitZ = 0;
                return;
            }

            int len = ISqrt(distSq);
            unitX = (int)(dx * MilliPerUnit / len);
            unitY = (int)(dy * MilliPerUnit / len);
            unitZ = (int)(dz * MilliPerUnit / len);
        }
    }
}
