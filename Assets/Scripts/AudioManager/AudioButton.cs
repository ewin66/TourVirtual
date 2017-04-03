using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioButton : MonoBehaviour {
	
	public SoundDefinitions SoundDefinition = SoundDefinitions.BUTTON_TICK;
	Button myButton;

	void Awake() {
		
	}
	void Start()
	{
		//mAudioGameController = GameObject.FindGameObjectWithTag("MainManager").GetComponent<AudioInGameController>();
		myButton = GetComponent<Button> ();
		if (myButton != null)
			myButton.onClick.AddListener (PlaySound);
	}


	void OnDestroy() {
		if (myButton!= null)
			myButton.onClick.RemoveListener(PlaySound);
	}

	void PlaySound()
	{
		switch (SoundDefinition) 
		{
			case SoundDefinitions.BUTTON_TICK:
                AudioInGameController.Instance.PlayButtonTick();
			break;
			case SoundDefinitions.BUTTON_FORWARD:
                AudioInGameController.Instance.PlayButtonForward();
			break;
			case SoundDefinitions.BUTTON_BACKWARD:
                AudioInGameController.Instance.PlayButtonBackward();
			break;
			case SoundDefinitions.BUTTON_ACCEPT:
                AudioInGameController.Instance.PlayButtonAccept();
			break;
		}
	}

	//private AudioInGameController mAudioGameController;
}
