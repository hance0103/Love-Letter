using System.Collections.Generic;
using UnityEngine;

namespace GameData.Scripts
{
    [CreateAssetMenu(fileName = "WaveBase", menuName = "Scriptable Objects/WaveBase")]
    public class WaveBase : ScriptableObject
    {
        public string id;
        public int level;
        public List<string> enemyID;
        public List<int> waveEnemyCount;
        public List<int> turnsToNextWave;
    }
}
