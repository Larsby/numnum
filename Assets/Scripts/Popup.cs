using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup : MonoBehaviour {

	public enum PopupButtonChoice
	{
		Unselected,
		OK,
		YES,
		NO,
		ABORT
	};

	public Button yesButton;
	public Button noButton;
	public Button okButton;
	public Button abortButton;
	public Text breadText;
	public Text header;
	public bool singleButton = false;
	public bool useHeader = false;

	private System.Action<PopupButtonChoice> callback;
	private PopupButtonChoice buttonChoice = PopupButtonChoice.Unselected;

	void Start () {
		yesButton.onClick.AddListener (YesButton);
		noButton.onClick.AddListener (NoButton);
		okButton.onClick.AddListener (OkButton);
		if (abortButton != null)
			abortButton.onClick.AddListener (AbortButton);

		HideUnhide ();
	}

	private void HideUnhide() {
		yesButton.gameObject.SetActive (!singleButton);
		noButton.gameObject.SetActive (!singleButton);
		okButton.gameObject.SetActive (singleButton);
		#if UNITY_IOS
		okButton.gameObject.SetActive (!singleButton);
		#endif
		if (header != null) {
			header.gameObject.SetActive (useHeader);
		}
	}

	void Update () {
	}

	private void ShowHide(bool show) {
		for (int i = 0; i < transform.childCount; i++) {
			transform.GetChild (i).gameObject.SetActive (show);
		}
	}

	private void Close() {
		ShowHide (false);
		if (callback != null)
			callback (buttonChoice);
	}

	public PopupButtonChoice GetButtonChoice() {
		return buttonChoice;
	}

	public void YesButton() {
		buttonChoice = PopupButtonChoice.YES;
		Close ();
	}

	public void NoButton() {
		buttonChoice = PopupButtonChoice.NO;
		Close ();
	}

	public void OkButton() {
		buttonChoice = PopupButtonChoice.OK;
		Close ();
	}

	public void AbortButton() {
		buttonChoice = PopupButtonChoice.ABORT;
		Close ();
	}

	public void Show(System.Action<PopupButtonChoice> callback) {
		buttonChoice = PopupButtonChoice.Unselected;
		ShowHide (true);
		this.callback = callback;
		HideUnhide ();
	}

	public void ShowYesNo(System.Action<PopupButtonChoice> callback, bool bHeader=true, string bText=null, string hText=null, string YEStext=null, string NOtext=null) {
		buttonChoice = PopupButtonChoice.Unselected;
		this.callback = callback;
		useHeader = bHeader;
		singleButton = false;
		ShowHide (true);
		HideUnhide ();
		if (bText != null)
			breadText.text = bText;
		if (hText != null)
			header.text = hText;
		if (YEStext != null)
			yesButton.GetComponentInChildren<Text>().text = YEStext;
		if (NOtext != null)
			noButton.GetComponentInChildren<Text>().text = NOtext;
	}

	public void ShowOk(System.Action<PopupButtonChoice> callback, bool bHeader=true, string bText=null, string hText=null, string OKtext=null) {
		buttonChoice = PopupButtonChoice.Unselected;
		this.callback = callback;
		useHeader = bHeader;
		singleButton = true;
		ShowHide (true);
		HideUnhide ();
		if (bText != null)
			breadText.text = bText;
		if (hText != null)
			header.text = hText;
		if (OKtext != null)
			okButton.GetComponentInChildren<Text>().text = OKtext;
	}


}
