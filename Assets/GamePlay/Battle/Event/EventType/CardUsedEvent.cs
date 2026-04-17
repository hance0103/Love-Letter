using GamePlay.Battle.Card;
using GamePlay.Battle.Field;

namespace GamePlay.Battle.Event.EventType
{
    /// <summary>
    /// 일반카드 사용시/캐릭터 카드 배치시 발생하는 이벤트
    /// </summary>
    public readonly struct CardUsedEvent
    {
        public readonly CardObject Card;
        public readonly FieldSlot TargetSlot;
        
        public CardUsedEvent(CardObject card, FieldSlot targetSlot)
        {
            Card = card;
            TargetSlot = targetSlot;
        }
    }
}