using Battle.Character;
using Battle.Character.Ability.A2D;
using SquareBattle.BattleCore;
using SquareBattle.BattleCore.Characters;
using UnityEngine;

/// <summary>
/// 将旧系统 SquareBattle.Projectile 适配到新系统 IProjectile2D 接口。
/// 挂载到 SquareBattle Projectile 预制体上，RangedAttackAbility2D 即可驱动旧投射物。
///
/// 工作方式：
///   RangedAttackAbility2D.Shoot() 调用 IProjectile2D.Init(dir, speed, damage, shooter)
///   → 本适配器收到后，以 Camp.Camp1 为默认阵营调用旧 Projectile.Init()
///   → 若需要精确阵营支持，可为 CharacterContext 追加 camp 字段后在此读取。
/// </summary>
[RequireComponent(typeof(Projectile))]
public class SquareBattleProjectileAdapter : MonoBehaviour, IProjectile2D
{
    private Projectile _projectile;

    private void Awake()
    {
        _projectile = GetComponent<Projectile>();
    }

    /// <summary>
    /// 新系统调用入口。speed/damage 由 RangedAttackAbility2D 传入，
    /// 但旧 Projectile 的 speed/damage 在 Inspector 上配置，这里只传方向与阵营。
    /// 若需覆盖 speed/damage，可通过反射或将旧 Projectile 字段改为 public。
    /// </summary>
    public void Init(Vector2 direction, float speed, int damage, BaseCharacter shooter)
    {
        // 旧 Projectile 需要 CharacterBase 作为 shooter；新系统传来的是 BaseCharacter。
        // 这里传 null 作为 shooter（旧系统 Projectile 的 _owner 只在 TakeDamage attacker 参数中使用，
        // 暂时传 null 不会崩溃，后续可补充真正的 CharacterBase 引用）。
        _projectile.Init(direction, Camp.Camp1, null);
    }
}
