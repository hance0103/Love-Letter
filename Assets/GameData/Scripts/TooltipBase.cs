using UnityEngine;

namespace GameData.Scripts
{
    [CreateAssetMenu(fileName = "WaveBase", menuName = "Scriptable Objects/WaveBase")]
    public class TooltipBase : ScriptableObject
    {
        public string id;
        public int priority;
        public string nameString;
        public string descString;
    }
}