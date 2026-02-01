using System;
using System.Collections.Generic;
using cfg.demo;
using Tools.ResourcesTool;
using UnityEngine;
using UserSave;

namespace Mask
{
    public class LevelList : MonoBehaviour
    {
        [SerializeField] private List<LevelListItem> items;
        [SerializeField] private GameObject panel;

        private GameObject wall;
        
        public void Start()
        {
            wall = MyResourcesManager.Instance.LoadAndInstantiate("Prefabs/WorldWall");
            Init(SaveManager.Instance.saveData.playerContext.levelIndex);
        }

        public void Init(int curLevelIndex)
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Init(i + 1, curLevelIndex < i + 1, () =>
                {
                    Destroy(panel);
                    Destroy(wall);
                });
            }
        }
    }
}