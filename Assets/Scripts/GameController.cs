using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	public IntermediateController interMediateControllerPrefab;

	public Button button1;
	public Button button2;
	public Button button3;
	public Button button4;
	public Button button5;
	public Button button6;
	public Button unlockButton;

	public Text sharkHiScore;
	public GameObject resetPanel;
	public Image opResetImage;
	public Sprite[] opResetSprites;

	public Sprite[] blueFishTextures;
	public Sprite[] redFishTextures;
	public Sprite[] flatFishTextures;
	public Sprite[] blowFishTextures;
	public Sprite[] brownFishTextures;
	public Sprite[] blackFishTextures;

	public GameObject fadePlane;
	public GameObject grid;
	private bool tableSelected = false;
	public AudioSource musicPlayer = null;

	public Image musicIcon;
	public Image sfxIcon;
	public Sprite musicOnSprite;
	public Sprite musicOffSprite;
	public Sprite sfxOnSprite;
	public Sprite sfxOffSprite;

	private IntermediateController iControl;

	public enum TableType { Undefined = -1, LillaPlus = 0, StoraPlus = 1, LillaMinus = 2, LillaMulti = 3, LillaDiv = 4, LillaMix = 5 };
	public enum GraphicsSetType { Fish_Blue, Fish_Red, Fish_Brown, Fish_Black, Fish_Flat, Fish_Blow };

	public GameObject [] levelIndicatorContainers;
	public GameObject indicatorOffPrefab;
	public GameObject indicatorOnPrefab;
	public GameObject indicatorJustSetPrefab;

	public RectTransform [] progressBars;

	public Image[] fishImages;

	private Image justSetIndicator = null;
	private int justSetStage = 0;

	private const float PROGRESS_WIDTH = 660;

	public GameObject sparkParticles;

	public const string unlockKey = "unlock";
	public int isUnlocked = 0;
	public GameObject adImage;
	private bool parentalGate = false;
	private bool restorePurchase = false;
	void Awake () {

		if (IntermediateController.instance == null) {
			Instantiate (interMediateControllerPrefab, Vector3.zero, Quaternion.identity); // Awake function of prefab is run immediately after Instantiate
		}
		iControl = IntermediateController.instance;

		if (!iControl.IsTexturesSet ()) {
			iControl.SetMusicPlayer (musicPlayer);
			iControl.SetTextureSet (blueFishTextures,  GameController.TableType.LillaPlus,  GameController.GraphicsSetType.Fish_Blue);
			iControl.SetTextureSet (redFishTextures,   GameController.TableType.StoraPlus,  GameController.GraphicsSetType.Fish_Red);
			iControl.SetTextureSet (flatFishTextures,  GameController.TableType.LillaMinus, GameController.GraphicsSetType.Fish_Flat);
			iControl.SetTextureSet (blowFishTextures,  GameController.TableType.LillaMulti, GameController.GraphicsSetType.Fish_Blow);
			iControl.SetTextureSet (blackFishTextures, GameController.TableType.LillaDiv,   GameController.GraphicsSetType.Fish_Black);
			iControl.SetTextureSet (brownFishTextures, GameController.TableType.LillaMix,   GameController.GraphicsSetType.Fish_Brown);
			iControl.SetTexturesFinished ();
		}
			
		if (button1) {
			button1.onClick.AddListener (() => {
				tableSelectOnClickEvent (0);
			});
			button2.onClick.AddListener (() => {
				tableSelectOnClickEvent (4);
			});
			button3.onClick.AddListener (() => {
				tableSelectOnClickEvent (2);
			});
			button4.onClick.AddListener (() => {
				tableSelectOnClickEvent (1); // the new shark mode; not implemented yet
			});
			button5.onClick.AddListener (() => {
				tableSelectOnClickEvent (3);
			});

			int nofLevelsStarted = IntermediateController.instance.Load ();
			if (nofLevelsStarted < 2) {
				button6.interactable = false;
			} else {
				button6.onClick.AddListener (() => {
					tableSelectOnClickEvent (5);
				});
			}


			if (PlayerPrefs.HasKey (unlockKey)) {
				isUnlocked = PlayerPrefs.GetInt (unlockKey);
			} else {
				PlayerPrefs.SetInt (unlockKey, 0);
				PlayerPrefs.Save ();
			}
		//	isUnlocked = 0;
			if (isUnlocked > 0) {
				unlockButton.gameObject.SetActive (false);
				adImage.gameObject.SetActive(false);
			}


			if (iControl.GetBestSharkScore() > 0) {
				sharkHiScore.text = "" + iControl.GetBestSharkScore();
			} else
				sharkHiScore.gameObject.SetActive (false);

			for (int j = 0; j < 4; j++) {
				int lvlIndex = j == 0? 0 : j + 1;
				float valuef = iControl.GetRelativeTableProgress(lvlIndex);

				if (iControl.GetTableLevel (lvlIndex) >= iControl.GetMaxLevel (lvlIndex))
					valuef = 1;

				// Debug.Log (j + " : " + valuef + "   " + iControl.GetTableLevel(j));

				if (iControl.GetFishWonLevel () == lvlIndex && iControl.WasProgressIncreased () && valuef < 1) {
					
					sparkParticles.transform.SetParent (progressBars [j].gameObject.transform, false);
					sparkParticles.SetActive (true);

					if (valuef > 0) {
						progressBarIndex = j;
						iTween.ValueTo (gameObject, iTween.Hash (
							"from", iControl.GetRelativeTableProgress (lvlIndex, true) * PROGRESS_WIDTH,
							"to", valuef * PROGRESS_WIDTH,
							"time", 0.8f,
							"onupdate", "IncreaseProgressAnim"));

						progressBars [j].sizeDelta = new Vector2 (iControl.GetRelativeTableProgress (lvlIndex, true) * PROGRESS_WIDTH, progressBars [j].sizeDelta.y);
						orgSparkX = sparkParticles.transform.position.x;
						sparkParticles.transform.position = new Vector3(orgSparkX + progressBars [j].sizeDelta.x / 15, sparkParticles.transform.position.y, sparkParticles.transform.position.z);

					} else {
						progressBars [j].sizeDelta = new Vector2 (valuef * PROGRESS_WIDTH, progressBars [j].sizeDelta.y);
						sparkParticles.transform.position = new Vector3(sparkParticles.transform.position.x + 54, sparkParticles.transform.position.y, sparkParticles.transform.position.z); // +54=approximate distance to small fish icon
						sparkParticles.gameObject.SetActive(false); // fix of some weird issue Simon had where the particles are shown in wrong place when fish was won. I cannot recreate this problem, so instead let's just remove the particles in this case...
					}
				}
				else
					progressBars [j].sizeDelta = new Vector2 (valuef * PROGRESS_WIDTH, progressBars [j].sizeDelta.y);
			}
				
			RectTransform rt = grid.GetComponent<RectTransform> ();
			rt.anchoredPosition = new Vector2 (rt.anchoredPosition.x, -(0 + ((float)Screen.height/(float)Screen.width) * 200f)); // ad hoc calculation of grid y pos depending on aspect ratio. Should be done in editor instead by having grid as child to logo?

			if (!iControl.IsMusicEnabled ())
				musicIcon.sprite = musicOffSprite;
			if (!iControl.IsSoundEffectsEnabled ())
				sfxIcon.sprite = sfxOffSprite;

			for (int i = 0; i < 5; i++) {
				for (int j = 0; j <= iControl.GetTableLevel(i); j++) {
					// Debug.Log(j + " " + iControl.GetTableLevel(i) + " " + iControl.GetFishWonLevel() + " " + i + " " + iControl.WasLevelIncreased());
					if (j == iControl.GetTableLevel(i) && iControl.GetFishWonLevel() == i && iControl.WasLevelIncreased()) {
						GameObject obj = Instantiate (indicatorOffPrefab, levelIndicatorContainers [i].gameObject.transform, false);
						justSetIndicator = obj.GetComponent<Image> ();
						justSetIndicator.color = iControl.IntColor (1, 65, 48);
						Invoke ("UpdateJustSetColor", 1f);
					} else
						Instantiate (indicatorOnPrefab, levelIndicatorContainers[i].gameObject.transform, false);
				}
				for (int j = 0; j < (iControl.GetNofFishInSet(i) - 1) - iControl.GetTableLevel(i); j++) {
					GameObject indicator = Instantiate (indicatorOffPrefab, levelIndicatorContainers[i].gameObject.transform, false);
				}
			}

			iControl.SetLevelIncreased (false);
			iControl.SetProgressIncreased (false);

			for (int i = 0; i < 4; i++) {
				int lvlIndex = i == 0? 0 : i + 1;

				fishImages [i].sprite = iControl.GetEnemyFishSprite (lvlIndex);
				// float progress = iControl.GetRelativeTableProgress (lvlIndex);
				// fishImages [i].color = new Color (1, 1, 1, progress + 0.33f);

				if (iControl.GetTableLevel (lvlIndex) >= iControl.GetMaxLevel (lvlIndex)) {
					fishImages [i].color = new Color (1, 1, 0, 0.3f); // do something when we are finished, put crown on fish?? Set diff color for now
				}

			}

		}
	}

	int progressBarIndex;
	float orgSparkX;
	public void IncreaseProgressAnim(float valuef){
		progressBars [progressBarIndex].sizeDelta = new Vector2 (valuef, progressBars [progressBarIndex].sizeDelta.y);
		//sparkParticles.transform.position = new Vector3(orgSparkX, sparkParticles.transform.position.y, sparkParticles.transform.position.z);
	}


	void UpdateJustSetColor() {
		justSetStage++;

		if (justSetStage == 1) {
			iControl.PlaySingleSfx (SingleSfx.AnswerCorrect);
			justSetIndicator.color = iControl.IntColor (79, 235, 138);
			Invoke ("UpdateJustSetColor", 0.07f);
		}
		if (justSetStage == 2) {
			justSetIndicator.color = iControl.IntColor (169, 255, 208);
			Invoke ("UpdateJustSetColor", 0.07f);
		}
		if (justSetStage == 3) {
			justSetIndicator.color = Color.white;
		}
	}


	void Start() {
		parentalGate = false;
		if (fadePlane != null) {
			fadePlane.SetActive (true);
			iTween.ColorTo (fadePlane, new Color (0, 0, 0, 0), 0.3f);
		}
	}

	private void fastIncreaseLevel(int levelIndex) {
		iControl.SetTable (levelIndex);
		iControl.IncreaseFishLevel ();
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Q))
			fastIncreaseLevel (0);
		if (Input.GetKeyDown (KeyCode.W))
			fastIncreaseLevel (2);
		if (Input.GetKeyDown (KeyCode.E))
			fastIncreaseLevel (3);
		if (Input.GetKeyDown (KeyCode.R))
			fastIncreaseLevel (4);
	}

	public void tableSelectOnClickEvent(int levelSelect)
	{
		if (levelSelect > 0 && isUnlocked == 0) {
			UnlockLevelsButton ();
			return;
		}

		if (tableSelected)
			return;
		tableSelected = true;

		iControl.PlaySingleSfx(SingleSfx.Button1);

		iControl.SetTable (levelSelect);
		iControl.SetLastLevelPlayed(levelSelect);
		if (fadePlane)
			iTween.ColorTo(fadePlane, new Color(0.05f, 0.5f, 0.6f), 0.3f); // works better (faster) than using LeanTween, for whatever reason
		Invoke ("tableSelectOnClickEventStep2", 0.4f);
	}

	private void tableSelectOnClickEventStep2() {
		SceneManager.LoadScene ("Cluster-FishSelect");
	}

	public void MusicToggleButton() {
		bool musicOn = !(iControl.IsMusicEnabled ());
		if (musicOn) {
			iControl.PlayMusic ();
			musicIcon.sprite = musicOnSprite;
		} else {
			iControl.StopMusic ();
			musicIcon.sprite = musicOffSprite;
		}

		iControl.SetMusicEnabled (musicOn);
		iControl.PlaySingleSfx(SingleSfx.Button2);
	}

	public void SfxToggleButton() {
		bool sfxOn = !(iControl.IsSoundEffectsEnabled ());

		if (sfxOn) {
			sfxIcon.sprite = sfxOnSprite;
		} else {
			sfxIcon.sprite = sfxOffSprite;
		}

		iControl.SetSoundEffectsEnabled (sfxOn);

		iControl.PlaySingleSfx(SingleSfx.Button2, true);
	}


	string [] resetQ = { "(12 x 3) - 14", "(18 - 3) x 4", "(5 x 20) / 2", "(14 x 2) + 17" };
	string [] resetA = { "22", "60", "50", "45" };
	private int activeResetPanelIndex;

	private void ShowResetPanel(int opIndex) {
		resetPanel.SetActive (true);
		opResetImage.sprite = opResetSprites [opIndex];
		activeResetPanelIndex = opIndex;

		Text Q = resetPanel.GetComponentInChildren<Text> ();
		Q.text = resetQ[opIndex] + " =";
		InputField A = resetPanel.GetComponentInChildren<InputField> ();
		A.text = "";
	}

	public void ShowResetPanelPlus() {
		parentalGate = false;
		ShowResetPanel (0);
	}
	public void ShowResetPanelMinus() {
		parentalGate = false;
		ShowResetPanel (1);
	}
	public void ShowResetPanelMulti() {
		parentalGate = false;
		ShowResetPanel (2);
	}
	public void ShowResetPanelDiv() {
		parentalGate = false;
		ShowResetPanel (3);
	}

	public void CancelResetPanel() {
		resetPanel.SetActive (false);
	}

	public void ShowParentalGate() {
		parentalGate = true;
		ShowResetPanel (Random.Range (0, 4));
	
	}
	public void OKResetPanel() {

		InputField A = resetPanel.GetComponentInChildren<InputField> ();

		if (A.text == resetA [activeResetPanelIndex]) {
			if (parentalGate == false) {
				iControl.SetTableLevel (activeResetPanelIndex == 0 ? 0 : activeResetPanelIndex + 1, 0);
				iControl.Save ();
				iControl.SetTexturesFinished ();
				SceneManager.LoadScene ("LevelSelect");
			} else {
				if (restorePurchase == false) {
					Purchaser p = GetComponent<Purchaser> ();
					p.BuyNonConsumable ();
				} else {
					Purchaser p = GetComponent<Purchaser> ();
					p.RestorePurchases ();
				}

			}
		} else {
			parentalGate = false;
			iTween.MoveFrom (resetPanel, new Vector3 (resetPanel.transform.position.x + 50, resetPanel.transform.position.y, resetPanel.transform.position.z), 0.5f);
		}
	}


	public void NotifyPurchaseMade() {
		PlayerPrefs.SetInt (unlockKey, 1);
		PlayerPrefs.Save ();

		SceneManager.LoadScene ("LevelSelect");
		// SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void NotifyUnlock(Popup.PopupButtonChoice buttonChoice) {
		if (buttonChoice == Popup.PopupButtonChoice.YES) {
			restorePurchase = false;
			ShowParentalGate ();
		}
		if (buttonChoice == Popup.PopupButtonChoice.OK) {
			restorePurchase = true;
			ShowParentalGate ();

		}
	
	}
	public void UnlockLevelsButton() {
		Popup popup = GameObject.FindObjectOfType<Popup> ();
		if (popup != null)
			popup.ShowYesNo (NotifyUnlock);
	}

}
