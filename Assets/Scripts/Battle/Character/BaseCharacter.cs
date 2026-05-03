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

        // ── 生命值 ──────────────────────────────────────────────────────────
        [Header("Stats")]
        [SerializeField] protected int maxHp = 100;
        protected int currentHp;
        protected bool isDead;

        public bool IsDead() => isDead;
        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        
        public virtual void Init(CharacterContext ctx)
        {
            this.ctx = ctx;
            abilitySystem = new AbilitySystem();
            currentHp = maxHp;
            isDead = false;
        }

        /// <summary>接受伤害，hp归零时调用 OnDie</summary>
        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;
            currentHp -= damage;
            currentHp = Mathf.Max(currentHp, 0);
            if (currentHp <= 0)
            {
                isDead = true;
                OnDie();
            }
        }

        protected virtual void OnDie()
        {
            Destroy(gameObject, 0.1f);
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

        /// <summary>
        /// 获取能力系统
        /// </summary>
        public AbilitySystem GetAbilitySystem()
        {
            return abilitySystem;
        }

        /// <summary>
        /// 获取角色上下文
        /// </summary>
        public CharacterContext GetContext()
        {
            return ctx;
        }
    }
}