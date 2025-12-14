using System.Linq;
using SolitaireTripicks.Cards;
using UnityEditor;
using UnityEngine;

namespace SolitaireTripicks.Editor
{
    public static class CardDataValidator
    {
        private const string CardsResourcePath = "Cards";

        [InitializeOnLoadMethod]
        private static void ValidateOnLoad()
        {
            EditorApplication.delayCall += ValidateAndLog;
        }

        [MenuItem("Tools/Cards/Validate Deck Assets")]
        private static void ValidateFromMenu()
        {
            ValidateAndLog(true);
        }

        private static void ValidateAndLog(bool showDialog = false)
        {
            var cards = Resources.LoadAll<CardData>(CardsResourcePath);

            if (cards == null || cards.Length == 0)
            {
                LogResult(false, new[] { "No card assets found in Resources/Cards." }, showDialog);
                return;
            }

            if (CardCollectionValidator.ValidateCompleteSet(cards, out var errors))
            {
                LogResult(true, errors, showDialog);
            }
            else
            {
                LogResult(false, errors, showDialog);
            }
        }

        private static void LogResult(bool success, System.Collections.Generic.IReadOnlyList<string> messages, bool showDialog)
        {
            var message = success
                ? "All 52 card assets are present."
                : string.Join("\n", messages.Distinct());

            if (success)
            {
                Debug.Log(message);
            }
            else
            {
                Debug.LogError(message);
            }

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Card Asset Validation", message, "OK");
            }
        }
    }
}
