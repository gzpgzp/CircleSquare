using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle3D.Character
{
    /// <summary>
    /// 角色上下文 - 创建角色时传入的初始化数据
    /// </summary>
    [Serializable]
    public class CharacterContext
    {
        [Header("基础信息")]
        public string characterName;
        public string characterId;
        public string prefabName = "Character_0";

        [Header("属性")]
        public float maxHealth = 100f;
        public float moveSpeed = 6f;
        public float maxEnergy = 100f;

        [Header("配置")]
        public bool isPlayer = false;
        public List<string> abilityIds = new List<string>();

        [Header("AI配置（仅AI角色）")]
        public float aiDetectRange = 15f;
        public float aiAttackRange = 3f;
        public List<string> aiStateIds = new List<string>();
    }
}
