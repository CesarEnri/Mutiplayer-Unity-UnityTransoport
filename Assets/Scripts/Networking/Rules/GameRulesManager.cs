using System;
using Unity.Netcode;
using Unity.VisualScripting;

namespace Core.Rules
{
    public class GameRulesManager:IDisposable
    {
        
        private NetworkVariable<float> _timeGame;


        public Action<bool> isEndGame { get; private set; }

        public GameRulesManager(GameRulesMode gameRulesMode)
        {             
            
        }
        
        

        public void Dispose()
        {
            _timeGame?.Dispose();
        }
    }

    public enum GameRulesMode
    {
        ByTime,
        ByPoint
    }
}