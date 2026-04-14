using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GamePlay.Battle.Card
{
    public class CardPool : MonoBehaviour
    {
        private readonly Queue<CardObject> _pool = new();
        private readonly HashSet<CardObject> _active = new();

        [SerializeField] private CardObject cardPrefab;

        public async UniTask<CardObject> Get(CardInstance data)
        {
            await UniTask.Yield();

            CardObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj = Instantiate(cardPrefab, transform);
            }

            obj.Init(data);
            _active.Add(obj);
            return obj;
        }

        public void Release(CardObject obj)
        {
            if (obj == null) return;

            obj.ResetObject();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform, false);

            _active.Remove(obj);
            _pool.Enqueue(obj);
        }
    }
}