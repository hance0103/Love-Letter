using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GameSystem.Managers;
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
        private const string CardPrefabAddress = "CardObject";
        // 셔플될때 들어가는 전체 카드
        [SerializeField] private List<CardInstance> allCards = new List<CardInstance>();
        public List<CardInstance> AllCards => allCards;
        // 뽑을 카드 더미
        [SerializeField] private List<CardInstance> draw = new List<CardInstance>();
        public List<CardInstance> Draw => draw;
        // 핸드
        [SerializeField] private List<CardInstance> hand = new List<CardInstance>();
        public List<CardInstance> Hand => hand;
        // 사용된 카드 더미
        [SerializeField] private List<CardInstance> used = new List<CardInstance>();
        public List<CardInstance> Used => used;

        public void InitDeck(List<CardInstance> cards)
        {
            allCards = cards;
            draw = allCards;
            ShuffleDeck();
        }
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

        public void DrawSixCards()
        {
            for (var i = 0; i < 6; i++)
            {
                _ = DrawOne();
            }
        }
        
        public async UniTaskVoid DrawOne()
        {
            // 덱이 비어있으면 셔플
            if (draw.Count == 0) ShuffleDeck();

            if (draw.Count == 0)
            {
                Debug.Log("뽑을 카드가 없다");
                return;
            }
            
            var card = draw[0];
            draw.RemoveAt(0);
            hand.Add(card);

            var cardObject = await GameManager.Inst.Resource.InstantiateAsync(CardPrefabAddress, CardUseManager.Instance.HandLayer);
            cardObject.GetComponent<CardObject>().Init(card);
        }
        
        public bool DiscardOne(CardInstance card)
        {
            if (!hand.Remove(card))
                return false;

            used.Add(card);
            return true;
        }
        
        
        // 실제 버리는 로직
        public void DiscardCards(List<CardInstance> cards)
        {
            foreach (var card in cards.Where(card => hand.Remove(card)))
            {
                used.Add(card);
            }
        }


        public void DisCardHand()
        {
            foreach (var card in hand.Where(card => hand.Remove(card)))
            {
                used.Add(card);
            }
            ReleaseHandObjects();
        }

        private void ReleaseHandObjects()
        {
            // 핸드 Transform 캐싱
            var handLayer = CardUseManager.Instance.HandLayer;
            // 자식 오브젝트 리스트 생성
            var targets = new List<GameObject>(handLayer.childCount);
            targets.AddRange(from Transform child in handLayer select child.gameObject);
            // 리스트에 있는 자식 오브젝트들 Release
            foreach (var target in targets)
            {
                GameManager.Inst.Resource.ReleaseInstance(target);
            }
        }
        
        public void AddCard(CardInstance card, AddCardPosition position)
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
