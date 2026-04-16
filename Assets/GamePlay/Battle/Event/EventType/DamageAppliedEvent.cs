using GamePlay.Battle.Card;

namespace GamePlay.Battle.Event.EventType
{
    public readonly struct DamageAppliedEvent
    {
        public readonly CardInstance Attacker;
        public readonly CardInstance Target;
        public readonly int Damage;

        public DamageAppliedEvent(CardInstance attacker, CardInstance target, int damage)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
        }
    }
}