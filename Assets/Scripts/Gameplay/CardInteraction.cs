using UnityEngine;
using UnityEngine.EventSystems;

namespace SolitaireTripicks.Cards
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CardView))]
    public class CardInteraction : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private CardView cardView;

        [SerializeField]
        private GameController gameController;

        public void Initialize(CardView view, GameController controller)
        {
            cardView = view;
            gameController = controller;
        }

        private void Awake()
        {
            if (cardView == null)
            {
                cardView = GetComponent<CardView>();
            }

            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
                if (gameController == null)
                {
                    Debug.LogWarning("No GameController found in the scene to receive card interactions.", this);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ForwardSelection();
        }

        private void OnMouseUpAsButton()
        {
            ForwardSelection();
        }

        private void ForwardSelection()
        {
            if (cardView == null || gameController == null)
            {
                return;
            }

            gameController.HandleCardSelected(cardView);
        }
    }
}
