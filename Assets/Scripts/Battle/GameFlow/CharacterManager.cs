using System.Collections.Generic;
using Battle.Character;
using Battle.Character.CharacterGlue;
using Battle.Inputs;
using Tools.Extend;
using Tools.GameObjectPools;
using UnityEngine;

namespace Battle.GameFlow
{
    public class CharacterManager
    {
        private const string BaseCharacterPath = "Prefabs/Characters/";

        // key = characterModelId, value = 预制体名称
        private readonly Dictionary<int, string> characterPrefabs = new Dictionary<int, string>();

        // key = 运行时自增 ID, value = 角色实例
        private readonly Dictionary<int, BaseCharacter> characters = new Dictionary<int, BaseCharacter>();

        private int _nextCharacterId = 0;

        // ──────────────────────── 初始化 ────────────────────────

        public void Init()
        {
            AbilityInitializer.Initializer();
        }

        /// <summary>
        /// 按需注册一个角色预制体到对象池。
        /// 同一个 modelId 多次注册会跳过。
        /// </summary>
        private void EnsurePrefabRegistered(int modelId)
        {
            if (characterPrefabs.ContainsKey(modelId)) return;

            string prefabName = $"Character_{modelId}";
            string path = BaseCharacterPath + prefabName;
            GameObjectPool.Instance.Load(path);
            characterPrefabs.Add(modelId, prefabName);
        }

        // ──────────────────────── 创建 ────────────────────────

        /// <summary>
        /// 从对象池取出预制体并绑定 Character2D，完成基础 Init。
        /// 不带 Brain，由 CreatePlayer / CreateEnemy 负责装配 Brain + Ability。
        /// </summary>
        private BaseCharacter CreateCharacter(CharacterContext ctx)
        {
            EnsurePrefabRegistered(ctx.characterModelId);

            var model = GameObjectPool.Instance.CreateGameObject(
                characterPrefabs[ctx.characterModelId]
            );
            var cha = model.TryGetOrAddComponent<Character2D>();
            cha.Init(ctx);
            cha.SetPos(ctx.spawnPosition);
            cha.gameObject.name = ctx.characterName;

            _nextCharacterId++;
            characters.Add(_nextCharacterId, cha);
            return cha;
        }

        /// <summary>
        /// 创建玩家角色：装配 PlayerBrain + 绑定按键输入的 Ability。
        /// </summary>
        public BaseCharacter CreatePlayer(CharacterContext ctx)
        {
            var cha = CreateCharacter(ctx);

            var brain  = new PlayerBrain();
            var input  = new PlayerInput();
            brain.InitInput(input);

            foreach (var id in ctx.abilities)
            {
                var ability = AbilityInitializer.CreateAbility(id);
                cha.AddAbility(ability);

                // 有 InputActionSO 配置的能力才绑定按键
                var config = AbilityInitializer.GetAbilityInputConfig(id);
                if (config != null)
                {
                    brain.AddAbility(ability, config);
                }
            }

            // Brain.Init 必须在所有 Ability 注入完之后调用
            brain.Init(cha);
            cha.AddBrain(brain);
            return cha;
        }

        /// <summary>
        /// 创建 AI 敌人：装配 AIBrain + 自动注入 Blackboard 的 AI Ability。
        /// </summary>
        public BaseCharacter CreateEnemy(CharacterContext ctx)
        {
            var cha   = CreateCharacter(ctx);
            var brain = new AIBrain();

            foreach (var id in ctx.abilities)
            {
                var ability = AbilityInitializer.CreateAbility(id);
                cha.AddAbility(ability);
                // AIBrain.AddAbility 会自动为 IAIAbility 注入 Blackboard
                brain.AddAbility(ability);
            }

            // Brain.Init 必须在所有 Ability 注入完之后调用
            brain.Init(cha);
            cha.AddBrain(brain);
            return cha;
        }

        // ──────────────────────── Tick ────────────────────────

        public void Tick(float dt)
        {
            foreach (var kv in characters)
                kv.Value.UpdateTick(dt);
        }

        public void FixedUpdate(float dt)
        {
            foreach (var kv in characters)
                kv.Value.Tick(dt);
        }

        // ──────────────────────── 查询 ────────────────────────

        public BaseCharacter GetCharacter(int runtimeId)
        {
            characters.TryGetValue(runtimeId, out var cha);
            return cha;
        }
    }
}