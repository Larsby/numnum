using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShowMenuOnTouch : MonoBehaviour
{

	private bool toggle;
	public GameObject thePnl;
	public Button button;
	public Sprite show;
	public Sprite hide;
	public GameController gameManager;
	public GameObject hideWhenShown;
	void Start ()
	{
		toggle = true;
		button = GetComponent<Button> ();
	}

	public void ToggleMenu ()
	{
//		gameManager.ToggleSettingsPanel ();

		if (toggle) {
			button.image.sprite = hide;
			thePnl.SetActive (true);
		} else { 
			button.image.sprite = show;
			thePnl.SetActive (false);
		}
		if (hideWhenShown != null) {
			if (toggle) {
				hideWhenShown.SetActive (false);
				Time.timeScale = 0;
			} else {
				hideWhenShown.SetActive (true);
				Time.timeScale = 1;

			
			}
		}
		toggle = !toggle;


	}

	void OnMouseDown ()
	{
		ToggleMenu ();
	}


}
