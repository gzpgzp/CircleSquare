using Battle.Inputs;

namespace Battle.Character.Ability.A2D
{
    public abstract class Ability2D : BaseAbility
    {
        protected Character2D character2D
        {
            get => (Character2D)owner;
        }
    }
}