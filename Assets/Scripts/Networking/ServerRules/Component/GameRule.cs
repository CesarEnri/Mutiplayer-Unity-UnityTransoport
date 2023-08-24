using System;
using Core.Player;
using Networking.Host;
using UI;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Leaderboard = UI.Leaderboardtype.LeaderboardCoins.Leaderboard;

namespace Networking.ServerRules.Component
{
    public class GameRule: NetworkBehaviour
    {
        [SerializeField] private GameRuleHUD gameRuleHUD;
        
        public NetworkVariable<int> timeRuleInt = new();

        public NetworkVariable<FixedString32Bytes> gameQueueMode = new();
        
        //private const int InitialValueCoins = 1;
        private bool TimeOn;
        private float TimeLeft = 300;
        
        private string _gameQueueMode;

        [SerializeField] private Leaderboard leaderboard;
        public override void OnNetworkSpawn()
        {
            
            if (IsServer || IsHost)
            {
                TimeOn = true;
                //gameIsOver.Value = false;
                gameQueueMode.Value = IsHost ? HostSingleton.Instance.gameInfo.gameQueue.ToString()
                    :ServerSingleton.Instance.gameInfo.gameQueue.ToString();
                
                timeRuleInt.Value = (int)TimeLeft;
                HandledServerOnline();
            }
            
            
            if (IsClient)
            {
                HandledClientOnline();
            }

        }

        private void HandledServerOnline()
        {
            _gameQueueMode = gameQueueMode.Value.ToString();

            if (gameQueueMode.Value == GameQueue.SoloTimeMatch.ToString())
            {
                TimeOn = true;
            }
            
            //Client invoke when enter.
            gameQueueMode.Value = "";
            gameQueueMode.Value = _gameQueueMode;
        }

        public Action<GameRule> actionOnClient;
        
        private void HandledClientOnline()
        {

            if (gameQueueMode.Value == GameQueue.SoloTimeMatch.ToString())
            {
                timeRuleInt.OnValueChanged += HandleTimeRuleClient;
                HandleTimeRuleClient(0, timeRuleInt.Value);
            }
            
            actionOnClient.Invoke(this);
        }
        
        private void Update()
        {
            if (_gameQueueMode == GameQueue.SoloTimeMatch.ToString())
            {
                if(!TimerModeGame()) return;
                if (IsServer || IsHost)
                {
                    timeRuleInt.Value = Convert.ToInt32(TimeLeft);
                }
            }

            if (_gameQueueMode == GameQueue.SoloPointsMatch.ToString())
            {
                //set value for game
            }
            
            
            if (_gameQueueMode == GameQueue.SoloDeathMatch.ToString())
            {
                //set value for game
            }
        }

        private bool TimerModeGame()
        {
            if (!TimeOn) return TimeOn;
            
            if (TimeLeft > 0)
                TimeLeft -= Time.deltaTime;
            else
            {
                TimeLeft = 0;
                TimeOn = false;
                FinishTimeRule();
            }

            return TimeOn;
        }
        
        private void HandleTimeRuleClient(int previousvalue, int newvalue)
        {
            if (newvalue > 0) return;
            FinishTimeRule();
        }

        private void FinishTimeRule()
        {
            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                player.TryGetComponent(out PlayerMovement playerMovementScript);
                playerMovementScript.HandleMovementPlayer(false);
            }

            var results = leaderboard.GetFinallyScore();

            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    gameRuleHUD.firstPlace.text = results[i];
                }

                if (i == 1)
                {
                    gameRuleHUD.secondPlace.text = results[i];
                }

                if (i == 2)
                {
                    gameRuleHUD.thirdPlace.text = results[i];
                }
            }
            
            Debug.Log("End Game!");
            gameRuleHUD.partyByTimeInterface.SetActive(true);
        }
    }
}