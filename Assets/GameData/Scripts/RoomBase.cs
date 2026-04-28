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
    }
}
