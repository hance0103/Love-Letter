using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GamePlay.Battle.Field;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;

        // // 마우스 올리거나 드래그하여 하이라이트된 카드
        // private GameObject _selectedCard;
        // // 클릭을 놓아서 사용이 확정된 카드
        // private CardInstance _usingCard;
        
        private void Awake()
        {
            Instance = this;
        }
        
        /// <summary>
        /// 여기는 현재 전투에서 사용하는 덱
        /// deck.AllCards에서는 일시적인 강화 전부 적용되어야함
        /// </summary>
        [SerializeField]
        private Deck playerDeck = new Deck();
        // [SerializeField]
        // private Deck enemyDeck = new Deck();
        
        [SerializeField] private Hand hand = new Hand();
        private readonly Dictionary<CardInstance, GameObject> _handDict = new Dictionary<CardInstance, GameObject>();
        
        [SerializeField]
        private FieldInstance playerField = new FieldInstance();
        [SerializeField]
        private FieldInstance enemyField = new FieldInstance();
        
        [SerializeField]
        private List<CardInstance> enemyWaitList = new List<CardInstance>();
            
        private int _currentMaxCost;
        private int _currentCost;

        private void Start()
        {
            BattleInit();
        }
        
        private void BattleInit()
        {
            playerDeck.InitDeck(GameManager.Inst.Party.CreateDeck()); 
            OnBattleStart();
        }
        
        private void OnBattleStart()
        {
            // 드로우 연출
            for (var i = 0; i < 6; i++)
            {
                _ = DrawOne();
            }
        }
        
        private void OnBattleEnd()
        {
            
        }
        
        public async UniTask DrawOne()
        {
            var card = playerDeck.DrawOne();
            if (card == null) return;
            
            hand.Add(card);
            await InstantiateCardObjectAsync(card);
        }

        public void DiscardCard(CardInstance cardInstance)
        {
            playerDeck.DiscardOne(cardInstance);
            ReleaseHandObject(cardInstance);
        }
        
        /// <summary>
        /// 카드 오브젝트 생성
        /// 나중에 핸드말고 다른곳에 생성할 일도 있을거 같음
        /// </summary>
        private const string CardPrefabAddress = "CardObject";
        private async UniTask InstantiateCardObjectAsync(CardInstance cardInstance, AddCardPosition cardPosition = AddCardPosition.Hand)
        {
            switch (cardPosition)
            {
                case AddCardPosition.Draw:
                    break;
                case AddCardPosition.Hand:
                {
                    var cardObject = await GameManager.Inst.Resource.InstantiateAsync(CardPrefabAddress, CardUseManager.Instance.HandLayer);
                    cardObject.GetComponent<CardObject>().Init(cardInstance);
                    _handDict[cardInstance] = cardObject;
                    break;
                }
                case AddCardPosition.Used:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cardPosition), cardPosition, null);
            }
        }
        
        /// <summary>
        /// 카드 오브젝트 릴리즈
        /// </summary>
        private void ReleaseHandObject(CardInstance cardInstance)
        {
            _handDict.TryGetValue(cardInstance, out var cardObject);
            if (cardObject == null) return;
            
            GameManager.Inst.Resource.ReleaseInstance(cardObject);
            _handDict.Remove(cardInstance);
        }
    }
}
