using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameData.Scripts;
using GameSystem.Enums;
using Unity.VisualScripting;
using UnityEngine;

namespace GameSystem.Managers
{
    public class DataManager
    {        
        // 여기서 데이터 받아오기
        private DataBase _dataBase;
        private readonly Dictionary<string, CardSpriteSet> _sprites = new();

        private const string CardDataBasePath = "CardDataBase";
        public async UniTask InitAsync()
        {
            // 데이터베이스 가져와서 초기화 시키기
            _dataBase = await GameManager.Inst.Resource.LoadAssetAsync<DataBase>(CardDataBasePath);
            _dataBase.Init();
            await CreateSpriteDictionary();
        }
        public CardBase GetCard(string id)
        {
            return _dataBase.GetCard(id);
        }

        public CardAbilityBase GetAbility(string id)
        {
            return _dataBase.GetAbility(id);
        }
        public Sprite GetCardSprite(string cardID)
        {
            if (_sprites.TryGetValue(cardID, out var sprite))
                return sprite.CardImg;

            Debug.LogWarning($"[CardSpriteProvider] Sprite를 찾지 못했습니다. cardID: {cardID}");
            return null;
        }
        public Sprite GetCardBackgroundSprite(string cardID)
        {
            if (_sprites.TryGetValue(cardID, out var sprite))
                return sprite.BackgroundImg;

            Debug.LogWarning($"[CardSpriteProvider] 배경 Sprite를 찾지 못했습니다. cardID: {cardID}");
            return null;
        }

        public string GetString(string stringID, Language language = Language.Kr)
        {
            var str = _dataBase.GetString(stringID);
            var result = "";
            switch (language)
            {
                case Language.Kr:
                    result += str.kr;
                    break;
                case Language.En:
                    result += str.en;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(language), language, null);
            }
            return result;
        }
        private async UniTask CreateSpriteDictionary()
        {
            _sprites.Clear();
            foreach (var cardData in _dataBase.allCards)
            {
                if (string.IsNullOrWhiteSpace(cardData.imgPath))
                {
                    Debug.LogWarning($"[CardSpriteProvider] imgPath가 비어있습니다. cardID: {cardData.cardID}");
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(cardData.backgroundPath))
                {
                    Debug.LogWarning($"[CardSpriteProvider] backgroundPath가 비어있습니다. cardID: {cardData.cardID}");
                    continue;
                }
                if (_sprites.ContainsKey(cardData.cardID))
                {
                    Debug.LogWarning($"[CardSpriteProvider] 중복 cardID: {cardData.cardID}");
                    continue;
                }
                
                var cardSprite = await GameManager.Inst.Resource.LoadAssetAsync<Sprite>(cardData.imgPath);
                var backgroundSprite = await GameManager.Inst.Resource.LoadAssetAsync<Sprite>(cardData.backgroundPath);
                if (cardSprite == null)
                {
                    Debug.LogWarning($"Sprite not found: {cardData.imgPath}");
                    continue;
                }
                if (backgroundSprite == null)
                {
                    Debug.LogWarning($"Sprite not found: {cardData.backgroundPath}");
                    continue;
                }

                var spriteSet = new CardSpriteSet
                {
                    CardImg = cardSprite,
                    BackgroundImg = backgroundSprite
                };

                _sprites.Add(cardData.cardID, spriteSet);
            }

            foreach (var fateCard in _dataBase.fateCards)
            {
                if (string.IsNullOrWhiteSpace(fateCard.imgPath))
                {
                    Debug.LogWarning($"[CardSpriteProvider] imgPath가 비어있습니다. cardID: {fateCard.cardID}");
                    continue;
                }
                
                if (_sprites.ContainsKey(fateCard.cardID))
                {
                    Debug.LogWarning($"[CardSpriteProvider] 중복 cardID: {fateCard.cardID}");
                    continue;
                }
                
                var cardSprite = await GameManager.Inst.Resource.LoadAssetAsync<Sprite>(fateCard.imgPath);
                var  backgroundSprite = await GameManager.Inst.Resource.LoadAssetAsync<Sprite>(fateCard.backgroundPath);
                if (cardSprite == null)
                {
                    Debug.LogWarning($"Sprite not found: {fateCard.imgPath}");
                    continue;
                }

                if (backgroundSprite == null)
                {
                    Debug.LogWarning($"Sprite not found: {fateCard.backgroundPath}");
                    continue;
                }
                var spriteSet = new CardSpriteSet
                {
                    CardImg = cardSprite,
                    BackgroundImg = backgroundSprite
                };
                
                _sprites.Add(fateCard.cardID, spriteSet);
            }
            
            
        }
        
        public void Release()
        {
            var releasedPaths = new HashSet<string>();

            foreach (var cardData in _dataBase.allCards)
            {
                if (string.IsNullOrWhiteSpace(cardData.imgPath))
                    continue;

                if (!releasedPaths.Add(cardData.imgPath))
                    continue;

                GameManager.Inst.Resource.ReleaseAsset(cardData.imgPath);
            }

            _sprites.Clear();
        }

    }

    public class CardSpriteSet
    {
        public Sprite CardImg;
        public Sprite BackgroundImg;
    }
}
