using System;
using System.Collections.Generic;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Card
{
    [CreateAssetMenu(fileName = "Card", menuName = "Card/New Card")]
    public class CardBase : ScriptableObject
    {
        public string cardID;
        public CardTier cardTier;
        public CardType cardType ;
        public int cardNum;
        public List<KeywordType> keywords;
        public int cost;
        public List<string> cardAbilityIDs;
        public List<string> nextPromotionIDs;
        public string nameString;
        public string descString;
        public string imgPath;


    }


}