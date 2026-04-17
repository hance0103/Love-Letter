using GamePlay.Battle.Card;
using GamePlay.Battle.Field;
using GameSystem.Enums;

namespace GamePlay.Battle.Event.EventType
{
    public readonly struct CardAbilityRequestEvent
    {
        public readonly CardInstance Card;
        public readonly CardEffectTriggerType TriggerType;
        public readonly FieldSlot TargetSlot;
        public readonly CardObject SourceCard;

        public CardAbilityRequestEvent(
            CardInstance card,
            CardEffectTriggerType triggerType,
            FieldSlot targetSlot = null,
            CardObject sourceCard = null)
        {
            Card = card;
            TriggerType = triggerType;
            TargetSlot = targetSlot;
            SourceCard = sourceCard;
        }
    }
    
    public readonly struct DamageEvent
    {
        public readonly CardInstance Agent;
        public readonly CardInstance Target;
        public readonly int Damage;

        public DamageEvent(CardInstance agent, CardInstance target, int damage)
        {
            this.Agent = agent;
            Target = target;
            Damage = damage;
        }
    }
    
    public readonly struct HealEvent
    {
        public readonly CardInstance Agent;
        public readonly CardInstance Target;
        public readonly int Amount;

        public HealEvent(CardInstance agent, CardInstance target, int amount)
        {
            this.Agent = agent;
            this.Target = target;
            this.Amount = amount;
        }
    }
    public struct IncreaseShieldEvent
    {
        public readonly CardInstance Agent;
        public readonly CardInstance Target;
        public readonly int Amount;

        public IncreaseShieldEvent(CardInstance agent, CardInstance target, int amount)
        {
            this.Agent = agent;
            this.Target = target;
            this.Amount = amount;
        }
    }
}