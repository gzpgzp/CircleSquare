using System;
using Tools.Singletons;

namespace UserSave
{
    public class UserManager : Singleton<UserManager>
    {
        public PlayerContext playerContext { get; private set; }

        public void Init()
        {
            playerContext = SaveManager.Instance.GetPlayerData();
        }

        public void ChangePlayerName(string name)
        {
            playerContext.name = name;
            SaveManager.Instance.UpdatePlayerData(playerContext);
        }

        public bool IsUser(string id)
        {
            return id == playerContext.id;
        }
    }
}