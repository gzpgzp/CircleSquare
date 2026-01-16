using System.Collections.Generic;
using Battle.Character.Ability;
using Battle.Character.CharacterMotor;
using UnityEngine;

namespace Battle.Character
{
    public class BaseCharacter : MonoBehaviour
    {
        private Dictionary<int,BaseAbility> abilities = new Dictionary<int,BaseAbility>();
        public CharacterMotor2D motor { get; protected set; } // 先写死2d吧
        
    }
}