using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    public class GameController : MonoBehaviour
    {
        public event Action<CardView> CardSelected;

        public event Action<int> StreakChanged;

        public event Action<int> ScoreChanged;

        public event Action GameWon;

        public event Action GameLost;

        [SerializeField]
        private LayoutSpawner layoutSpawner;

        [SerializeField]
        [Tooltip("Optional visual to display the current waste pile top card.")]
        private CardView wastePileView;

        private readonly List<CardView> tableau = new();
        private readonly Stack<CardData> stockPile = new();
        private CardData wasteTopCard;
        private Deck deck;
        private int streak;
        private int score;

        private void Start()
        {
            StartNewGame();
        }

        public void HandleCardSelected(CardView card)
        {
            if (card == null)
            {
                return;
            }

            Debug.Log($"Card selected: {card.CardData?.name ?? card.name}", card);

            if (!TryPlayCard(card))
            {
                if (!HasPlayableMove())
                {
                    DrawFromStockToWaste();
                }
            }

            CheckEndConditions();

            CardSelected?.Invoke(card);
        }

        public bool CanPlay(CardView card)
        {
            if (card == null || card.CardData == null || wasteTopCard == null)
            {
                return false;
            }

            if (!tableau.Contains(card))
            {
                return false;
            }

            if (layoutSpawner != null && !layoutSpawner.IsCardUnblocked(card))
            {
                return false;
            }

            var cardRank = card.CardData.Rank;
            var wasteRank = wasteTopCard.Rank;
            var rankDiff = Mathf.Abs((int)cardRank - (int)wasteRank);

            if (rankDiff == 1)
            {
                return true;
            }

            return (cardRank == Rank.Ace && wasteRank == Rank.King) || (cardRank == Rank.King && wasteRank == Rank.Ace);
        }

        private void StartNewGame()
        {
            tableau.Clear();
            stockPile.Clear();
            wasteTopCard = null;
            streak = 0;
            score = 0;

            deck = Deck.FromResources();
            deck.Shuffle();

            var tableauCount = layoutSpawner != null ? layoutSpawner.NodeCount : 0;
            var tableauCards = tableauCount > 0 ? deck.DrawMany(tableauCount).ToList() : new List<CardData>();

            if (layoutSpawner != null)
            {
                layoutSpawner.Spawn(tableauCards);
                tableau.AddRange(layoutSpawner.SpawnedCards);
                FlipUnblockedCards();
            }

            while (!deck.IsEmpty)
            {
                stockPile.Push(deck.Draw());
            }

            DrawFromStockToWaste();
        }

        private bool TryPlayCard(CardView card)
        {
            if (!CanPlay(card))
            {
                return false;
            }

            tableau.Remove(card);
            card.SetFaceUp(true);
            wasteTopCard = card.CardData;
            UpdateWastePileView();

            streak++;
            score += streak;
            StreakChanged?.Invoke(streak);
            ScoreChanged?.Invoke(score);

            UpdateUnblockedCards();

            if (tableau.Count == 0)
            {
                GameWon?.Invoke();
            }
            else if (!HasPlayableMove())
            {
                DrawFromStockToWaste();
            }

            return true;
        }

        private void UpdateUnblockedCards()
        {
            if (layoutSpawner == null)
            {
                return;
            }

            layoutSpawner.UpdateUnblockedStates();

            foreach (var view in tableau)
            {
                if (!view.IsFaceUp && layoutSpawner.IsCardUnblocked(view))
                {
                    view.SetFaceUp(true);
                }
            }
        }

        private void FlipUnblockedCards()
        {
            if (layoutSpawner == null)
            {
                foreach (var view in tableau)
                {
                    view.SetFaceUp(true);
                }

                return;
            }

            layoutSpawner.UpdateUnblockedStates();

            foreach (var view in tableau)
            {
                if (layoutSpawner.IsCardUnblocked(view))
                {
                    view.SetFaceUp(true, true);
                }
            }
        }

        private bool HasPlayableMove()
        {
            return tableau.Any(CanPlay);
        }

        private void DrawFromStockToWaste()
        {
            if (stockPile.Count == 0)
            {
                ResetStreak();
                return;
            }

            wasteTopCard = stockPile.Pop();
            UpdateWastePileView();
            ResetStreak();
        }

        private void ResetStreak()
        {
            if (streak == 0)
            {
                return;
            }

            streak = 0;
            StreakChanged?.Invoke(streak);
        }

        private void CheckEndConditions()
        {
            if (tableau.Count == 0)
            {
                GameWon?.Invoke();
                return;
            }

            if (stockPile.Count == 0 && !HasPlayableMove())
            {
                GameLost?.Invoke();
            }
        }

        private void UpdateWastePileView()
        {
            if (wastePileView != null && wasteTopCard != null)
            {
                wastePileView.SetCard(wasteTopCard, true, true);
            }
        }
    }
}
