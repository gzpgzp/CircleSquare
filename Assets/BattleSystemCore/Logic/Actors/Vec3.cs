using System;

namespace BattleSystemCore.Logic.Actors
{
    /// <summary>
    /// 空间向量/位置（千分单位，全整型）：API风格对标UnityEngine.Vector3（运算符重载、zero、Distance等），
    /// 但所有运算均为确定性整数运算。表现层换算：new Vector3(v.x/1000f, v.y/1000f, v.z/1000f)。
    /// 2D战斗只用x/y（z恒0）；3D用x/z为地面、y为高度。
    /// 标量乘/除为分量整型运算（除法向零取整），调用方需保证数值范围不溢出（常规千分单位量级下安全）。
    /// </summary>
    public struct Vec3 : IEquatable<Vec3>
    {
        public int x;
        public int y;
        public int z;

        public Vec3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>零向量</summary>
        public static Vec3 zero => default;

        public static Vec3 operator +(Vec3 a, Vec3 b)
            => new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vec3 operator -(Vec3 a, Vec3 b)
            => new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vec3 operator -(Vec3 a)
            => new Vec3(-a.x, -a.y, -a.z);

        public static Vec3 operator *(Vec3 a, int scalar)
            => new Vec3(a.x * scalar, a.y * scalar, a.z * scalar);

        public static Vec3 operator *(int scalar, Vec3 a)
            => new Vec3(a.x * scalar, a.y * scalar, a.z * scalar);

        /// <summary>分量整除（向零取整）。divisor不得为0</summary>
        public static Vec3 operator /(Vec3 a, int divisor)
            => new Vec3(a.x / divisor, a.y / divisor, a.z / divisor);

        public static bool operator ==(Vec3 a, Vec3 b)
            => a.x == b.x && a.y == b.y && a.z == b.z;

        public static bool operator !=(Vec3 a, Vec3 b)
            => !(a == b);

        /// <summary>长度平方（milli²）。long以容纳大坐标</summary>
        public long sqrMagnitude
            => (long)x * x + (long)y * y + (long)z * z;

        /// <summary>长度（milli，整数开方向下取整）</summary>
        public int magnitude => FixMath.ISqrt(sqrMagnitude);

        /// <summary>两点距离平方。射程/范围判定优先用此方法（无开方，更快更精确）</summary>
        public static long DistanceSq(Vec3 a, Vec3 b)
            => FixMath.DistanceSquare(a.x, a.y, a.z, b.x, b.y, b.z);

        /// <summary>两点距离（milli，向下取整）</summary>
        public static int Distance(Vec3 a, Vec3 b)
            => FixMath.ISqrt(DistanceSq(a, b));

        /// <summary>指向target的方向单位向量（分量整型，向量长度约MilliPerUnit）。重合时返回zero</summary>
        public Vec3 DirectionTo(Vec3 target)
        {
            FixMath.GetDirection(x, y, z, target.x, target.y, target.z,
                out int ux, out int uy, out int uz);
            return new Vec3(ux, uy, uz);
        }

        public bool Equals(Vec3 other)
            => x == other.x && y == other.y && z == other.z;

        public override bool Equals(object obj)
            => obj is Vec3 other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(x, y, z);

        public override string ToString()
            => $"({x}, {y}, {z})";
    }
}
