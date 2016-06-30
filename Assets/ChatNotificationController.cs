﻿using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using SmartLocalization;
using UnityEngine.Assertions;

[RequireComponent(typeof(Animator))]
public class ChatNotificationController : MonoBehaviour
{

	[SerializeField, Range(0f, 50f)] private float _secondsVisible = 10f;
	[SerializeField] private Animator _animator;
	[SerializeField] private Text _titleText;
	[SerializeField] private Text _messageText;
	

	private string Message {
		get { return _messageText.text; }
		set { _messageText.text = value; }
	}

	private string ChannelName
	{
		get { return _titleText.text; }
		set { _titleText.text = value; }
	}

	//============================================================

	private void Awake ()
	{
		_animator = GetComponent<Animator> ();
	}

	private void OnDestroy()
	{
		_titleText = null;
		_messageText = null;
		_animator = null;
	}

	private void Start()
	{
		Assert.IsNotNull(_messageText, "ChatNotificationController::Start>> Message not defined!!");
	}
	
	//==============================================================================

	public void ShowMessage(string channelId, string txt)
	{
		ChannelName = GetChannelName(channelId);
		Message = txt.Split('#').Last();
		ShowNotification();
	}
	
	public void ShowNotification()
	{
		_animator.SetBool("IsOpen", true);
	}

	public void HideNotification()
	{
		StopCoroutine(Autoclose());
		_animator.SetBool ("IsOpen", false);
		Message = "";
		StartCoroutine (DisableMe());
	}

	public void StartTimerToAutoclose()
	{
		StartCoroutine(Autoclose());
	}


	//=======================================================
	
	private IEnumerator DisableMe()
	{
		while (_animator.GetCurrentAnimatorStateInfo(0).IsName("Open")) 
			yield return null;

		enabled = false;
	}

	private IEnumerator Autoclose()
	{
		yield return new WaitForSeconds(_secondsVisible);
		HideNotification();
	}

	//------------------------------------------------------------

	private static string GetChannelName(string channelId)
	{
		if (ChatManager.Instance.UserName != null) {
			int userNameIndex = channelId.IndexOf(ChatManager.Instance.UserName, StringComparison.Ordinal);
			if(userNameIndex > 0)
				return channelId.Remove(userNameIndex, ChatManager.Instance.UserName.Length).Trim(':');
		}

		return channelId;
	}
}
