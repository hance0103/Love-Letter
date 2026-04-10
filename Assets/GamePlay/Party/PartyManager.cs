using System.Collections.Generic;
using System.Linq;
using GamePlay.Battle.Card;
using UnityEngine;

namespace GamePlay.Party
{
    public class PartyManager : MonoBehaviour
    {
        [SerializeField] private List<PartyMember> partyMembers = new List<PartyMember>();
        // 보유 유물 리스트
        
        
        // 게임 시작시 파티 초기화
        public void InitParty()
        {
            
        }
        
        public List<CardInstance> CreateDeck()
        {
            var deck = new List<CardInstance>();
            foreach (var instance in partyMembers.Select(member => new CardInstance(member.GetData())))
            {
                // 유물 설정 및 기타 버프/디버프 설정도 해줘야함
                deck.Add(instance);
            }

            return deck;
        }
        
    }
}