using System;

namespace Battle
{
    [Serializable]
    public class PlayerContext
    {
        public string name;
        public string id;
        public int levelIndex;
        public bool isGuided; 
    }
}