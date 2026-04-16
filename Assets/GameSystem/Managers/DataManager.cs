using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameData.SO.Scripts;
using GamePlay.Card;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameSystem.Managers
{
    public class DataManager
    {        
        // 여기서 데이터 받아오기
        private CardDataBase _dataBase;
        private readonly Dictionary<string, Sprite> _sprites = new();

        private const string CardDataBasePath = "CardDataBase";
        public async UniTask InitAsync()
        {
            // 데이터베이스 가져와서 초기화 시키기
            _dataBase = await GameManager.Inst.Resource.LoadAssetAsync<CardDataBase>(CardDataBasePath);
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
        public Sprite GetSprite(string cardID)
        {
            if (_sprites.TryGetValue(cardID, out var sprite))
                return sprite;

            Debug.LogWarning($"[CardSpriteProvider] Sprite를 찾지 못했습니다. cardID: {cardID}");
            return null;
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
                
                if (_sprites.ContainsKey(cardData.cardID))
                {
                    Debug.LogWarning($"[CardSpriteProvider] 중복 cardID: {cardData.cardID}");
                    continue;
                }
                
                var sprite = await GameManager.Inst.Resource.LoadAssetAsync<Sprite>(cardData.imgPath);
                if (sprite == null)
                {
                    Debug.LogWarning($"Sprite not found: {cardData.imgPath}");
                    continue;
                }
                _sprites.Add(cardData.cardID, sprite);
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
}
