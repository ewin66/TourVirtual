﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SmartLocalization;

public class LoadingContentText : MonoBehaviour {
	

	public static LoadingContentText Instance { get; private set; }
	Text TextField;


	void Awake () {
		Instance = this;
		TextField = GetComponent<Text> ();
		TextField.text = "";
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void SetText(string locID) {
		Instance.TextField.text = LanguageManager.Instance.GetTextValue(locID);
	}
}
