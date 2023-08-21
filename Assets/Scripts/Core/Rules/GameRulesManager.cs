using UI.Leaderboard;
using Unity.Netcode;

namespace Core.Rules
{
    public class GameRulesManager:NetworkBehaviour
    {
        
        private NetworkList<LeaderboardEntityState> leaderboardEntities;
        
        public GameRulesManager(GameRulesMode gameRulesMode)
        {             
            
        }
        
        public override void OnNetworkSpawn()
        {
            
        }
    }

    public enum GameRulesMode
    {
        ByTime,
        ByPoint
    }
}