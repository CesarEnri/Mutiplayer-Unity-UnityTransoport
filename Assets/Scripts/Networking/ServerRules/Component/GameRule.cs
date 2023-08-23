using System;
using Core.Player;
using Networking.Client;
using Networking.Host;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Networking.ServerRules.Component
{
    public class GameRule: NetworkBehaviour
    {
        [SerializeField] private GameRuleDisplay GameRuleDisplay;
        //public NetworkVariable<GameQueue> gameRulesModeNetworkVariable  = new();
        
        public NetworkVariable<int> maxCoinsCollect = new();
        public NetworkVariable<FixedString32Bytes> nameGameQueueMode = new();
        
        private const int InitialValueCoins = 1;
        private bool TimeOn;
        private float TimeLeft = 15;

        private string _nameGameQueue ="";
        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                TimeOn = true;
                maxCoinsCollect.Value = InitialValueCoins;
                
                _nameGameQueue = HostSingleton.Instance.gameQueue.ToString();
                nameGameQueueMode.Value = _nameGameQueue;

                if (_nameGameQueue == "Solo")
                {
                    TimeLeft = 40;
                }

                OnClientConnect();

            }


            if (IsServer)
            {
                TimeOn = true;
                maxCoinsCollect.Value = InitialValueCoins;
                
                _nameGameQueue = ServerSingleton.Instance.gameQueue.ToString();
                nameGameQueueMode.Value = _nameGameQueue;

                OnClientConnect();
            }
            
            
            if (IsClient)
            {
                maxCoinsCollect.OnValueChanged += HandleTimeRuleClient;
                HandleTimeRuleClient(0, maxCoinsCollect.Value);
                nameGameQueueMode.OnValueChanged += SetModeClient;
                SetModeClient("", nameGameQueueMode.Value);
            }

        }

        private void SetModeClient(FixedString32Bytes previousvalue, FixedString32Bytes newvalue)
        {
            GameRuleDisplay.SetValueGameQueue(previousvalue, newvalue);
        }

        private void OnClientConnect()
        {
            nameGameQueueMode.Value = "";
            nameGameQueueMode.Value = _nameGameQueue;
        }


        private void HandleTimeRuleClient(int previousvalue, int newvalue)
        {
            if (newvalue <= 0)
            {
                HandleTimeRule();
            }
        }
        private void Update()
        {
            if (TimeOn)
            {
                if (TimeLeft > 0)
                {
                    TimeLeft -= Time.deltaTime;
                }
                else
                {
                    //Debug.Log("Time is up");
                    TimeLeft = 0;
                    TimeOn = false;
                    HandleTimeRule();
                }
            }
            if (IsServer || IsHost)
            {
                maxCoinsCollect.Value = Convert.ToInt32(TimeLeft);
            }
        }
        
        private static void HandleTimeRule()
        {
            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                player.TryGetComponent(out PlayerMovement playerMovementScript);
                playerMovementScript.HandleMovementPlayer(false);
            }
        }

    }
}