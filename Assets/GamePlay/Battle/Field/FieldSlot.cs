using System;
using GamePlay.Battle.Card;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Field
{
    public class FieldSlot : MonoBehaviour, IFieldSlot
    {
        [SerializeField]
        private CardInstance cardInstance;

        private bool _isCardIn;
        public CardOwner slotOwner;
        
        private void Awake()
        {
            _isCardIn = false;
        }

        public bool IsEmptySlot()
        {
            return !_isCardIn;
        }
        
        // 이 일반카드가 사용 가능한 카드인지
        public bool CanUseThisNormalCard(CardInstance cardInstance)
        {
            return _isCardIn;
        }
        
        // 사용 여부와 관계 없이 해당 슬롯에 캐릭터 카드를 올릴수 있는지만 판단
        public bool CanDrop(CardInstance card)
        {
            if (slotOwner != CardOwner.Player) return false;
            
            if (_isCardIn)
            {
                // 이미 카드가 올려진 슬롯에 올리는 경우
                // 일단은 false 리턴
            }
            else
            {
                // 비어있는 슬롯
                return true;
            }
            return false;
        }

        public void OnDrop(CardInstance card)
        {
            cardInstance = card;
            _isCardIn = true;
        }

        public void ClearSlot()
        {
            cardInstance = null;
            _isCardIn = false;
        }

        public Transform GetTransform()
        {
            return transform;
        }
    }
}
