using System;
using Adventure;
using Battle;

namespace UserSave
{
    [Serializable]
    public class SaveData
    {
        public PlayerContext playerContext;
        public int saveId; // todo
        public QuestSaveData questSaveData;
    }
}