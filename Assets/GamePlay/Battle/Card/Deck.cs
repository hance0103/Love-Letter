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
        Used
    }
    
    [Serializable]
    public class Deck
    {
        // 이번 전투에서 사용될 전체 카드 리스트
        [SerializeField] private List<CardInstance> allCards = new List<CardInstance>();
        public List<CardInstance> AllCards => allCards;
        // 뽑을 카드 더미
        [SerializeField] private List<CardInstance> draw = new List<CardInstance>();
        public List<CardInstance> Draw => draw;
        // 사용된 카드 더미
        [SerializeField] private List<CardInstance> used = new List<CardInstance>();
        public List<CardInstance> Used => used;
        

        
        /// <summary>
        /// 덱 초기화
        /// </summary>
        /// <param name="cards"></param>
        public void InitDeck(List<CardInstance> cards)
        {
            allCards = cards;
            draw = allCards;
            ShuffleDeck();
        }
        
        /// <summary>
        /// 덱 셔플
        /// </summary>
        public void ShuffleDeck()
        {
            var shufflePool = new List<CardInstance>();
            
            shufflePool.AddRange(draw);
            shufflePool.AddRange(used);
            
            draw.Clear();
            used.Clear();
            
            // allCards 복사
            draw = new List<CardInstance>(shufflePool);

            // Fisher-Yates Shuffle
            for (var i = draw.Count - 1; i > 0; i--)
            {
                var randomIndex = Random.Range(0, i + 1);

                (draw[i], draw[randomIndex]) = (draw[randomIndex], draw[i]);
            }
        }
        
        
        /// <summary>
        /// index 만큼 카드 드로우
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<CardInstance> DrawCards(int index)
        {
            var drawnCards = new List<CardInstance>();
            for (var i = 0; i < index; i++)
            {
                drawnCards.Add(DrawOne());
            }
            return drawnCards;
        }
        
        /// <summary>
        /// 카드 한장 드로우
        /// </summary>
        public CardInstance DrawOne()
        {
            // 덱이 비어있으면 셔플
            if (draw.Count == 0) ShuffleDeck();

            if (draw.Count == 0)
            {
                Debug.Log("뽑을 카드가 없다");
                return null;
            }
            
            var card = draw[0];
            draw.RemoveAt(0);
            return card;
        }
        
        /// <summary>
        /// 카드 버리기
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public void DiscardOne(CardInstance card)
        {
            used.Add(card);
        }
        
        
        /// <summary>
        /// 카드 여러장 버리기
        /// </summary>
        /// <param name="cards"></param>
        public void DiscardCards(List<CardInstance> cards)
        {
            foreach (var card in cards)
            {
                used.Add(card);
            }
        }
        
        /// <summary>
        /// 특정 위치에 카드 추가
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AddCard(CardInstance card, AddCardPosition position)
        {
            switch (position)
            {
                case AddCardPosition.Draw:
                {
                    draw.Add(card);
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
    [Serializable]
    public class Hand
    {
        // 핸드
        [SerializeField]
        private ObservableList<CardInstance> cards = new ObservableList<CardInstance>();
        public void Add(CardInstance cardInstance)
        {
            cards.Add(cardInstance);
        }

        public void Remove(CardInstance cardInstance)
        {
            cards.Remove(cardInstance);
        }
        
    }
}
