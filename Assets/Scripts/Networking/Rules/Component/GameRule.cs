using System;
using Core.Player;
using Unity.Netcode;
using UnityEngine;

namespace Networking.Rules.Component
{
    public class GameRule: NetworkBehaviour
    {
        public NetworkVariable<int> MaxCoinsCollect = new();
        
        private const int InitialValueCoins = 1;
        private bool TimeOn;
        private float TimeLeft = 15;

        public override void OnNetworkSpawn()
        {
            if (IsServer || IsHost)
            {
                TimeOn = true;
                MaxCoinsCollect.Value = InitialValueCoins;
            }
            else
            {
                MaxCoinsCollect.OnValueChanged += HandleTimeRuleClient;
            }
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
                MaxCoinsCollect.Value = Convert.ToInt32(TimeLeft);    
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