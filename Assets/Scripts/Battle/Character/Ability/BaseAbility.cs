using System;
using Battle.Inputs;
using Unity.VisualScripting;

namespace Battle.Character.Ability
{
    // 用来识别和阻碍
    [Flags]
    public enum AbilityTag
    {
        None = 0,
        Action = 1 << 0,
        Movement = 1 << 1,
        Hurt = 1 << 2,
        Control = 1 << 3,
    }

    public abstract class BaseAbility
    {
        protected BaseCharacter character;
        protected CharacterContext context;
        protected AbilitySystem system;

        protected InputSlot inputSlot;

        public int priority { get; protected set; }
        protected bool isEnabled;

        public AbilityTag OwnTag { get; protected set; }

        public AbilityTag BlockTags { get; protected set; }

        public bool IsEnabled
        {
            get => isEnabled;
        }

        public BaseAbility(BaseCharacter character)
        {
            // 初始化数据和脚本类
            this.character = character;
        }

        public virtual void Init(CharacterContext context, AbilitySystem system)
        {
            this.context = context;
            this.system = system;
        }

        protected virtual void OnUpdate(float deltaTime)
        {
            // 更新
        }

        protected virtual void OnActive()
        {
            // 激活
        }

        protected virtual void OnUnactive()
        {
            // 不激活
        }

        public virtual void SetEnabled(bool isEnabled)
        {
            if (this.isEnabled == isEnabled)
            {
                return;
            }

            if (isEnabled)
            {
                OnActive();
            }
            else
            {
                OnUnactive();
            }
        }

        public virtual void Tick(float deltaTime)
        {
            if (isEnabled)
            {
                if (!system.IsTagBlocked(OwnTag))
                {
                    OnUpdate(deltaTime);
                }
            }
            else
            {
            }
        }
    }
}