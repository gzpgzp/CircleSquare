using BattleSystemCore.Logic.Actors;

namespace BattleSystemCore.Logic.Physics
{
    /// <summary>攻击判定形状类型</summary>
    public enum AttackShapeType : byte
    {
        /// <summary>圆形（以origin为心，radius为半径的水平圆）</summary>
        Sphere = 1,

        /// <summary>扇形（origin为顶点，forward为平分线方向，radius为半径，minDot为半角余弦阈值）</summary>
        Sector = 2,

        /// <summary>轴对齐矩形（center为中心，halfExtents为半尺寸；水平放置，y不参与）</summary>
        Box = 3,
    }

    /// <summary>
    /// 攻击形状数据（纯数据，可直接映射Luban配置字段）。
    /// 扇形不存角度存minDot（与朝向夹角余弦的万分比阈值，10000=仅正前方，7071≈半角45°，0=±90°）：
    /// 余弦阈值把"角度比较"化为整数点积比较，避免浮点三角函数，确定性成立。
    /// </summary>
    public struct AttackShape
    {
        public AttackShapeType type;

        /// <summary>Sphere/Sector：半径（milli）</summary>
        public int radius;

        /// <summary>Sector：与朝向夹角余弦阈值（万分比）</summary>
        public int minDot;

        /// <summary>Box：半尺寸（x/z参与，y忽略）</summary>
        public Vec3 halfExtents;

        public static AttackShape Sphere(int radiusMilli)
        {
            return new AttackShape { type = AttackShapeType.Sphere, radius = radiusMilli };
        }

        public static AttackShape Sector(int radiusMilli, int minDotTenThousandths)
        {
            return new AttackShape
            {
                type = AttackShapeType.Sector,
                radius = radiusMilli,
                minDot = minDotTenThousandths,
            };
        }

        public static AttackShape Box(int halfExtentXMilli, int halfExtentZMilli)
        {
            return new AttackShape
            {
                type = AttackShapeType.Box,
                halfExtents = new Vec3(halfExtentXMilli, 0, halfExtentZMilli),
            };
        }
    }

    /// <summary>
    /// 命中判定纯函数库：形状×点的求交判定，全整数运算（DistanceSq判距、ISqrt、点积判角），确定性成立。
    /// 地面战斗形态约定：所有判定在水平面（x/z）进行，y不参与（跳跃/击飞中仍可被选中，
    /// 与LOL/格斗类逻辑层惯例一致）；若日后需要高度遮蔽（低身躲技能），在此层加yRange参数即可。
    /// point统一带pointRadius（目标身体半径）：命中=形状按pointRadius膨胀后包含该点，大体型目标边缘也可吃到判定。
    /// </summary>
    public static class HitTest
    {
        /// <summary>水平面（x/z）距离平方</summary>
        public static long HorizontalDistSq(Vec3 a, Vec3 b)
        {
            long dx = b.x - a.x;
            long dz = b.z - a.z;
            return dx * dx + dz * dz;
        }

        /// <summary>按shape类型分派的命中判定。forward仅扇形使用（水平朝向，y分量忽略）</summary>
        public static bool HitShape(
            in AttackShape shape, Vec3 origin, Vec3 forward, Vec3 point, int pointRadius)
        {
            switch (shape.type)
            {
                case AttackShapeType.Sphere: return HitSphere(shape.radius, origin, point, pointRadius);
                case AttackShapeType.Sector: return HitSector(shape.radius, shape.minDot, origin, forward, point, pointRadius);
                case AttackShapeType.Box: return HitBox(shape.halfExtents, origin, point, pointRadius);
                default: return false;
            }
        }

        /// <summary>圆判定：水平距离 ≤ 半径+目标半径</summary>
        public static bool HitSphere(int radius, Vec3 center, Vec3 point, int pointRadius)
        {
            long rSum = (long)radius + pointRadius;
            return HorizontalDistSq(center, point) <= rSum * rSum;
        }

        /// <summary>
        /// 扇形判定：先半径剪枝（同圆判定），再朝向点积判角。
        /// 数学：dot=d·forward，要求 cosθ ≥ minDot/10000。forward长度约定≈MilliPerUnit（DirectionTo输出），
        /// 整数化为 dot*MilliPerUnit ≥ minDot*dist（中间量long，常规量级无溢出）。
        /// point落在顶点（dist=0）视为命中；forward水平分量为零时判不命中（朝向退化无意义，调用方应保证有效）。
        /// </summary>
        public static bool HitSector(
            int radius, int minDot, Vec3 apex, Vec3 forward, Vec3 point, int pointRadius)
        {
            long dx = point.x - apex.x;
            long dz = point.z - apex.z;
            long distSq = dx * dx + dz * dz;
            long rSum = (long)radius + pointRadius;
            if (distSq > rSum * rSum)
            {
                return false;
            }

            if (distSq == 0)
            {
                return true;
            }

            long fx = forward.x;
            long fz = forward.z;
            if (fx == 0 && fz == 0)
            {
                return false;
            }

            long dot = dx * fx + dz * fz;
            if (dot <= 0 && minDot >= 0)
            {
                // 背向剪枝（minDot非负时背向必不命中；minDot为负的钝角扇形走完整判定）
                return false;
            }

            int dist = FixMath.ISqrt(distSq);
            return dot * FixMath.MilliPerUnit >= (long)minDot * dist;
        }

        /// <summary>轴对齐矩形判定：|dx|≤半宽+半径 且 |dz|≤半深+半径（Minkowski膨胀，y不参与）</summary>
        public static bool HitBox(Vec3 halfExtents, Vec3 center, Vec3 point, int pointRadius)
        {
            long dx = point.x - center.x;
            long dz = point.z - center.z;
            long hx = (long)halfExtents.x + pointRadius;
            long hz = (long)halfExtents.z + pointRadius;
            return dx >= -hx && dx <= hx && dz >= -hz && dz <= hz;
        }
    }
}
