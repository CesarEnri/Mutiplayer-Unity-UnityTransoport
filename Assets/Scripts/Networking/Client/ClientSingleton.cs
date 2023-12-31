﻿using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Client
{
    public class ClientSingleton : MonoBehaviour
    {
        private static ClientSingleton instance;

        public ClientGameManager GameManager { get; private set; }

        public GameInfo gameInfo;
        
        public static ClientSingleton Instance
        {
            get
            {
                if (instance != null) { return instance; }

                instance = FindObjectOfType<ClientSingleton>();

                if (instance == null)
                {
                    return null;
                }

                return instance;
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> CreateClient()
        {
            GameManager = new ClientGameManager();

            return await GameManager.InitAsync();
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}