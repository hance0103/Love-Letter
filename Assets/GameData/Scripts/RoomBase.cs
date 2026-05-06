using System;
using GameSystem.Enums;
using UnityEngine;

namespace GameData.Scripts
{
    [CreateAssetMenu(fileName = "RoomBase", menuName = "Scriptable Objects/RoomBase")]
    public class RoomBase : ScriptableObject
    {
        public string id;
        public int floor;
        public int room;
        public int level;
        public EncounterTier tier;
        public int clearGoldMin;
        public int clearGoldMax;
        public RewardProbability rewardProbability;
    }
    
    [Serializable]
    public class RewardProbability
    {
        public int probMimicBattle;
        public int probCommonRelic;
        public int probRareRelic;
        public int probEpicRelic;
        public int probCharacterCard;
        public int probNormalCard;
        public int probCommonAcc;
        public int probRareAcc;
        public int probEpicAcc;
    }
}
