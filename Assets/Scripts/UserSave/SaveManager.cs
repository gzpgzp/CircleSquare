using System.IO;
using Battle;
using Tools.Singletons;
using UnityEngine;

namespace UserSave
{
    public class SaveManager : Singleton<SaveManager>
    {
        private string saveFolder => Application.persistentDataPath + "/Saves";
        private string savePath => Path.Combine(saveFolder, "save.json");

        public SaveData saveData { get; private set; }

        // private bool isNewLoadDebug = true;
        
        public PlayerContext GetPlayerData()
        {
            if (!HasSave())
            {
                var playerContext = new PlayerContext()
                {
                    id = System.Guid.NewGuid().ToString(),
                    name = "Player",
                    levelIndex = 1,
                };

                saveData = new SaveData()
                {
                    playerContext = playerContext,
                };
                
                Save(saveData);
                return playerContext;
            }
            else
            {
                saveData = Load();
                return saveData.playerContext;
            }
        }

        public void UpdatePlayerData(PlayerContext playerContext)
        {
            saveData.playerContext = playerContext;
            Save(saveData);
        }

        public void Save()
        {
            Save(saveData);
        }

        // 保存数据
        public void Save(SaveData data)
        {
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Saved to: " + savePath);
        }

        // 加载数据
        public SaveData Load()
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("No save file found.");
                return null;
            }

            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Loaded from: " + savePath);
            return data;
        }

        // 删除存档
        public void Delete()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted.");
            }
        }

        // 检查是否有存档
        public bool HasSave()
        {
            return File.Exists(savePath);
        }
    }
}