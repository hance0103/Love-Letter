using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Card;

namespace GamePlay.Party
{
    [Serializable]
    public class PartyMember
    {
        protected CardBase Data;
        protected List<string> AccList;


        protected PartyMember(CardBase data)
        {
            Data = data;
            AccList = new List<string>();
        }
        
        // 악세서리 추가
        public virtual void AddAcc(string acc)
        {
            if (AccList.Count != 0) return;
            AccList.Add(acc);
        }

        public virtual void RemoveAcc(string acc)
        {
            if (AccList.Contains(acc)) AccList.Remove(acc);
        }
    }
}
