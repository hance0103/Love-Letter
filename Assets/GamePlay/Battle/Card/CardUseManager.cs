using System;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Card
{
    public class CardUseManager : MonoBehaviour
    {
        public static CardUseManager Instance;
        [SerializeField] private Transform dragLayer;
        [SerializeField] private Transform handLayer;
        public Transform DragLayer => dragLayer;
        public Transform HandLayer => handLayer;
        private void Awake()
        {
            Instance = this;
        }
        
    }
}