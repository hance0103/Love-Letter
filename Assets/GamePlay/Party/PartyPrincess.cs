using System;
using System.Collections.Generic;
using GamePlay.Card;

namespace GamePlay.Party
{
    [Serializable]
    public class PartyPrincess : PartyMember
    {
        public PartyPrincess(CardBase data) : base(data)
        {
            Data = data;
            AccList = new List<string>();
        }

        public override void AddAcc(string acc)
        {
            AccList.Add(acc);
        }

        public override void RemoveAcc(string acc)
        {
            if (AccList.Contains(acc)) AccList.Remove(acc);
        }
    }
}
