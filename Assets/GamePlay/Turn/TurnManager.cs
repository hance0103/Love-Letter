using System;
using GameSystem.Enums;
using UnityEngine;
namespace GamePlay.Turn
{
    [Serializable]
    public class TurnManager
    {
        [SerializeField]
        public TurnOwner currentTurnOwner;

        public void SetTurnOwner(TurnOwner turnOwner)
        {
            
            Debug.Log($"{turnOwner.ToString()} 턴");
            currentTurnOwner = turnOwner;
        }

        private void ChangeTurn()
        {
            currentTurnOwner = 
                (currentTurnOwner == TurnOwner.Player) ? TurnOwner.Enemy : TurnOwner.Player;
        }
    }

}