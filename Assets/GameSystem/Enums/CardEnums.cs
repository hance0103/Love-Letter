namespace GameSystem.Enums
{
    public enum CardTier
    {
        Common,
        Rare,
        Epic
    }
    public enum CardType
    {
        Character,
        Normal
    }

    public enum ConditionType
    {
        None,
        Join,
    }

    public enum ActionType
    {
        Damage,
        Shield,
        Freeze,
        ReinforceATK,
        
        
        None
    }
    public enum KeywordType
    {
        
    }

    public enum ActionTarget
    {
        Self,
        FrontEnemy,
        RandomEnemy,
        BackEnemy,
        AllAlly,
        RandomAlly,
        FrontAllAlly,
        NearAlly,
        FrontSingleAlly,
        AllEnemy,
        Target
    }
}