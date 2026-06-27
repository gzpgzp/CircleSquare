using System.Collections.Generic;
using Battle.Character.Ability;

namespace Battle.Character
{
    public class CharacterContext
    {
        public int characterModelId;
        public bool isAI;
        public List<int> abilities;
    }
}