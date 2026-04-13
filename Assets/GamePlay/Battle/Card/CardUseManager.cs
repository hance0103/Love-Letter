using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card.CardHandler;
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
        public static bool HasInstance => Instance != null;
        
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

        private readonly List<RaycastResult> _raycastResults = new();
        private readonly Dictionary<CardType, ICardUseHandler> _handlers = new();

        private InputAction _leftClick;
        private InputAction _rightClick;

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

        public Transform DragLayer => dragLayer;
        public Transform HandLayer => handLayer;
        public Transform FieldCardLayer => fieldCardLayer;
        public CardTargetArrow TargetArrow => targetArrow;

        public CardUseState State => state;
        public CardObject CurrentHover => currentHover;
        public CardObject SelectedCard => selectedCard;
        public FieldSlot SelectedSlot => selectedSlot;

        public Vector3 MouseScreenPos => _mouseScreenPos;
        public Vector3 SelectionStartWorldPos => _selectionStartWorldPos;
        public Vector3 DragVelocity { get => _dragVelocity; set => _dragVelocity = value; }
        public Vector3 DragOffset { get => _dragOffset; set => _dragOffset = value; }
        public float DragFollowSmoothTime => dragFollowSmoothTime;

        public bool IsBusy { get => _isBusy; set => _isBusy = value; }
        public bool IsPointerHeld { get => _isPointerHeld; set => _isPointerHeld = value; }
        public bool HasReleasedSinceSelection { get => _hasReleasedSinceSelection; set => _hasReleasedSinceSelection = value; }
        public int SelectionVersion => _selectionVersion;

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

            _handlers[CardType.Character] = new CharacterCardUseHandler();
            _handlers[CardType.Normal] = new NormalCardUseHandler();
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
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
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

                    if (selectedCard != null && _handlers.TryGetValue(_selectedCardType, out var handler))
                    {
                        handler.UpdateSelection(this, selectedCard);
                    }
                    break;

                case CardUseState.Resolving:
                    break;
            }
        }

        public void RefreshPointerSnapshot()
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

        public CardObject FindTopCard()
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

        public FieldSlot FindTopSlot()
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

                        // 일반카드는 첫 클릭 직후 화살표를 한 번 더 강제로 갱신
                        if (selectedCard != null && _selectedCardType == CardType.Normal && targetArrow != null)
                        {
                            Canvas.ForceUpdateCanvases();

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

        private bool CanResolveCurrentSelection()
        {
            if (selectedCard == null) return false;
            if (!_handlers.TryGetValue(_selectedCardType, out var handler)) return false;

            return handler.CanResolve(this, selectedCard, selectedSlot);
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

                if (_handlers.TryGetValue(_selectedCardType, out var handler))
                {
                    handler.BeginSelection(this, selectedCard);
                }
                else
                {
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

                if (_handlers.TryGetValue(_selectedCardType, out var handler))
                {
                    await handler.Resolve(this, selectedCard, selectedSlot, myVersion);
                }
                else
                {
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

        private async UniTask ReturnSelectedCardToOriginAsync()
        {
            if (selectedCard == null)
            {
                ForceReset();
                return;
            }

            try
            {
                if (_handlers.TryGetValue(_selectedCardType, out var handler))
                {
                    await handler.ReturnToOrigin(this, selectedCard);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
        }

        public async UniTask CancelSelectionAsync()
        {
            _selectionVersion++;

            if (selectedCard == null)
            {
                ForceReset();
                return;
            }

            try
            {
                if (_handlers.TryGetValue(_selectedCardType, out var handler))
                {
                    await handler.ReturnToOrigin(this, selectedCard);
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ForceReset();
            }
        }

        public void ResetSelectionState()
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

            ClearArrow();
        }

        public void ForceReset()
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

            ClearArrow();
        }

        public void SetState(CardUseState newState)
        {
            state = newState;
        }

        public void SetSelectedSlot(FieldSlot slot)
        {
            selectedSlot = slot;
        }

        public void ClearArrow()
        {
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
        public Vector3 SmoothFollow(Vector3 current, Vector3 target)
        {
            return Vector3.SmoothDamp(
                current,
                target,
                ref _dragVelocity,
                dragFollowSmoothTime
            );
        }

        public Transform GetCardPositionTransform(AddCardPosition cardPosition)
        {
            switch (cardPosition)
            {
                case AddCardPosition.Hand: return handLayer;
                case AddCardPosition.Draw:
                case AddCardPosition.Used:
                default:
                {
                    return null;
                }
            }
        }
    }
}