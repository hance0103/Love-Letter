using System;
using Cysharp.Threading.Tasks;
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

        public async UniTask Resolve(CardUseManager manager, CardObject card, FieldSlot slot, int selectionVersion)
        {
            if (manager == null || card == null)
            {
                manager?.ForceReset();
                return;
            }

            var currentSlot = card.CurrentSlot;

            if (slot == null)
            {
                await ReturnToOrigin(manager, card);
                return;
            }

            if (!CanResolve(manager, card, slot))
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

                if (currentSlot == null)
                {
                    var success = BattleManager.Instance.PlaceCharacterCardToField(card, slot);
                    if (!success)
                    {
                        await ReturnToOrigin(manager, card);
                        return;
                    }
                }
                else if (currentSlot != slot)
                {
                    var success = BattleManager.Instance.MoveCharacterCardToFieldSlot(card, currentSlot, slot);
                    if (!success)
                    {
                        await ReturnToOrigin(manager, card);
                        return;
                    }
                }

                await card.ReturnToSlotAsync(slot, manager.FieldCardLayer);

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
                manager.IsBusy = false;
            }
        }

        public void EndSelection(CardUseManager manager, CardObject card)
        {
        }
    }
}