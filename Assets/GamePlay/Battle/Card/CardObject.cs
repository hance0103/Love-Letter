using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Battle.Field;
using GameSystem.Managers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Battle.Card
{
    public class CardObject : MonoBehaviour
    {
        [SerializeField] private CardInstance cardInstance;
        public CardInstance CardInstance => cardInstance;
        
        [Header("UI")]
        [SerializeField] private TMP_Text cardName;
        [SerializeField] private TMP_Text cardDesc;
        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text cardNum;
        [SerializeField] private TMP_Text currentHp;
        [SerializeField] private TMP_Text currentAtk;
        [SerializeField] private TMP_Text increaseAtk;
        [SerializeField] private TMP_Text currentShd;
        [SerializeField] private TMP_Text currentAC;
        
        [Header("Move")]
        [SerializeField] private float returnDuration = 0.2f;
        [SerializeField] private Ease returnEase = Ease.OutCubic;

        [Header("Hover")]
        [SerializeField] private float hoverOffsetY = 30f;
        [SerializeField] private float hoverScaleMultiplier = 1.5f;
        [SerializeField] private float hoverDuration = 0.15f;
        
        private int _hoverSiblingIndex = -1;
        private bool _hasHoverSiblingCache;
        
        [Header("Use")]
        [SerializeField] private float useDuration = 0.2f;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        private Tween _moveTween;
        private Tween _scaleTween;

        private Vector3 _baseScale;

        private Transform _cachedParent;
        private int _cachedSiblingIndex;
        private Vector2 _cachedAnchoredPos;

        private bool _isHovered;
        private bool _isSelected;
        private bool _isReturning;

        private FieldSlot _currentSlot;

        public RectTransform RectTransform => _rectTransform;

        public FieldSlot CurrentSlot => _currentSlot;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _baseScale = transform.localScale;
        }

        private void OnDisable()
        {
            KillTweens();

            if (CardUseManager.HasInstance)
            {
                CardUseManager.Instance.NotifyCardDisabled(this);
            }
        }

        public void Init(CardInstance instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("CardInstance object is null.");
                return;
            }
            
            this.cardInstance = instance;
            _isHovered = false;
            _isSelected = false;
            _isReturning = false;
            _currentSlot = null;

            KillTweens();

            transform.localScale = _baseScale;
            _canvasGroup.blocksRaycasts = true;
            
            
            var cardSprite = GameManager.Inst.Data.GetSprite(instance.Data.cardID);
            if (cardSprite != null) cardImage.sprite = cardSprite;
            
            SetOptionalText(cardNum, instance.Data.cardNum);
            SetOptionalText(currentHp, instance.CurrentHp);
            SetOptionalText(currentAtk, instance.CurrentATK);
            SetOptionalText(increaseAtk, instance.InCreasedAtk);
            SetOptionalText(currentShd, instance.CurrentShield);
            SetOptionalText(currentAC, instance.CurrentActionCount);
        }
        private void SetOptionalText(TMP_Text target, int value)
        {
            if (target == null) return;

            var root = target.transform.parent != null
                ? target.transform.parent.gameObject
                : target.gameObject;

            if (value < 0)
            {
                root.SetActive(false);
                return;
            }

            root.SetActive(true);
            target.text = value.ToString();
        }


        public void ResetObject()
        {
            KillTweens();

            cardInstance = null;
            _isHovered = false;
            _isSelected = false;
            _isReturning = false;
            _currentSlot = null;

            _hasHoverSiblingCache = false;
            _hoverSiblingIndex = -1;

            transform.localScale = _baseScale;
            _rectTransform.anchoredPosition = Vector2.zero;

            _canvasGroup.blocksRaycasts = true;
        }
        
        public void SetCurrentSlot(FieldSlot slot)
        {
            _currentSlot = slot;
        }

        public void ClearCurrentSlot()
        {
            _currentSlot = null;
        }

        public void CacheCurrentTransform()
        {
            _cachedParent = transform.parent;
            _cachedSiblingIndex = transform.GetSiblingIndex();
            _cachedAnchoredPos = _rectTransform.anchoredPosition;
        }

        public void BeginSelection(RectTransform dragLayer)
        {
            if (dragLayer == null) return;

            CacheCurrentTransform();

            KillTweens();
            

            
            _isSelected = true;
            _isHovered = false;
            _canvasGroup.blocksRaycasts = false;

            transform.SetParent(dragLayer, true);
            transform.SetAsLastSibling();
            RectTransform.rotation = Quaternion.identity;
            transform.localScale = _baseScale;
        }

        public void BeginSelectionForTargeting()
        {
            CacheCurrentTransform();

            KillTweens();

            _isSelected = true;
            _isHovered = false;
            _canvasGroup.blocksRaycasts = false;
            RectTransform.rotation = Quaternion.identity;
            transform.localScale = _baseScale;
        }

        public void EndSelection()
        {
            _isSelected = false;
            _isHovered = false;
            _canvasGroup.blocksRaycasts = true;
            transform.localScale = _baseScale;

            _hasHoverSiblingCache = false;
            _hoverSiblingIndex = -1;
        }

        public void ForceEnter()
        {
            if (_isSelected || _isReturning) return;
            if (_isHovered) return;

            _isHovered = true;

            if (!_hasHoverSiblingCache && transform.parent != null)
            {
                _hoverSiblingIndex = transform.GetSiblingIndex();
                _hasHoverSiblingCache = true;
            }
            transform.SetAsLastSibling();
            
            KillScaleTweenOnly();
            KillMoveTweenOnly();

            _scaleTween = transform.DOScale(_baseScale * hoverScaleMultiplier, hoverDuration).SetEase(Ease.OutCubic);
        }

        public void ForceExit()
        {
            if (!_isHovered && !_isSelected) return;

            _isHovered = false;

            if (_isSelected) return;
            
            if (_hasHoverSiblingCache && transform.parent != null)
            {
                int maxIndex = transform.parent.childCount - 1;
                int restoreIndex = Mathf.Clamp(_hoverSiblingIndex, 0, maxIndex);
                transform.SetSiblingIndex(restoreIndex);
            }
            
            _hasHoverSiblingCache = false;
            _hoverSiblingIndex = -1;

            KillScaleTweenOnly();
            KillMoveTweenOnly();

            _scaleTween = transform.DOScale(_baseScale, hoverDuration).SetEase(Ease.OutCubic);
        }

        public void SetAnchoredPosition(Vector2 anchoredPos)
        {
            if (_rectTransform == null) return;
            _rectTransform.anchoredPosition = anchoredPos;
        }

        public void SetAnchoredPositionSmooth(Vector2 targetPos, float followSpeed)
        {
            if (_rectTransform == null) return;

            _rectTransform.anchoredPosition = Vector2.Lerp(
                _rectTransform.anchoredPosition,
                targetPos,
                Time.deltaTime * followSpeed
            );
        }
        public Tween MoveToFocusAsync(Vector2 targetAnchoredPos, float duration = 0.2f)
        {
            KillTweens();

            _isHovered = false;
            _isReturning = false;

            _moveTween = _rectTransform.DOAnchorPos(targetAnchoredPos, duration).SetEase(Ease.OutCubic);
            _scaleTween = transform.DOScale(_baseScale * 1.5f, duration).SetEase(Ease.OutCubic);

            return _moveTween;
        }
        public void MoveToFocus(float duration = 0.2f)
        {
            KillTweens();

            _isHovered = false;
            _isReturning = false;

            _moveTween = _rectTransform.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutCubic);
            _scaleTween = transform.DOScale(_baseScale * 1.5f, duration).SetEase(Ease.OutCubic);
        }
        public Vector3 GetArrowStartPosition()
        {
            return _rectTransform.position;
        }

        public async UniTask ReturnToHandAsync()
        {
            if (_cachedParent == null) return;

            _isReturning = true;

            try
            {
                KillTweens();

                var worldPos = _rectTransform.position;
                var worldRot = _rectTransform.rotation;

                transform.SetParent(_cachedParent, true);
                _rectTransform.position = worldPos;
                _rectTransform.rotation = worldRot;

                if (_cachedSiblingIndex >= 0 && _cachedSiblingIndex < _cachedParent.childCount)
                {
                    transform.SetSiblingIndex(_cachedSiblingIndex);
                }

                _moveTween = _rectTransform.DOAnchorPos(_cachedAnchoredPos, returnDuration).SetEase(returnEase);
                //_rotateTween = _rectTransform.DOLocalRotateQuaternion(_cachedRotation, returnDuration).SetEase(returnEase);
                _scaleTween = transform.DOScale(_baseScale, returnDuration).SetEase(returnEase);

                await UniTask.WhenAll(
                    _moveTween.AsyncWaitForCompletion().AsUniTask(),
                    //_rotateTween.AsyncWaitForCompletion().AsUniTask(),
                    _scaleTween.AsyncWaitForCompletion().AsUniTask()
                );

                _currentSlot = null;
            }
            finally
            {
                _isReturning = false;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public async UniTask ReturnToHandLayoutAsync()
        {
            await ReturnToHandAsync();
        }
        public async UniTask ReturnToSlotAsync(FieldSlot slot, RectTransform fieldCardLayer, Camera uiCamera = null)
        {
            if (slot == null || fieldCardLayer == null) return;

            _isReturning = true;

            try
            {
                KillTweens();

                transform.SetParent(fieldCardLayer, true);
                transform.SetAsLastSibling();

                var slotRect = slot.RectTransform;
                if (slotRect == null) return;
                
                var cardWorldCenter = _rectTransform.TransformPoint(_rectTransform.rect.center);
                var slotWorldCenter = slotRect.TransformPoint(slotRect.rect.center);
                

                
                
                _moveTween = _rectTransform.DOMove(slotWorldCenter, returnDuration).SetEase(returnEase);
                _scaleTween = _rectTransform.DOScale(_baseScale, returnDuration).SetEase(returnEase);

                await UniTask.WhenAll(
                    _moveTween.AsyncWaitForCompletion().AsUniTask(),
                    _scaleTween.AsyncWaitForCompletion().AsUniTask()
                );

                _currentSlot = slot;
            }
            finally
            {
                _isReturning = false;
                _canvasGroup.blocksRaycasts = true;
            }
        }
        // public async UniTask ReturnToSlotAsync(FieldSlot slot, RectTransform fieldCardLayer)
        // {
        //     if (slot == null || fieldCardLayer == null) return;
        //
        //     _isReturning = true;
        //
        //     try
        //     {
        //         KillTweens();
        //
        //         var currentWorldPos = _rectTransform.position;
        //         var currentWorldRot = _rectTransform.rotation;
        //
        //         transform.SetParent(fieldCardLayer, true);
        //         _rectTransform.position = currentWorldPos;
        //         _rectTransform.rotation = currentWorldRot;
        //         transform.SetAsLastSibling();
        //
        //         var slotRect = slot.RectTransform;
        //         if (slotRect == null) return;
        //
        //         var screenPoint = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);
        //
        //         RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //             fieldCardLayer,
        //             screenPoint,
        //             null,
        //             out var targetAnchoredPos
        //         );
        //
        //         _moveTween = _rectTransform.DOAnchorPos(targetAnchoredPos, returnDuration).SetEase(returnEase);
        //         //_rotateTween = _rectTransform.DOLocalRotate(Vector3.zero, returnDuration).SetEase(returnEase);
        //         _scaleTween = transform.DOScale(_baseScale, returnDuration).SetEase(returnEase);
        //
        //         await UniTask.WhenAll(
        //             _moveTween.AsyncWaitForCompletion().AsUniTask(),
        //             //_rotateTween.AsyncWaitForCompletion().AsUniTask(),
        //             _scaleTween.AsyncWaitForCompletion().AsUniTask()
        //         );
        //
        //         _currentSlot = slot;
        //     }
        //     finally
        //     {
        //         _isReturning = false;
        //         _canvasGroup.blocksRaycasts = true;
        //     }
        // }

        public async UniTask PlayUseAsync()
        {
            KillTweens();

            _canvasGroup.blocksRaycasts = false;

            await transform.DOScale(_baseScale * 1.1f, useDuration * 0.5f)
                .SetEase(Ease.OutCubic)
                .AsyncWaitForCompletion();

            await transform.DOScale(Vector3.zero, useDuration * 0.5f)
                .SetEase(Ease.InBack)
                .AsyncWaitForCompletion();
        }

        private void KillTweens()
        {
            _moveTween?.Kill();
            _scaleTween?.Kill();

            _moveTween = null;
            _scaleTween = null;
        }

        private void KillMoveTweenOnly()
        {
            _moveTween?.Kill();
            _moveTween = null;
        }

        private void KillScaleTweenOnly()
        {
            _scaleTween?.Kill();
            _scaleTween = null;
        }
    }
}