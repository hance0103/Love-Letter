using System;
using System.Collections.Generic;
using GameSystem.Enums;
using UnityEngine;

namespace GameData.SO.Scripts
{
    [CreateAssetMenu(fileName = "CardAbilityBase", menuName = "Card/CardAbilityBase")]
    public class CardAbilityBase : ScriptableObject
    {
        public string abilityID;
        public ActionTarget actionTarget;
        public List<AbilityActionSet> actionListA;
        public ConditionSet condition;
        public List<AbilityActionSet> actionListB;
    }

    [Serializable]
    public class AbilityActionSet
    {
        public ActionType actionType;
        public int actionValue;
    }

    [Serializable]
    public class ConditionSet
    {
        public ConditionType conditionType;
        public int conditionValue;
    }
}
