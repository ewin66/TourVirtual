using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GUIGameScreen : GUIScreen
{
	public Text RoomTitle;
	public Transform ButtonContainer;
	public ChatNotificationController _chatNotifier;
	
	public override void Awake () {
		base.Awake ();
		RoomManager.Instance.OnSceneReady += HandleOnSceneChange;
		ChatManager.Instance.OnMessagesChange += Messages_OnChangeHandle;
		_chatNotifier.gameObject.SetActive (false);
	}
	
	public override void Start () {
		base.Start ();
		_viewContentButton = ButtonContainer.FindChild("View Button").gameObject;
		_shopContentButton = ButtonContainer.transform.FindChild("Buy Button").gameObject;
		_playContentButton = ButtonContainer.transform.FindChild("Play Button").gameObject;
		UpdateRoomTitle();
	}
	
	void HandleOnSceneChange () {
		UpdateRoomTitle();
	}
	
	void UpdateRoomTitle() {
		if (RoomManager.Instance.Room != null) {
			RoomTitle.text = RoomManager.Instance.Room.Name;
		}
	}
	
	void OnEnable() {
	}
	
	void UpdateTitle() {
		GameObject titleObj = GameObject.Find ("Level Name");
		if (titleObj != null) {
			titleObj.GetComponent<Animator>().SetBool("IsOpen", true);
		}
	}
	
	public override void CloseWindow() {
		GameObject titleObj = GameObject.Find ("Level Name");
		if (titleObj != null) {
			if (titleObj.GetComponent<Animator>().GetBool("IsOpen"))
				titleObj.GetComponent<Animator>().SetBool("IsOpen", false);
		}
	}
	
	public override void Update()
	{
		base.Update();
		
		bool activateView = NeedViewButton;
		
		if (_viewContentButton != null &&
		    _viewContentButton.activeSelf != activateView)
		{
			
			_viewContentButton.SetActive(activateView);
			
			if (activateView)
			{
				_viewContentButton.transform.localPosition = Vector3.zero;
				_viewContentTween = Go.from(
					_viewContentButton.transform, 2, new GoTweenConfig()
					.position(_viewContentButton.transform.position + new Vector3(300, 0, 0))
					.setEaseType(GoEaseType.ElasticOut)
					);
			}
		}
		/*
        bool activateShop = NeedShopButton;
        if (_shopContentButton != null &&
            _shopContentButton.activeSelf != activateShop)
        {

            _shopContentButton.SetActive(activateShop);

            if (activateShop)
            {
                _shopContentButton.transform.localPosition = Vector3.zero;
                _shopContentTween = Go.from(
                    _shopContentButton.transform, 2, new GoTweenConfig()
                    .position(_shopContentButton.transform.position + new Vector3(300, 0, 0))
                    .setEaseType(GoEaseType.ElasticOut)
                    );
            }
        }
        */
		bool activatePlay = NeedPlayButton;
		if (_playContentButton != null &&
		    _playContentButton.activeSelf != activatePlay)
		{
			_playContentButton.SetActive(activatePlay);
			
			if (activatePlay)
			{
				_playContentButton.transform.localPosition = Vector3.zero;
				_playContentTween = Go.from(
					_playContentButton.transform, 2, new GoTweenConfig()
					.position(_playContentButton.transform.position + new Vector3(300, 0, 0))
					.setEaseType(GoEaseType.ElasticOut)
					);
			}
		}
	}
	
	bool NeedViewButton {
		get {
			return ContentManager.Instance.ContentNear && !ContentManager.Instance.ContentNear.ContentKey.Contains("JUEGO");
		}
	}
	
	bool NeedShopButton {
		get {
			return ContentInfo.ContentSelected != null && ContentInfo.ContentSelected.Money;
		}
	}
	
	bool NeedPlayButton {
		get {
			return ContentManager.Instance.ContentNear && ContentManager.Instance.ContentNear.ContentKey.Contains("JUEGO") && !HiddenObjects.HiddenObjects.Instance.enabled;
		}
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="channelName"></param>
	private void Messages_OnChangeHandle(string channelName)
	{
		if(channelName == ChatManager.Instance.RoomChannel)
			return;

		ChatMessage msg = ChatManager.Instance.GetMessagesFromChannel(channelName).LastOrDefault();
		if (msg != null) {
			_chatNotifier.gameObject.SetActive(true);
			_chatNotifier.ShowMessage(channelName, msg.Text);
		}
	}
	
	private GameObject _viewContentButton;
	private GameObject _shopContentButton;
	private GameObject _playContentButton;
	protected AbstractGoTween _viewContentTween;
	protected AbstractGoTween _shopContentTween;
	protected AbstractGoTween _playContentTween;
}
