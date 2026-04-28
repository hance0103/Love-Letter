using System.Collections.Generic;
using GameSystem.Enums;
using UnityEngine;

namespace GameData.Scripts
{
    [CreateAssetMenu(fileName = "Card", menuName = "Card/New Card")]
    public class CardBase : ScriptableObject
    {
        public string cardID;
        public CardTier cardTier;
        public CardType cardType ;
        public List<KeywordType> keywords = new List<KeywordType>();
        public int cardNum;
        public int HP;
        public int ATK;
        public int actionCount;
        public List<string> cardAbilityIDs;
        public List<string> nextPromotionIDs;
        public string nameString;
        public string descString;
        public string imgPath;
    }
}