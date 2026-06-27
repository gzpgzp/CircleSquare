using System.Collections.Generic;
using GameFramework;
using Tools.EventTool;
using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 挂机自动战斗系统 - 纯数值战斗，无碰撞检测
    /// 玩家队伍 vs 敌人队伍，自动攻击，击杀后刷新敌人
    /// </summary>
    public class AutoCombatSystem : MMEventListener<CharacterDeadEvent>
    {
        /// <summary>玩家队伍</summary>
        private List<CharacterEntity> playerTeam = new List<CharacterEntity>();

        /// <summary>当前敌人队伍</summary>
        private List<CharacterEntity> enemyTeam = new List<CharacterEntity>();

        /// <summary>敌人配置池</summary>
        private List<cfg.Adventure.CharacterInfo> enemyConfigs;

        /// <summary>当前波次</summary>
        public int currentWave { get; private set; } = 1;

        /// <summary>总击杀数</summary>
        public int totalKills { get; private set; }

        /// <summary>累计获得金币</summary>
        public int totalGold { get; private set; }

        /// <summary>战斗是否进行中</summary>
        public bool isRunning { get; private set; }

        /// <summary>每波敌人数量</summary>
        private int enemiesPerWave = 1;

        /// <summary>敌人等级基于波次的成长</summary>
        private int enemyLevelBase = 1;

        public void Init(List<CharacterEntity> playerTeam)
        {
            this.playerTeam = playerTeam;

            // 从Luban表加载敌人配置（根据角色表筛选敌人）
            var allChars = ConfigManager.Instance.GetAllCharacterInfos();
            enemyConfigs = allChars.FindAll(c => c.CharacterId.StartsWith("enemy_"));
            if (enemyConfigs == null || enemyConfigs.Count == 0)
            {
                Debug.LogWarning("[AutoCombat] No enemy configs found in table.");
            }

            this.MMEventStartListening<CharacterDeadEvent>();
        }

        public void Destroy()
        {
            this.MMEventStopListening<CharacterDeadEvent>();
        }

        /// <summary>
        /// 开始挂机战斗
        /// </summary>
        public void StartCombat()
        {
            isRunning = true;
            SpawnEnemyWave();
            Debug.Log($"[AutoCombat] Combat started! Wave {currentWave}");
        }

        /// <summary>
        /// 暂停战斗
        /// </summary>
        public void PauseCombat()
        {
            isRunning = false;
        }

        /// <summary>
        /// 每帧更新（由外部调用）
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!isRunning) return;

            // 检查玩家是否全灭
            if (IsTeamDead(playerTeam))
            {
                OnPlayerTeamWiped();
                return;
            }

            // 玩家攻击敌人
            TickTeamAttack(playerTeam, enemyTeam, deltaTime);

            // 敌人攻击玩家
            TickTeamAttack(enemyTeam, playerTeam, deltaTime);
        }

        private void TickTeamAttack(List<CharacterEntity> attackers, List<CharacterEntity> defenders, float deltaTime)
        {
            foreach (var attacker in attackers)
            {
                if (attacker.isDead) continue;
                if (attacker.combatState != CombatState.Fighting)
                {
                    attacker.combatState = CombatState.Fighting;
                }

                if (attacker.UpdateAttackTimer(deltaTime))
                {
                    // 选择目标（第一个活着的敌人）
                    var target = GetFirstAliveTarget(defenders);
                    if (target != null)
                    {
                        PerformAttack(attacker, target);
                    }
                }
            }
        }

        private void PerformAttack(CharacterEntity attacker, CharacterEntity target)
        {
            float damage = attacker.CalculateAttackDamage(out bool isCrit);
            target.TakeDamage(damage, isCrit);
        }

        private CharacterEntity GetFirstAliveTarget(List<CharacterEntity> team)
        {
            foreach (var entity in team)
            {
                if (!entity.isDead) return entity;
            }
            return null;
        }

        private bool IsTeamDead(List<CharacterEntity> team)
        {
            foreach (var entity in team)
            {
                if (!entity.isDead) return false;
            }
            return true;
        }

        #region 波次管理

        private void SpawnEnemyWave()
        {
            enemyTeam.Clear();

            if (enemyConfigs == null || enemyConfigs.Count == 0) return;

            int enemyLevel = enemyLevelBase + (currentWave - 1);
            int count = enemiesPerWave;

            for (int i = 0; i < count; i++)
            {
                var config = enemyConfigs[Random.Range(0, enemyConfigs.Count)];
                var enemy = new CharacterEntity(config, CharacterCamp.Enemy, enemyLevel);
                enemy.combatState = CombatState.Fighting;
                enemyTeam.Add(enemy);
            }

            Debug.Log($"[AutoCombat] Wave {currentWave}: Spawned {count} enemies (Lv.{enemyLevel})");
        }

        private void OnEnemyKilled(CharacterEntity enemy)
        {
            totalKills++;

            // 奖励经验和金币
            int expReward = 10 + currentWave * 5;
            int goldReward = 5 + currentWave * 2;

            // 给所有存活玩家加经验
            foreach (var player in playerTeam)
            {
                if (!player.isDead)
                {
                    player.levelSystem.AddExp(expReward);
                }
            }

            totalGold += goldReward;
            GainGoldEvent.Trigger(goldReward);

            // 报告任务进度（击杀类任务）
            QuestConditionReportEvent.Trigger(QuestConditionType.KillEnemy, enemy.characterId, 1);

            // 检查是否该波全灭
            if (IsTeamDead(enemyTeam))
            {
                OnWaveCleared();
            }
        }

        private void OnWaveCleared()
        {
            currentWave++;

            // 玩家回一些血（挂机恢复）
            foreach (var player in playerTeam)
            {
                if (!player.isDead)
                {
                    float healAmount = player.stats.GetValue(StatType.MaxHp) * 0.3f;
                    player.Heal(healAmount);
                }
            }

            // 刷下一波
            SpawnEnemyWave();
        }

        private void OnPlayerTeamWiped()
        {
            isRunning = false;
            Debug.Log("[AutoCombat] Player team wiped! Auto-reviving...");

            // 自动复活（挂机游戏友好）
            foreach (var player in playerTeam)
            {
                player.Revive();
                player.ResetAttackTimer();
            }

            // 重置波次或保持当前波次重试
            SpawnEnemyWave();
            isRunning = true;
        }

        #endregion

        #region 事件

        public void OnMMEvent(CharacterDeadEvent eventType)
        {
            if (eventType.camp == CharacterCamp.Enemy)
            {
                // 找到对应的敌人实体
                foreach (var enemy in enemyTeam)
                {
                    if (enemy.characterId == eventType.characterId && enemy.isDead)
                    {
                        OnEnemyKilled(enemy);
                        break;
                    }
                }
            }
        }

        #endregion

        #region 公开查询

        public List<CharacterEntity> GetPlayerTeam() => playerTeam;
        public List<CharacterEntity> GetEnemyTeam() => enemyTeam;

        #endregion
    }
}
