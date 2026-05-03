using Battle.Character.Ability.A2D;
using UnityEngine;

/// <summary>
/// 将 Unity 的碰撞回调转发给 DriftMoveAbility2D。
/// 挂载到 Car 角色预制体的同一 GameObject 上，与 Character2D 共存。
///
/// 使用方式：
///   Character2D 上通过代码 GetComponent<DriftMoveCollisionBridge>()
///   或在 Awake 时自动查找并绑定 DriftMoveAbility2D。
/// </summary>
public class DriftMoveCollisionBridge : MonoBehaviour
{
    private DriftMoveAbility2D _driftAbility;

    /// <summary>由外部（如 Character2D 的 Init 后）注入 Ability 引用</summary>
    public void Bind(DriftMoveAbility2D ability)
    {
        _driftAbility = ability;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _driftAbility?.OnCollision(collision);
    }
}
