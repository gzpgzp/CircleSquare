using UnityEngine;

namespace SquareBattle.BattleCore
{
    /// <summary>
    /// 角色生成数据——在 Spawn 时覆盖预制体上的默认配置。
    /// 所有字段均可选：为 null / 负数时表示"不覆盖，保留预制体默认值"。
    /// </summary>
    public class CharacterSpawnData
    {
        // ── 基础 ──────────────────────────────────────────────

        /// <summary>阵营，null = 保留预制体配置</summary>
        public Camp? camp;

        // ── 属性 ──────────────────────────────────────────────

        /// <summary>最大生命值，<= 0 = 保留预制体配置</summary>
        public int maxHp;

        /// <summary>移动速度，<= 0 = 保留预制体配置</summary>
        public float moveSpeed;

        // ── 初始化 ────────────────────────────────────────────

        /// <summary>是否强制随机初始方向，null = 保留预制体配置</summary>
        public bool? randomInitDir;

        // ── 构造：链式配置风格 ─────────────────────────────────

        public CharacterSpawnData WithCamp(Camp c)          { camp         = c;  return this; }
        public CharacterSpawnData WithMaxHp(int hp)         { maxHp        = hp; return this; }
        public CharacterSpawnData WithMoveSpeed(float spd)  { moveSpeed    = spd; return this; }
        public CharacterSpawnData WithRandomDir(bool rnd)   { randomInitDir = rnd; return this; }
    }
}
