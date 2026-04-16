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
        [SerializeField] private int currentATK;
        [SerializeField] private int increasedAtk;
        [SerializeField] private int currentShield;
        [SerializeField] private int baseActionCount;
        [SerializeField] private int currentActionCount;
        [SerializeField] private string cardDesc;

        public CardBase Data => data;
        public int CurrentCardNum => currentCardNum;
        public int CurrentHp => currentHp;
        public int CurrentATK => currentATK;
        public int InCreasedAtk => increasedAtk;
        public int CurrentShield => currentShield;
        public int BaseActionCount => baseActionCount;
        public int CurrentActionCount => currentActionCount;
        public string CardDesc => cardDesc;
        public CardType CardType => data != null ? data.cardType : CardType.Normal;
        
        public CardInstance(CardBase data)
        {
            this.data = data;

            if (data == null) return;

            currentCardNum = data.cardNum;
            currentHp = data.HP;
            currentATK = data.ATK;
            increasedAtk = -1;
            currentShield = 0;
            baseActionCount = data.actionCount;
            currentActionCount = baseActionCount;
            cardDesc = data.descString;
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

                case CardInstanceValueType.C_ATK:
                    currentATK += amount;
                    break;

                case CardInstanceValueType.C_SHD:
                    currentShield += amount;
                    break;

                case CardInstanceValueType.C_AC:
                    currentActionCount += amount;
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
                EventBus.Publish(new CharacterActionEvent(this));
            }
        }
    }

    public enum CardInstanceValueType
    {
        C_NUM,
        C_HP,
        C_ATK,
        C_SHD,
        C_AC
    }
}
