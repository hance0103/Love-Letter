using System;
using System.Collections.Generic;
using System.Linq;
using GameData.Scripts;
using GamePlay.Card;
using UnityEngine;

namespace GamePlay.Party
{
    [Serializable]
    public class PartyMember
    {
        [SerializeField]
        protected CardBase data;
        [SerializeField]
        protected List<string> accList;

        public CardBase GetData()
        {
            return data;
        }
        protected PartyMember(CardBase data)
        {
            this.data = data;
            accList = new List<string>();
        }
        
        // 악세서리 추가
        public virtual void AddAcc(string acc)
        {
            if (accList.Count != 0) return;
            accList.Add(acc);
        }

        public virtual void RemoveAcc(string acc)
        {
            if (accList.Contains(acc)) accList.Remove(acc);
        }
    }
}
