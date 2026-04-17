using GamePlay.Battle.Card;

namespace GamePlay.Battle.Event.EventType
{
    public readonly struct CharacterActionEvent
    {
        public readonly CardInstance ActionOwner;

        public CharacterActionEvent(CardInstance actionOwner)
        {
            ActionOwner = actionOwner;
        }
        
    }
}
