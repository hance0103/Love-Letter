using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Transform dragLayer;
        [SerializeField] private Transform handLayer;
        [SerializeField] private Transform fieldCardLayer;

        [SerializeField] private CardUseState state = CardUseState.Idle;
        [SerializeField] private CardObject currentHover;
        [SerializeField] private CardObject selectedCard;
        [SerializeField] private FieldSlot selectedSlot;

        public Transform DragLayer => dragLayer;
        public Transform HandLayer => handLayer;
        public Transform FieldCardLayer => fieldCardLayer;

        private InputAction _leftClick;
        private InputAction _rightClick;

        private readonly List<RaycastResult> _raycastResults = new();

        private RectTransform _selectedRectTransform;
        private CardType _selectedCardType;

        private Vector3 _mouseScreenPos;
        private Vector3 _selectionStartWorldPos;

        private bool _canFollowMouse;
        private bool _isBusy;
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

            UpdateRaycastResults();

            switch (state)
            {
                case CardUseState.Idle:
                    UpdateHover();
                    break;
                case CardUseState.Selected:
                    UpdateSelectedSlot();
                    UpdateSelectedCardFollow();
                    break;
                case CardUseState.Resolving:
                    break;
            }
        }

        private async void OnLeftClickDown(InputAction.CallbackContext context)
        {
            if (_isBusy) return;
            if (state != CardUseState.Idle) return;
            if (currentHover == null) return;
            if (dragLayer == null || handLayer == null) return;

            _isBusy = true;

            try
            {
                await BeginSelectionAsync(currentHover);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
        }

        private async void OnLeftClickUp(InputAction.CallbackContext context)
        {
            if (_isBusy) return;
            if (state != CardUseState.Selected) return;
            if (selectedCard == null) return;

            _isBusy = true;

            try
            {
                await ResolveSelectionAsync();
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

        private void UpdateRaycastResults()
        {
            if (Mouse.current == null) return;
            if (EventSystem.current == null) return;

            try
            {
                _mouseScreenPos = Mouse.current.position.ReadValue();

                var eventData = new PointerEventData(EventSystem.current)
                {
                    position = _mouseScreenPos
                };

                _raycastResults.Clear();
                EventSystem.current.RaycastAll(eventData, _raycastResults);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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
        [Header("카드 이동")]
        [SerializeField] private float dragFollowSmoothTime = 0.04f;
        private Vector3 _dragVelocity;
        private void UpdateSelectedCardFollow()
        {
            if (!_canFollowMouse) return;
            if (selectedCard == null) return;
            if (_selectedRectTransform == null) return;

            try
            {
                var currentPos = _selectedRectTransform.position;
                var targetPos = _mouseScreenPos;

                _selectedRectTransform.position = Vector3.SmoothDamp(
                    currentPos,
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

        private UniTask BeginSelectionAsync(CardObject card)
        {
            if (card == null)
            {
                ForceReset();
                return UniTask.CompletedTask;
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
                _canFollowMouse = true;

                selectedCard.BeginSelection(dragLayer);
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

            return UniTask.CompletedTask;
        }

        private async UniTask ResolveSelectionAsync()
        {
            var myVersion = _selectionVersion;

            try
            {
                _canFollowMouse = false;
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
                await CancelSelectionAsync();
                return;
            }

            if (!slot.CanDrop(card.CardInstance))
            {
                await CancelSelectionAsync();
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
                await CancelSelectionAsync();
                return;
            }

            if (!slot.CanUseThisNormalCard(card.CardInstance))
            {
                await CancelSelectionAsync();
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

                if (card != null)
                {
                    card.EndSelection();
                }

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

            _canFollowMouse = false;
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

            _canFollowMouse = false;
            _isBusy = false;
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
                _selectionVersion++;
                selectedCard = null;
                selectedSlot = null;
                _selectedRectTransform = null;
                _selectedCardType = default;
                _selectionStartWorldPos = Vector3.zero;
                _canFollowMouse = false;
                _isBusy = false;
                state = CardUseState.Idle;
            }
        }
    }
}