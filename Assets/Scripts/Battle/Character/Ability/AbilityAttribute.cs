using System;

namespace Battle.Character.Ability
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AbilityAttribute : Attribute
    {
        public AbilityEnum  Id;
        public AbilityAttribute(AbilityEnum  id) => Id = id;
    }
}