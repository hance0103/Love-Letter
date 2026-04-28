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
        public List<string> cardAbilityIDs = new List<string>();
        public List<string> nextPromotionIDs = new List<string>();
        public string nameString;
        public string descString;
        public string imgPath;
        public List<string> tooltipIDs = new List<string>();
        public string backgroundPath;


        public void InitData()
        {
            cardID = "";
            cardTier = 0;
            cardType = 0;
            keywords.Clear();
            cardNum = 0;
            HP = 0;
            ATK = 0;
            actionCount = 0;
            cardAbilityIDs.Clear();
            nextPromotionIDs.Clear();
            nameString = "";
            descString = "";
            imgPath = "";
            tooltipIDs.Clear();
            backgroundPath = "";
        }
    }
}