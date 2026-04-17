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
        public List<AbilityActionSet> actionListA = new();
        public ConditionSet condition;
        public List<AbilityActionSet> actionListB = new();
        public string actionAString;
        public string actionBString;
    }

    [Serializable]
    public class AbilityActionSet
    {
        public AbilityActionSet()
        {
            actionType = ActionType.None;
            actionValue = -1;
        }
        public AbilityActionSet(ActionType type, int value)
        {
            actionType = type;
            actionValue = value;
        }
        public ActionType actionType;
        public int actionValue;
    }

    [Serializable]
    public class ConditionSet
    {
        public ConditionSet()
        {
            conditionType = ConditionType.None;
            conditionValue = 0;
        }
        public ConditionSet(ConditionType type, int value)
        {
            conditionType = type;
            conditionValue = value;
        }
        public ConditionType conditionType;
        public int conditionValue;
    }
}
