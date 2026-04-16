using GamePlay.Battle.Card;
using GamePlay.Battle.Field;

namespace GamePlay.Battle.Event.EventType
{
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