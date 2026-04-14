using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;

namespace GamePlay.Battle.Card
{
    public class HandSortingManager : MonoBehaviour
    {
        [Header("Hand Root")]
        [SerializeField] private RectTransform handRoot;

        [Header("Fan Settings")]
        [SerializeField] private float cardSpacing = 160f;
        [SerializeField] private float maxAngle = 18f;
        [SerializeField] private float curveHeight = 60f;
        [SerializeField] private float baseY = 0f;
        [SerializeField] private float sortDuration = 0.2f;

        private readonly List<Tween> _tweens = new();

        private BattleManager _battleManager;
        private ObservableList<CardInstance> _observedHand;
        private bool _layoutDirty;

        public RectTransform HandRoot => handRoot;
        public Vector2 HandCenterAnchoredPosition => new(0f, baseY + curveHeight);

        public void Init()
        {
            _battleManager = BattleManager.Instance;
            if (_battleManager == null) return;

            _observedHand = _battleManager.Hand.Cards;
            _observedHand.OnChanged -= OnHandChanged;
            _observedHand.OnChanged += OnHandChanged;

            _layoutDirty = true;
        }

        private void OnDisable()
        {
            if (_observedHand != null)
            {
                _observedHand.OnChanged -= OnHandChanged;
            }

            KillTweens();
        }

        public void MarkDirty()
        {
            _layoutDirty = true;
        }

        public void RequestRefresh()
        {
            _layoutDirty = true;
        }

        private void OnHandChanged()
        {
            _layoutDirty = true;
        }

        private void LateUpdate()
        {
            if (!_layoutDirty) return;

            _layoutDirty = false;
            RefreshHandLayout();
        }

        private void RefreshHandLayout()
        {
            if (_battleManager == null) return;

            var cards = new List<CardObject>();
            var selected = CardUseManager.HasInstance ? CardUseManager.Instance.SelectedCard : null;

            foreach (var cardInstance in _battleManager.Hand.Cards.Items)
            {
                if (_battleManager.TryGetHandCardObject(cardInstance, out var cardObject))
                {
                    if (cardObject == selected)
                    {
                        continue;
                    }

                    cards.Add(cardObject);
                }
            }

            SortHand(cards);
        }

        public void SortHand(List<CardObject> cards)
        {
            if (handRoot == null) return;

            KillTweens();

            if (cards == null || cards.Count == 0) return;

            int count = cards.Count;
            float centerIndex = (count - 1) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var card = cards[i];
                if (card == null) continue;

                var rect = card.RectTransform;
                if (rect == null) continue;

                if (rect.parent != handRoot)
                {
                    rect.SetParent(handRoot, false);
                }

                float offsetFromCenter = i - centerIndex;
                float normalized = count <= 1 ? 0f : offsetFromCenter / centerIndex;

                float x = offsetFromCenter * cardSpacing;
                float y = baseY + (-(normalized * normalized) * curveHeight + curveHeight);
                float angle = -normalized * maxAngle;

                _tweens.Add(rect.DOAnchorPos(new Vector2(x, y), sortDuration).SetEase(Ease.OutCubic));
                _tweens.Add(rect.DOLocalRotate(new Vector3(0f, 0f, angle), sortDuration).SetEase(Ease.OutCubic));

                rect.SetSiblingIndex(i);
            }
        }

        private void KillTweens()
        {
            foreach (var tween in _tweens)
            {
                tween?.Kill();
            }

            _tweens.Clear();
        }
    }
}