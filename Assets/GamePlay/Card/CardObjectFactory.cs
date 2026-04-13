using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Card
{
    public static class CardObjectFactory
    {
        private const string CardPrefabAddress = "CardObject";
        public static async UniTask<CardObject> CreateAsync(Transform parent = null)
        {
            var go = await GameManager.Inst.Resource.InstantiateAsync(CardPrefabAddress, parent);

            var cardObject = go.GetComponent<CardObject>();
            if (cardObject == null)
            {
                Debug.LogError("CardObject 없음");
                return null;
            }

            return cardObject;
        }
    }
}
