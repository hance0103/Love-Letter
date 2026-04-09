using System;
using System.Collections.Generic;
using GamePlay.Battle.Card;
using GamePlay.Battle.Field;
using GamePlay.Card;
using GamePlay.Turn;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;


        // 나중에 private로 바꿀것
        // 턴 관련 코드 일단 전부 주석처리
        //public TurnManager TurnManager { get; private set; }

        // 마우스 올리거나 드래그하여 하이라이트된 카드
        private GameObject _selectedCard;
        // 클릭을 놓아서 사용이 확정된 카드
        private CardInstance _usingCard;
        
        
        private void Awake()
        {
            Instance = this;
            //TurnManager = new TurnManager();
        }
        
        /// <summary>
        /// 여기는 현재 전투에서 사용하는 덱
        /// deck.AllCards에서는 일시적인 강화 전부 적용되어야함
        /// </summary>
        [SerializeField]
        private Deck playerDeck = new Deck();
        [SerializeField]
        private Deck enemyDeck = new Deck();
        
        [SerializeField]
        private FieldInstance playerField = new FieldInstance();
        [SerializeField]
        private FieldInstance enemyField = new FieldInstance();
        
        [SerializeField]
        private List<CardInstance> enemyWaitList = new List<CardInstance>();
            
        private int _currentMaxCost;
        private int _currentCost;

        private void Start()
        {
            BattleInit();
        }

        private void BattleInit()
        {
            OnBattleStart();
        }
        
        private void OnBattleStart()
        {
            //_currentCost = currentMaxCost;
            // 셔플 연출
            playerDeck.ShuffleDeck();
            enemyDeck.ShuffleDeck();
            
            // // 드로우 연출
            // playerDeck.DrawOne();
            // enemyDeck.DrawOne();
            
            // //턴 설정
            // TurnManager.SetTurnOwner(TurnOwner.Player);
            //
            // OnTurnStart(TurnManager.currentTurnOwner);
        }
        
        private void OnBattleEnd()
        {
            
        }
        
        // private void OnTurnStart(TurnOwner turnOwner)
        // {
        //     switch (turnOwner)
        //     {
        //         case TurnOwner.Player:
        //         {
        //             // _currentCost = currentMaxCost;
        //             playerDeck.DrawOne();
        //             
        //         }
        //             break;
        //         case TurnOwner.Enemy:
        //         {
        //             enemyDeck.DrawOne();
        //         }
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(turnOwner), turnOwner, null);
        //     }
        //
        // }

        // private void PlayEnemyTurn()
        // {
        //     TurnManager.SetTurnOwner(TurnOwner.Enemy);
        //     var enemyUsingCardID = enemyDeck.Hand[0];
        //     
        //     
        //     CardUseManager.UseCard(enemyUsingCardID, TurnOwner.Enemy);
        //     enemyDeck.DiscardOne(enemyUsingCardID);
        //     
        //     OnTurnEnd();
        // }
        //
        // // 플레이어의 턴 종료버튼
        // public void EndTurn()
        // {
        //     OnTurnEnd();
        //     PlayEnemyTurn();
        // }
        // private void OnTurnEnd()
        // {
        //     switch (TurnManager.currentTurnOwner)
        //     {
        //         case TurnOwner.Player:
        //         {
        //             // 플레이어 핸드 버릴 카드 선택 => 팝업 UI
        //             
        //             // 일단 첫번째 카드 버리도록
        //             playerDeck.DiscardOne(playerDeck.Hand[0]);
        //             
        //             
        //             TurnManager.SetTurnOwner(TurnOwner.Enemy);
        //             OnTurnStart(TurnOwner.Enemy);
        //         } break;
        //         case TurnOwner.Enemy:
        //         {
        //             TurnManager.SetTurnOwner(TurnOwner.Player);
        //             OnTurnStart(TurnOwner.Player);
        //         }
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }
    }
}
