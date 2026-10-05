using Battle3D.Character.Ability;

namespace Battle3D.Character.Brain
{
    /// <summary>
    /// 脑系统基类 - 控制角色的行为决策
    /// PlayerBrain 处理玩家输入，AIBrain 处理AI逻辑
    /// 参考现有项目 CharacterBrain 设计
    /// </summary>
    public abstract class CharacterBrain
    {
        protected BaseCharacter character;
        protected AbilitySystem abilitySystem;

        public virtual void Init(BaseCharacter character, AbilitySystem abilitySystem)
        {
            this.character = character;
            this.abilitySystem = abilitySystem;
        }

        /// <summary>每帧更新（Update中调用）</summary>
        public abstract void Tick(float dt);

        /// <summary>物理帧更新（FixedUpdate中调用）</summary>
        public virtual void FixedTick(float dt) { }

        /// <summary>激活指定槽位的能力</summary>
        protected bool ActivateAbility(AbilitySlot slot)
        {
            return abilitySystem.ActivateSlot(slot);
        }
    }
}
