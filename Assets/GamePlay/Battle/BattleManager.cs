using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using GameSystem.Managers;
using UnityEngine;

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

            if (handSortingManager != null)
            {
                handSortingManager.Init();
            }
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

        public bool RemoveCharacterCardFromField(CardObject cardObject, CardOwner owner)
        {
            if (cardObject == null) return false;

            if (!_fieldInstanceDict.TryGetValue(owner, out var fieldInstance))
            {
                return false;
            }

            var removed = fieldInstance.RemoveCardFromField(cardObject);
            if (!removed) return false;

            if (cardObject.CurrentSlot != null)
            {
                cardObject.CurrentSlot.ClearSlot();
            }

            cardObject.ClearCurrentSlot();
            return true;
        }

        public void UseNormalCard(CardObject cardObject, FieldSlot targetSlot)
        {
            if (cardObject == null || cardObject.CardInstance == null || targetSlot == null) return;

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
    }
}