

using System;
using GamePlay.Card;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle
{
    public class CardUseManager
    {
        public void UseCard(string cardIndex, TurnOwner turnOwner)
        {
            switch (turnOwner)
            {
                case TurnOwner.Player:
                {
                    Debug.Log($"플레이어 카드 {cardIndex} 사용");
                }
                    break;
                case TurnOwner.Enemy:
                {
                    Debug.Log($"적 카드 {cardIndex} 사용");
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turnOwner), turnOwner, null);
            }
        }
        
        private CardBase FindCardByIndex(string cardIndex)
        {
            // 데이터에서 카드 받아오기
            return new CardBase();
        }


    }
}