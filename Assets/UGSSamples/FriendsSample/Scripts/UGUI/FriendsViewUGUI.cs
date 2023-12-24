using System;
using System.Collections;
using System.Collections.Generic;
using Networking.Friends;
using Unity.Services.Samples.Friends;
using Unity.Services.Samples.Friends.UGUI;
using UnityEngine;
using UnityEngine.UI;


    public class FriendsViewUGUI : ListViewUGUI, IFriendsListView
    {
        [SerializeField] RectTransform m_ParentTransform = null;
        [SerializeField] FriendEntryViewUGUI m_FriendEntryViewPrefab = null;

        List<FriendEntryViewUGUI> m_FriendEntries = new List<FriendEntryViewUGUI>();
        List<FriendsEntryData> m_FriendsEntryDatas = new List<FriendsEntryData>();

        public Action<string> onRemove { get; set; }
        public Action<string> onBlock { get; set; }

        public Action<string> OnInvite { get; set; }

        public void BindList(List<FriendsEntryData> friendEntryDatas)
        {
            m_FriendsEntryDatas = friendEntryDatas;
        }

        public override void Refresh()
        {
            m_FriendEntries.ForEach(entry => Destroy(entry.gameObject));
            m_FriendEntries.Clear();

            
            foreach (var friendsEntryData in m_FriendsEntryDatas)
            {
                var entry = Instantiate(m_FriendEntryViewPrefab, m_ParentTransform);
                entry.Init(friendsEntryData.Name, friendsEntryData.Id,  friendsEntryData.Availability, friendsEntryData.Activity);
                entry.removeFriendButton.onClick.AddListener(() =>
                {
                    onRemove?.Invoke(friendsEntryData.Id);
                    entry.gameObject.SetActive(false);
                });
                entry.blockFriendButton.onClick.AddListener(() =>
                {
                    onBlock?.Invoke(friendsEntryData.Id);
                    entry.gameObject.SetActive(false);
                });
                entry.invitePlayerToParty.onClick.AddListener(() => 
                {
                    //StartCoroutine(DisableButtonInvite(()));
                    OnInvite?.Invoke(friendsEntryData.Id);
                });
                m_FriendEntries.Add(entry);
            }
        }


        private void Start()
        {
            OnInvite += SendInvitation;
        }

        private void SendInvitation(string userId)
        {
            //StartCoroutine(DisableButtonInvite());
            InviteFriendToMyLobby.Instance.CreateInvitation(userId);
        }

        IEnumerator DisableButtonInvite(Button gameObject)
        {
            //gameObject.SetActive(false);
            yield return new WaitForSeconds(4f);
            //gameObject.SetActive(true);
        }
    }
