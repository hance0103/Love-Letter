namespace GameSystem.Enums
{
    public enum CardTier
    {
        Common,
        Rare,
        Unique
    }
    public enum CardType
    {
        Character,
        Normal
    }
    public enum CharacterType
    {
        Guard = 1,
        Priestess = 2,
        Baron = 3,
        Maid = 4,
        Prince = 5,
        King = 6,
        Countess = 7,
        Princess = 8,
        MaxNumber
    }

    public enum CurseType
    {
        
    }

    public enum BlessType
    {
        God
    }

    public enum SummonType
    {
        
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
        Enemy,
        Self
    }
}