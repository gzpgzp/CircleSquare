using System.Collections.Generic;
using Battle.Character.Ability;
using Battle.Character.Ability.A2D;
using Battle.Inputs;
using UnityEngine;

namespace Battle.Character.CharacterGlue
{
    public class AIBrain : CharacterBrain
    {
        private AIInput input;

        /// <summary>AI 感知共享数据板，由本 Brain 写入，AI Ability 只读</summary>
        public AIBlackboard Blackboard { get; } = new AIBlackboard();

        // 每局需要感知敌人的能力列表
        private readonly List<IAIAbility> aiAbilities = new List<IAIAbility>();

        public void InitInput(AIInput input)
        {
            this.input = input;
        }

        /// <summary>
        /// 添加一个 Ability：若它实现了 IAIAbility 接口，则自动注入 Blackboard
        /// </summary>
        public void AddAbility(BaseAbility ability)
        {
            if (ability is IAIAbility aiAbility)
            {
                aiAbility.BindBlackboard(Blackboard);
                aiAbilities.Add(aiAbility);
            }
        }

        public override void Tick(float dt)
        {
            // 1. 寻找最近敌人并更新黑板
            var target = FindNearestEnemy();
            Blackboard.Update(character, target);
        }

        /// <summary>
        /// 找到场上最近的敌方角色。
        /// 新系统没有 Camp 概念，这里简单实现为：排除自身、找距离最小的存活角色。
        /// 如项目后续引入 Camp 概念，可在此处添加阵营过滤。
        /// </summary>
        private BaseCharacter FindNearestEnemy()
        {
            var all = Object.FindObjectsByType<BaseCharacter>(FindObjectsSortMode.None);
            float minDist = float.MaxValue;
            BaseCharacter nearest = null;

            foreach (var c in all)
            {
                if (c == character) continue;
                if (c.IsDead()) continue;

                float dist = Vector2.Distance(
                    character.transform.position,
                    c.transform.position
                );
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = c;
                }
            }

            return nearest;
        }
    }
}