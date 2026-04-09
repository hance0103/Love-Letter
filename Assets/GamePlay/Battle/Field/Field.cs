using System;
using GamePlay.Battle.Card;
using UnityEngine;

namespace GamePlay.Battle.Field
{
    [Serializable]
    public class FieldInstance

    {
    [SerializeField] public CardInstance card1;
    [SerializeField] public CardInstance card2;
    [SerializeField] public CardInstance card3;
    [SerializeField] public CardInstance card4;
    }

    public interface IFieldSlot
    {
        bool CanDrop(CardInstance card);
        void OnDrop(CardInstance card);
        void ClearSlot();
        Transform GetTransform();
    }
}
