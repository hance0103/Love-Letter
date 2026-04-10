using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Battle.Field;
using GamePlay.Card;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePlay.Battle.Card
{
    public class CardObject : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        
        // UI적으로 보이는 카드 정보
        [SerializeField] private Image cardImage;
        
        [SerializeField] private TMP_Text cardName;
        [SerializeField] private TMP_Text currentHp;
        [SerializeField] private TMP_Text currentATK;
        [SerializeField] private TMP_Text currentShield;
        [SerializeField] private TMP_Text currentActionCount;
        
        
        [SerializeField] private CardInstance cardInstance;


        
        [SerializeField] private FieldSlot currentSlot;
        
        [Header("카드 오브젝트 설정값")]
        [SerializeField] private float hoverScale = 1.5f;
        [SerializeField] private float hoverDuration = 0.2f;
        private Vector3 _originalScale;
        
        private RectTransform _rectTransform;
        private Vector3 _originalWorldPos;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private bool _isDragging;
        private bool _isReturning;
        private Tween _scaleTween;
        private Tween _moveTween;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _originalScale = transform.localScale;
            _isDragging = false;
            _isReturning = false;
        }

        public void Init(CardInstance instance)
        {
            //cardImage.sprite = instance.cardImage;
            cardName.text = instance.data.nameString;
            currentHp.text = instance.currentHp.ToString();
            currentATK.text = instance.currentAttackPower.ToString();
            currentActionCount.text = instance.currentActionCount.ToString();
            
            
            cardInstance = instance;
        }


        
        // 마우스 올렸을때 확대
        public void OnPointerEnter(PointerEventData eventData)
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_originalScale * hoverScale, hoverDuration)
                .SetEase(Ease.OutBack);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_originalScale, hoverDuration)
                .SetEase(Ease.OutQuad);
        }

        // 카드 드래그
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isDragging || _isReturning) return;
            
            _isDragging = true;
            _moveTween?.Kill();
            
            _canvasGroup.blocksRaycasts = false;
            _originalWorldPos = _rectTransform.position;
            transform.SetParent(CardUseManager.Instance.DragLayer, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            try
            {
                
                _canvasGroup.blocksRaycasts = true;
                _isReturning = true;
                _isDragging = false;

                var target = GetDropTarget(eventData);

                if (target == null || !target.CanDrop(cardInstance)) return;
                
                target.OnDrop(cardInstance);

                if (target.GetTransform() is not { } t) return;
                
                if (currentSlot != null)
                    currentSlot.ClearSlot();
                
                transform.SetParent(t, true);
                currentSlot = t.GetComponent<FieldSlot>();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await CardReturn();
            }
            finally
            {
                await CardReturn();
                _isReturning = false;
            }

        }

        private async UniTask CardReturn()
        {
            try
            {
                _moveTween?.Kill();
                if (currentSlot == null)
                {
                    transform.SetParent(CardUseManager.Instance.HandLayer, true);
                    _moveTween = _rectTransform.DOMove(_originalWorldPos, 0.2f);
                }
                else
                {
                    var slotTransform = currentSlot.transform;
                    transform.SetParent(slotTransform, true);
                    _moveTween = _rectTransform.DOMove(slotTransform.position, 0.2f);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                await _moveTween.AsyncWaitForCompletion();
            }
        }
        IFieldSlot GetDropTarget(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            return results.Select(result => result.gameObject.GetComponent<IFieldSlot>()).FirstOrDefault(target => target != null);
        }
    }
    
}
