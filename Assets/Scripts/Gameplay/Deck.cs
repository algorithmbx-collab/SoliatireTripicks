using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    public class Deck
    {
        private readonly List<CardData> cards;
        private readonly System.Random random;

        public Deck(IEnumerable<CardData> initialCards, int? seed = null)
        {
            var cardList = initialCards?.ToList() ?? throw new ArgumentNullException(nameof(initialCards));

            if (!CardCollectionValidator.ValidateCompleteSet(cardList, out var errors))
            {
                throw new InvalidOperationException($"Invalid card library: {string.Join(", ", errors)}");
            }

            cards = new List<CardData>(cardList);
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        public static Deck FromResources(int? seed = null)
        {
            var loadedCards = Resources.LoadAll<CardData>("Cards");
            var hasCards = loadedCards != null && loadedCards.Length > 0;
            if (hasCards && CardCollectionValidator.ValidateCompleteSet(loadedCards, out _))
            {
                return new Deck(loadedCards, seed);
            }

            Debug.LogWarning("Card assets are missing or incomplete in Resources/Cards. Generating a fallback deck at runtime.");
            var generatedCards = GenerateFullDeck();
            return new Deck(generatedCards, seed);
        }

        public int Count => cards.Count;

        public bool IsEmpty => cards.Count == 0;

        public CardData Draw()
        {
            if (cards.Count == 0)
            {
                throw new InvalidOperationException("Cannot draw from an empty deck.");
            }

            var index = cards.Count - 1;
            var card = cards[index];
            cards.RemoveAt(index);
            return card;
        }

        public IReadOnlyList<CardData> DrawMany(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > cards.Count)
            {
                throw new InvalidOperationException("Cannot draw more cards than the deck contains.");
            }

            var drawn = cards.GetRange(cards.Count - count, count);
            cards.RemoveRange(cards.Count - count, count);
            return drawn;
        }

        public void Shuffle()
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }

        private static IEnumerable<CardData> GenerateFullDeck()
        {
            var generated = new List<CardData>(52);
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    var card = ScriptableObject.CreateInstance<CardData>();
                    card.name = $"{rank} of {suit}";
                    card.Initialize(rank, suit);
                    generated.Add(card);
                }
            }

            return generated;
        }
    }

    public static class CardCollectionValidator
    {
        public static bool ValidateCompleteSet(IEnumerable<CardData> cards, out IReadOnlyList<string> errors)
        {
            var errorList = new List<string>();
            var cardList = cards?.ToList() ?? new List<CardData>();

            var combinations = new HashSet<(Rank, Suit)>();
            foreach (var card in cardList)
            {
                if (card == null)
                {
                    errorList.Add("CardData reference is null.");
                    continue;
                }

                var key = (card.Rank, card.Suit);
                if (!combinations.Add(key))
                {
                    errorList.Add($"Duplicate card found: {card.name} ({card.Rank} of {card.Suit}).");
                }
            }

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    if (!combinations.Contains((rank, suit)))
                    {
                        errorList.Add($"Missing card: {rank} of {suit}.");
                    }
                }
            }

            errors = errorList;
            return errorList.Count == 0;
        }
    }
}
