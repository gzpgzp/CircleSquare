using System.Collections.Generic;
using Tools.Singletons;

namespace SquareBattle.BattleCore
{
    public class SquareBattleCharacterRegistry : Singleton<SquareBattleCharacterRegistry>
    {
        public readonly List<CharacterBase> AllCharacters = new();

        public void Register(CharacterBase character)
        {
            AllCharacters.Add(character);
        }

        public void Unregister(CharacterBase character)
        {
            AllCharacters.Remove(character);
        }
    }
}