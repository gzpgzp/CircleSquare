using System.Collections.Generic;
using Battle.Character;
using GameFramework;
using Tools.ResourcesTool;

namespace Battle.GameFlow
{
    public class CharacterManager
    {
        private const string BaseCharacterPath = "Prefabs/Characters";
        
        public void Init()
        {
            CharacterInitializer.Initializer();
        }

        public BaseCharacter CreateCharacterObj()
        {
            var go = MyResourcesManager.Instance.LoadAndInstantiate(BaseCharacterPath);
            return null;
        }

        public void CreateCharacter()
        {
            
        }

        // 创建主角，链接input
        public void CreateMainCharacter()
        {
            var ctx = new CharacterContext()
            {
                abilities = new List<int>()
                {
                    10,
                }
            };
            foreach (var id in ctx.abilities)
            {
                var abilityEnum = ConfigManager.Instance.GetAbilityById(id);
                var ability = CharacterInitializer.CreateAbility(abilityEnum);
                
                var character = CreateCharacterObj();
                character.AddAbility(ability);
            }
        }

        public void CreateEnemy()
        {
            
        }
    }
}