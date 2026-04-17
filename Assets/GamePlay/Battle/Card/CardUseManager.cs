using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay.Battle.Card.CardHandler;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GamePlay.Battle.Card
{
    public enum CardUseState
    {
        None,
        Selected,
        Using,
        Resolving
    }
    public class CardUseManager : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GraphicRaycaster uiRaycaster;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private RectTransform handLayer;
        [SerializeField] private RectTransform dragLayer;
        [SerializeField] private RectTransform fieldCardLayer;
        

        [Header("Arrow")]
        [SerializeField] private CardTargetArrow targetArrow;

        private readonly List<RaycastResult> _raycastResults = new();
        private PointerEventData _pointerEventData;

        private readonly CharacterCardUseHandler _characterHandler = new CharacterCardUseHandler();
        private readonly NormalCardUseHandler _normalHandler = new NormalCardUseHandler();
        
        private bool _prevLeftPressed;
        private bool _prevRightPressed;

        public static CardUseManager Instance { get; private set; }
        public static bool HasInstance => Instance != null;

        public CardUseState State { get; private set; } = CardUseState.None;

        public CardObject HoveredCard { get; private set; }
        public CardObject SelectedCard { get; private set; }
        public FieldSlot SelectedSlot { get; private set; }

        public Vector2 MouseScreenPos { get; private set; }
        public bool IsPointerHeld { get; private set; }
        public bool HasReleasedSinceSelection { get; set; }
        public bool IsBusy { get; set; }

        public Vector3 DragVelocity { get; set; }
        public Vector3 DragOffset { get; set; }

        public int SelectionVersion { get; private set; }

        public RectTransform HandLayer => handLayer;
        public RectTransform DragLayer => dragLayer;
        public RectTransform FieldCardLayer => fieldCardLayer;
        public CardTargetArrow TargetArrow => targetArrow;

        public Vector2 HandCenterAnchoredPosition
        {
            get
            {
                if (BattleManager.HasInstance && BattleManager.Instance.HandSortingManager != null)
                {
                    return BattleManager.Instance.HandSortingManager.HandCenterAnchoredPosition;
                }

                return Vector2.zero;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (eventSystem != null)
            {
                _pointerEventData = new PointerEventData(eventSystem);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Mouse.current == null) return;
            if (uiRaycaster == null || eventSystem == null) return;

            UpdatePointerState();
            RaycastUI();

            if (SelectedCard == null)
            {
                UpdateHover();
            }
            else
            {
                UpdateSelectedCard();
            }

            HandleInput();
        }

        private void UpdatePointerState()
        {
            if (Mouse.current == null) return;
            MouseScreenPos = Mouse.current.position.ReadValue();
        }

        private void RaycastUI()
        {
            _raycastResults.Clear();

            _pointerEventData.position = MouseScreenPos;
            uiRaycaster.Raycast(_pointerEventData, _raycastResults);
        }

        private void UpdateHover()
        {
            var nextHovered = FindTopCard();

            if (HoveredCard == nextHovered) return;

            if (HoveredCard != null)
            {
                HoveredCard.ForceExit();
            }

            HoveredCard = nextHovered;

            if (HoveredCard != null)
            {
                HoveredCard.ForceEnter();
            }
        }

        private void UpdateSelectedCard()
        {
            if (IsBusy) return;
            if (SelectedCard == null || SelectedCard.CardInstance == null) return;

            var handler = GetHandler(SelectedCard.CardInstance.CardType);
            handler?.UpdateSelection(this, SelectedCard);
        }

        private void HandleInput()
        {
            bool leftPressed = Mouse.current.leftButton.isPressed;
            bool rightPressed = Mouse.current.rightButton.isPressed;

            bool leftDown = leftPressed && !_prevLeftPressed;
            bool leftUp = !leftPressed && _prevLeftPressed;
            bool rightDown = rightPressed && !_prevRightPressed;

            if (leftDown)
            {
                OnLeftDown();
            }

            if (leftUp)
            {
                OnLeftUp();
            }

            if (rightDown)
            {
                OnRightDown();
            }

            _prevLeftPressed = leftPressed;
            _prevRightPressed = rightPressed;
        }

        private void OnLeftDown()
        {
            // 선택 중인 경우
            if (State == CardUseState.Selected)
            {
                IsPointerHeld = false;
                HasReleasedSinceSelection = true;
                if (IsBusy) return;
                ResolveSelectionAsync().Forget();
            }
            
            IsPointerHeld = true;
            HasReleasedSinceSelection = false;

            if (IsBusy) return;

            if (SelectedCard == null)
            {
                var clickedCard = FindTopCard();
                if (clickedCard == null) return;

                SelectCard(clickedCard);
            }
        }

        private void OnLeftUp()
        {
            // 선택중이 아니라면
            if (State != CardUseState.Selected) return;
            
            var slot = FindTopSlot();
            if (slot == null) return;
            if (slot == SelectedCard.CurrentSlot) return;
            
            IsPointerHeld = false;
            HasReleasedSinceSelection = true;

            if (IsBusy) return;
            if (SelectedCard == null) return;

            ResolveSelectionAsync().Forget();
        }

        private void OnRightDown()
        {
            if (IsBusy) return;
            if (SelectedCard == null) return;

            CancelSelectionAsync().Forget();
        }

        private void SelectCard(CardObject card)
        {
            if (card == null || card.CardInstance == null) return;
            
            HoveredCard = null;

            SelectedCard = card;
            SelectedSlot = null;
            SelectionVersion++;

            SetState(CardUseState.Selected);
            
            var handler = GetHandler(card.CardInstance.CardType);
            handler?.BeginSelection(this, card);
            
        }

        private async UniTaskVoid ResolveSelectionAsync()
        {
            if (SelectedCard == null || SelectedCard.CardInstance == null) return;

            IsBusy = true;

            var card = SelectedCard;
            var slot = FindTopSlot();
            SetSelectedSlot(slot);

            var handler = GetHandler(card.CardInstance.CardType);
            
            if (handler == null)
            {
                ForceReset();
                return;
            }
            await handler.Resolve(this, card, slot, SelectionVersion);
        }

        public async UniTask CancelSelectionAsync()
        {
            if (SelectedCard == null || SelectedCard.CardInstance == null) return;

            IsBusy = true;

            var card = SelectedCard;
            var handler = GetHandler(card.CardInstance.CardType);

            if (handler == null)
            {
                ForceReset();
                return;
            }

            await handler.ReturnToOrigin(this, card);
        }

        public CardObject FindTopCard()
        {
            foreach (var result in _raycastResults)
            {
                var card = result.gameObject.GetComponentInParent<CardObject>();
                if (card != null)
                {
                    return card;
                }
            }

            return null;
        }

        public FieldSlot FindTopSlot()
        {
            foreach (var result in _raycastResults)
            {
                var slot = result.gameObject.GetComponentInParent<FieldSlot>();
                if (slot != null)
                {
                    return slot;
                }
            }
            return null;
        }

        public void SetSelectedSlot(FieldSlot slot)
        {
            SelectedSlot = slot;
        }

        public void SetState(CardUseState state)
        {
            State = state;
        }

        public void ResetSelectionState()
        {
            if (SelectedCard != null && SelectedCard.CardInstance != null)
            {
                var handler = GetHandler(SelectedCard.CardInstance.CardType);
                handler?.EndSelection(this, SelectedCard);
            }

            SelectedCard = null;
            SelectedSlot = null;
            DragVelocity = Vector3.zero;
            DragOffset = Vector3.zero;
            HasReleasedSinceSelection = false;
            IsPointerHeld = false;
            IsBusy = false;

            SetState(CardUseState.None);
            ClearArrow();

            if (BattleManager.HasInstance)
            {
                BattleManager.Instance.RequestHandLayoutRefresh();
            }
        }

        public void ForceReset()
        {
            if (HoveredCard != null)
            {
                HoveredCard.ForceExit();
                HoveredCard = null;
            }

            SelectedCard = null;
            SelectedSlot = null;
            DragVelocity = Vector3.zero;
            DragOffset = Vector3.zero;
            HasReleasedSinceSelection = false;
            IsPointerHeld = false;
            IsBusy = false;

            SetState(CardUseState.None);
            ClearArrow();

            if (BattleManager.HasInstance)
            {
                BattleManager.Instance.RequestHandLayoutRefresh();
            }
        }

        public void ClearArrow()
        {
            targetArrow?.Hide();
        }

        public Vector2 ScreenToLocalPointInLayer(RectTransform targetLayer, Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetLayer,
                screenPos,
                null,
                out var localPoint
            );

            return localPoint;
        }

        private ICardUseHandler GetHandler(CardType cardType)
        {
            return cardType switch
            {
                CardType.Character => _characterHandler,
                CardType.Normal => _normalHandler,
                _ => null
            };
        }
        public void NotifyCardDisabled(CardObject card)
        {
            if (card == null) return;

            if (HoveredCard == card)
            {
                HoveredCard = null;
            }

            if (SelectedCard == card)
            {
                ForceReset();
            }
        }
    }
}