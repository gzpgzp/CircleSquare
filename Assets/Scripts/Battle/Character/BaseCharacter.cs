using System.Collections.Generic;
using Battle.Character.Ability;
using Battle.Character.CharacterMotor;
using GameFramework;
using UnityEngine;

namespace Battle.Character
{
    public class BaseCharacter : MonoBehaviour
    {
        private List<BaseAbility> abilities = new List<BaseAbility>();
        public CharacterMotor2D motor { get; protected set; } // 先写死2d吧
        

        public void AddAbility(BaseAbility ability)
        {
            abilities.Add(ability);
        }
    }
}