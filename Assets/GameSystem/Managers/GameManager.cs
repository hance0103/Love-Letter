using GamePlay.Battle;
using GamePlay.Turn;
using UnityEngine;

namespace GameSystem.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        
        private static GameManager _instance;
        public static GameManager Inst { get { Init(); return _instance; } }
        private void Awake()
        {
            Init();
        }

        static void Init()
        {
            if (_instance != null) return;
            
            
            var go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();
            }

            _instance = go.GetComponent<GameManager>();
        }
        
        #endregion
        #region Managers
        public BattleManager Battle => BattleManager.Instance;
        public SaveLoadManager Save => SaveLoadManager.Instance;
        #endregion
        
        
    }
}
