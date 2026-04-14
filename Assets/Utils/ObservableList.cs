using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public class ObservableList<T>
    {
        [SerializeField] private List<T> list = new();

        public event Action OnChanged;

        public IReadOnlyList<T> Items => list;
        public int Count => list.Count;
        public void Add(T item)
        {
            list.Add(item);
            OnChanged?.Invoke();
        }

        public bool Remove(T item)
        {
            bool removed = list.Remove(item);
            if (removed)
            {
                OnChanged?.Invoke();
            }

            return removed;
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

        public List<T> ToList()
        {
            return new List<T>(list);
        }
    }
}