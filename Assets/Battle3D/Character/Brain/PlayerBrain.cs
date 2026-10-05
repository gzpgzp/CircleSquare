using Battle3D.Character.Ability;
using Battle3D.Input;
using UnityEngine;

namespace Battle3D.Character.Brain
{
    /// <summary>
    /// 玩家脑系统 - 处理玩家输入并驱动能力系统
    /// 将输入信号转化为能力激活和移动指令
    /// </summary>
    public class PlayerBrain : CharacterBrain
    {
        private PlayerInputController inputController;
        private Transform cameraTransform;

        public void InitInput(PlayerInputController input)
        {
            inputController = input;
            BindInputActions();
        }

        public void SetCameraTransform(Transform cam)
        {
            cameraTransform = cam;
        }

        private void BindInputActions()
        {
            if (inputController == null) return;

            // 绑定技能输入到对应槽位
            inputController.OnBasicAttack += () => ActivateAbility(AbilitySlot.BasicAttack);
            inputController.OnDodge += () => ActivateAbility(AbilitySlot.Dodge);
            inputController.OnSkill1 += () => ActivateAbility(AbilitySlot.Skill1);
            inputController.OnSkill2 += () => ActivateAbility(AbilitySlot.Skill2);
            inputController.OnUltimate += () => ActivateAbility(AbilitySlot.Ultimate);
            inputController.OnJump += () =>
            {
                if (character.Motor != null)
                    character.Motor.Jump(0f); // 使用默认跳跃力
            };
        }

        public override void Tick(float dt)
        {
            if (inputController == null) return;

            // 处理移动输入
            HandleMovement(dt);

            // 处理视角输入（摄像机旋转由 BattleCameraController 处理）
        }

        public override void FixedTick(float dt)
        {
            // FixedUpdate 中不需要额外处理
        }

        private void HandleMovement(float dt)
        {
            if (character.Motor == null) return;

            Vector2 moveInput = inputController.MoveInput;
            if (moveInput.sqrMagnitude < 0.01f)
            {
                character.Motor.StopHorizontalMovement();
                return;
            }

            // 计算相对于摄像机的移动方向
            Vector3 moveDirection = CalculateMoveDirection(moveInput);
            character.Motor.Move(moveDirection, character.MoveSpeed, dt);
        }

        private Vector3 CalculateMoveDirection(Vector2 input)
        {
            if (cameraTransform == null)
            {
                // 没有摄像机时，使用世界坐标方向
                return new Vector3(input.x, 0f, input.y).normalized;
            }

            // 相对于摄像机方向的移动
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 direction = cameraForward * input.y + cameraRight * input.x;
            return direction.normalized;
        }
    }
}
