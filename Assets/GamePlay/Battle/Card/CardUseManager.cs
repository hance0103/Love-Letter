using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace GamePlay.Battle.Card
{
    public enum CardUseState
    {
        Idle,
        Selected,
        Resolving
    }

    public class CardUseManager : MonoBehaviour
    {
        public static CardUseManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Transform dragLayer;
        [SerializeField] private Transform handLayer;
        [SerializeField] private Transform fieldCardLayer;
        [SerializeField] private CardTargetArrow targetArrow;

        [Header("Follow")]
        [SerializeField] private float dragFollowSmoothTime = 0.04f;

        [Header("Runtime")]
        [SerializeField] private CardUseState state = CardUseState.Idle;
        [SerializeField] private CardObject currentHover;
        [SerializeField] private CardObject selectedCard;
        [SerializeField] private FieldSlot selectedSlot;

        public Transform DragLayer => dragLayer;
        public Transform HandLayer => handLayer;
        public Transform FieldCardLayer => fieldCardLayer;
        public CardUseState State => state;
        public CardObject CurrentHover => currentHover;
        public CardObject SelectedCard => selectedCard;
        public FieldSlot SelectedSlot => selectedSlot;

        private InputAction _leftClick;
        private InputAction _rightClick;

        private readonly List<RaycastResult> _raycastResults = new();

        private RectTransform _selectedRectTransform;
        private CardType _selectedCardType;

        private Vector3 _mouseScreenPos;
        private Vector3 _selectionStartWorldPos;
        private Vector3 _dragVelocity;
        private Vector3 _dragOffset;

        private bool _isBusy;
        private bool _isPointerHeld;
        private bool _hasReleasedSinceSelection;
        private int _selectionVersion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            if (playerInput == null)
            {
                Debug.LogError("CardUseManager: PlayerInput이 없습니다.");
                return;
            }

            try
            {
                _leftClick = playerInput.actions["LeftClick"];
                _rightClick = playerInput.actions["RightClick"];
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnEnable()
        {
            if (_leftClick != null)
            {
                _leftClick.started += OnLeftClickDown;
                _leftClick.canceled += OnLeftClickUp;
            }

            if (_rightClick != null)
            {
                _rightClick.performed += OnRightClick;
            }
        }

        private void OnDisable()
        {
            if (_leftClick != null)
            {
                _leftClick.started -= OnLeftClickDown;
                _leftClick.canceled -= OnLeftClickUp;
            }

            if (_rightClick != null)
            {
                _rightClick.performed -= OnRightClick;
            }
        }

        private void Update()
        {
            if (_isBusy) return;

            RefreshPointerSnapshot();

            switch (state)
            {
                case CardUseState.Idle:
                    UpdateHover();
                    break;

                case CardUseState.Selected:
                    UpdateSelectedSlot();

                    switch (_selectedCardType)
                    {
                        case CardType.Character:
                            UpdateSelectedCharacterCardFollow();
                            break;

                        case CardType.Normal:
                            UpdateNormalCardTargetArrow();
                            break;
                    }
                    break;

                case CardUseState.Resolving:
                    break;
            }
        }

        private void RefreshPointerSnapshot()
        {
            if (Mouse.current == null) return;
            if (EventSystem.current == null) return;

            _mouseScreenPos = Mouse.current.position.ReadValue();

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = _mouseScreenPos
            };

            _raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, _raycastResults);
        }

        private void UpdateHover()
        {
            try
            {
                var topCard = FindTopCard();

                if (currentHover == topCard) return;

                currentHover?.ForceExit();
                currentHover = topCard;
                currentHover?.ForceEnter();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void UpdateSelectedSlot()
        {
            try
            {
                selectedSlot = FindTopSlot();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void UpdateSelectedCharacterCardFollow()
        {
            if (state != CardUseState.Selected) return;
            if (selectedCard == null) return;
            if (_selectedRectTransform == null) return;

            try
            {
                var targetPos = _mouseScreenPos + _dragOffset;

                _selectedRectTransform.position = Vector3.SmoothDamp(
                    _selectedRectTransform.position,
                    targetPos,
                    ref _dragVelocity,
                    dragFollowSmoothTime
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void UpdateNormalCardTargetArrow()
        {
            if (state != CardUseState.Selected) return;
            if (selectedCard == null) return;
            if (targetArrow == null) return;

            try
            {
                targetArrow.UpdateArrow(
                    selectedCard.GetArrowStartPosition(),
                    _mouseScreenPos
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private CardObject FindTopCard()
        {
            foreach (var hit in _raycastResults)
            {
                if (hit.gameObject == null) continue;

                var card = hit.gameObject.GetComponentInParent<CardObject>();
                if (card != null)
                {
                    return card;
                }
            }

            return null;
        }

        private FieldSlot FindTopSlot()
        {
            foreach (var hit in _raycastResults)
            {
                if (hit.gameObject == null) continue;

                var slot = hit.gameObject.GetComponentInParent<FieldSlot>();
                if (slot != null)
                {
                    return slot;
                }
            }

            return null;
        }

        private bool CanResolveCurrentSelection()
        {
            if (selectedCard == null) return false;
            if (selectedSlot == null) return false;

            return _selectedCardType switch
            {
                CardType.Character => selectedSlot.CanDrop(selectedCard.CardInstance),
                CardType.Normal => selectedSlot.CanUseThisNormalCard(selectedCard.CardInstance),
                _ => false
            };
        }

        private async void OnLeftClickDown(InputAction.CallbackContext context)
        {
            if (_isBusy) return;

            try
            {
                RefreshPointerSnapshot();

                switch (state)
                {
                    case CardUseState.Idle:
                    {
                        if (currentHover == null)
                        {
                            currentHover = FindTopCard();
                        }

                        if (currentHover == null) return;

                        if (dragLayer == null || handLayer == null)
                        {
                            Debug.LogError("CardUseManager: dragLayer 또는 handLayer가 비어 있습니다.");
                            return;
                        }

                        _isPointerHeld = true;
                        _isBusy = true;
                        await BeginSelectionAsync(currentHover);

                        if (selectedCard != null && _selectedCardType == CardType.Normal && targetArrow != null)
                        {
                            var startPos = selectedCard.GetArrowStartPosition();
                            targetArrow.Show(startPos, _mouseScreenPos);
                            targetArrow.UpdateArrow(startPos, _mouseScreenPos);
                        }

                        break;
                    }

                    case CardUseState.Selected:
                    {
                        _isPointerHeld = true;
                        selectedSlot = FindTopSlot();

                        if (selectedCard == null)
                        {
                            ForceReset();
                            return;
                        }

                        if (_hasReleasedSinceSelection)
                        {
                            _isBusy = true;

                            if (CanResolveCurrentSelection())
                            {
                                await ResolveSelectionAsync();
                            }
                            else
                            {
                                await ReturnSelectedCardToOriginAsync();
                            }
                        }

                        break;
                    }

                    case CardUseState.Resolving:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
        }

        private async void OnLeftClickUp(InputAction.CallbackContext context)
        {
            _isPointerHeld = false;

            if (_isBusy) return;
            if (state != CardUseState.Selected) return;
            if (selectedCard == null) return;

            try
            {
                RefreshPointerSnapshot();
                selectedSlot = FindTopSlot();

                if (!_hasReleasedSinceSelection && CanResolveCurrentSelection())
                {
                    _isBusy = true;
                    await ResolveSelectionAsync();
                    return;
                }

                _hasReleasedSinceSelection = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await CancelSelectionAsync();
            }
        }

        private async void OnRightClick(InputAction.CallbackContext context)
        {
            if (_isBusy) return;
            if (state != CardUseState.Selected) return;

            _isBusy = true;

            try
            {
                await CancelSelectionAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
        }

        private async UniTask BeginSelectionAsync(CardObject card)
        {
            if (card == null)
            {
                ForceReset();
                return;
            }

            _selectionVersion++;
            var myVersion = _selectionVersion;

            try
            {
                currentHover?.ForceExit();
                currentHover = null;

                selectedCard = card;
                selectedSlot = null;
                _selectedRectTransform = card.RectTransform;
                _selectedCardType = card.CardInstance.data.cardType;
                _selectionStartWorldPos = card.RectTransform.position;

                state = CardUseState.Selected;

                switch (_selectedCardType)
                {
                    case CardType.Character:
                    {
                        _dragVelocity = Vector3.zero;
                        _dragOffset = Vector3.zero;
                        _hasReleasedSinceSelection = false;

                        selectedCard.BeginSelection(dragLayer);
                        selectedCard.RectTransform.position = _mouseScreenPos;

                        if (targetArrow != null)
                        {
                            targetArrow.Hide();
                        }

                        break;
                    }

                    case CardType.Normal:
                    {
                        _dragVelocity = Vector3.zero;
                        _dragOffset = Vector3.zero;
                        _hasReleasedSinceSelection = true;

                        selectedCard.BeginSelectionForTargeting();
                        selectedSlot = FindTopSlot();

                        if (targetArrow != null)
                        {
                            var startPos = selectedCard.GetArrowStartPosition();
                            targetArrow.Show(startPos, _mouseScreenPos);
                            targetArrow.UpdateArrow(startPos, _mouseScreenPos);
                        }

                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
            finally
            {
                if (myVersion == _selectionVersion)
                {
                    _isBusy = false;
                }
            }

            await UniTask.CompletedTask;
        }

        private async UniTask ResolveSelectionAsync()
        {
            var myVersion = _selectionVersion;

            try
            {
                state = CardUseState.Resolving;

                switch (_selectedCardType)
                {
                    case CardType.Character:
                        await ResolveCharacterCardAsync(myVersion);
                        break;

                    case CardType.Normal:
                        await ResolveNormalCardAsync(myVersion);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await CancelSelectionAsync();
            }
            finally
            {
                if (myVersion == _selectionVersion)
                {
                    _isBusy = false;
                }
            }
        }

        private async UniTask ResolveCharacterCardAsync(int version)
        {
            var card = selectedCard;
            var slot = selectedSlot;

            if (card == null)
            {
                ForceReset();
                return;
            }

            if (slot == null)
            {
                state = CardUseState.Selected;
                _isBusy = false;
                return;
            }

            if (!slot.CanDrop(card.CardInstance))
            {
                state = CardUseState.Selected;
                _isBusy = false;
                return;
            }

            try
            {
                var previousSlot = card.CurrentSlot;
                if (previousSlot != null && previousSlot != slot)
                {
                    previousSlot.ClearSlot();

                    if (BattleManager.Instance != null)
                    {
                        BattleManager.Instance.RemoveCharacterCardFromField(card, previousSlot.SlotOwner);
                    }
                }

                if (BattleManager.Instance == null)
                {
                    await CancelSelectionAsync();
                    return;
                }

                var success = BattleManager.Instance.PlaceCharacterCardToField(card, slot);
                if (!success)
                {
                    await CancelSelectionAsync();
                    return;
                }

                slot.OnDrop(card.CardInstance);
                card.SetCurrentSlot(slot);

                await card.ReturnToSlotAsync(slot, fieldCardLayer);

                if (version != _selectionVersion) return;

                if (card != null)
                {
                    card.EndSelection();
                }

                ResetSelectionState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await CancelSelectionAsync();
            }
        }

        private async UniTask ResolveNormalCardAsync(int version)
        {
            var card = selectedCard;
            var slot = selectedSlot;

            if (card == null)
            {
                ForceReset();
                return;
            }

            if (slot == null)
            {
                state = CardUseState.Selected;
                _isBusy = false;
                return;
            }

            if (!slot.CanUseThisNormalCard(card.CardInstance))
            {
                state = CardUseState.Selected;
                _isBusy = false;
                return;
            }

            try
            {
                await card.PlayUseAsync();

                if (version != _selectionVersion) return;

                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.DiscardCard(card.CardInstance);
                }

                ResetSelectionState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await CancelSelectionAsync();
            }
        }

        private async UniTask ReturnSelectedCardToOriginAsync()
        {
            var card = selectedCard;
            var startPos = _selectionStartWorldPos;

            if (card == null)
            {
                ForceReset();
                return;
            }

            try
            {
                if (card.CurrentSlot != null)
                {
                    await card.ReturnToSlotAsync(card.CurrentSlot, fieldCardLayer);
                }
                else
                {
                    await card.ReturnToHandAsync(handLayer, startPos);
                }

                card.EndSelection();
                ResetSelectionState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private async UniTask CancelSelectionAsync()
        {
            _selectionVersion++;

            var card = selectedCard;
            var startPos = _selectionStartWorldPos;

            if (card == null)
            {
                ForceReset();
                return;
            }

            try
            {
                if (card.CurrentSlot != null)
                {
                    await card.ReturnToSlotAsync(card.CurrentSlot, fieldCardLayer);
                }
                else
                {
                    await card.ReturnToHandAsync(handLayer, startPos);
                }

                card.EndSelection();
                ResetSelectionState();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void ResetSelectionState()
        {
            state = CardUseState.Idle;

            selectedCard = null;
            selectedSlot = null;
            _selectedRectTransform = null;
            _selectedCardType = default;
            _selectionStartWorldPos = Vector3.zero;

            _isPointerHeld = false;
            _dragVelocity = Vector3.zero;
            _dragOffset = Vector3.zero;
            _hasReleasedSinceSelection = false;

            if (targetArrow != null)
            {
                targetArrow.Hide();
            }
        }

        private void ForceReset()
        {
            _selectionVersion++;

            state = CardUseState.Idle;

            currentHover = null;
            selectedCard = null;
            selectedSlot = null;
            _selectedRectTransform = null;
            _selectedCardType = default;
            _selectionStartWorldPos = Vector3.zero;

            _isPointerHeld = false;
            _dragVelocity = Vector3.zero;
            _dragOffset = Vector3.zero;
            _hasReleasedSinceSelection = false;
            _isBusy = false;

            if (targetArrow != null)
            {
                targetArrow.Hide();
            }
        }

        public void ClearHover(CardObject target)
        {
            if (target == null) return;
            if (currentHover != target) return;

            currentHover = null;
        }

        public void NotifyCardDisabled(CardObject target)
        {
            if (target == null) return;

            if (currentHover == target)
            {
                currentHover = null;
            }

            if (selectedCard == target)
            {
                ForceReset();
            }
        }
    }
}