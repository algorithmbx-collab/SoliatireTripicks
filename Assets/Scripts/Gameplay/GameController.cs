using System;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    public class GameController : MonoBehaviour
    {
        public event Action<CardView> CardSelected;

        public void HandleCardSelected(CardView card)
        {
            if (card == null)
            {
                return;
            }

            Debug.Log($"Card selected: {card.CardData?.name ?? card.name}", card);
            CardSelected?.Invoke(card);
        }
    }
}
