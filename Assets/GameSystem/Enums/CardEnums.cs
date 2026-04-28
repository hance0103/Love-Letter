namespace GameSystem.Enums
{
    public enum CardTier
    {
        Common,
        Rare,
        Epic,
        None
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
        Heal,
        IncreaseShield,
        IncreaseATK,
        DecreaseATK,
        DecreaseActionCount,
        IncreaseActionCount,
        Burn,
        CreateCardToHand,
        BurnDMG,
        CreateCardToField,
        BurnATK,
        BurnBySelfBurn,
        Bloodrage,
        
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
        Target,
        AllEnemyAlly
    }

    public enum ActualActionTarget
    {
        Front,
        Back,
        All,
        Near,
        Random
    }
}