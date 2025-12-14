using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolitaireTripicks.Cards
{
    [CreateAssetMenu(fileName = "LayoutDefinition", menuName = "Cards/Layout Definition", order = 1)]
    public class LayoutDefinition : ScriptableObject
    {
        [Serializable]
        public class LayoutNode
        {
            [SerializeField]
            private string cardId;

            [SerializeField]
            private Vector2 position;

            [SerializeField]
            private List<string> blockedBy = new();

            public string CardId => cardId;

            public Vector2 Position => position;

            public IReadOnlyList<string> BlockedBy => blockedBy;
        }

        [SerializeField]
        private List<LayoutNode> nodes = new();

        public IReadOnlyList<LayoutNode> Nodes => nodes;

        public LayoutNode GetNodeById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return nodes.Find(node => node.CardId == id);
        }
    }
}
