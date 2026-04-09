using System;
using GamePlay.Battle.Card;
using UnityEngine;

namespace GamePlay.Battle.Field
{
    public class FieldSlot : MonoBehaviour, IFieldSlot
    {
        [SerializeField]
        private CardInstance cardInstance;

        [SerializeField] private bool isCardIn;
        private void Awake()
        {
            isCardIn = false;
        }

        public bool CanDrop(CardInstance card)
        {
            return !isCardIn;
        }

        public void OnDrop(CardInstance card)
        {
            cardInstance = card;
            isCardIn = true;
        }

        public void ClearSlot()
        {
            cardInstance = null;
            isCardIn = false;
        }

        public Transform GetTransform()
        {
            return transform;
        }
    }
}
