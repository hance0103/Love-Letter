namespace GameSystem.Enums
{
    public enum CardOwner
    {
        Player,
        Enemy,
    }
    public enum CardEffectTriggerType
    {
        NormalCardUse,
        CharacterAutoAction,
        OnSummon,
        OnDeath,
        OnTurnStart,
        OnTurnEnd
    }
}