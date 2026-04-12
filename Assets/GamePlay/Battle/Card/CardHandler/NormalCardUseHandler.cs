using System;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using UnityEngine;

namespace GamePlay.Battle.Card.CardHandler
{
    public class NormalCardUseHandler : ICardUseHandler
    {
        public CardType CardType => CardType.Normal;

        public void BeginSelection(CardUseManager manager, CardObject card)
        {
            manager.DragVelocity = Vector3.zero;
            manager.DragOffset = Vector3.zero;
            manager.HasReleasedSinceSelection = false;

            card.BeginSelectionForTargeting();
            manager.SetSelectedSlot(manager.FindTopSlot());

            if (manager.TargetArrow != null)
            {
                var startPos = card.GetArrowStartPosition();
                manager.TargetArrow.Show(startPos, manager.MouseScreenPos);
                manager.TargetArrow.UpdateArrow(startPos, manager.MouseScreenPos);
            }
        }

        public void UpdateSelection(CardUseManager manager, CardObject card)
        {
            if (manager.State != CardUseState.Selected) return;
            if (card == null) return;
            if (manager.TargetArrow == null) return;

            manager.TargetArrow.UpdateArrow(
                card.GetArrowStartPosition(),
                manager.MouseScreenPos
            );
        }

        public bool CanResolve(CardUseManager manager, CardObject card, FieldSlot slot)
        {
            if (card == null) return false;
            if (slot == null) return false;

            return slot.CanUseThisNormalCard(card.CardInstance);
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

            if (!slot.CanUseThisNormalCard(card.CardInstance))
            {
                manager.SetState(CardUseState.Selected);
                manager.IsBusy = false;
                return;
            }

            try
            {
                await card.PlayUseAsync();

                if (selectionVersion != manager.SelectionVersion) return;

                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.DiscardCard(card.CardInstance);
                }

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
                await card.ReturnToHandAsync(manager.HandLayer, manager.SelectionStartWorldPos);
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