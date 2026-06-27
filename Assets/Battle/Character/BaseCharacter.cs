using System;
using Battle.Character.Ability;
using Battle.Character.CharacterGlue;
using Battle.Character.CharacterMotor;
using UnityEngine;

namespace Battle.Character
{
    // 这个脚本后面还要根据不同的模式切不同的脚本
    public class BaseCharacter : MonoBehaviour
    {
        private CharacterBrain brain;
        private AbilitySystem abilitySystem;
        private CharacterContext ctx;
        
        public virtual void Init(CharacterContext ctx)
        {
            this.ctx = ctx;
            abilitySystem = new AbilitySystem();
        }

        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }

        public void AddBrain(CharacterBrain brain)
        {
            this.brain = brain;
        }

        public void AddAbility(BaseAbility ability)
        {
            ability.BindCharacter(this);
            abilitySystem.AddAbility(ability);
        }

        public virtual void UpdateTick(float dt)
        {
            brain?.Tick(dt);
        }

        public virtual void Tick(float dt)
        {
            abilitySystem.Tick(dt);
        }
    }
}