using System;
using UnityEngine;

namespace Battle3D.Input
{
    /// <summary>
    /// 玩家输入控制器 - 统一处理键鼠和手柄输入
    /// 当前使用旧版 Input 系统实现，后续可替换为 New Input System
    /// 通过事件委托将输入信号传递给 PlayerBrain
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        [Header("灵敏度")]
        [SerializeField] private float mouseSensitivity = 2f;

        // 移动输入
        public Vector2 MoveInput { get; private set; }

        // 视角输入
        public Vector2 LookInput { get; private set; }

        // 动作事件（委托方式，供 PlayerBrain 绑定）
        public event Action OnBasicAttack;
        public event Action OnDodge;
        public event Action OnSkill1;
        public event Action OnSkill2;
        public event Action OnUltimate;
        public event Action OnJump;
        public event Action OnLockOn;

        // 蓄力相关
        public event Action OnSkill1Hold;
        public event Action OnSkill1Release;
        public event Action OnSkill2Hold;
        public event Action OnSkill2Release;

        // 状态
        private bool isSkill1Held;
        private bool isSkill2Held;
        public bool IsSkill1Held => isSkill1Held;
        public bool IsSkill2Held => isSkill2Held;

        private void Update()
        {
            ReadMovement();
            ReadLook();
            ReadActions();
        }

        #region 移动输入

        private void ReadMovement()
        {
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            float v = UnityEngine.Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(h, v).normalized;
        }

        #endregion

        #region 视角输入

        private void ReadLook()
        {
            float mouseX = UnityEngine.Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y") * mouseSensitivity;
            LookInput = new Vector2(mouseX, mouseY);
        }

        #endregion

        #region 动作输入

        private void ReadActions()
        {
            // 普攻 - 鼠标左键
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                OnBasicAttack?.Invoke();
            }

            // 闪避 - 空格键
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                OnDodge?.Invoke();
            }

            // 跳跃 - 左Shift
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift))
            {
                OnJump?.Invoke();
            }

            // 技能1 - Q键（支持蓄力）
            HandleSkillInput(KeyCode.Q,
                ref isSkill1Held,
                OnSkill1,
                OnSkill1Hold,
                OnSkill1Release);

            // 技能2 - E键（支持蓄力）
            HandleSkillInput(KeyCode.E,
                ref isSkill2Held,
                OnSkill2,
                OnSkill2Hold,
                OnSkill2Release);

            // 终极技能 - R键
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                OnUltimate?.Invoke();
            }

            // 锁定目标 - Tab键
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                OnLockOn?.Invoke();
            }
        }

        private void HandleSkillInput(KeyCode key, ref bool isHeld,
            Action onTap, Action onHold, Action onRelease)
        {
            if (UnityEngine.Input.GetKeyDown(key))
            {
                isHeld = true;
                onTap?.Invoke();
                onHold?.Invoke();
            }

            if (UnityEngine.Input.GetKeyUp(key))
            {
                if (isHeld)
                {
                    isHeld = false;
                    onRelease?.Invoke();
                }
            }
        }

        #endregion

        #region 手柄支持（预留）

        /// <summary>
        /// 手柄输入支持 - 后续接入 New Input System 后启用
        /// 当前预留接口
        /// </summary>
        private void ReadGamepadInput()
        {
            // 左摇杆 -> MoveInput
            // 右摇杆 -> LookInput
            // RT -> BasicAttack
            // B -> Dodge
            // LB -> Skill1
            // RB -> Skill2
            // LT+RT -> Ultimate
            // Right Stick Click -> LockOn
        }

        #endregion

        #region 公共接口

        /// <summary>设置鼠标灵敏度</summary>
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }

        /// <summary>锁定光标</summary>
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>解锁光标</summary>
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        #endregion
    }
}
