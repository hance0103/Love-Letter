using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Card
{
    public class CardPool : MonoBehaviour
    {
        private Queue<CardObject> _pool = new();
        private HashSet<CardObject> _active = new();

        public async UniTask<CardObject> Get(CardInstance data)
        {
            CardObject obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj = await CardObjectFactory.CreateAsync(transform);
            }
            obj.Init(data);
            _active.Add(obj);
            return obj;
        }

        public void Release(CardObject obj)
        {
            obj.ResetObject();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(transform);
            
            _active.Remove(obj);
            _pool.Enqueue(obj);
        }

        private void OnDestroy()
        {
            foreach (var obj in _active)
            {
                GameManager.Inst.Resource.ReleaseInstance(obj.gameObject);
            }

            foreach (var obj in _pool)
            {
                GameManager.Inst.Resource.ReleaseInstance(obj.gameObject);
            }
        }
    }
}