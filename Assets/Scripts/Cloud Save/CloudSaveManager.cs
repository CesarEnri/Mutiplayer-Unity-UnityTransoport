using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;

namespace Cloud_Save
{
    public class CloudSaveManager : MonoBehaviour
    {
        public TMP_Text status;
        public TMP_InputField inputToSave;

        private async Task Start()
        {
            await UnityServices.InitializeAsync();
        }

        public async void SaveData()
        {
            var data = new Dictionary<string, object> { { "FirstData", inputToSave.text } };
            await CloudSaveService.Instance.Data.ForceSaveAsync(data);
        }

        public async void LoadData()
        {
            var serverData = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string>{"FirstData"});

            if (serverData.TryGetValue("FirstData", out var value))
            {
                status.text = value;
            }
            else
            {
                Debug.Log("Key no found!");
            }

        }

        public async Task DeleteKey()
        {
            await CloudSaveService.Instance.Data.ForceDeleteAsync("FirstData");
        }


        public async Task RetriveAllKeys()
        {
            var allKeys = await CloudSaveService.Instance.Data.RetrieveAllKeysAsync();

            for (int i = 0; i < allKeys.Count; i++)
            {
             Debug.Log(allKeys[i]);   
            }
        }
    }
}
