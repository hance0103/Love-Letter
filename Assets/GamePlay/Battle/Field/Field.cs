using System;
using System.Collections.Generic;
using GamePlay.Battle.Card;
using UnityEngine;

namespace GamePlay.Battle.Field
{
    [Serializable]
    public class FieldInstance
    {
        [SerializeField] private List<CardInstance> cards = new();

        private readonly Dictionary<CardInstance, CardObject> _cardDict = new();

        public IReadOnlyList<CardInstance> Cards => cards;

        public void Init(int slotCount)
        {
            cards = new List<CardInstance>(slotCount);
            for (var i = 0; i < slotCount; i++)
            {
                cards.Add(null);
            }

            _cardDict.Clear();
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < cards.Count;
        }

        public bool IsEmpty(int index)
        {
            if (!IsValidIndex(index)) return false;
            return cards[index] == null;
        }

        public CardInstance GetCard(int index)
        {
            if (!IsValidIndex(index)) return null;
            return cards[index];
        }

        public CardObject GetCardObject(CardInstance cardInstance)
        {
            if (cardInstance == null) return null;

            _cardDict.TryGetValue(cardInstance, out var cardObject);
            return cardObject;
        }

        public bool AddCardToField(CardObject card, int index)
        {
            if (card == null) return false;
            if (card.CardInstance == null) return false;
            if (!IsValidIndex(index)) return false;
            if (cards[index] != null) return false;

            cards[index] = card.CardInstance;
            _cardDict[card.CardInstance] = card;
            return true;
        }

        public bool RemoveCardFromField(CardObject card)
        {
            if (card == null) return false;
            return RemoveCardFromField(card.CardInstance);
        }

        public bool RemoveCardFromField(CardInstance cardInstance)
        {
            if (cardInstance == null) return false;

            var removed = false;

            for (var i = 0; i < cards.Count; i++)
            {
                if (cards[i] != cardInstance) continue;

                cards[i] = null;
                removed = true;
                break;
            }

            _cardDict.Remove(cardInstance);
            return removed;
        }
    }
    public interface IFieldSlot
    {
        bool CanDrop(CardInstance card);
        void OnDrop(CardInstance card);
        void ClearSlot();
        Transform GetTransform();
    }
}
