using System;
using System.Collections.Generic;
using System.IO;
using Battle.Character.Ability;
using cfg;
using SimpleJSON;
using Tools.DataReader;
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

        public AbilityEnum[] GetCharacterAbilities(int id)
        {
            return ToEnumArray(tables.TbCharacterInfo.Get(id).Abilities);
        }
        
        AbilityEnum[] ToEnumArray(int[] ids)
        {
            var result = new AbilityEnum[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                result[i] = (AbilityEnum)Enum.ToObject(typeof(AbilityEnum), ids[i]);
            }
            return result;
        }
    }
}