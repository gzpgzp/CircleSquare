using System;
using System.Collections.Generic;
using Battle.Character.Ability;
using Tools.DataReader;
using Tools.Singletons;
using UnityEngine;

namespace GameFramework
{
    [Serializable]
    public class AbilityId
    {
        public int Id { get; set; }
        public AbilityEnum AbilityEnum { get; set; }
    }

    public class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<int,AbilityEnum> idToAbility = new Dictionary<int,AbilityEnum>();
        
        public void Init()
        {
            ReadConfig();
        }

        private void ReadConfig()
        {
            var file = Application.streamingAssetsPath + "/Heroes.csv";
            var abilities = CSVUtils.ReadCSV<AbilityId>(file);
            foreach (var ability in abilities)
            {
                idToAbility.Add(ability.Id, ability.AbilityEnum);
            }
        }

        public AbilityEnum GetAbilityById(int id)
        {
            if (idToAbility.TryGetValue(id,out var value))
            {
                return value;
            }

            return AbilityEnum.None;
        }
    }
}