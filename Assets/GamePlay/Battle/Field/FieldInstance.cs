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

        private readonly Dictionary<CardInstance, CardObject> _cardObjectByInstance = new();

        public IReadOnlyList<CardInstance> Cards => cards;

        public void Init(int slotCount)
        {
            cards = new List<CardInstance>(slotCount);

            for (int i = 0; i < slotCount; i++)
            {
                cards.Add(null);
            }

            _cardObjectByInstance.Clear();
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

            _cardObjectByInstance.TryGetValue(cardInstance, out var cardObject);
            return cardObject;
        }

        public bool AddCardToField(CardObject cardObject, int index)
        {
            if (cardObject == null) return false;
            if (cardObject.CardInstance == null) return false;
            if (!IsValidIndex(index)) return false;
            if (cards[index] != null) return false;

            cards[index] = cardObject.CardInstance;
            _cardObjectByInstance[cardObject.CardInstance] = cardObject;
            return true;
        }

        public bool RemoveCardFromField(CardObject cardObject)
        {
            if (cardObject == null) return false;
            return RemoveCardFromField(cardObject.CardInstance);
        }

        public bool RemoveCardFromField(CardInstance cardInstance)
        {
            if (cardInstance == null) return false;

            bool removed = false;

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != cardInstance) continue;

                cards[i] = null;
                removed = true;
                break;
            }

            _cardObjectByInstance.Remove(cardInstance);
            return removed;
        }
    }
}