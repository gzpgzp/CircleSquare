using Unity.VisualScripting;

namespace Battle.Character.Ability
{
    public abstract class BaseAbility
    {
        protected BaseCharacter character;
        protected bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
        }

        public virtual void Init(BaseCharacter character)
        {
            // 初始化数据和脚本类
            this.character = character;
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

        public virtual void Update(float deltaTime)
        {
            if (isEnabled)
            {
                OnUpdate(deltaTime);
            }
        }
    }
}