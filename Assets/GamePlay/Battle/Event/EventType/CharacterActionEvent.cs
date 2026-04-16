using GamePlay.Battle.Card;

namespace GamePlay.Battle.Event.EventType
{
    public readonly struct CharacterActionEvent
    {
        public readonly CardInstance ActionObject;

        public CharacterActionEvent(CardInstance actionObject)
        {
            ActionObject = actionObject;
        }
        
    }
}
