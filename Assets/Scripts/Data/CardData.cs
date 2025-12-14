using UnityEngine;

namespace SolitaireTripicks.Cards
{
    public enum Suit
    {
        Clubs = 0,
        Diamonds = 1,
        Hearts = 2,
        Spades = 3
    }

    public enum Rank
    {
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13
    }

    [CreateAssetMenu(fileName = "CardData", menuName = "Cards/Card Data", order = 0)]
    public class CardData : ScriptableObject
    {
        [SerializeField]
        private Rank rank;

        [SerializeField]
        private Suit suit;

        [SerializeField]
        private Sprite faceSprite;

        [SerializeField]
        private Sprite backSprite;

        public Rank Rank => rank;

        public Suit Suit => suit;

        public Sprite FaceSprite => faceSprite;

        public Sprite BackSprite => backSprite;

        public void Initialize(Rank newRank, Suit newSuit, Sprite face = null, Sprite back = null)
        {
            rank = newRank;
            suit = newSuit;
            faceSprite = face;
            backSprite = back;
        }
    }
}
