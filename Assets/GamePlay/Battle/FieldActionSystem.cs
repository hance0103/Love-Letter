using System.Linq;
using GamePlay.Battle.Card;
using GamePlay.Battle.Event;
using GamePlay.Battle.Event.EventType;
using UnityEngine;

namespace GamePlay.Battle
{
    public class FieldActionSystem : MonoBehaviour
    {
        public void Init()
        {
            
        }
        private void OnEnable()
        {
            EventBus.Subscribe<CardUsedEvent>(HandleCardUsed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CardUsedEvent>(HandleCardUsed);
        }

        private void HandleCardUsed(CardUsedEvent evt)
        {
            // 여기서 어떤 카드를 얼마나 깎을지 정하기
            var currentCard = evt.Card;
            ReduceAllFieldActionCounts(currentCard);
        }

        private void ReduceAllFieldActionCounts(CardObject currentCard)
        {
            Debug.Log("행카 깎기");
            // 일단은 모든 카드 1 깎기
            var cards = BattleManager.Instance.GetAllFieldCards();
            foreach (var card in cards.Where(card => card != currentCard))
            {
                card.CardInstance.DecreaseActionCount(1);
                card.UpdateActionCount();
            }
        }
    }
}
