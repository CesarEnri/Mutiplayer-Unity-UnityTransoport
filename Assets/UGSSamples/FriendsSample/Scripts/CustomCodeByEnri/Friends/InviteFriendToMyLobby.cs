using System.Threading.Tasks;
using TMPro;
using Unity.Services.Friends;
using Unity.Services.Friends.Notifications;
using Unity.Services.Samples.Friends.UGUI;
using UnityEngine;

namespace Networking.Friends
{
    public class InviteFriendToMyLobby : MonoBehaviour
    {
        public static InviteFriendToMyLobby Instance { get; private set; }
        
        public TMP_InputField targetUserId, lobby;

        public TMP_Text labelText;
        
        private void Start()
        {
            Instance = this;
            
            SubscribeToMessageOtherUser();
        }

        private async void SubscribeToMessageOtherUser()
        {
            await FriendsService.Instance.InitializeAsync();
            FriendsService.Instance.MessageReceived += ReceivedInvitation;
            Debug.Log("Conectado");
        }


        public void CreateInvitation(string toFriendId)
        {
            SendInvitation(toFriendId, lobby.text ); 
        }


        private async void SendInvitation(string toFriendId, string joinCode)
        {
            await FriendsService.Instance.InitializeAsync();
            var message = new LobbyCodeMessage
            {
                LobbyJoinCode = joinCode 
            };
            await FriendsService.Instance.MessageAsync(toFriendId, message);
        }

        
        private void ReceivedInvitation(IMessageReceivedEvent @event)
        {
            var user = @event.UserId;
            var codeLobby = @event.GetAs<LobbyCodeMessage>();

            Debug.Log(codeLobby.LobbyJoinCode);

            labelText.text = user + " ---- " + codeLobby.LobbyJoinCode;
        }

        private void OnDestroy()
        {
            FriendsService.Instance.MessageReceived -= ReceivedInvitation;
        }
    }
}