using System;
using System.Collections.Generic;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Card
{
    [Serializable]
    public class CardAbility : ScriptableObject
    {
        public string abilityIndex;
        public List<ActionSet> actionA;
        public ConditionType condition;
        public int conditionValue;
        public List<ActionSet> actionB;
    }
    
    
    [Serializable]  
    public class ActionSet
    {
        public ActionType actionType;
        public int actionValue;
    }
}
