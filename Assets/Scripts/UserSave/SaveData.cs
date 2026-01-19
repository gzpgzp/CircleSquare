using System;
using Battle;

namespace UserSave
{
    [Serializable]
    public class SaveData
    {
        public PlayerContext playerContext;
        public int saveId; // todo
    }
}