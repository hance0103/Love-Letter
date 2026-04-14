using System;
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
        [SerializeField] private int currentShield;
        [SerializeField] private int currentActionCount;
        [SerializeField] private string cardDesc;

        public CardBase Data => data;
        public int CurrentCardNum => currentCardNum;
        public int CurrentHp => currentHp;
        public int CurrentATK => currentATK;
        public int CurrentShield => currentShield;
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
            currentShield = 0;
            currentActionCount = data.actionCount;
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
