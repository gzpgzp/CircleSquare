using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battle.Character.Ability;
using cfg;
using SimpleJSON;
using Tools.Singletons;
using UnityEngine;

namespace GameFramework
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        private Tables tables;
        
        public void Init()
        {
            string gameConfDir = Application.streamingAssetsPath + "/OutputData";
            tables = new cfg.Tables(file => JSON.Parse(File.ReadAllText($"{gameConfDir}/{file}.json")));
        }

        #region Battle

        public AbilityEnum GetAbilityById(int id)
        {
            return (AbilityEnum)id;
        }

        public AbilityEnum[] GetCharacterAbilities(int id)
        {
            return ToEnumArray(tables.TbCharacterInfo.Get(id).Abilities);
        }

        #endregion

        #region 道具

        /// <summary>
        /// 通过字符串ID获取道具配置
        /// </summary>
        public cfg.Adventure.ItemInfo GetItemInfo(string itemId)
        {
            return tables.TbItemInfo.GetOrDefault(itemId);
        }

        /// <summary>
        /// 通过数字ID获取道具配置
        /// </summary>
        public cfg.Adventure.ItemInfo GetItemInfoById(int id)
        {
            return tables.TbItemInfo.DataList.Find(item => item.Id == id);
        }

        /// <summary>
        /// 获取所有道具配置列表
        /// </summary>
        public List<cfg.Adventure.ItemInfo> GetAllItemInfos()
        {
            return tables.TbItemInfo.DataList;
        }

        #endregion

        #region 角色

        /// <summary>
        /// 通过字符串ID获取角色配置
        /// </summary>
        public cfg.Adventure.CharacterInfo GetCharacterInfo(string characterId)
        {
            return tables.TbAdventureCharacter.GetOrDefault(characterId);
        }

        /// <summary>
        /// 通过数字ID获取角色配置
        /// </summary>
        public cfg.Adventure.CharacterInfo GetCharacterInfoById(int id)
        {
            return tables.TbAdventureCharacter.DataList.Find(c => c.Id == id);
        }

        /// <summary>
        /// 获取所有角色配置
        /// </summary>
        public List<cfg.Adventure.CharacterInfo> GetAllCharacterInfos()
        {
            return tables.TbAdventureCharacter.DataList;
        }

        #endregion

        #region 地点

        /// <summary>
        /// 通过字符串ID获取地点配置
        /// </summary>
        public cfg.Adventure.LocationInfo GetLocationInfo(string locationId)
        {
            return tables.TbLocationInfo.GetOrDefault(locationId);
        }

        /// <summary>
        /// 获取所有地点配置
        /// </summary>
        public List<cfg.Adventure.LocationInfo> GetAllLocationInfos()
        {
            return tables.TbLocationInfo.DataList;
        }

        #endregion

        #region 任务

        /// <summary>
        /// 通过字符串ID获取任务配置
        /// </summary>
        public cfg.Adventure.QuestInfo GetQuestInfo(string questId)
        {
            return tables.TbQuestInfo.GetOrDefault(questId);
        }

        /// <summary>
        /// 获取所有任务配置
        /// </summary>
        public List<cfg.Adventure.QuestInfo> GetAllQuestInfos()
        {
            return tables.TbQuestInfo.DataList;
        }

        /// <summary>
        /// 获取指定任务的所有条件
        /// </summary>
        public List<cfg.Adventure.QuestConditionCfg> GetQuestConditions(string questId)
        {
            return tables.TbQuestCondition.DataList.Where(c => c.QuestId == questId).ToList();
        }

        /// <summary>
        /// 获取指定任务的所有奖励
        /// </summary>
        public List<cfg.Adventure.QuestRewardCfg> GetQuestRewards(string questId)
        {
            return tables.TbQuestReward.DataList.Where(r => r.QuestId == questId).ToList();
        }

        #endregion

        #region 内部工具

        AbilityEnum[] ToEnumArray(int[] ids)
        {
            var result = new AbilityEnum[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                result[i] = (AbilityEnum)Enum.ToObject(typeof(AbilityEnum), ids[i]);
            }
            return result;
        }

        #endregion
    }
}