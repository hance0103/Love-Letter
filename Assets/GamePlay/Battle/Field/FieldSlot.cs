using GamePlay.Battle.Card;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Field
{
    public class FieldSlot : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private CardOwner slotOwner;
        [SerializeField] private CardInstance cardInstance;
        [SerializeField] private bool isCardIn;

        public int SlotIndex => slotIndex;
        public CardOwner SlotOwner => slotOwner;
        public CardInstance CardInstance => cardInstance;
        public bool IsOccupied => isCardIn;

        public bool IsEmptySlot()
        {
            return !isCardIn;
        }

        public bool CanUseThisNormalCard(CardInstance card)
        {
            if (card == null) return false;
            return isCardIn;
        }

        public bool CanDrop(CardInstance card)
        {
            if (card == null) return false;
            if (slotOwner != CardOwner.Player) return false;
            if (isCardIn) return false;

            return true;
        }

        public void OnDrop(CardInstance card)
        {
            cardInstance = card;
            isCardIn = card != null;
        }

        public void ClearSlot()
        {
            cardInstance = null;
            isCardIn = false;
        }

        public RectTransform RectTransform => transform as RectTransform;
    }
}