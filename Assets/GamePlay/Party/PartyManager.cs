using System.Collections.Generic;
using GamePlay.Battle.Card;
using UnityEngine;

namespace GamePlay.Party
{
    public class PartyManager : MonoBehaviour
    {
        [SerializeField] private List<PartyMember> _partyMembers = new List<PartyMember>();
        // 보유 유물 리스트
        
        
        // 게임 시작시 파티 초기화
        public void InitParty()
        {
            // 파티에 공주만 추가
        }
        
        
        
        public List<CardInstance> CreateDeck()
        {
            var deck = new List<CardInstance>();
            
            
            
            return deck;
        }
        
    }
}