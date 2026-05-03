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
        protected BaseCharacter owner;
        protected AbilitySystem system;
        protected InputActionSO input;
        protected AbilityContext ctx;

        public int priority { get; protected set; }
        protected bool isEnabled;

        public AbilityTag OwnTag { get; protected set; }
        public AbilityTag BlockTags { get; protected set; }
        
        protected float abilityTimer = 0f;
        protected float coolDown = 0f;
        
        public bool IsEnabled
        {
            get => isEnabled;
        }
        
        public virtual void InitContext(AbilityContext ctx)
        {
            this.ctx = ctx;
        }

        public virtual void BindCharacter(BaseCharacter owner)
        {
            this.owner = owner;
        }

        public virtual void BindSystem(AbilitySystem system)
        {
            this.system = system;
        }

        public virtual void BindInput(InputAction input) { }
        public virtual void UnbindInput(InputAction input) { }
        

        protected virtual void OnTickUpdate(float deltaTime)
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

            this.isEnabled = isEnabled;

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
                    OnTickUpdate(deltaTime);
                }
            }
            else
            {
            }
        }
    }
}