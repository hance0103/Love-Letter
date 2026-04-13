using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Battle.Field;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Battle.Card
{
    public class CardObject : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image cardImage;
        [SerializeField] private TMP_Text cardName;
        [SerializeField] private TMP_Text cardNum;
        [SerializeField] private TMP_Text currentHp;
        [SerializeField] private TMP_Text currentATK;
        [SerializeField] private TMP_Text currentShield;
        [SerializeField] private TMP_Text currentActionCount;
        [SerializeField] private TMP_Text cardDesc;

        [Header("Runtime")]
        [SerializeField] private CardInstance cardInstance;
        [SerializeField] private FieldSlot currentSlot;

        [Header("Hover")]
        [SerializeField] private float hoverScale = 1.15f;
        [SerializeField] private float hoverDuration = 0.15f;

        [Header("Selection")]
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float selectedScaleDuration = 0.08f;

        [Header("Move")]
        [SerializeField] private float moveDuration = 0.15f;
        [SerializeField] private float snapToSlotDuration = 0.22f;
        [SerializeField] private Ease snapToSlotEase = Ease.OutCubic;

        public CardInstance CardInstance => cardInstance;
        public FieldSlot CurrentSlot => currentSlot;
        public RectTransform RectTransform => _rectTransform;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector3 _baseScale;

        private Tween _scaleTween;
        private Tween _moveTween;

        private bool _isSelected;
        private bool _isReturning;
        private int _originalSiblingIndex = -1;

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

        public void Init(CardInstance instance)
        {
            if (instance == null)
            {
                Debug.LogError("CardObject.Init: instance가 null입니다.");
                return;
            }

            cardInstance = instance;

            if (cardName != null) cardName.text = instance.data.nameString;
            if (cardDesc != null) cardDesc.text = instance.cardDesc;

            SetOptionalText(cardNum, instance.data.cardNum);
            SetOptionalText(currentHp, instance.currentHp);
            SetOptionalText(currentATK, instance.currentATK);
            SetOptionalText(currentShield, instance.currentShield);
            SetOptionalText(currentActionCount, instance.currentActionCount);
        }

        public void ResetObject()
        {
            
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

        public void ForceEnter()
        {
            if (_isSelected || _isReturning) return;
            if (!isActiveAndEnabled) return;

            _scaleTween?.Kill();

            if (_originalSiblingIndex < 0)
            {
                _originalSiblingIndex = transform.GetSiblingIndex();
            }

            transform.SetAsLastSibling();
            _scaleTween = transform.DOScale(_baseScale * hoverScale, hoverDuration)
                .SetEase(Ease.OutQuad);
        }

        public void ForceExit()
        {
            if (!gameObject.activeInHierarchy) return;

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_baseScale, hoverDuration)
                .SetEase(Ease.OutQuad);

            RestoreSiblingIndex();
        }

        public void BeginSelection(Transform dragLayer)
        {
            if (dragLayer == null)
            {
                Debug.LogError("CardObject.BeginSelection: dragLayer가 null입니다.");
                return;
            }

            _moveTween?.Kill();
            _scaleTween?.Kill();

            _isSelected = true;
            _isReturning = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }

            if (_originalSiblingIndex < 0)
            {
                _originalSiblingIndex = transform.GetSiblingIndex();
            }

            transform.SetParent(dragLayer, true);
            transform.SetAsLastSibling();

            _scaleTween = transform.DOScale(_baseScale * selectedScale, selectedScaleDuration)
                .SetEase(Ease.OutQuad);
        }

        public void BeginSelectionForTargeting()
        {
            _moveTween?.Kill();
            _scaleTween?.Kill();

            _isSelected = true;
            _isReturning = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }

            if (_originalSiblingIndex < 0)
            {
                _originalSiblingIndex = transform.GetSiblingIndex();
            }

            transform.SetAsLastSibling();

            _scaleTween = transform.DOScale(_baseScale * selectedScale, selectedScaleDuration)
                .SetEase(Ease.OutQuad);
        }

        public void EndSelection()
        {
            _isSelected = false;

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            if (!gameObject.activeInHierarchy) return;

            _scaleTween?.Kill();
            _scaleTween = transform.DOScale(_baseScale, hoverDuration)
                .SetEase(Ease.OutQuad);
        }

        public void SetCurrentSlot(FieldSlot slot)
        {
            currentSlot = slot;
        }

        public void ClearCurrentSlot()
        {
            currentSlot = null;
        }

        public Vector3 GetArrowStartPosition()
        {
            return _rectTransform.position;
        }

        public async UniTask MoveToWorldPositionAsync(Vector3 worldPos, float duration = -1f)
        {
            if (_rectTransform == null) return;

            _moveTween?.Kill();
            _moveTween = _rectTransform.DOMove(worldPos, duration > 0f ? duration : moveDuration)
                .SetEase(Ease.OutQuad);

            try
            {
                await _moveTween.AsyncWaitForCompletion();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public async UniTask ReturnToHandAsync(Transform handLayer, Vector3 targetWorldPos)
        {
            if (handLayer == null)
            {
                Debug.LogError("CardObject.ReturnToHandAsync: handLayer가 null입니다.");
                return;
            }

            _isReturning = true;

            try
            {
                transform.SetParent(handLayer, true);
                await MoveToWorldPositionAsync(targetWorldPos);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (this != null)
                {
                    _isReturning = false;
                    _isSelected = false;

                    if (_canvasGroup != null)
                    {
                        _canvasGroup.blocksRaycasts = true;
                    }

                    RestoreSiblingIndex();
                }
            }
        }

        public async UniTask ReturnToSlotAsync(FieldSlot slot, Transform fieldCardLayer)
        {
            if (slot == null)
            {
                Debug.LogError("CardObject.ReturnToSlotAsync: slot이 null입니다.");
                return;
            }

            if (fieldCardLayer == null)
            {
                Debug.LogError("CardObject.ReturnToSlotAsync: fieldCardLayer가 null입니다.");
                return;
            }

            _isReturning = true;

            try
            {
                transform.SetParent(fieldCardLayer, true);

                _moveTween?.Kill();
                _scaleTween?.Kill();

                _moveTween = _rectTransform.DOMove(slot.transform.position, snapToSlotDuration)
                    .SetEase(snapToSlotEase);

                _scaleTween = transform.DOScale(_baseScale * 1.05f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.OutQuad);

                await _moveTween.AsyncWaitForCompletion();

                if (this != null)
                {
                    transform.localScale = _baseScale;
                    currentSlot = slot;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (this != null)
                {
                    _isReturning = false;
                    _isSelected = false;

                    if (_canvasGroup != null)
                    {
                        _canvasGroup.blocksRaycasts = true;
                    }

                    RestoreSiblingIndex();
                }
            }
        }

        public async UniTask PlayUseAsync()
        {
            if (cardInstance == null)
            {
                Debug.LogError("CardObject.PlayUseAsync: cardInstance가 null입니다.");
                return;
            }

            try
            {
                await UniTask.CompletedTask;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void RestoreSiblingIndex()
        {
            if (_originalSiblingIndex < 0) return;
            if (transform.parent == null) return;

            var targetIndex = Mathf.Clamp(_originalSiblingIndex, 0, Mathf.Max(0, transform.parent.childCount - 1));
            transform.SetSiblingIndex(targetIndex);
            _originalSiblingIndex = -1;
        }

        private void OnDisable()
        {
            _scaleTween?.Kill();
            _moveTween?.Kill();

            if (CardUseManager.Instance != null)
            {
                CardUseManager.Instance.ClearHover(this);
                CardUseManager.Instance.NotifyCardDisabled(this);
            }
        }
    }
}