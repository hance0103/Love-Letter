using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Battle.Field;
using GameSystem.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePlay.Battle.Card
{
    public class CardObject : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        // UI적으로 보이는 카드 정보
        [SerializeField] private Image cardImage;
        
        [SerializeField] private TMP_Text cardName;
        [SerializeField] private TMP_Text cardNum;
        [SerializeField] private TMP_Text currentHp;
        [SerializeField] private TMP_Text currentATK;
        [SerializeField] private TMP_Text currentShield;
        [SerializeField] private TMP_Text currentActionCount;
        
        [SerializeField] private TMP_Text cardDesc;
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
        
        private bool _isReturning;
        private Tween _scaleTween;
        private Tween _moveTween;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _originalScale = transform.localScale;
            _isReturning = false;
        }

        public void Init(CardInstance instance)
        {
            cardName.text = instance.data.nameString;
            //cardImage.sprite = instance.cardImage;
            
            if (instance.data.cardNum < 0)
            {
                cardNum.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                cardNum.text = instance.data.cardNum.ToString();
            }
            
            
            // 체력 설정
            if (instance.currentHp < 0)
            {
                currentHp.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                currentHp.text = instance.currentHp.ToString();
            }
            
            // 공격력 설정
            if (instance.currentATK < 0)
            {
                currentATK.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                currentATK.text = instance.currentATK.ToString();
            }
            
            // 행동 카운트 설정
            if (instance.currentActionCount < 0)
            {
                currentActionCount.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                currentActionCount.text = instance.currentActionCount.ToString();
            }
            cardDesc.text = instance.cardDesc;
            
            cardInstance = instance;
        }
    
        // 카드 확대
        public void ForceEnter()
        {
            _scaleTween?.Kill();
            transform.SetAsLastSibling();
            _scaleTween = transform.DOScale(_originalScale * hoverScale, hoverDuration)
                .SetEase(Ease.OutBack);
        }
        
        //카드 축소
        public void ForceExit()
        {
            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_originalScale, hoverDuration)
                .SetEase(Ease.OutQuad);
        }

        // 카드 드래그
        public void OnPointerDown(PointerEventData eventData)
        {
            if (CardUseManager.Instance.isDragging || _isReturning) return;
            
            switch (cardInstance.data.cardType)
            {
                case CardType.Character:
                {
                    CardUseManager.Instance.isDragging = true;
                    _moveTween?.Kill();
            
                    _canvasGroup.blocksRaycasts = false;
                    _originalWorldPos = _rectTransform.position;
                    CardUseManager.Instance.SelectCard(cardInstance);
                    transform.SetParent(CardUseManager.Instance.DragLayer, true);
                    break;
                }
                case CardType.Normal:
                {
                    // 타겟 지정 화살표 생성
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            

        }

        public void OnDrag(PointerEventData eventData)
        {
            switch (cardInstance.data.cardType)
            {
                case CardType.Character:
                    _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
                    break;
                case CardType.Normal:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            switch (cardInstance.data.cardType)
            {
                case CardType.Character:
                {
                    try
                    {
                        _canvasGroup.blocksRaycasts = true;
                        CardUseManager.Instance.isDragging = false;

                        var target = CardUseManager.Instance.CurrentSelectedSlot;
                        // 슬롯에 올리지 않았을 경우 바로 손으로 리턴시킨다.
                        if (target == null)
                        {
                            await CardReturn();
                            return;
                        }

                        switch (cardInstance.data.cardType)
                        {
                            case CardType.Character:
                            {
                                // 놓을수 없는 슬롯일 경우 되돌아옴
                                if (!target.CanDrop(cardInstance))
                                {
                                    await CardReturn();
                                    return;
                                }
                                    

                                // 일단 카드를 놓았어
                                target.OnDrop(cardInstance);
                                // 예외처리
                                if (target.GetTransform() is not { } t) return;

                                // 슬롯 초기화 한번 해주고
                                if (currentSlot != null)
                                    currentSlot.ClearSlot();

                                // 부모 오브젝트 바꿔주기
                                transform.SetParent(t, true);
                                // 현재 이 카드가 올라간 슬롯 변경
                                currentSlot = t.GetComponent<FieldSlot>();
                                // 슬롯 중앙으로 이동
                                await CardReturn();
                                break;
                            }
                            case CardType.Normal:
                            {
                                // 사용 가능한 슬롯에 올리지 않았으면 리턴
                                if (!target.CanUseThisNormalCard(cardInstance))
                                {
                                    await CardReturn();
                                    return;
                                }

                                // 카드 효과 처리
                                await CardUse(cardInstance);
                                _moveTween?.Kill();
                                _scaleTween?.Kill();
                                BattleManager.Instance.DiscardCard(cardInstance);
                                break;
                            }

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        await CardReturn();
                    }
                    finally
                    {
                        
                        CardUseManager.Instance.SelectCard(null);
                    } 
                } break;
                case CardType.Normal:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private async UniTask CardUse(CardInstance cardInstance)
        {
            // 카드 효과 처리
            var data = cardInstance.data;
            Debug.Log($"{data.nameString} 카드 사용");
        }
        private async UniTask CardReturn()
        {
            try
            {
                _isReturning = true;
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
                _isReturning = false;
            }
        }
        
        private void OnDisable()
        {
            CardUseManager.ClearHover(this);
        }
    }
    
}
