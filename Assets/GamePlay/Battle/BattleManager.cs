using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using GameSystem.Managers;
using UnityEngine;

namespace GamePlay.Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance;

        [SerializeField] private Deck playerDeck = new();
        [SerializeField] private Hand hand = new();

        private readonly Dictionary<CardInstance, GameObject> _handDict = new();

        [SerializeField] private FieldInstance playerField = new();
        [SerializeField] private FieldInstance enemyField = new();

        [SerializeField] private List<CardInstance> enemyWaitList = new();

        [SerializeField] private int playerFieldSlotCount = 4;
        [SerializeField] private int enemyFieldSlotCount = 4;

        private int _currentMaxCost;
        private int _currentCost;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            BattleInit();
        }

        private void BattleInit()
        {
            try
            {
                var deckSource = GameManager.Inst.Party.CreateDeck();
                playerDeck.InitDeck(deckSource);

                playerField.Init(playerFieldSlotCount);
                enemyField.Init(enemyFieldSlotCount);

                OnBattleStart();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnBattleStart()
        {
            for (var i = 0; i < 6; i++)
            {
                _ = DrawOne();
            }
        }

        public async UniTask DrawOne()
        {
            try
            {
                var card = playerDeck.DrawOne();
                if (card == null) return;

                hand.Add(card);
                await InstantiateCardObjectAsync(card);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void DiscardCard(CardInstance cardInstance)
        {
            if (cardInstance == null) return;

            try
            {
                hand.Remove(cardInstance);
                playerDeck.DiscardOne(cardInstance);
                ReleaseHandObject(cardInstance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public bool PlaceCharacterCardToField(CardObject card, FieldSlot slot)
        {
            if (card == null || slot == null) return false;
            if (card.CardInstance == null) return false;

            try
            {
                FieldInstance targetField = null;

                switch (slot.SlotOwner)
                {
                    case CardOwner.Player:
                        targetField = playerField;
                        break;
                    case CardOwner.Enemy:
                        targetField = enemyField;
                        break;
                    default:
                        return false;
                }

                if (targetField == null) return false;

                var slotIndex = slot.SlotIndex;
                if (!targetField.AddCardToField(card, slotIndex))
                {
                    return false;
                }

                hand.Remove(card.CardInstance);
                _handDict.Remove(card.CardInstance);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public bool RemoveCharacterCardFromField(CardObject card, CardOwner owner)
        {
            if (card == null) return false;

            try
            {
                return owner switch
                {
                    CardOwner.Player => playerField.RemoveCardFromField(card),
                    CardOwner.Enemy => enemyField.RemoveCardFromField(card),
                    _ => false
                };
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private const string CardPrefabAddress = "CardObject";

        private async UniTask InstantiateCardObjectAsync(CardInstance cardInstance, AddCardPosition cardPosition = AddCardPosition.Hand)
        {
            try
            {
                switch (cardPosition)
                {
                    case AddCardPosition.Draw:
                        break;

                    case AddCardPosition.Hand:
                    {
                        if (CardUseManager.Instance == null)
                        {
                            Debug.LogError("BattleManager.InstantiateCardObjectAsync: CardUseManager.Instance가 null입니다.");
                            return;
                        }

                        var cardObject = await GameManager.Inst.Resource.InstantiateAsync(
                            CardPrefabAddress,
                            CardUseManager.Instance.HandLayer
                        );

                        if (cardObject == null)
                        {
                            Debug.LogError("BattleManager.InstantiateCardObjectAsync: 카드 프리팹 생성 실패");
                            return;
                        }

                        var cardComponent = cardObject.GetComponent<CardObject>();
                        if (cardComponent == null)
                        {
                            Debug.LogError("BattleManager.InstantiateCardObjectAsync: CardObject 컴포넌트가 없습니다.");
                            return;
                        }

                        cardComponent.Init(cardInstance);
                        _handDict[cardInstance] = cardObject;
                        break;
                    }

                    case AddCardPosition.Used:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(cardPosition), cardPosition, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ReleaseHandObject(CardInstance cardInstance)
        {
            try
            {
                if (!_handDict.TryGetValue(cardInstance, out var cardObject)) return;

                if (cardObject != null)
                {
                    GameManager.Inst.Resource.ReleaseInstance(cardObject);
                }

                _handDict.Remove(cardInstance);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}