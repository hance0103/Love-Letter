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
            if (manager == null || card == null) return;

            manager.DragVelocity = Vector3.zero;
            manager.DragOffset = Vector3.zero;
            manager.HasReleasedSinceSelection = false;

            card.BeginSelectionForTargeting();
            
            card.MoveToFocus();

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
            manager.SetSelectedSlot(manager.FindTopSlot());
            
            if (manager.State != CardUseState.Selected) return;
            
            if (manager.TargetArrow != null)
            {
                var startPos = card.GetArrowStartPosition();
                if (!manager.TargetArrow.gameObject.activeSelf)
                    manager.TargetArrow.Show(startPos, manager.MouseScreenPos);
                else
                    manager.TargetArrow.UpdateArrow(startPos, manager.MouseScreenPos);
            }
        }

        public bool CanResolve(CardUseManager manager, CardObject card, FieldSlot slot)
        {
            if (card == null || card.CardInstance == null) return false;
            if (slot == null) return false;

            return slot.CanUseThisNormalCard(card.CardInstance);
        }

        public async UniTask Resolve(CardUseManager manager, CardObject card, FieldSlot targetSlot, int selectionVersion)
        {
            if (manager == null || card == null)
            {
                manager?.ForceReset();
                return;
            }
            manager?.ClearArrow();
            manager?.SetState(CardUseState.Using);
            if (targetSlot == null || !targetSlot.CanUseThisNormalCard(card.CardInstance))
            {
                await ReturnToOrigin(manager, card);
                return;
            }

            try
            {
                await card.PlayUseAsync();

                if (selectionVersion != manager.SelectionVersion) return;

                manager.ResetSelectionState();

                if (BattleManager.HasInstance)
                {
                    BattleManager.Instance.UseNormalCard(card, targetSlot);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await manager.CancelSelectionAsync();
            }
            finally
            {
                manager.SetState(CardUseState.None);
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
            manager?.ClearArrow();
            try
            {
                await card.ReturnToHandLayoutAsync();
                card.EndSelection();
                manager.ResetSelectionState();

                if (BattleManager.HasInstance)
                {
                    BattleManager.Instance.RequestHandLayoutRefresh();
                }
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