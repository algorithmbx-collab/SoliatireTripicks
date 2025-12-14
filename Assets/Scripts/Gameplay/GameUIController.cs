using UnityEngine;
using UnityEngine.UI;

namespace SolitaireTripicks.Cards
{
    [DisallowMultipleComponent]
    public class GameUIController : MonoBehaviour
    {
        [SerializeField]
        private GameController gameController;

        [Header("HUD")]
        [SerializeField]
        private Text scoreText;

        [SerializeField]
        private Text streakText;

        [SerializeField]
        private Text stockText;

        [SerializeField]
        private Image wasteImage;

        [Header("Panels")]
        [SerializeField]
        private GameObject winPanel;

        [SerializeField]
        private GameObject losePanel;

        [SerializeField]
        private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField]
        private Button drawButton;

        [SerializeField]
        private Button undoButton;

        [SerializeField]
        private Button restartButton;

        [SerializeField]
        private Button pauseButton;

        [SerializeField]
        private Button resumeButton;

        [SerializeField]
        private Button winRestartButton;

        [SerializeField]
        private Button loseRestartButton;

        private void Awake()
        {
            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
            }

            HideAllPanels();

            WireButtons();

            if (undoButton != null)
            {
                undoButton.interactable = false;
            }
        }

        private void OnEnable()
        {
            SubscribeToController();
        }

        private void OnDisable()
        {
            UnsubscribeFromController();
        }

        private void Start()
        {
            RefreshAll();
        }

        public void OnDrawButtonClicked()
        {
            gameController?.DrawFromStock();
        }

        public void OnRestartButtonClicked()
        {
            HideAllPanels();
            gameController?.RestartGame();
            RefreshAll();
        }

        public void OnPauseButtonClicked()
        {
            gameController?.PauseGame();
            ShowPausePanel();
        }

        public void OnResumeButtonClicked()
        {
            HideAllPanels();
            gameController?.ResumeGame();
            RefreshAll();
        }

        public void OnWinRestartButtonClicked()
        {
            OnRestartButtonClicked();
        }

        public void OnLoseRestartButtonClicked()
        {
            OnRestartButtonClicked();
        }

        private void WireButtons()
        {
            if (drawButton != null)
            {
                drawButton.onClick.AddListener(OnDrawButtonClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
            }

            if (winRestartButton != null)
            {
                winRestartButton.onClick.AddListener(OnWinRestartButtonClicked);
            }

            if (loseRestartButton != null)
            {
                loseRestartButton.onClick.AddListener(OnLoseRestartButtonClicked);
            }
        }

        private void SubscribeToController()
        {
            if (gameController == null)
            {
                return;
            }

            gameController.ScoreChanged += OnScoreChanged;
            gameController.StreakChanged += OnStreakChanged;
            gameController.StockCountChanged += OnStockCountChanged;
            gameController.WasteCardChanged += OnWasteCardChanged;
            gameController.GameWon += OnGameWon;
            gameController.GameLost += OnGameLost;
        }

        private void UnsubscribeFromController()
        {
            if (gameController == null)
            {
                return;
            }

            gameController.ScoreChanged -= OnScoreChanged;
            gameController.StreakChanged -= OnStreakChanged;
            gameController.StockCountChanged -= OnStockCountChanged;
            gameController.WasteCardChanged -= OnWasteCardChanged;
            gameController.GameWon -= OnGameWon;
            gameController.GameLost -= OnGameLost;
        }

        private void RefreshAll()
        {
            if (gameController == null)
            {
                UpdateScoreText(0);
                UpdateStreakText(0);
                UpdateStockText(0);
                UpdateWasteCard(null);
                return;
            }

            UpdateScoreText(gameController.Score);
            UpdateStreakText(gameController.Streak);
            UpdateStockText(gameController.StockCount);
            UpdateWasteCard(gameController.WasteTopCard);
        }

        private void OnScoreChanged(int newScore)
        {
            UpdateScoreText(newScore);
        }

        private void OnStreakChanged(int newStreak)
        {
            UpdateStreakText(newStreak);
        }

        private void OnStockCountChanged(int newCount)
        {
            UpdateStockText(newCount);
        }

        private void OnWasteCardChanged(CardData card)
        {
            UpdateWasteCard(card);
        }

        private void OnGameWon()
        {
            HideAllPanels();
            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }
        }

        private void OnGameLost()
        {
            HideAllPanels();
            if (losePanel != null)
            {
                losePanel.SetActive(true);
            }
        }

        private void ShowPausePanel()
        {
            HideAllPanels();
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        private void HideAllPanels()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }

            if (losePanel != null)
            {
                losePanel.SetActive(false);
            }

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void UpdateScoreText(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {newScore}";
            }
        }

        private void UpdateStreakText(int newStreak)
        {
            if (streakText != null)
            {
                streakText.text = $"Streak: {newStreak}";
            }
        }

        private void UpdateStockText(int newCount)
        {
            if (stockText != null)
            {
                stockText.text = $"Stock: {newCount}";
            }
        }

        private void UpdateWasteCard(CardData card)
        {
            if (wasteImage == null)
            {
                return;
            }

            wasteImage.sprite = card != null ? card.FaceSprite : null;
            var color = wasteImage.color;
            color.a = card != null ? 1f : 0.25f;
            wasteImage.color = color;
        }
    }
}
