using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Card;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GamePlay.Battle
{
    public enum AddCardPosition
    {
        Draw,
        Hand,
        Used
    }
    
    [Serializable]
    public class Deck
    {
        // 셔플될때 들어가는 전체 카드
        [SerializeField] private List<string> allCards = new List<string>();
        public List<string> AllCards => allCards;
        // 뽑을 카드 더미
        [SerializeField] private List<string> draw = new List<string>();
        public List<string> Draw => draw;
        // 핸드
        [SerializeField] private List<string> hand = new List<string>();
        public List<string> Hand => hand;
        // 사용된 카드 더미
        [SerializeField] private List<string> used = new List<string>();
        public List<string> Used => used;

        public void InitDeck()
        {
            // 파티에서 덱 복사해서 allCards에 넣어주기
        }
        public void ShuffleDeck()
        {
            draw.Clear();
            used.Clear();
            
            // allCards 복사
            draw = new List<string>(allCards);

            // Fisher-Yates Shuffle
            for (var i = draw.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);

                (draw[i], draw[randomIndex]) = (draw[randomIndex], draw[i]);
            }
        }

        public void DrawOne()
        {
            // 덱이 비어있으면 셔플
            if (draw.Count == 0) ShuffleDeck();
            
            var card = draw[0];
            draw.RemoveAt(0);
            hand.Add(card);
        }
        
        public bool DiscardOne(string card)
        {
            if (!hand.Remove(card))
                return false;

            used.Add(card);
            return true;
        }
        
        
        // 실제 버리는 로직
        public void DiscardCards(List<string> cards)
        {
            foreach (var card in cards.Where(card => hand.Remove(card)))
            {
                used.Add(card);
            }
        }
        
        public void AddCard(string card, AddCardPosition position)
        {
            switch (position)
            {
                case AddCardPosition.Draw:
                {
                    draw.Add(card);
                }
                    break;
                case AddCardPosition.Hand:
                {
                    hand.Add(card);
                }
                    break;
                case AddCardPosition.Used:
                {
                    used.Add(card);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
            allCards.Add(card);
        }
    }
}
