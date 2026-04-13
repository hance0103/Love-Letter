using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace GamePlay.Battle.Card
{
    public enum AddCardPosition
    {
        Draw,
        Hand,
        Used,
    }

    [Serializable]
    public class Deck
    {
        [SerializeField] private List<CardInstance> allCards = new();
        public IReadOnlyList<CardInstance> AllCards => allCards;

        [SerializeField] private List<CardInstance> draw = new();
        public IReadOnlyList<CardInstance> Draw => draw;

        [SerializeField] private List<CardInstance> used = new();
        public IReadOnlyList<CardInstance> Used => used;

        public void InitDeck(List<CardInstance> cards)
        {
            if (cards == null)
            {
                allCards = new List<CardInstance>();
                draw = new List<CardInstance>();
                used = new List<CardInstance>();
                return;
            }

            allCards = new List<CardInstance>(cards);
            draw = new List<CardInstance>(cards);
            used = new List<CardInstance>();

            ShuffleDrawOnly();
        }

        public void ShuffleDeck()
        {
            var shufflePool = new List<CardInstance>(draw.Count + used.Count);
            shufflePool.AddRange(draw);
            shufflePool.AddRange(used);

            draw.Clear();
            used.Clear();

            draw = shufflePool;
            ShuffleList(draw);
        }

        public void ShuffleDrawOnly()
        {
            ShuffleList(draw);
        }

        private void ShuffleList(List<CardInstance> list)
        {
            if (list == null || list.Count <= 1) return;

            for (var i = list.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        public List<CardInstance> DrawCards(int count)
        {
            var drawnCards = new List<CardInstance>();

            if (count <= 0) return drawnCards;

            for (var i = 0; i < count; i++)
            {
                var card = DrawOne();
                if (card == null) break;
                drawnCards.Add(card);
            }

            return drawnCards;
        }

        public CardInstance DrawOne()
        {
            if (draw.Count == 0)
            {
                ShuffleDeck();
            }

            if (draw.Count == 0)
            {
                Debug.Log("뽑을 카드가 없다");
                return null;
            }

            var card = draw[0];
            draw.RemoveAt(0);
            return card;
        }

        public void DiscardOne(CardInstance card)
        {
            if (card == null) return;
            used.Add(card);
        }

        public void DiscardCards(List<CardInstance> cards)
        {
            if (cards == null) return;

            foreach (var card in cards)
            {
                if (card == null) continue;
                used.Add(card);
            }
        }

        public void AddCard(CardInstance card, AddCardPosition position)
        {
            if (card == null) return;

            allCards.Add(card);

            switch (position)
            {
                case AddCardPosition.Draw:
                    draw.Add(card);
                    break;
                case AddCardPosition.Hand:
                    break;
                case AddCardPosition.Used:
                    used.Add(card);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
        }

        public bool Contains(CardInstance card)
        {
            return allCards.Contains(card);
        }
    }

    [Serializable]
    public class Hand
    {
        [SerializeField] private ObservableList<CardInstance> cards = new();
        public ObservableList<CardInstance> Cards => cards;

        public void Add(CardInstance cardInstance)
        {
            if (cardInstance == null) return;
            cards.Add(cardInstance);
        }

        public void Remove(CardInstance cardInstance)
        {
            if (cardInstance == null) return;
            cards.Remove(cardInstance);
        }

        public bool Contains(CardInstance cardInstance)
        {
            return cards.Contains(cardInstance);
        }

        public void Clear()
        {
            cards.Clear();
        }
    }
}