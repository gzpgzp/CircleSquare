using Battle.Character.CharacterMotor;
using Battle.Inputs;

namespace Battle.Character.Ability.A2D
{
    public abstract class Ability2D : BaseAbility
    {
        protected Character2D character2D
        {
            get => (Character2D)owner;
        }

        protected CharacterMotor2D motor
        {
            get => character2D.motor;
        }
    }
}