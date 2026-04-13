using System;
using System.Collections.Generic;
using GamePlay.Card;
using UnityEngine;

namespace GameData.SO.Scripts
{
    [Serializable]
    [CreateAssetMenu(fileName = "CardDataBase", menuName = "Card/CardDataBase")]
    public class CardDataBase : ScriptableObject
    {
        public List<CardBase> allCards = new();
        public List<CardAbilityBase> allAbilities = new();
        
        private Dictionary<string, CardBase> _cardDataDict;
        private Dictionary<string, CardAbilityBase> _abilityDataDict;
        public void Init()
        {
            // 딕셔너리 구축
            _cardDataDict = new Dictionary<string, CardBase>();
            _abilityDataDict = new Dictionary<string, CardAbilityBase>();
            
            foreach (var card in allCards)
            {
                _cardDataDict[card.cardID] = card;
            }

            foreach (var ability in allAbilities)
            {
                _abilityDataDict[ability.abilityID] = ability;
            }
        }

        public CardBase GetCard(string cardID)
        {
            return _cardDataDict.GetValueOrDefault(cardID);
        }

        public CardAbilityBase GetAbility(string abilityID)
        {
            return _abilityDataDict.GetValueOrDefault(abilityID);
        }
    }
}
