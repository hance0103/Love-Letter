using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData.Scripts;
using GamePlay.Battle.Card;
using GamePlay.Battle.Event;
using GamePlay.Battle.Event.EventType;
using GamePlay.Battle.Field;
using GamePlay.Card;
using GameSystem.Enums;
using GameSystem.Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using EventBus = GamePlay.Battle.Event.EventBus;

namespace GamePlay.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        [Header("References")]
        [SerializeField] private CardPool cardPool;
        [SerializeField] private HandSortingManager handSortingManager;
        
        [Header("UI Parents")]
        [SerializeField] private RectTransform handCardRoot;

        [Header("Slots")]
        [SerializeField] private List<FieldSlot> playerSlots = new();
        [SerializeField] private List<FieldSlot> enemySlots = new();

        [Header("Runtime")]
        [SerializeField] private Deck deck = new();
        [SerializeField] private Hand hand = new();

        [Header("Battle Setting")]
        [SerializeField] private int drawAmount = 6;
        [SerializeField] private int maxHand = 10;
        
        [Header("Enemy")]
        [SerializeField] private List<CardBase> enemies = new();

        private readonly Dictionary<CardOwner, FieldInstance> _fieldInstanceDict = new();
        private readonly Dictionary<CardInstance, CardObject> _handCardObjects = new();

        public HandSortingManager HandSortingManager => handSortingManager;
        public Deck Deck => deck;
        public Hand Hand => hand;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => GameManager.Inst.Initialized);
            Init(GameManager.Inst.Party.CreateDeck());
            
            await DrawCardsAsync(drawAmount);
            
            
            // 적 생성
            // foreach (var enemy in enemies)
            // {
            //
            // }
            
            // 일단 임시로
            var cardObject = await cardPool.Get(new CardInstance(enemies[0], CardOwner.Enemy));
            if (cardObject == null) return;
                
            var success = PlaceCharacterCardToField(cardObject, enemySlots[0]);
            if (!success) return;

            await cardObject.ReturnToSlotAsync(enemySlots[0], CardUseManager.Instance.FieldCardLayer);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Init(List<CardInstance> startDeck)
        {
            deck.InitDeck(startDeck);
            hand.Clear();

            _fieldInstanceDict.Clear();

            var playerField = new FieldInstance();
            playerField.Init(playerSlots.Count);
            _fieldInstanceDict.Add(CardOwner.Player, playerField);

            var enemyField = new FieldInstance();
            enemyField.Init(enemySlots.Count);
            _fieldInstanceDict.Add(CardOwner.Enemy, enemyField);

            foreach (var slot in playerSlots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }

            foreach (var slot in enemySlots)
            {
                if (slot != null)
                {
                    slot.ClearSlot();
                }
            }

            foreach (var pair in _handCardObjects)
            {
                if (pair.Value != null && cardPool != null)
                {
                    cardPool.Release(pair.Value);
                }
            }

            _handCardObjects.Clear();
            handSortingManager?.Init();
        }

        public async UniTask DrawCardsAsync(int count)
        {
            int canDrawCount = Mathf.Min(count, maxHand - hand.Cards.Count);
            if (canDrawCount <= 0) return;

            var cards = deck.DrawCards(canDrawCount);

            foreach (var card in cards)
            {
                if (card == null) continue;

                hand.Add(card);

                var cardObject = await cardPool.Get(card);
                if (cardObject == null) continue;

                if (handCardRoot != null)
                {
                    cardObject.transform.SetParent(handCardRoot, false);
                }

                cardObject.SetCurrentSlot(null);
                _handCardObjects[card] = cardObject;
            }

            RequestHandLayoutRefresh();
        }

        public async UniTask DrawOneAsync()
        {
            if (hand.Cards.Count >= maxHand) return;

            var card = deck.DrawOne();
            if (card == null) return;

            hand.Add(card);

            var cardObject = await cardPool.Get(card);
            if (cardObject == null) return;

            if (handCardRoot != null)
            {
                cardObject.transform.SetParent(handCardRoot, false);
            }

            cardObject.SetCurrentSlot(null);
            _handCardObjects[card] = cardObject;

            RequestHandLayoutRefresh();
        }

        public bool TryGetHandCardObject(CardInstance cardInstance, out CardObject cardObject)
        {
            return _handCardObjects.TryGetValue(cardInstance, out cardObject);
        }

        public bool PlaceCharacterCardToField(CardObject cardObject, FieldSlot slot)
        { 
            if (cardObject == null || slot == null) return false;
            if (!slot.CanDrop(cardObject.CardInstance)) return false;
            if (!_fieldInstanceDict.TryGetValue(slot.SlotOwner, out var fieldInstance))
            {
                return false;
            }
            bool added = fieldInstance.AddCardToField(cardObject, slot.SlotIndex);
            if (!added) return false;

            slot.OnDrop(cardObject.CardInstance);
            cardObject.SetCurrentSlot(slot);

            if (hand.Contains(cardObject.CardInstance))
            {
                hand.Remove(cardObject.CardInstance);
            }

            RequestHandLayoutRefresh();
            return true;
        }

        public bool MoveCharacterCardToFieldSlot(CardObject cardObject, FieldSlot fromSlot, FieldSlot toSlot)
        {
            if (cardObject == null || fromSlot == null || toSlot == null) return false;
            if (fromSlot == toSlot) return true;
            if (!toSlot.CanDrop(cardObject.CardInstance)) return false;

            if (!_fieldInstanceDict.TryGetValue(fromSlot.SlotOwner, out var fromField))
            {
                return false;
            }

            if (!_fieldInstanceDict.TryGetValue(toSlot.SlotOwner, out var toField))
            {
                return false;
            }

            bool removed = fromField.RemoveCardFromField(cardObject);
            if (!removed) return false;

            fromSlot.ClearSlot();

            bool added = toField.AddCardToField(cardObject, toSlot.SlotIndex);
            if (!added)
            {
                fromField.AddCardToField(cardObject, fromSlot.SlotIndex);
                fromSlot.OnDrop(cardObject.CardInstance);
                return false;
            }

            toSlot.OnDrop(cardObject.CardInstance);
            cardObject.SetCurrentSlot(toSlot);
            return true;
        }

        public bool RemoveCharacterCardFromField(CardInstance cardInstance)
        {
            if (cardInstance == null) return false;

            if (!_fieldInstanceDict.TryGetValue(cardInstance.CardOwner, out var fieldInstance))
            {
                return false;
            }

            var cardObject = fieldInstance.GetCardObject(cardInstance);
            var removed = fieldInstance.RemoveCardFromField(cardInstance);
            if (!removed) return false;

            if (cardObject.CurrentSlot != null)
            {
                cardObject.CurrentSlot.ClearSlot();
            }

            var slot = cardObject.CurrentSlot;
            slot.ClearSlot();
            cardObject.ClearCurrentSlot();
            cardPool.Release(cardObject);
            
            return true;
        }

        public void UseNormalCard(CardObject cardObject, FieldSlot targetSlot)
        {
            if (cardObject == null || cardObject.CardInstance == null || targetSlot == null) return;

            
            EventBus.Publish(new CardAbilityRequestEvent(
                cardObject.CardInstance, 
                CardEffectTriggerType.NormalCardUse, 
                targetSlot));
            DiscardCard(cardObject.CardInstance);
        }

        public void DiscardCard(CardInstance card)
        {
            if (card == null) return;

            if (hand.Contains(card))
            {
                hand.Remove(card);
            }

            deck.DiscardOne(card);

            if (_handCardObjects.Remove(card, out var cardObject))
            {
                if (cardPool != null)
                {
                    cardPool.Release(cardObject);
                }
            }

            RequestHandLayoutRefresh();
        }

        public CardObject GetFieldCardObject(CardOwner owner, int slotIndex)
        {
            if (!_fieldInstanceDict.TryGetValue(owner, out var fieldInstance))
            {
                return null;
            }

            var card = fieldInstance.GetCard(slotIndex);
            if (card == null) return null;

            return fieldInstance.GetCardObject(card);
        }

        public FieldSlot GetFieldSlot(CardOwner owner, int slotIndex)
        {
            var slots = owner == CardOwner.Player ? playerSlots : enemySlots;
            if (slotIndex < 0 || slotIndex >= slots.Count) return null;
            return slots[slotIndex];
        }

        public void RequestHandLayoutRefresh()
        {
            if (handSortingManager != null)
            {
                handSortingManager.RequestRefresh();
            }
        }

        public void OnClickDiscardButton()
        {
            _ = DiscardHandAndDrawAsync();
        }
        public async UniTask DiscardHandAndDrawAsync()
        {
            if (CardUseManager.HasInstance)
            {
                CardUseManager.Instance.ForceReset();
            }

            var cardsToDiscard = new List<CardInstance>(hand.Cards.Items);

            foreach (var card in cardsToDiscard)
            {
                if (card == null) continue;

                if (hand.Contains(card))
                {
                    hand.Remove(card);
                }

                deck.DiscardOne(card);

                if (_handCardObjects.Remove(card, out var cardObject))
                {
                    if (cardPool != null)
                    {
                        cardPool.Release(cardObject);
                    }
                }
            }

            RequestHandLayoutRefresh();
            await DrawCardsAsync(drawAmount);
        }

        public List<CardObject> GetAllFieldCards()
        {
            var result = new List<CardObject>();

            foreach (var slot in playerSlots)
            {
                if (slot == null) continue;

                var cardObject = GetFieldCardObject(CardOwner.Player, slot.SlotIndex);
                if (cardObject != null)
                {
                    result.Add(cardObject);
                }
            }

            foreach (var slot in enemySlots)
            {
                if (slot == null) continue;

                var cardObject = GetFieldCardObject(CardOwner.Enemy, slot.SlotIndex);
                if (cardObject != null)
                {
                    result.Add(cardObject);
                }
            }

            return result;
        }
        
        /// <summary>
        /// owner는 효과의 대상이 되는 카드의 주인을 의미함
        /// </summary>
        /// <param name="target"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public List<CardInstance> GetFieldCard(ActualActionTarget target, CardOwner owner)
        {
            var result = new List<CardInstance>();
            
            _fieldInstanceDict.TryGetValue(owner, out var fieldInstance);
            if (fieldInstance == null) return result;
            
            var actualField = fieldInstance.Cards.Where(card => card != null).ToList();
            if (actualField.Count <= 0) return result;
            
            var count = actualField.Count;
            
            switch (target)
            {
                case ActualActionTarget.Front:
                {
                    result.Add(actualField[0]);
                    break;
                }
                case ActualActionTarget.Back:
                {
                    result.Add(actualField[count - 1]);
                    break;
                }
                case ActualActionTarget.All:
                {
                    result.AddRange(actualField);
                    break;
                }
                case ActualActionTarget.Random:
                {
                    var index = UnityEngine.Random.Range(0, count);
                    result.Add(actualField[index]);
                    break;
                }
                case ActualActionTarget.Near:
                {
                    // 얜 나중에 처리하자
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }

            return result;
        }
        /// <summary>
        /// 해당 카드 앞에 있는 모든 카드
        /// </summary>
        /// <param name="cardInstance"></param>
        /// <returns></returns>
        public List<CardInstance> GetFrontCards(CardInstance cardInstance)
        {
            var result = new List<CardInstance>();
            if (cardInstance == null || !_fieldInstanceDict.TryGetValue(cardInstance.CardOwner, out var fieldInstance)) 
                return result;

            var myIndex = GetFieldSlotIndex(cardInstance);
            if (myIndex <= 0) return result;

            for (var i = 0; i < myIndex; i++)
            {
                var target = fieldInstance.Cards[i];
                if (target != null && target != cardInstance && target.Data != null)
                {
                    result.Add(target);
                }
            }
            return result;
        }
        /// <summary>
        /// 해당 카드의 앞뒤 카드 반환
        /// </summary>
        /// <param name="cardInstance"></param>
        /// <returns></returns>
        public List<CardInstance> GetNearCards(CardInstance cardInstance)
        {
            var result = new List<CardInstance>();
            if (cardInstance == null) return result;

            var front = GetFrontCard(cardInstance);
            if (front != null)
                result.Add(front);

            var back = GetBackCard(cardInstance);
            if (back != null)
                result.Add(back);

            return result;
        }
        
        /// <summary>
        /// 바로 앞 카드
        /// </summary>
        /// <param name="cardInstance"></param>
        /// <returns></returns>
        public CardInstance GetFrontCard(CardInstance cardInstance)
        {
            if (cardInstance == null) return null;

            if (!_fieldInstanceDict.TryGetValue(cardInstance.CardOwner, out var fieldInstance))
            {
                return null;
            }
            
            // 해당 카드의 인덱스
            var myIndex = GetFieldSlotIndex(cardInstance);
            return myIndex <= 0 ? null : fieldInstance.Cards[myIndex - 1];
        }
        /// <summary>
        /// 바로 뒤 카드
        /// </summary>
        /// <param name="cardInstance"></param>
        /// <returns></returns>
        public CardInstance GetBackCard(CardInstance cardInstance)
        {
            if (cardInstance == null) return null;

            if (!_fieldInstanceDict.TryGetValue(cardInstance.CardOwner, out var fieldInstance))
            {
                return null;
            }

            var myIndex = GetFieldSlotIndex(cardInstance);
            return myIndex < 0 ? null : fieldInstance.Cards[myIndex + 1];
        }
        public int GetFieldSlotIndex(CardInstance cardInstance)
        {
            if (cardInstance == null) return -1;

            if (!_fieldInstanceDict.TryGetValue(cardInstance.CardOwner, out var fieldInstance))
            {
                return -1;
            }

            for (int i = 0; i < fieldInstance.Cards.Count; i++)
            {
                if (fieldInstance.Cards[i] == cardInstance)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public FieldSlot GetFieldSlot(CardInstance cardInstance)
        {
            if (cardInstance == null) return null;

            var slotIndex = GetFieldSlotIndex(cardInstance);
            if (slotIndex < 0) return null;

            return GetFieldSlot(cardInstance.CardOwner, slotIndex);
        }

        public void RefreshAllFieldCards()
        {
            var cards = GetAllFieldCards();
            foreach (var card in cards)
            {
                card.RefreshCardInfo();
            }
        }
    }

}