using System;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Event;
using GamePlay.Battle.Event.EventType;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Card.CardHandler
{
    public class CharacterCardUseHandler : ICardUseHandler
    {
        public CardType CardType => CardType.Character;

        public void BeginSelection(CardUseManager manager, CardObject card)
        {
            if (manager == null || card == null) return;

            manager.ClearArrow();
            manager.DragVelocity = Vector3.zero;
            manager.DragOffset = Vector3.zero;
            manager.HasReleasedSinceSelection = false;
            
            card.BeginSelection(manager.DragLayer);

            var mouseLocalPos = manager.ScreenToLocalPointInLayer(manager.DragLayer, manager.MouseScreenPos);
            card.SetAnchoredPosition(mouseLocalPos);
            card.RectTransform.rotation = Quaternion.identity;
            if (BattleManager.HasInstance)
            {
                BattleManager.Instance.RequestHandLayoutRefresh();
            }
        }

        public void UpdateSelection(CardUseManager manager, CardObject card)
        {
            if (manager == null || card == null) return;
            if (manager.State != CardUseState.Selected) return;

            card.RectTransform.localRotation = Quaternion.identity;
            
            var targetLocalPos = manager.ScreenToLocalPointInLayer(manager.DragLayer, manager.MouseScreenPos);
            card.SetAnchoredPositionSmooth(targetLocalPos, 20f);
        }

        public bool CanResolve(CardUseManager manager, CardObject card, FieldSlot slot)
        {
            if (card == null || card.CardInstance == null) return false;
            if (slot == null) return false;

            var currentSlot = card.CurrentSlot;

            if (currentSlot == null)
            {
                return slot.CanDrop(card.CardInstance);
            }

            if (slot == currentSlot)
            {
                return true;
            }

            return slot.CanDrop(card.CardInstance);
        }

        public async UniTask Resolve(CardUseManager manager, CardObject card, FieldSlot targetSlot, int selectionVersion)
        {
            if (manager == null || card == null)
            {
                manager?.ForceReset();
                return;
            }

            var currentSlot = card.CurrentSlot;
            if (targetSlot == null)
            {
                await ReturnToOrigin(manager, card);
                return;
            }

            if (!CanResolve(manager, card, targetSlot))
            {
                await ReturnToOrigin(manager, card);
                return;
            }
            try
            {
                if (!BattleManager.HasInstance)
                {
                    await manager.CancelSelectionAsync();
                    return;
                }
                
                // 핸드에서 카드가 나갈 경우
                if (currentSlot == null)
                {
                    
                    var success = BattleManager.Instance.PlaceCharacterCardToField(card, targetSlot);
                    if (!success)
                    {
                        await ReturnToOrigin(manager, card);
                        return;
                    }
                    // 여기서 이벤트 발생시켜주기
                    EventBus.Publish(new CardUsedEvent(card, targetSlot));
                }
                else if (currentSlot != targetSlot)
                {   
                    // 슬롯에서 슬롯으로 움직이는 경우
                    var success = BattleManager.Instance.MoveCharacterCardToFieldSlot(card, currentSlot, targetSlot);
                    if (!success)
                    {
                        await ReturnToOrigin(manager, card);
                        return;
                    }
                }
                
                await card.ReturnToSlotAsync(targetSlot, manager.FieldCardLayer);

                if (selectionVersion != manager.SelectionVersion) return;

                card.EndSelection();
                manager.ResetSelectionState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await manager.CancelSelectionAsync();
            }
            finally
            {
                manager.IsBusy = false;
            }
        }

        public async UniTask ReturnToOrigin(CardUseManager manager, CardObject card)
        {
            if (manager == null || card == null)
            {
                manager?.ForceReset();
                return;
            }
            manager.SetState(CardUseState.Resolving);
            try
            {
                if (card.CurrentSlot != null)
                {
                    await card.ReturnToSlotAsync(card.CurrentSlot, manager.FieldCardLayer);
                }
                else
                {
                    await card.ReturnToHandLayoutAsync();
                }

                card.EndSelection();
                manager.ResetSelectionState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                manager.ForceReset();
            }
            finally
            {
                manager.SetState(CardUseState.None);
                manager.IsBusy = false;
            }
        }

        public void EndSelection(CardUseManager manager, CardObject card)
        {
        }
    }
}