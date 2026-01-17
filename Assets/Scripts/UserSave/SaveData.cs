using System;

namespace UserSave
{
    [Serializable]
    public class PlayerContext
    {
        public string name;
        public string id;
    }
    
    [Serializable]
    public class SaveData
    {
        public PlayerContext playerContext;
    }
}