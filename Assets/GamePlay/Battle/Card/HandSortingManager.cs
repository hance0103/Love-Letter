using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GamePlay.Battle.Card
{
    public class HandSortingManager : MonoBehaviour
    {

        public void Init()
        {
            BattleManager.Instance.Hand.Cards.OnChanged += OnHandChanged;
        }
        private void OnDisable()
        {
            BattleManager.Instance.Hand.Cards.OnChanged -= OnHandChanged;
        }

        private void OnHandChanged()
        {
            Debug.Log("OnHandChanged");
        }
    }
}
