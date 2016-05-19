﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.Chat;
using SmartLocalization;

public class ChatMessage {
	public string Sender;
	public string Text;
	public bool Readed;

	public ChatMessage(string sender, string text, bool readed) {
		Sender = sender;
		Text = text;
		Readed = readed;
	}
}

public class ChatManager : Photon.PunBehaviour, IChatClientListener {

	static public string CHANNEL_COMMUNITYMANAGER = "Community Manager";

	static public string ROOM_CHANNEL_NAME {
		get{
			return LanguageManager.Instance.GetTextValue("TVB.Chat.RoomChannelName");
		}
	}

	public delegate void MessagesChangeEvent(string channelName);
	public event MessagesChangeEvent OnMessagesChange;

	static public ChatManager Instance {
		get {
			GameObject chatManagerObj = GameObject.FindGameObjectWithTag("ChatManager");
			return chatManagerObj != null ? chatManagerObj.GetComponent<ChatManager>() : null;
		}
	}

	public string ChatAppId;                    // set in inspector. Your Chat AppId (don't mix it with Realtime/Turnbased Apps).

	public int MessagesFromHistory = -1;

	public string UserName { get; set; }
	public ChatClient ChatClient;

	private string _channelSelected;
	public string ChannelSelectedId {
		get {
			return _channelSelected;
		}

		set {
			_channelSelected = value;
		}
	}

	public string ChannelSelectedName {
		get {
			return GetChannelName(_channelSelected);
		}
	}

	public List<ChatMessage> Messages {
		get {
			return GetMessagesFromChannel(ChannelSelectedId);
		}
	}

	public List<ChatMessage> GetMessagesFromChannel(string channelId) {
		string channelName = GetChannelName(channelId);
		return History.ContainsKey(channelName) ? History[channelName] : new List<ChatMessage>();
	}

	public string GetChannelName(string channelId) {
		return IsPublicChannel(channelId) ? channelId : ChatClient.GetPrivateChannelNameByUser(channelId);
	}

	public Dictionary<string, List<ChatMessage>> History = new Dictionary<string, List<ChatMessage>>();

	void Start () {
	}

	public IEnumerator Connect() {
		if (string.IsNullOrEmpty(this.UserName)) {
			UserName = PhotonNetwork.player.name;
		}
		
		ChatClient = new ChatClient(this);
		ChatClient.Connect(ChatAppId, "1.0", UserName, null);

		while (!ChatClient.CanChat) {
			yield return null;
		}
	}

	/// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
	public void OnApplicationQuit() {
		if (ChatClient != null) {
			ChatClient.Disconnect();
		}
	}
	
	public void Update() {
		if (ChatClient != null) {
			ChatClient.Service();  // make sure to call this regularly! it limits effort internally, so calling often is ok!
		}
	}

	public void SendMessage(string text) {
		string chn = _channelSelected ?? CHANNEL_COMMUNITYMANAGER;
#if UNITY_EDITOR
		Debug.LogError("[SendMessage] in <" + name + ">: Se enviará al canal: " + chn);
#endif
		SendMessage(chn, text);
	}

	public void SendMessage(string channelName, string text) {
		if (IsPublicChannel(channelName)) {
            ChatClient.PublishMessage(channelName, text);
		}
		else {
            ChatClient.SendPrivateMessage(channelName, text);
		}
	}

	public void OnConnected() {
		//Debug.Log (">>> Chat OnConnected");
		ChatClient.SetOnlineStatus(ChatUserStatus.Online);
		ChatClient.Subscribe( new string[] { CHANNEL_COMMUNITYMANAGER }, 0 );
	}
	
	public void OnDisconnected() {
		//Debug.Log (">>> Chat OnDisconnected");
        if (!PhotonHandler.AppQuits)
            ChatClient.Connect(ChatAppId, "1.0", UserName, null);
	}
	
	public void OnChatStateChange(ChatState state) {
		//Debug.Log (">>> Chat OnChatStateChange: " + state.ToString());
	}
	
	public void OnSubscribed(string[] channels, bool[] results)	{
		//Debug.LogError (">>> Chat OnSubscribed to: " + channels.stringArrayToString());
	}
	public void OnUnsubscribed(string[] channels) {
		//Debug.Log (">>> Chat OnUnsubscribed to: " + channels.stringArrayToString());
	}

    public override void OnDisconnectedFromPhoton()
    {
        OnDisconnected();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages) {
	// Debug.Log (string.Format ("OnGetMessages [{0}]", channelName));

		if (!History.ContainsKey(channelName)) {
			History.Add(channelName, new List<ChatMessage>());
		}

		for ( int i = 0; i < senders.Length; i++ ) {
			History[channelName].Add(new ChatMessage(senders[i], messages[i] as string, false));
		}

		if (OnMessagesChange != null /*&& channelName.Equals(ChannelSelectedName)*/) {
			OnMessagesChange(channelName);
		}
	}
	
	public void OnPrivateMessage(string sender, object message, string channelName) {
//		Debug.Log (string.Format ("OnPrivateMessage [{0}]: {1}: {2}", channelName, sender, message));
		if (!History.ContainsKey(channelName)) {
			History.Add(channelName, new List<ChatMessage>());

			ChatChannel ch = ChatClient.PrivateChannels[ channelName ];
			foreach ( object msg in ch.Messages ) {
				// Debug.Log (string.Format("PrivateMessage: {0} -> {1}: {2}", sender, UserName, msg));
				History[channelName].Add(new ChatMessage(sender, msg as string, false));
			}
		}
		else {
			History[channelName].Add(new ChatMessage(sender, message as string, false));
		}

		if (OnMessagesChange != null /*&& channelName.Equals(ChannelSelectedName)*/) {
			OnMessagesChange(channelName);
		}
	}
	
	public void OnStatusUpdate(string user, int status, bool gotMessage, object message) {
//		Debug.Log ("OnStatusUpdate");
	}

	public override void OnJoinedRoom() {
		_roomChannel = PhotonNetwork.room.name.Split('#')[0];
		#if UNITY_EDITOR
			Debug.Log ("OnJoinedRoom");
			Debug.LogError (">>> JOINED THE ROOM: " + _roomChannel);
		#endif

        if (ChatClient != null && ChatClient.CanChat) {
			ChatClient.Subscribe( new string[] { _roomChannel }, MessagesFromHistory );
		}

		ChannelSelectedId = _roomChannel;
	}
	
	public override void OnLeftRoom() {
		if (_roomChannel != null) {
			/*
			if (History.ContainsKey(_channelSubscription)) {
				History[_channelSubscription].Clear();
			}

			if (OnPublicMessagesChange != null) {
				OnPublicMessagesChange();
			}
			*/

			if (ChatClient != null && ChatClient.CanChat) {
				ChatClient.Unsubscribe( new string[] { _roomChannel } );
			}
			//_roomChannel = null;
		}
	}

	public bool IsPublicChannel(string channel) {
		return channel.Equals(ChatManager.CHANNEL_COMMUNITYMANAGER) || channel.Equals(_roomChannel);
	}

	private string _roomChannel;
}
