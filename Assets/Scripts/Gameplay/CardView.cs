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

        [Header("Overlay")]
        [SerializeField]
        private TextMesh label;

        [SerializeField]
        private bool createLabelIfMissing = true;

        [Header("Feedback")]
        [SerializeField]
        [Range(1f, 1.5f)]
        private float validPlayScale = 1.1f;

        [SerializeField]
        [Min(0f)]
        private float validPlayDuration = 0.18f;

        public CardData CardData { get; private set; }

        public bool IsFaceUp => isFaceUp;

        private static Sprite fallbackFaceSprite;
        private static Sprite fallbackBackSprite;

        private bool isFaceUp;
        private Coroutine flipRoutine;
        private Coroutine pulseRoutine;
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

            EnsureLabelExists();

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

            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
            }
        }

        public void SetCard(CardData data, bool faceUp = false, bool instant = true)
        {
            CardData = data;

            if (frontRenderer != null)
            {
                frontRenderer.sprite = data != null && data.FaceSprite != null ? data.FaceSprite : GetFallbackFaceSprite();
            }

            if (backRenderer != null)
            {
                backRenderer.sprite = data != null && data.BackSprite != null ? data.BackSprite : GetFallbackBackSprite();
            }

            UpdateLabel(CardData);
            SetFaceUp(faceUp, instant);
        }

        public void SetFaceUp(bool faceUp, bool instant = false)
        {
            var wasFaceUp = isFaceUp;

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
                if (!wasFaceUp && faceUp)
                {
                    AudioManager.Instance?.PlayFlip();
                }

                return;
            }

            flipRoutine = StartCoroutine(FlipRoutine(faceUp, wasFaceUp));
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

        public void PlayValidPlayFeedback()
        {
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
            }

            pulseRoutine = StartCoroutine(ValidPlayPulse());
        }

        private IEnumerator FlipRoutine(bool faceUp, bool wasFaceUp)
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
            if (!wasFaceUp && faceUp)
            {
                AudioManager.Instance?.PlayFlip();
            }

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

        private IEnumerator ValidPlayPulse()
        {
            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, validPlayDuration);
            while (elapsed < duration)
            {
                var t = elapsed / duration;
                var scale = Mathf.Lerp(validPlayScale, 1f, t * t);
                transform.localScale = originalScale * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
            pulseRoutine = null;
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

            if (label != null)
            {
                label.gameObject.SetActive(faceUp);
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

        private void EnsureLabelExists()
        {
            if (label != null || !createLabelIfMissing)
            {
                return;
            }

            var labelObject = new GameObject("CardLabel");
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            labelObject.transform.localRotation = Quaternion.identity;
            labelObject.transform.localScale = Vector3.one * 0.015f;

            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 1f;
            label.fontSize = 80;
            label.text = string.Empty;

            var meshRenderer = label.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var sortingLayerId = frontRenderer != null ? frontRenderer.sortingLayerID : 0;
                var sortingOrder = frontRenderer != null ? frontRenderer.sortingOrder + 2 : 2;
                meshRenderer.sortingLayerID = sortingLayerId;
                meshRenderer.sortingOrder = sortingOrder;
            }
        }

        private void UpdateLabel(CardData data)
        {
            if (label == null)
            {
                return;
            }

            if (data == null)
            {
                label.text = string.Empty;
                return;
            }

            label.text = $"{GetRankString(data.Rank)}{GetSuitSymbol(data.Suit)}";
            label.color = data.Suit == Suit.Hearts || data.Suit == Suit.Diamonds ? new Color(0.82f, 0.1f, 0.16f) : Color.black;
        }

        private static string GetRankString(Rank rank)
        {
            return rank switch
            {
                Rank.Ace => "A",
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                _ => ((int)rank).ToString()
            };
        }

        private static string GetSuitSymbol(Suit suit)
        {
            return suit switch
            {
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                Suit.Spades => "♠",
                _ => "?"
            };
        }

        private static Sprite GetFallbackFaceSprite()
        {
            if (fallbackFaceSprite != null)
            {
                return fallbackFaceSprite;
            }

            fallbackFaceSprite = CreateSpriteWithBorder(new Color(0.95f, 0.95f, 0.94f), new Color(0.8f, 0.8f, 0.82f));
            return fallbackFaceSprite;
        }

        private static Sprite GetFallbackBackSprite()
        {
            if (fallbackBackSprite != null)
            {
                return fallbackBackSprite;
            }

            fallbackBackSprite = CreateSpriteWithBorder(new Color(0.14f, 0.35f, 0.64f), new Color(0.08f, 0.2f, 0.42f));
            return fallbackBackSprite;
        }

        private static Sprite CreateSpriteWithBorder(Color fillColor, Color borderColor)
        {
            const int width = 256;
            const int height = 360;
            const int border = 10;

            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                name = "GeneratedCardSprite"
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var isBorder = x < border || x >= width - border || y < border || y >= height - border;
                    texture.SetPixel(x, y, isBorder ? borderColor : fillColor);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
