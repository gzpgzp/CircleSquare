using Battle.Character.Ability;
using Battle.Inputs;

namespace Battle.Character.CharacterGlue
{
    public class AIBrain : CharacterBrain
    {
        private AIInput input;

        public void InitInput(AIInput input)
        {
            this.input = input;
        }

        public virtual void AddAbility(BaseAbility ability)
        {
            
        }
        
        public override void Tick(float dt)
        {
            
        }
    }
}