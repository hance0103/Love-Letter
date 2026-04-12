using UnityEngine;

namespace GamePlay.Battle.Card
{
    public class CardTargetArrow : MonoBehaviour
    {
        [SerializeField] private RectTransform body;
        [SerializeField] private RectTransform head;
        [SerializeField] private float minLength = 40f;
        [SerializeField] private float headOffset = 20f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        public void Show(Vector3 startWorldPos, Vector3 endWorldPos)
        {
            gameObject.SetActive(true);
            UpdateArrow(startWorldPos, endWorldPos);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdateArrow(Vector3 startWorldPos, Vector3 endWorldPos)
        {
            var dir = endWorldPos - startWorldPos;
            var distance = dir.magnitude;

            if (distance < 0.001f)
            {
                distance = minLength;
                dir = Vector3.up;
            }

            var normalized = dir.normalized;
            var angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;

            _rectTransform.position = startWorldPos;
            _rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (body != null)
            {
                var bodySize = body.sizeDelta;
                bodySize.x = Mathf.Max(minLength, distance - headOffset);
                body.sizeDelta = bodySize;
                body.anchoredPosition = new Vector2(bodySize.x * 0.5f, 0f);
            }

            if (head != null)
            {
                head.anchoredPosition = new Vector2(distance, 0f);
            }
        }
    }
}