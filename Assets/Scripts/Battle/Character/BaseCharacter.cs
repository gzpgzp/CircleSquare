using System.Collections.Generic;
using Battle.Character.Ability;
using UnityEngine;

namespace Battle.Character
{
    public class BaseCharacter : MonoBehaviour
    {
        private Dictionary<int,BaseAbility> abilities = new Dictionary<int,BaseAbility>();

    }
}