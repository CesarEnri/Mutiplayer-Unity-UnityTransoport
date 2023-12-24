using TMPro;
using Unity.Services.Friends.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Samples.Friends.UGUI
{
    public class FriendEntryViewUGUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_NameText = null;
        [SerializeField] TextMeshProUGUI m_ActivityText = null;
        [SerializeField] Image m_PresenceColorImage = null;

        public string userId;
        
        public Button removeFriendButton = null;
        public Button blockFriendButton = null;
        public Button invitePlayerToParty = null;//this button only is avaliable in lobby scene

        public void Init(string playerName, string userIdParam, Availability presenceAvailabilityOptions, string activity)
        {
            m_NameText.text = playerName;
            var index = (int)presenceAvailabilityOptions - 1;
            var presenceColor = ColorUtils.GetPresenceColor(index);
            m_PresenceColorImage.color = presenceColor;
            m_ActivityText.text = activity;
            userId = userIdParam;
        }
    }
}