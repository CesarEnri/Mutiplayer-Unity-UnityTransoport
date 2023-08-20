﻿using System;
using Networking.Client;
using Networking.Host;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text queueStatusText;
        [SerializeField] private TMP_Text queueTimerText;
        [SerializeField] private TMP_Text findMatchButtonText;


        [SerializeField] private TMP_InputField joinCodeField;

        private bool _isMatchmaking;
        private bool _isCancelling;
        
        private void Start()
        {
            if (ClientSingleton.Instance == null) return;

            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

            queueStatusText.text = string.Empty;
            queueTimerText.text = string.Empty;
        }

        public async void FindMatchPressed()
        {
            if(_isCancelling) return;
            
            if (_isMatchmaking)
            {
                queueStatusText.text = "Cancelling...";
                _isCancelling = false;
                await ClientSingleton.Instance.GameManager.CancelMatchmaking();
                _isCancelling = false;
                _isMatchmaking = false;
                findMatchButtonText.text = "Find Match";
                queueStatusText.text = string.Empty;
                return;

            }
            
            ClientSingleton.Instance.GameManager.MatchmakerAsync(OnMatchMade);
            findMatchButtonText.text = "Cancel";
            queueStatusText.text = "Searching...";
            _isMatchmaking = true;
            
        }

        private void OnMatchMade(MatchmakerPollingResult result)
        {
            switch (result)
            {
                case MatchmakerPollingResult.Success:
                    queueStatusText.text = "Connecting...";
                    break;
                case MatchmakerPollingResult.TicketCreationError:
                    queueStatusText.text = "TicketCreationError";
                    break;
                case MatchmakerPollingResult.TicketCancellationError:
                    queueStatusText.text = "TicketCancellationError";
                    break;
                case MatchmakerPollingResult.TicketRetrievalError:
                    queueStatusText.text = "TicketRetrievalError";
                    break;
                case MatchmakerPollingResult.MatchAssignmentError:
                    queueStatusText.text = "MatchAssignmentError";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }


        public async void StartHost()
        {
            await HostSingleton.Instance.HostGameManager.StartHostAsync();
        }

        public async void StartClient()
        {
            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCodeField.text);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}