using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Leaderboardtype.LeaderboardCoins
{
    public class LeaderboardEntityDisplay : MonoBehaviour
    {
        [SerializeField] private TMP_Text displayText;
        
        public FixedString32Bytes displayName { get; private set; }

        public int TeamIndex { get; private set; }
        public ulong ClientId { get; private set; }
        public int Coins { get; private set; }
        
        public int Kill { get; private set; }
        public int AssistantKill { get; private set; }

        public void Initialise(ulong clientId, FixedString32Bytes displayName, int coins, int kill)
        {
            ClientId = clientId;
            this.displayName = displayName;

            UpdateCoins(coins);
            UpdateKill(kill);
        }

        public void Initialise(int teamIndex, FixedString32Bytes displayName, int coins, int kill)
        {
            TeamIndex = teamIndex;
            this.displayName = displayName;

            UpdateCoins(coins);
            UpdateKill(kill);
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

        public void UpdateKill(int kill)
        {
            Kill = kill;

            UpdateText();
        }

        public void UpdateText()
        {
            displayText.text = $"{transform.GetSiblingIndex() + 1}. {displayName} ({Coins}) -- {Kill}";
        }
    }  
}