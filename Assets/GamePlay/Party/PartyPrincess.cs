using System;
using System.Collections.Generic;
using GameData.Scripts;
using GamePlay.Card;

namespace GamePlay.Party
{
    [Serializable]
    public class PartyPrincess : PartyMember
    {
        public PartyPrincess(CardBase data) : base(data)
        {
            base.data = data;
            accList = new List<string>();
        }

        public override void AddAcc(string acc)
        {
            accList.Add(acc);
        }

        public override void RemoveAcc(string acc)
        {
            if (accList.Contains(acc)) accList.Remove(acc);
        }
    }
}
