using Battle.Character.Ability;
using Battle.Inputs;

namespace Battle.Character.CharacterGlue
{
    // 主要控制角色的行为，所以是input的控制方
    public abstract class CharacterBrain
    {
        protected BaseCharacter character;

        public virtual void Init(BaseCharacter character)
        {
            this.character = character;
        }

        public abstract void Tick(float dt);
    }
}