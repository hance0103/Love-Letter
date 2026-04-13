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

        private static void Init()
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
        public ResourceManager Resource { get; private set; }
        public PartyManager Party { get; private set; }
        public SaveLoadManager Save { get; private set; }
        public DataManager Data { get; private set; }
        #endregion

        private void Awake()
        {
            Init();
            _ = InitManagers();
            Debug.Log("GameManager Initialized");
        }

        private async UniTask InitManagers()
        {
            Party = gameObject.GetComponent<PartyManager>();
            if (Party == null) gameObject.AddComponent<PartyManager>();
            
            Resource = new ResourceManager();
            await Resource.InitAsync();
            
            Save = new SaveLoadManager();
            Save.Init();
            
            
            Data = new DataManager();
            await Data.InitAsync();
        }
    }
}
