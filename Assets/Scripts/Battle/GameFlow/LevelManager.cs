using System.Collections.Generic;
using System.Linq;
using Battle.Character;
using Cameras;
using GameFramework;
using Mask;
using Tools.EventTool;
using Tools.ResourcesTool;
using UnityEngine;
using UserSave;

namespace Battle.GameFlow
{
    public class LevelManager
    {
        private MaskLevel currLevel = null;
        private PlayerController2D player = null;

        private int currLevelIndex = 1;

        public void CreateLevel(int levelIndex)
        {
            if (currLevel != null)
            {
                GameObject.Destroy(currLevel.gameObject);
            }
            
            currLevelIndex = levelIndex;
            currLevel = MyResourcesManager.Instance.LoadAndInstantiate($"Prefabs/Maps/Map_{currLevelIndex}").GetComponent<MaskLevel>();
            player = MyResourcesManager.Instance.LoadAndInstantiate("Prefabs/Characters/Character").GetComponent<PlayerController2D>();
            player.transform.position = currLevel.respawnTr.position;
            player.StartLevel();
            
            currLevel.StartLevel();
            
            CameraManager.Instance.ChangeCameraFollowLimit(levelIndex);
            CameraManager.Instance.MainCameraFollowTarget(player.transform);
        }

        public void RestartLevel()
        {
            GameObject.Destroy(player.gameObject);
            player = MyResourcesManager.Instance.LoadAndInstantiate("Prefabs/Characters/Character").GetComponent<PlayerController2D>();
            player.transform.position = currLevel.respawnTr.position;
            player.StartLevel();
            
            CameraManager.Instance.MainCameraFollowTarget(player.transform);
            currLevel.Restart();
        }

        public void Tick(float dt)
        {
            
        }

        public void EnterNextLevel()
        {
            currLevelIndex += 1;
            CreateLevel(currLevelIndex);
        }

        public void PauseLevel()
        {
            player.Pause();
        }

        public bool CheckNextLevel()
        {
            if (currLevelIndex >= ConfigManager.Instance.GetMaxLevel())
            {
                Debug.Log("没有关卡了");
                return false;
            }
            else
            {
                SaveManager.Instance.saveData.playerContext.levelIndex += 1;
                SaveManager.Instance.Save();
                return true;
            }
        }
    }
}