using System.Collections.Generic;
using Battle.Character;
using GameFramework;
using Tools.ResourcesTool;
using UnityEngine;

namespace Battle.GameFlow
{
    public class LevelManager
    {
        public void CreateLevel()
        {
            var player = new CharacterContext()
            {
                isAI = false,
                abilities =  new List<int>()
                {
                    10,
                    20,
                    40,
                },
                characterModelId = 1,
            };
            var mainCharacter = GameWorld.Instance.gameManager.characterManager.CreatePlayer(player);
            mainCharacter.SetPos(Vector3.zero);

            var map = MyResourcesManager.Instance.LoadAndInstantiate("Prefabs/Maps/Map_1");
        }

        public void RestartLevel()
        {
            
        }

        public void Tick(float dt)
        {
            
        }
    }
}