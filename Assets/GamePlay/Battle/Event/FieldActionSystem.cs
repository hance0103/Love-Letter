using System.Linq;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GamePlay.Battle.Event.EventType;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Event
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
            // 여기서 행동카운트를 얼마나 깎을지 정하기
            var currentCard = evt.Card;
            
            // 일반 카드이면 카드 효과 처리
            if (currentCard.CardInstance.CardType == CardType.Normal)
                ExecuteCardAction(evt);
            
            ReduceAllFieldActionCounts(currentCard, 1);
        }
        private void ReduceAllFieldActionCounts(CardObject currentCard, int amount)
        {
            var cards = BattleManager.Instance.GetAllFieldCards();
            foreach (var card in cards.Where(card => card != currentCard))
            {
                if (card == null || card.CardInstance == null) continue;
                card.CardInstance.DecreaseActionCount(amount);
                card.RefreshCardInfo();
            }
        }

        private void ExecuteCardAction(CardUsedEvent evt)
        {
            var card = evt.Card;
            var slot = evt.TargetSlot;

            if (card == null || slot == null || card.CardInstance == null || card.CardInstance.CardType != CardType.Normal)
                return;
            
            BattleManager.Instance.UseNormalCard(card, slot);
        }
    }
}
