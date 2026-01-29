using Battle.Character.Ability;
using Battle.Inputs;
using GameFramework;
using Tools.ResourcesTool;

namespace Battle.Character.CharacterGlue
{
    public class PlayerBrain : CharacterBrain
    {
        public PlayerInput input;

        public void InitInput(PlayerInput input)
        {
            this.input = input;
        }

        public void AddAbility(BaseAbility ability, InputActionSO config)
        {
            var inputAction = input.AddInput(config);
            ability.BindInput(inputAction);
            ability.InitContext(new AbilityContext()
            {
                DirectionInput = from => { return GameWorld.Instance.ComputeMouse(from); }
            });
        }

        public override void Tick(float dt)
        {
            input.Update(dt);
        }
    }
}