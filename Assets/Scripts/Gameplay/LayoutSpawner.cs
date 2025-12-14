using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    [DisallowMultipleComponent]
    public class LayoutSpawner : MonoBehaviour
    {
        [SerializeField]
        private LayoutDefinition layoutDefinition;

        [SerializeField]
        private CardView cardPrefab;

        [SerializeField]
        private Transform cardParent;

        [SerializeField]
        [Tooltip("Offset applied on the Z axis to keep spawned cards visible when stacked.")]
        private float stackingDepthStep = -0.01f;

        [SerializeField]
        [Tooltip("Optional initial cards to assign when spawning.")]
        private List<CardData> initialCards;

        [SerializeField]
        private bool spawnOnStart = true;

        private readonly List<SpawnedCard> spawnedCards = new();
        private readonly Dictionary<string, SpawnedCard> spawnedCardsById = new();

        public int NodeCount => layoutDefinition != null ? layoutDefinition.Nodes.Count : 0;

        public IEnumerable<CardView> SpawnedCards => spawnedCards.Select(spawned => spawned.View);

        private void Start()
        {
            if (spawnOnStart)
            {
                Spawn(initialCards);
            }
        }

        public void Spawn(IReadOnlyList<CardData> cards = null)
        {
            ClearSpawnedCards();

            if (layoutDefinition == null || cardPrefab == null)
            {
                Debug.LogWarning("LayoutDefinition or Card prefab is not assigned on LayoutSpawner.", this);
                return;
            }

            var parent = cardParent == null ? transform : cardParent;
            var nodes = layoutDefinition.Nodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var view = Instantiate(cardPrefab, parent);
                view.transform.localPosition = new Vector3(node.Position.x, node.Position.y, stackingDepthStep * i);
                view.transform.localRotation = Quaternion.identity;

                CardData cardData = null;
                if (cards != null && i < cards.Count)
                {
                    cardData = cards[i];
                }

                view.SetCard(cardData, false, true);

                var spawnedCard = new SpawnedCard(node, view);
                spawnedCards.Add(spawnedCard);

                if (!string.IsNullOrWhiteSpace(node.CardId))
                {
                    spawnedCardsById[node.CardId] = spawnedCard;
                }
            }

            UpdateUnblockedStates();
        }

        public bool TryGetCard(string cardId, out CardView cardView)
        {
            if (spawnedCardsById.TryGetValue(cardId, out var spawned))
            {
                cardView = spawned.View;
                return true;
            }

            cardView = null;
            return false;
        }

        public bool IsCardUnblocked(string cardId)
        {
            return spawnedCardsById.TryGetValue(cardId, out var spawned) && spawned.IsUnblocked;
        }

        public bool IsCardUnblocked(CardView cardView)
        {
            foreach (var spawned in spawnedCards)
            {
                if (spawned.View == cardView)
                {
                    return spawned.IsUnblocked;
                }
            }

            return false;
        }

        public bool TryGetNode(CardView cardView, out LayoutDefinition.LayoutNode node)
        {
            foreach (var spawned in spawnedCards)
            {
                if (spawned.View == cardView)
                {
                    node = spawned.Node;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public void MarkCardRemoved(CardView cardView)
        {
            foreach (var spawned in spawnedCards)
            {
                if (spawned.View == cardView)
                {
                    spawned.IsRemoved = true;
                    if (spawned.View != null)
                    {
                        spawned.View.gameObject.SetActive(false);
                    }

                    UpdateUnblockedStates();
                    break;
                }
            }
        }

        public void UpdateUnblockedStates()
        {
            foreach (var spawned in spawnedCards)
            {
                spawned.IsUnblocked = !spawned.IsRemoved && AreBlockersCleared(spawned.Node);
            }
        }

        private bool AreBlockersCleared(LayoutDefinition.LayoutNode node)
        {
            var blockers = node.BlockedBy;
            if (blockers == null || blockers.Count == 0)
            {
                return true;
            }

            foreach (var blockerId in blockers)
            {
                if (!spawnedCardsById.TryGetValue(blockerId, out var spawned))
                {
                    continue;
                }

                if (!spawned.IsRemoved)
                {
                    return false;
                }
            }

            return true;
        }

        private void ClearSpawnedCards()
        {
            foreach (var spawned in spawnedCards)
            {
                if (spawned?.View != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(spawned.View.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(spawned.View.gameObject);
                    }
                }
            }
            spawnedCards.Clear();
            spawnedCardsById.Clear();
        }

        private void OnDrawGizmos()
        {
            if (layoutDefinition == null)
            {
                return;
            }

            var nodes = layoutDefinition.Nodes;
            var nodePositions = new Dictionary<string, Vector3>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var localPos = new Vector3(node.Position.x, node.Position.y, 0f);
                var worldPos = transform.TransformPoint(localPos);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(worldPos, 0.08f);

                if (!string.IsNullOrWhiteSpace(node.CardId))
                {
                    nodePositions[node.CardId] = worldPos;
                }

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(worldPos + Vector3.up * 0.1f, string.IsNullOrWhiteSpace(node.CardId) ? $"Node {i}" : node.CardId);
#endif
            }

            Gizmos.color = Color.cyan;
            foreach (var node in nodes)
            {
                if (node.BlockedBy == null || node.BlockedBy.Count == 0)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(node.CardId) && nodePositions.TryGetValue(node.CardId, out var from))
                {
                    foreach (var blockerId in node.BlockedBy)
                    {
                        if (string.IsNullOrWhiteSpace(blockerId))
                        {
                            continue;
                        }

                        if (nodePositions.TryGetValue(blockerId, out var to))
                        {
                            Gizmos.DrawLine(from, to);
                        }
                    }
                }
            }
        }

        private class SpawnedCard
        {
            public LayoutDefinition.LayoutNode Node { get; }

            public CardView View { get; }

            public bool IsRemoved { get; set; }

            public bool IsUnblocked { get; set; }

            public SpawnedCard(LayoutDefinition.LayoutNode node, CardView view)
            {
                Node = node;
                View = view;
                IsRemoved = false;
            }
        }
    }
}
