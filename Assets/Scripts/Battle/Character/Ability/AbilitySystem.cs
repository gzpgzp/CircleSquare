using System.Collections.Generic;
using Battle.Inputs;

namespace Battle.Character.Ability
{
    public class AbilitySystem
    {
        private List<BaseAbility> abilities = new List<BaseAbility>();
        private AbilityTag blockedTags = AbilityTag.None;

        public void AddAbility(BaseAbility ability)
        {
            ability.BindSystem(this);
            abilities.Add(ability);
            abilities.Sort((a,b)=>b.priority.CompareTo(a.priority));
        }
        
        public bool IsTagBlocked(AbilityTag tag)
        {
            return (blockedTags & tag) != 0;
        }

        public void AddBlock(AbilityTag tag)
        {
            blockedTags |= tag;
        }

        public void RemoveBlock(AbilityTag tag)
        {
            blockedTags &= ~tag;
        }

        public void Tick(float dt)
        {
            foreach (var ability in abilities)
            {
                ability.Tick(dt);
            }
        }
    }
}