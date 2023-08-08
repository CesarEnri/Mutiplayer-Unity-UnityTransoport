﻿using System.Threading.Tasks;
using Networking.Client;
using UnityEngine;

namespace Networking.Host
{
    public class HostSingleton: MonoBehaviour
    {
        private static HostSingleton instance;
        private HostGameManager _hostGameManager;

        private static HostSingleton Instace
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindObjectOfType<HostSingleton>();

                if (instance == null)
                {
                    Debug.LogError("No HostSingleton in the scene!");
                    return null;
                }

                return instance;
            }
        }
        

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CreateHost()
        {
            _hostGameManager = new HostGameManager();
        }
    }
}