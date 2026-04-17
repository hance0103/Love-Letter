using System;
using Cysharp.Threading.Tasks;
using GameData.SO.Scripts;
using UnityEngine;

namespace GamePlay.Battle.Card
{
    [Serializable]
    public class CardAbility
    {
        [SerializeField] private CardAbilityBase abilityData;
        [SerializeField] private CardInstance abilityOwner;

        public async UniTask UseCardAbility()
        {
            
        }
    }
}
