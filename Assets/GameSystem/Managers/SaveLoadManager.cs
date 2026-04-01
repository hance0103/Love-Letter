using UnityEngine;

namespace GameSystem.Managers
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance;
        private void Awake()
        {
            Instance = this;
        }

        public void SaveData()
        {
            
        }

        public void LoadData()
        {
            
        }
    }
}