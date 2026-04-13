using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GamePlay.Battle.Field;
using GamePlay.Card;
using GameSystem.Enums;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;
        
        [Header("Battle Scene Objects")]
        [SerializeField] private CardPool cardPool;

        private HandSortingManager _handSort;
        
        [Header("Cards")]
        [SerializeField] private Deck playerDeck = new();
        [SerializeField] private Hand hand = new();
        [SerializeField] private List<CardInstance> enemyWaitList = new();
        
        public Hand Hand => hand;
        
        private Dictionary<CardInstance, GameObject> _handDict = new();
        
        [Header("Field")]
        [SerializeField] private FieldInstance playerField = new();
        [SerializeField] private FieldInstance enemyField = new();
        
        [SerializeField] private int playerFieldSlotCount = 4;
        [SerializeField] private int enemyFieldSlotCount = 4;

        private int _currentMaxCost;
        private int _currentCost;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("BattleManager 중복 생성됨");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _handSort = GetComponent<HandSortingManager>();
            _handSort.Init();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            BattleInit();
        }

        private void BattleInit()
        {
            try
            {
                var deckSource = GameManager.Inst.Party.CreateDeck();
                playerDeck.InitDeck(deckSource);

                playerField.Init(playerFieldSlotCount);
                enemyField.Init(enemyFieldSlotCount);

                OnBattleStart();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnBattleStart()
        {
            for (var i = 0; i < 6; i++)
            {
                _ = DrawOne();
            }
        }

        public async UniTask DrawOne()
        {
            try
            {
                var card = playerDeck.DrawOne();
                if (card == null) return;

                hand.Add(card);
                var go = await InstantiateCardObjectAsync(card);
                _handDict[card] = go;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DiscardCard(CardInstance cardInstance)
        {
            if (cardInstance == null) return;
            try
            {
                ReleaseHandObject(cardInstance);
                hand.Remove(cardInstance);
                playerDeck.DiscardOne(cardInstance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public bool PlaceCharacterCardToField(CardObject card, FieldSlot slot)
        {
            if (card == null || slot == null) return false;
            if (card.CardInstance == null) return false;

            try
            {
                FieldInstance targetField = null;

                switch (slot.SlotOwner)
                {
                    case CardOwner.Player:
                        targetField = playerField;
                        break;
                    case CardOwner.Enemy:
                        targetField = enemyField;
                        break;
                    default:
                        return false;
                }

                if (targetField == null) return false;

                var slotIndex = slot.SlotIndex;
                if (!targetField.AddCardToField(card, slotIndex))
                {
                    return false;
                }

                hand.Remove(card.CardInstance);
                _handDict.Remove(card.CardInstance);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public bool RemoveCharacterCardFromField(CardObject card, CardOwner owner)
        {
            if (card == null) return false;

            try
            {
                return owner switch
                {
                    CardOwner.Player => playerField.RemoveCardFromField(card),
                    CardOwner.Enemy => enemyField.RemoveCardFromField(card),
                    _ => false
                };
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
        private async UniTask<GameObject> InstantiateCardObjectAsync(CardInstance cardInstance, AddCardPosition cardPosition = AddCardPosition.Hand)
        {
            try
            {
                 // 이거 오브젝트풀에서 받아오기
                 var cardObject =  await cardPool.Get(cardInstance);
                 if (CardUseManager.Instance == null) return null;
                 var parent = CardUseManager.Instance.GetCardPositionTransform(cardPosition);
                 
                 if (parent != null)
                 {
                     cardObject.transform.SetParent(parent);
                     cardObject.transform.localPosition = Vector3.zero;
                 }
                 else
                 {
                     // 핸드에 생성하지 않는 경우
                     // 뽑을카드 | 버린카드 | 상대 덱
                 }
                 return cardObject.gameObject;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private void ReleaseHandObject(CardInstance cardInstance)
        {
            try
            {
                if (!_handDict.TryGetValue(cardInstance, out var cardObject)) return;
                
                if (cardObject != null)
                {
                    var card = cardObject.GetComponent<CardObject>();
                    cardPool.Release(card);
                }
                _handDict.Remove(cardInstance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}