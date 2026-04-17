namespace GamePlay.Battle.Event.EventType
{
    public readonly struct ActionCountEvent
    {
        public readonly bool IsIncrease;
        public readonly int Amount;
        public ActionCountEvent(bool isIncrease, int amount)
        {
            this.IsIncrease = isIncrease;
            this.Amount = amount;
        }
    }
}
