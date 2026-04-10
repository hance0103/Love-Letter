using System;
using GamePlay.Card;
using UnityEngine;

namespace GamePlay.Battle.Card
{
    // 전투에서 실제로 사용 및 변경되고 전투가 끝나면 초기화 되는 영역
    [Serializable]
    public class CardInstance
    {
        public CardBase data;

        public Sprite cardImage;
        public int currentCardNum;
        public int currentHp;
        public int currentAttackPower;
        public int currentShield;
        public int currentActionCount;
        
        public CardInstance(CardBase data)
        {
            this.data = data;
            // 다른 변수들 설정
        }
            
        public void ChangeCardInstance(CardInstanceValueType valueType)
        {
        
        }
        private void GetCardSprite()
        {
            // 데이터 이미지 경로에서 cardImage에 넣어주기
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
