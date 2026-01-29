using System.Collections.Generic;
using System.Text;
using Battle.Character;
using Battle.Character.CharacterGlue;
using Battle.Inputs;
using Tools.Extend;
using Tools.GameObjectPools;

namespace Battle.GameFlow
{
    public class CharacterManager
    {
        private const string BaseCharacterPath = "Prefabs/Characters/";
        private Dictionary<int, string> characterPrefabs = new Dictionary<int, string>();
        private Dictionary<int, BaseCharacter> characters = new Dictionary<int, BaseCharacter>();

        private int characterID = 0;
        
        public void Init()
        {
            AbilityInitializer.Initializer();
            
            var sb = new StringBuilder();

            var characterIDs = new List<int>() { 1 };
            for (int i = 0; i < characterIDs.Count; i++)
            {
                var id = characterIDs[i];
                sb.Append(BaseCharacterPath);
                sb.Append($"Character_{id}");
                
                GameObjectPool.Instance. Load(sb.ToString());
                characterPrefabs.Add(id, $"Character_{id}");
                
                sb.Clear();
            }

            characterID = 0;
        }

        public BaseCharacter CreateCharacter(CharacterContext ctx)
        {
            var cid = ctx.characterModelId;
            var model = GameObjectPool.Instance.CreateGameObject(characterPrefabs[cid]);
            var cha = model.TryGetOrAddComponent<Character2D>();
            cha.Init(ctx);
            characterID++;
            characters.Add(characterID, cha);
            return cha;
        }

        public BaseCharacter CreatePlayer(CharacterContext ctx)
        {
            var cha = CreateCharacter(ctx);
            var brain = new PlayerBrain();
            var input = new PlayerInput();
            brain.Init(cha); 
            brain.InitInput(input);
            
            foreach (var id in ctx.abilities)
            {
                var ability = AbilityInitializer.CreateAbility(id);
                cha.AddAbility(ability);
                
                var config = AbilityInitializer.GetAbilityInputConfig(id);
                if (config != null)
                {
                    brain.AddAbility(ability, config);
                }
            }
            cha.AddBrain(brain);
            return cha;
        }

        public BaseCharacter CreateEnemy(CharacterContext ctx)
        {
            var cha = CreateCharacter(ctx);
            var brain = new AIBrain();
            var input = new AIInput();
            brain.Init(cha);
            brain.InitInput(input);
            
            foreach (var id in ctx.abilities)
            {
                var ability = AbilityInitializer.CreateAbility(id);
                cha.AddAbility(ability);
                brain.AddAbility(ability);
            }
            cha.AddBrain(brain);
            return cha;
        }

        public void Tick(float dt)
        {
            foreach (var character in characters)
            {
                character.Value.UpdateTick(dt);
            }
        }

        public void FixedUpdate(float dt)
        {
            foreach (var character in characters)
            {
                character.Value.Tick(dt);
            }   
        }
    }
}