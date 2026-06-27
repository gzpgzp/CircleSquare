using Tools.EventTool;

namespace Adventure.Character
{
    /// <summary>
    /// 角色升级事件
    /// </summary>
    public struct CharacterLevelUpEvent
    {
        public string characterId;
        public int newLevel;

        static CharacterLevelUpEvent e;

        public static void Trigger(string characterId, int newLevel)
        {
            e.characterId = characterId;
            e.newLevel = newLevel;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 角色受伤事件
    /// </summary>
    public struct CharacterDamagedEvent
    {
        public string characterId;
        public float damage;
        public float currentHp;
        public bool isCrit;

        static CharacterDamagedEvent e;

        public static void Trigger(string characterId, float damage, float currentHp, bool isCrit)
        {
            e.characterId = characterId;
            e.damage = damage;
            e.currentHp = currentHp;
            e.isCrit = isCrit;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 角色死亡事件
    /// </summary>
    public struct CharacterDeadEvent
    {
        public string characterId;
        public CharacterCamp camp;

        static CharacterDeadEvent e;

        public static void Trigger(string characterId, CharacterCamp camp)
        {
            e.characterId = characterId;
            e.camp = camp;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 获得经验事件
    /// </summary>
    public struct GainExpEvent
    {
        public string characterId;
        public int expAmount;

        static GainExpEvent e;

        public static void Trigger(string characterId, int expAmount)
        {
            e.characterId = characterId;
            e.expAmount = expAmount;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 获得金币事件
    /// </summary>
    public struct GainGoldEvent
    {
        public int amount;

        static GainGoldEvent e;

        public static void Trigger(int amount)
        {
            e.amount = amount;
            MMEventManager.TriggerEvent(e);
        }
    }
}
