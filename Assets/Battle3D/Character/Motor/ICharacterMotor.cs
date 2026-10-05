using UnityEngine;

namespace Battle3D.Character.Motor
{
    /// <summary>
    /// 角色运动系统接口 - 抽象2D/3D运动差异
    /// 所有运动相关的操作都通过此接口进行
    /// </summary>
    public interface ICharacterMotor
    {
        /// <summary>当前速度</summary>
        Vector3 Velocity { get; }

        /// <summary>是否在地面上</summary>
        bool IsGrounded { get; }

        /// <summary>是否可以移动（受击、硬直时不可移动）</summary>
        bool CanMove { get; set; }

        /// <summary>初始化运动系统</summary>
        void Init(Transform owner);

        /// <summary>移动（方向 + 速度）</summary>
        void Move(Vector3 direction, float speed, float dt);

        /// <summary>跳跃</summary>
        void Jump(float force);

        /// <summary>闪避/冲刺</summary>
        void Dodge(Vector3 direction, float distance, float duration);

        /// <summary>施加击退力</summary>
        void ApplyKnockback(Vector3 force);

        /// <summary>每帧更新（FixedUpdate中调用）</summary>
        void FixedTick(float dt);

        /// <summary>停止水平移动</summary>
        void StopHorizontalMovement();

        /// <summary>设置重力倍率</summary>
        void SetGravityScale(float scale);

        /// <summary>获取重力倍率</summary>
        float GetGravityScale();

        /// <summary>重置重力倍率</summary>
        void ResetGravityScale();
    }
}
