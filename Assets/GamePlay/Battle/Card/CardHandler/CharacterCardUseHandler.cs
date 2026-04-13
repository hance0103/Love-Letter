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
            manager.ClearArrow();

            manager.DragVelocity = Vector3.zero;
            manager.DragOffset = Vector3.zero;
            manager.HasReleasedSinceSelection = false;

            card.BeginSelection(manager.DragLayer);
            card.RectTransform.position = manager.MouseScreenPos;
        }

        public void UpdateSelection(CardUseManager manager, CardObject card)
        {
            if (manager.State != CardUseState.Selected) return;
            if (card == null) return;
            if (card.RectTransform == null) return;

            var targetPos = manager.MouseScreenPos + manager.DragOffset;

            card.RectTransform.position = manager.SmoothFollow(
                card.RectTransform.position,
                targetPos
            );
        }

        public bool CanResolve(CardUseManager manager, CardObject card, FieldSlot slot)
        {
            if (card == null) return false;
            if (slot == null) return false;

            return slot.CanDrop(card.CardInstance);
        }

        public async UniTask Resolve(CardUseManager manager, CardObject card, FieldSlot slot, int selectionVersion)
        {
            if (card == null)
            {
                manager.ForceReset();
                return;
            }

            if (slot == null)
            {
                manager.SetState(CardUseState.Selected);
                manager.IsBusy = false;
                return;
            }

            if (!slot.CanDrop(card.CardInstance))
            {
                manager.SetState(CardUseState.Selected);
                manager.IsBusy = false;
                return;
            }

            try
            {
                var previousSlot = card.CurrentSlot;
                if (previousSlot != null && previousSlot != slot)
                {
                    previousSlot.ClearSlot();

                    if (BattleManager.HasInstance)
                    {
                        BattleManager.Instance.RemoveCharacterCardFromField(card, previousSlot.SlotOwner);
                    }
                }

                if (!BattleManager.HasInstance)
                {
                    await manager.CancelSelectionAsync();
                    return;
                }

                var success = BattleManager.Instance.PlaceCharacterCardToField(card, slot);
                if (!success)
                {
                    await manager.CancelSelectionAsync();
                    return;
                }

                slot.OnDrop(card.CardInstance);
                card.SetCurrentSlot(slot);

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
        }

        public async UniTask ReturnToOrigin(CardUseManager manager, CardObject card)
        {
            if (card == null)
            {
                manager.ForceReset();
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
                    await card.ReturnToHandAsync(manager.HandLayer, manager.SelectionStartWorldPos);
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
            manager.ClearArrow();
        }
    }
}