﻿using System.Threading.Tasks;
using Networking.Client;
using Networking.Host;
using UnityEngine;

namespace Networking
{
    public class ApplicationController: MonoBehaviour
    {
        [SerializeField] private ClientSingleton clientPrefab;
        [SerializeField] private HostSingleton hostPrefab;
        
        private async void Start()
        {
            DontDestroyOnLoad(gameObject);

            await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);

        }


        private async Task LaunchInMode(bool isDedicatedServer)
        {
            if (isDedicatedServer)
            {
                
            }
            else
            {
                var hostSingleton = Instantiate(hostPrefab);
                hostSingleton.CreateHost();
                
                var clientSingleton = Instantiate(clientPrefab);
                var autenticated = await clientSingleton.CreateClient();

                if (autenticated)
                {
                    clientSingleton.ClientGameManager.GoToMenu();
                }

                //Go to Main Menu

            }
            
        }


    }
}