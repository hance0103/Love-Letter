using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 핸드 정리용 옵저버리스트
    /// 핸드리스트에 변동이 발생하면 이벤트를 발생시켜 바로 정렬할수 있게 해준다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ObservableList<T>
    {
        [SerializeField] private List<T> list = new List<T>();
        
        public event Action OnChanged;

        public void Add(T item)
        {
            list.Add(item);
            OnChanged?.Invoke();
        }
        
        public void Remove(T item)
        {
            if (list.Remove(item))
                OnChanged?.Invoke();
        }

        public void Clear()
        {
            list.Clear();
            OnChanged?.Invoke();
        }
        public bool Contains(T item)
        {
            return list.Contains(item);
        }
        public IReadOnlyList<T> Items => list;
    }
}
