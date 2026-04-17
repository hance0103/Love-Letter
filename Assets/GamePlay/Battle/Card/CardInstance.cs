using System;
using GamePlay.Battle.Event;
using GamePlay.Battle.Event.EventType;
using GamePlay.Card;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Card
{
    // 전투에서 실제로 사용 및 변경되고 전투가 끝나면 초기화 되는 영역
    [Serializable]
    public class CardInstance
    {
        [SerializeField] private CardBase data;
        [SerializeField] private int currentCardNum;
        [SerializeField] private int currentHp;
        [SerializeField] private int baseATK;
        [SerializeField] private int increasedATK;
        [SerializeField] private int currentATK;
        [SerializeField] private int currentShield;
        [SerializeField] private int baseActionCount;
        [SerializeField] private int currentActionCount;
        [SerializeField] private string cardDesc;
        [SerializeField] private CardOwner cardOwner;
        public CardBase Data => data;
        public int CurrentCardNum => currentCardNum;
        public int CurrentHp => currentHp;
        public int BaseATK => baseATK;
        public int InCreasedATK => increasedATK;
        public int CurrentATK => currentATK;
        public int CurrentShield => currentShield;
        public int BaseActionCount => baseActionCount;
        public int CurrentActionCount => currentActionCount;
        public string CardDesc => cardDesc;
        public CardType CardType => data != null ? data.cardType : CardType.Normal;
        public CardOwner CardOwner => cardOwner;
        
        public CardInstance(CardBase data, CardOwner cardOwner = CardOwner.Player)
        {
            this.data = data;

            if (data == null) return;

            currentCardNum = data.cardNum;
            currentHp = data.HP;
            baseATK = data.ATK;
            increasedATK = 0;
            currentATK = baseATK + increasedATK;
            currentShield = 0;
            baseActionCount = data.actionCount;
            currentActionCount = baseActionCount;
            cardDesc = data.descString;
            
            this.cardOwner = cardOwner;
        }
            
        public void ChangeCardInstanceValue(CardInstanceValueType valueType, int amount)
        {
            switch (valueType)
            {
                case CardInstanceValueType.C_NUM:
                    currentCardNum += amount;
                    break;

                case CardInstanceValueType.C_HP:
                    currentHp += amount;
                    break;

                case CardInstanceValueType.I_ATK:
                    currentATK += amount;
                    increasedATK += amount;
                    break;

                case CardInstanceValueType.C_SHD:
                    currentShield += amount;
                    break;
            }
        }

        public void DecreaseActionCount(int amount)
        {
            currentActionCount -= amount;
            
            if (currentActionCount <= 0)
            {
                currentActionCount = 0;
                // 캐릭터 카드 행동 이벤트 Publish
                EventBus.Publish(new CardAbilityRequestEvent(this, CardEffectTriggerType.CharacterAutoAction));
            }
        }
    }

    public enum CardInstanceValueType
    {
        C_NUM,
        C_HP,
        C_SHD,
        I_ATK,
        C_AC
    }
}
