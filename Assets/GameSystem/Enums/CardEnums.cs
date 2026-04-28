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
        Normal,
        None
    }

    public enum ConditionType
    {
        DualWin,
        Death,
        
        None,
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
        IncreaseNum,
        DecreaseNum,
        BurnDouble,
        
        None
    }
    public enum KeywordType
    {
        
        
        None
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
        AllEnemyAlly,
        Attacker,
        
        None
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