using System;
using System.Collections.Generic;
using GameSystem.Enums;
using UnityEngine;

namespace GameData.Scripts
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
    public class AbilityActionSet : IEquatable<AbilityActionSet>
    {
        [SerializeField] private ActionType actionType;
        [SerializeField] private int actionValue;
        public ActionType ActionType => actionType;
        public int ActionValue => actionValue;
        
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
        

        public bool Equals(AbilityActionSet other)
        {
            if (other == null) return false;
            
            return ActionType == other.ActionType && 
                   ActionValue == other.ActionValue;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AbilityActionSet);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ActionType, ActionValue);
        }

        public static bool operator ==(AbilityActionSet a, AbilityActionSet b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(AbilityActionSet a, AbilityActionSet b)
        {
            return !(a == b);
        }
    }

    [Serializable]
    public class ConditionSet : IEquatable<ConditionSet>
    {
        [SerializeField] private ConditionType conditionType;
        [SerializeField] private  int conditionValue;
        
        public ConditionType ConditionType => conditionType;
        public int ConditionValue => conditionValue;
        
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
        

        public bool Equals(ConditionSet other)
        {
            if (other == null) return false;
            return ConditionType == other.ConditionType &&
                   ConditionValue == other.ConditionValue;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConditionSet);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ConditionType, ConditionValue);
        }
        
        public static bool operator ==(ConditionSet a, ConditionSet b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(ConditionSet a, ConditionSet b)
        {
            return !(a == b);
        }
    }
}
