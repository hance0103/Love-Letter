using System;
using System.Collections.Generic;
using GamePlay.Battle.Field;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace GamePlay.Battle.Card
{
    public enum CardUseState
    {
        Idle,
        Selected,
        Using,
    }
    
    public class CardUseManager : MonoBehaviour
    {
        public static CardUseManager Instance;
        public CardUseState state;
        
        private PlayerInput _playerInput;
        private InputAction _leftClick;
        private InputAction _rightClick;
        
        [SerializeField] private Transform dragLayer;
        [SerializeField] private Transform handLayer;
        public Transform DragLayer => dragLayer;
        public Transform HandLayer => handLayer;
        
        // 카드 마우스 이벤트 관련 변수
        private static CardObject _currentHover;
        private static CardInstance _currentSelectedCard;
        private static FieldSlot _currentSelectedSlot;
        public CardObject CurrentHover => _currentHover;
        public CardInstance CurrentSelectedCard => _currentSelectedCard;
        public FieldSlot CurrentSelectedSlot => _currentSelectedSlot;
        public bool isDragging = false;
        private Vector3 _lastMousePos;
        
        
        private List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private void Awake()
        {
            Instance = this;
            
            _playerInput = GetComponent<PlayerInput>();
            _leftClick = _playerInput.actions["LeftClick"];
            _rightClick = _playerInput.actions["RightClick"];
        }

        private void OnEnable()
        {
            _leftClick.performed += OnLeftClick;
            _rightClick.performed += OnRightClick;
        }

        private void OnDisable()
        {
            _leftClick.performed -= OnLeftClick;
            _rightClick.performed -= OnRightClick;
        }

        private void Update()
        {
            UpdateRayCast();
            UpdateHover();
            UpdateCardTarget();
        }

        private void OnLeftClick(InputAction.CallbackContext context)
        {
            switch (state)
            {
                case CardUseState.Idle:
                    break;
                case CardUseState.Selected:
                    break;
                case CardUseState.Using:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnRightClick(InputAction.CallbackContext context)
        {
            switch (state)
            {
                
            }
        }
        // 카드 호버 및 타겟 지정 관련 레이캐스트
        // 프레임마다 호출
        private void UpdateRayCast()
        {
            if (Mouse.current == null) return;

            var mousePos = Mouse.current.position.ReadValue();

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, _raycastResults);
        }

        public void SelectCard(CardInstance card)
        {
            _currentSelectedCard = card;
        }
        private void UpdateCardTarget()
        {
            if (_currentSelectedCard == null) return;
            
            FieldSlot slot = null;
            foreach (var s in _raycastResults)
            {
                slot = s.gameObject.GetComponent<FieldSlot>();
                if (slot != null) break;
            }
            if (_currentSelectedSlot == slot) return;
            
            if (_currentSelectedSlot != null)
            {
                // 기존에 선택되었던 슬롯 해제
            }

            _currentSelectedSlot = slot;
            
            if (_currentSelectedSlot != null)
            {
                // 새로운 슬롯 선택
            }
        }
        private void UpdateHover()
        {
            if (isDragging) return;
            
            CardObject topCard = null;
            // 가장 위에 있는 CardObject 찾기
            foreach (var r in _raycastResults)
            {
                topCard = r.gameObject.GetComponent<CardObject>();
                if (topCard != null) break;
            }
            
            // Hover 대상이 바뀌었을 때만 처리
            if (_currentHover == topCard)
                return;

            // 기존 카드 Hover 해제
            _currentHover?.ForceExit();

            _currentHover = topCard;
            
            // 새 카드에 Hover 적용
            _currentHover?.ForceEnter();
        }
        // 외부에서 강제로 Hover 해제할 때 (예: 카드 삭제, 씬 변경)
        public static void ClearHover(CardObject target)
        {
            if (_currentHover != target) return;
            
            _currentHover.ForceExit();
            _currentHover = null;
        }
    }
}