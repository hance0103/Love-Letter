using Cysharp.Threading.Tasks;
using GamePlay.Party;
using UnityEngine;

namespace GameSystem.Managers
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        private static GameManager _instance;
        public static GameManager Inst { get { Init(); return _instance; } }
        static void Init()
        {
            if (_instance != null) return;
            var go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<GameManager>();
            }
            _instance = go.GetComponent<GameManager>();
            DontDestroyOnLoad(go);
        }
        
        #endregion
        #region Managers
        public SaveLoadManager Save => SaveLoadManager.Instance;
        private ResourceManager _resource;
        public ResourceManager Resource => _resource;
        private PartyManager _party;
        public PartyManager Party => _party;
        #endregion

        private void Awake()
        {
            Init();
            _resource = new ResourceManager();
            _resource.InitAsync().Forget();
            
            _party = gameObject.GetComponent<PartyManager>();
            if (_party == null) gameObject.AddComponent<PartyManager>();
        }
    }
}
