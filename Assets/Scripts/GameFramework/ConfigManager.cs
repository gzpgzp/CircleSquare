using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Battle.Character.Ability;
using cfg;
using SimpleJSON;
using Tools.DataReader;
using Tools.Singletons;
using UnityEngine;
using CharacterInfo = cfg.Battle.CharacterInfo;

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

        public AbilityEnum GetAbilityById(int id)
        {
            return (AbilityEnum)id;
            // if (idToAbility.TryGetValue(id,out var value))
            // {
            //     return value;
            // }
            //
            // return AbilityEnum.None;
        }

        public CharacterInfo GetCharacterAbilities(int id)
        {
            return tables.TbCharacterInfo.Get(id);
        }

        // public AbilityEnum[] GetCharacterAbilities(int id)
        // {
        //     return ToEnumArray(tables.TbCharacterInfo.Get(id).Abilities);
        // }
        
        AbilityEnum[] ToEnumArray(int[] ids)
        {
            var result = new AbilityEnum[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                result[i] = (AbilityEnum)Enum.ToObject(typeof(AbilityEnum), ids[i]);
            }
            return result;
        }

        public int GetMaxLevel()
        {
            return 2;
        }

        public bool CheckNextGuide(int index)
        {
            return index <= tables.TbGuide.DataList.Count;
        }

        public string GetGuideText(int id)
        {
            return tables.TbGuide.Get(1).Text;
        }

        public List<string> GetGuideTexts()
        {
            var list = new List<string>();
            foreach (var guide in tables.TbGuide.DataList)
            {
                list.Add(guide.Text);
            }

            return list;
        }
    }
}