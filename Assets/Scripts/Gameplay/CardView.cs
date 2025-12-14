using System.Collections;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public class CardView : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField]
        private SpriteRenderer frontRenderer;

        [SerializeField]
        private SpriteRenderer backRenderer;

        [Header("Animation")]
        [SerializeField]
        [Min(0f)]
        private float flipDuration = 0.35f;

        [SerializeField]
        [Min(0f)]
        private float hoverScaleMultiplier = 1.05f;

        [SerializeField]
        private Color selectedTint = Color.white;

        [SerializeField]
        private bool startFaceUp;

        public CardData CardData { get; private set; }

        public bool IsFaceUp => isFaceUp;

        private bool isFaceUp;
        private Coroutine flipRoutine;
        private Vector3 originalScale;
        private Color? originalFrontColor;
        private Color? originalBackColor;

        private void Awake()
        {
            originalScale = transform.localScale;
            if (frontRenderer != null)
            {
                originalFrontColor = frontRenderer.color;
            }

            if (backRenderer != null)
            {
                originalBackColor = backRenderer.color;
            }

            if (startFaceUp)
            {
                ApplyFaceState(true);
            }
            else
            {
                ApplyFaceState(false);
            }
        }

        private void OnDestroy()
        {
            if (flipRoutine != null)
            {
                StopCoroutine(flipRoutine);
            }
        }

        public void SetCard(CardData data, bool faceUp = false, bool instant = true)
        {
            CardData = data;

            if (frontRenderer != null)
            {
                frontRenderer.sprite = data != null ? data.FaceSprite : null;
            }

            if (backRenderer != null)
            {
                backRenderer.sprite = data != null ? data.BackSprite : null;
            }

            SetFaceUp(faceUp, instant);
        }

        public void SetFaceUp(bool faceUp, bool instant = false)
        {
            if (isFaceUp == faceUp && flipRoutine == null)
            {
                return;
            }

            if (flipRoutine != null)
            {
                StopCoroutine(flipRoutine);
                flipRoutine = null;
            }

            if (instant || flipDuration <= 0f)
            {
                ApplyFaceState(faceUp);
                return;
            }

            flipRoutine = StartCoroutine(FlipRoutine(faceUp));
        }

        public void SetSelected(bool selected)
        {
            if (!originalFrontColor.HasValue)
            {
                originalFrontColor = frontRenderer != null ? frontRenderer.color : Color.white;
            }

            if (!originalBackColor.HasValue)
            {
                originalBackColor = backRenderer != null ? backRenderer.color : Color.white;
            }

            if (frontRenderer != null)
            {
                frontRenderer.color = selected ? selectedTint : originalFrontColor.Value;
            }

            if (backRenderer != null)
            {
                backRenderer.color = selected ? selectedTint : originalBackColor.Value;
            }
        }

        private IEnumerator FlipRoutine(bool faceUp)
        {
            var halfDuration = Mathf.Max(0.01f, flipDuration * 0.5f);
            var elapsed = 0f;

            while (elapsed < halfDuration)
            {
                var t = elapsed / halfDuration;
                SetHorizontalScale(Mathf.Lerp(1f, 0f, t));
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetHorizontalScale(0f);
            ApplyFaceState(faceUp);

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                var t = elapsed / halfDuration;
                SetHorizontalScale(Mathf.Lerp(0f, 1f, t));
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetHorizontalScale(1f);
            flipRoutine = null;
        }

        private void ApplyFaceState(bool faceUp)
        {
            isFaceUp = faceUp;

            if (frontRenderer != null)
            {
                frontRenderer.enabled = faceUp;
            }

            if (backRenderer != null)
            {
                backRenderer.enabled = !faceUp;
            }
        }

        private void SetHorizontalScale(float value)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Sign(scale.x) * value;
            transform.localScale = scale;
        }

        private void OnMouseEnter()
        {
            transform.localScale = originalScale * hoverScaleMultiplier;
        }

        private void OnMouseExit()
        {
            transform.localScale = originalScale;
        }
    }
}
