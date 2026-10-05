using System.Collections.Generic;
using Battle3D.AI;
using Battle3D.AI.States;
using Battle3D.Character;
using Battle3D.Character.Ability;
using Battle3D.Character.Brain;
using Battle3D.Combat;
using Battle3D.Core;
using Battle3D.Input;
using Tools.ResourcesTool;
using UnityEngine;

namespace Battle3D.GameFlow
{
    /// <summary>
    /// 角色管理器 - 管理所有角色的生命周期
    /// 负责创建玩家和敌人，驱动更新循环
    /// 参考现有项目 CharacterManager 并增强
    /// </summary>
    public class CharacterManager
    {
        private Dictionary<int, BaseCharacter> characters = new Dictionary<int, BaseCharacter>();
        private BaseCharacter playerCharacter;
        private int nextCharacterId = 0;

        // Prefab 基础路径（Resources 下）
        private const string PrefabBasePath = "Prefabs/Characters/";

        public BaseCharacter PlayerCharacter => playerCharacter;
        public IReadOnlyDictionary<int, BaseCharacter> AllCharacters => characters;

        public void Init()
        {
            AbilityInitializer.Initialize();
            Debug.Log("[CharacterManager] Initialized.");
        }

        #region 创建角色

        /// <summary>
        /// 创建玩家角色
        /// </summary>
        public BaseCharacter CreatePlayer(CharacterContext ctx)
        {
            ctx.isPlayer = true;
            var player = CreateCharacter(ctx);
            if (player == null) return null;

            playerCharacter = player;

            // 创建玩家脑
            var brain = new PlayerBrain();
            brain.Init(player, player.AbilitySystemComponent);

            // 绑定输入
            var inputController = Object.FindFirstObjectByType<PlayerInputController>();
            if (inputController != null)
            {
                brain.InitInput(inputController);
            }

            // 绑定摄像机（用于移动方向计算）
            var cam = UnityEngine.Camera.main?.transform;
            if (cam != null)
            {
                brain.SetCameraTransform(cam);
            }

            // 设置战斗摄像机跟随目标
            var cameraController = Object.FindFirstObjectByType<Battle3D.Camera.BattleCameraController>();
            if (cameraController != null)
            {
                cameraController.SetFollowTarget(player.transform);
            }

            player.SetBrain(brain);
            Debug.Log($"[CharacterManager] Player '{ctx.characterName}' created.");
            return player;
        }

        /// <summary>
        /// 创建AI敌人
        /// </summary>
        public BaseCharacter CreateEnemy(CharacterContext ctx)
        {
            ctx.isPlayer = false;
            var enemy = CreateCharacter(ctx);
            if (enemy == null) return null;

            // 创建AI脑
            var brain = new AIBrain();
            brain.Init(enemy, enemy.AbilitySystemComponent);
            brain.detectRange = ctx.aiDetectRange;
            brain.attackRange = ctx.aiAttackRange;

            // 初始化状态机
            var stateMachine = new StateMachine();
            AddStandardAIStates(stateMachine, enemy, brain);

            brain.InitStateMachine(stateMachine);
            stateMachine.SetInitialState(AIStateEnum.Idle);

            enemy.SetBrain(brain);
            Debug.Log($"[CharacterManager] Enemy '{ctx.characterName}' created.");
            return enemy;
        }

        /// <summary>
        /// 通用角色创建 - 从 Resources 加载 Prefab 并实例化
        /// </summary>
        private BaseCharacter CreateCharacter(CharacterContext ctx)
        {
            // 从 Resources 加载并实例化 Prefab
            string path = PrefabBasePath + ctx.prefabName;
            var prefab = MyResourcesManager.Instance.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[CharacterManager] Prefab not found: {path}");
                return null;
            }

            var go = MyResourcesManager.Instance.LoadAndInstantiate(prefab);
            go.name = $"Character_{ctx.characterName}";

            // 获取或添加必要组件
            var char3D = go.GetComponent<Character3D>();
            if (char3D == null)
            {
                char3D = go.AddComponent<Character3D>();
            }

            // 绑定 Animator（如果 Prefab 上有）
            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                char3D.SetAnimator(animator);
            }

            // 初始化
            char3D.Init(ctx);

            // 注册
            int id = nextCharacterId++;
            characters[id] = char3D;

            // 监听死亡事件
            GameEventSystem.Subscribe<DeathData>(GameEvents.OnCharacterDeath, (data) =>
            {
                if (data.character == char3D)
                {
                    OnCharacterDeath(id, char3D);
                }
            });

            Debug.Log($"[CharacterManager] Character '{ctx.characterName}' instantiated from prefab '{ctx.prefabName}'.");
            return char3D;
        }

        #endregion

        #region AI状态注册

        private void AddStandardAIStates(StateMachine machine, BaseCharacter character, AIBrain brain)
        {
            var states = new BaseState[]
            {
                new IdleState(character, machine),
                new PatrolState(character, machine),
                new ChaseState(character, machine),
                new AttackState(character, machine),
                new RetreatState(character, machine),
                new StunState(character, machine),
                new DeadState(character, machine),
            };

            foreach (var state in states)
            {
                state.SetBrain(brain);
                machine.AddState(state);
            }
        }

        #endregion

        #region 更新循环

        public void Tick(float dt)
        {
            foreach (var kvp in characters)
            {
                if (kvp.Value != null && kvp.Value.IsAlive)
                {
                    kvp.Value.UpdateTick(dt);
                }
            }
        }

        public void FixedTick(float dt)
        {
            foreach (var kvp in characters)
            {
                if (kvp.Value != null && kvp.Value.IsAlive)
                {
                    kvp.Value.Tick(dt);
                }
            }
        }

        #endregion

        #region 角色管理

        /// <summary>
        /// 移除角色
        /// </summary>
        public void RemoveCharacter(int id)
        {
            if (characters.TryGetValue(id, out var character))
            {
                if (character != null)
                {
                    Object.Destroy(character.gameObject);
                }
                characters.Remove(id);
            }
        }

        /// <summary>
        /// 获取所有活着的敌人
        /// </summary>
        public List<BaseCharacter> GetAliveEnemies()
        {
            var enemies = new List<BaseCharacter>();
            foreach (var kvp in characters)
            {
                if (kvp.Value != null && !kvp.Value.IsPlayer && kvp.Value.IsAlive)
                {
                    enemies.Add(kvp.Value);
                }
            }
            return enemies;
        }

        private void OnCharacterDeath(int id, BaseCharacter character)
        {
            Debug.Log($"[CharacterManager] Character '{character.CharacterId}' died.");
            // 延迟移除或播放死亡动画后移除
        }

        #endregion
    }
}
