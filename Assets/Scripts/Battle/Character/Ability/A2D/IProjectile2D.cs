using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// 投射物统一接口。
    /// 新系统内的投射物组件实现此接口，即可被 RangedAttackAbility2D 驱动。
    /// 
    /// 旧系统（SquareBattle）的 Projectile 适配方案：
    ///   为旧 Projectile 预制体额外挂一个 SquareBattleProjectileAdapter 组件（实现此接口），
    ///   在 Init 里调用旧 Projectile.Init()，就可以无缝桥接，无需修改旧代码。
    /// </summary>
    public interface IProjectile2D
    {
        /// <summary>
        /// 初始化投射物。
        /// </summary>
        /// <param name="direction">归一化飞行方向</param>
        /// <param name="speed">飞行速度</param>
        /// <param name="damage">命中伤害</param>
        /// <param name="shooter">发射者（用于归属判断，可为 null）</param>
        void Init(Vector2 direction, float speed, int damage, BaseCharacter shooter);
    }
}
