using System.Collections.Generic;
using System.Linq;
using GamePlay.Battle.Card;
using UnityEngine;

namespace GamePlay.Party
{
    // 일단은 MonoBehaviour 달아두긴 했는데 얘는 게임매니저 통해서만 접근할거임 나중에 코드 수정함
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