using Inputs;

namespace Battle.Character.CharacterGlue
{
    public abstract class CharacterBrain
    {
        protected BaseCharacter character;
        protected InputManager input;

        public virtual void Init(BaseCharacter character, InputManager input)
        {
            this.character = character;
            this.input = input;
        }

        public abstract void OnUpdate(float dt);
    }
}