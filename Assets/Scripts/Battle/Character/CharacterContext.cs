using System.Collections.Generic;
using Battle.Character.Ability;
using UnityEngine;

namespace Battle.Character
{
    /// <summary>
    /// 创建角色时传入的数据上下文。
    /// LevelManager 写入，CharacterManager / Ability 只读。
    /// </summary>
    public class CharacterContext
    {
        /// <summary>角色模型 ID（对应预制体 Character_{id}）</summary>
        public int characterModelId;

        /// <summary>true = 由 AI 控制，false = 由玩家控制</summary>
        public bool isAI;

        /// <summary>处登場位置</summary>
        public Vector3 spawnPosition;

        /// <summary>角色显示名（调试、UI等用）</summary>
        public string characterName = "Character";

        /// <summary>
        /// 需要挂载的能力列表（对应 AbilityEnum 整数值）。
        /// 玩家角色会为其中有 InputActionSO 的能力绑定按键。
        /// </summary>
        public List<int> abilities;
    }
}