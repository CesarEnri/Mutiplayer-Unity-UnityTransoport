using TMPro;
using Unity.Collections;
using UnityEngine;

namespace UI.Leaderboard
{
    public class LeaderboardEntityDisplayCoin : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;

        private FixedString32Bytes _displayName;

        public int TeamIndex { get; private set; }
        public ulong ClientId { get; private set; }
        public int Coins { get; private set; }

        public void Initialise(ulong clientId, FixedString32Bytes displayName, int coins)
        {
            ClientId = clientId;
            _displayName = displayName;

            UpdateCoins(coins);
        }

        public void Initialise(int teamIndex, FixedString32Bytes displayName, int coins)
        {
            TeamIndex = teamIndex;
            _displayName = displayName;

            UpdateCoins(coins);
        }

        public void SetColour(Color colour)
        {
            displayText.color = colour;
        }

        public void UpdateCoins(int coins)
        {
            Coins = coins;

            UpdateText();
        }

        public void UpdateText()
        {
            displayText.text = $"{transform.GetSiblingIndex() + 1}. {_displayName} ({Coins})";
        }
    }
}