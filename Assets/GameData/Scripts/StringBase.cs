using UnityEngine;

namespace GameData.Scripts
{
    [CreateAssetMenu(fileName = "StringDataBase", menuName = "Scriptable Objects/StringDataBase")]
    public class StringBase : ScriptableObject
    {
        public string id;
        public string kr;
        public string en;
    }
}
