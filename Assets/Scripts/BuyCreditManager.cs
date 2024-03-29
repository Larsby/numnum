using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class BuyCreditManager : MonoBehaviour {

	public Text creditsText;
	public GameObject buyButtonPrefab;
	public GameObject buttonContainer;
	public GameObject purchaseButton;
	private void CreateButton(int prize, int credits, string preText = "", string postText = "") {

		string buttontext = preText + prize + " - " + credits + postText;

		GameObject g = Instantiate (buyButtonPrefab, buttonContainer.transform, false);
		Text t = g.GetComponentInChildren<Text> ();
		t.text = buttontext;
		if (PlayerPrefs.HasKey ("icecream") == false) {
			Button b = g.GetComponentInChildren<Button> ();

			b.onClick.AddListener (() => {
				GetComponent<Purchaser> ().BuyNonConsumable ();
				//Buy (prize, credits);
			});

		} else {
			purchaseButton.SetActive (false);
		}
	}

	void Start () {

	//	creditsText.text = "" + StaticManager.GetNumberOfCredits ();
		/*
		CreateButton (10, 300);
		CreateButton (30, 1000);
		CreateButton (50, 2000);
		CreateButton (100, 5000, "MEGA! - ");
		*/
		if (PlayerPrefs.HasKey ("icecream") == true) {
			//purchaseButton.GetComponent<Button> ().enabled = false;
			purchaseButton.SetActive(false);
		}

	}
	
	void Update () {
	}


	public void BackButton() {
		SceneManager.LoadScene ("Main");
	}
	public void UnlockLevel() {
		PlayerPrefs.SetInt ("icecream", 1);
		PlayerPrefs.Save ();
		purchaseButton.GetComponent<Button> ().enabled = false;
	}
	public void Purchase() {
		gameObject.GetComponent<Purchaser> ().BuyNonConsumable ();
	
	}
	public void Restore() {
		gameObject.GetComponent<Purchaser> ().RestorePurchases ();
	}
	public void Buy(int prize, int credits) {

		// Debug.Log (prize + " " + credits);

	//	StaticManager.AddCredits (credits);
	//	creditsText.text = "" + StaticManager.GetNumberOfCredits ();
	}

}
