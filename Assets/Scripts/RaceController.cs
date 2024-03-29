using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RaceController : MonoBehaviour
{

	public FishController fishPlayer;
	public FishController fishEnemy;
	private int questionIndex;
	private QAcollection qac;

	public Text answerLeft;
	public Text answerRight;
	public Text answerUp;
	public Text answerDown;
	public Text questionText;
	public Text readyText;
	public GameObject endLine;
	public GameObject responseBox;
	public EdibleController ediblePrefab;

	public float answerDelay;
	private float answerDelayCounter;
	private int previousAnswerIndex;
	private int touchAnswer = -1;
	private float endCounter = -1;

	public TextAsset testJson;

	public Image progressPlayer;
	public Image progressEnemy;
	private float progressMaxWidth;
	private RectTransform progressEnemyTransform;
	private RectTransform progressPlayerTransform;
	private IntermediateController iControl;
	public GameObject interMediateControllerPrefab;
	private Camera cam;
	private float camInitialZoom;
	private float camMinimumLeft;

	enum Evaluation
	{
		Correct,
		Incorrect,
		Reset}
	;

	enum RaceState
	{
		PreRace,
		Race,
		PrePostRace,
		PostRace}
	;

	private RaceState raceState = RaceState.PreRace;
	private float preRaceDelay = 0.5f, preRaceDelayTimer;
	private bool bRaceStarted = false;
	private float endYMove = 0;
	private bool skipDance = false;
	private bool playerWon = false;

	private float prevCamZoom = -64;
	private float screenWidthUnits;

	private CanvasGroup responseCanvas;
	public float fadeTimeUI = 1.0f;

	public Canvas canvasRef;

	public GameObject onOffParticles;
	public GameObject onOffFish;
	public GameObject onOffJelly;
	public GameObject onOffSurface1;
	public GameObject onOffSurface2;
	public GameObject onOffBubbles;
	public GameObject onOffSeabed;
	public GameObject onOffRocks;
	public GameObject onOffBackdrop;
	public GameObject onOffWeed;
	public GameObject onoOffButtons;
	public Text fpsText;

	public CloudController[] clouds;

	public Image doffeGladImage;
	public Image doffeSadImage;
	private float doffeSadTimer = -1000;

	public GameObject fadePlane;
	private bool hasFadedIn = false;

	public GameObject victoryTransform;
	public GameObject victoryFish;
	public Image fishGainImage;
	public Image fishGainCoverImage;
	public Animation victoryFishAnimation;
	private CanvasGroup fishGainCanvasGroup;
	private Sprite enemySprite;
	private bool levelIncreased = false;

	private float fps_accum = 0;
	private int fps_frames_count = 0;
	private float fps_timeleft;
	private  float fps_updateInterval = 0.5f;

	private int baseFishDebugIndex = -1;

	private bool isLeading = true, isLeadingSet = false;
	private float farBehindTimer = 0;
	public CanvasGroup backCanvas;
	public GameObject responseScaler;
	public GameObject countScaler;

	private SingleSfx closeClapType = SingleSfx.Undefined;
	private float closeClapTimer = 0;

	public Sprite[] answerPastilles;

	public Material[] colorizedMaterials;
	private bool returningToMain = false;

	public GameObject doffeContainer;
	public GameObject noffeContainer;

	public Animator bubblePopper;

	private void PopulateVisualOptions ()
	{
		answerLeft.text = qac.questions [questionIndex].options [0].o;
		answerUp.text = qac.questions [questionIndex].options [1].o;
		answerRight.text = qac.questions [questionIndex].options [2].o;
		answerDown.text = qac.questions [questionIndex].options [3].o;
		questionText.text = qac.questions [questionIndex].question;
		questionText.text = questionText.text.Replace (" ", "");
	}

	void Awake ()
	{
		iControl = IntermediateController.instance;
		if (iControl == null) {
			Instantiate (interMediateControllerPrefab, Vector3.zero, Quaternion.identity);
			iControl = IntermediateController.instance;
		}

		responseCanvas = responseBox.GetComponent<CanvasGroup> ();
	}

	void Start ()
	{
		QACollectionProducer qp = new QACollectionProducer ();
		//qac = qp.GetCollection (testJson.ToString());

		qac = qp.ProduceCollection ("Stora plus med tiotalsövergång", 16, 4, QACollectionProducer.MathOperation.Plus, 10, 10, 1, 9, QACollectionProducer.Op2Type.AllowSwitch, 20, 7, 0);

/*
		for (int l = 0; l < 6*6; l++) {
			QAcollection testQ = MakeSingleCollection (qp, 0, l, true, -1);
			Debug.Log(JsonUtility.ToJson(testQ));
		}
*/

		fps_timeleft = fps_updateInterval;

		if (iControl != null) {
			qac = MakeCollection (qp);
		}

		if (qac == null)
			qac = qp.GetCollection ("{\"name\":\"Invalid data\",\"nofOptions\":4,\"questions\":[{\"answerIndex\":-1,\"question\":\"?\",\"options\":[{\"o\":\"?\"},{\"o\":\"?\"},{\"o\":\"?\"},{\"o\":\"?\"}]}]}");
		//Debug.Log(JsonUtility.ToJson(qac));

		questionIndex = 0;
		answerDelayCounter = 0;
		PopulateVisualOptions ();

		cam = Camera.main;
		camInitialZoom = 999;

		Vector3 oldCamPos = cam.transform.position;
		cam.transform.position = new Vector3 (0, 0, -90);
		Vector3 pos = cam.ViewportToWorldPoint (new Vector3 (1, 0, -cam.transform.position.z - 0.5f));

		GameObject fl = GameObject.Find ("finish_line");
		endLine.gameObject.transform.position = new Vector3 (fl.transform.position.x, transform.position.y, -0.5f);
//		endLine.gameObject.transform.position = new Vector3 (pos.x * 6 -3, transform.position.y, -0.5f);

		cam.transform.position = oldCamPos;

		progressPlayerTransform = progressPlayer.GetComponent<RectTransform> ();
		progressMaxWidth = progressPlayerTransform.rect.width;
		progressPlayerTransform.sizeDelta = new Vector2 (0, progressPlayerTransform.rect.height);

		progressEnemyTransform = progressEnemy.GetComponent<RectTransform> ();
		progressEnemyTransform.sizeDelta = new Vector2 (0, progressEnemyTransform.rect.height);

		camMinimumLeft = -90f; // fishPlayer.gameObject.transform.position.x; // (can't be really sure this runs before fish Start(), which due to the new trash talking might change the fish position)

		preRaceDelayTimer = preRaceDelay;
		responseBox.SetActive (false);
		responseCanvas.alpha = 0;

		if (fadePlane != null)
			fadePlane.SetActive (true);

		// iControl.PlayRandomFromType (SfxType.Start); // to be replaced with 1,2,3...?

		fishGainCanvasGroup = victoryTransform.GetComponent<CanvasGroup> ();
		enemySprite = iControl.GetEnemyFishSprite ();

		if ((float)Screen.width / (float)Screen.height < 1.34) { // 1024/768
			responseScaler.transform.localScale = new Vector3 (1.2f, 1.2f, 1);
			responseScaler.transform.localPosition = new Vector3 (210, 130, 0);

			countScaler.transform.localScale = new Vector3 (1.7f, 1.7f, 1);
			countScaler.transform.localPosition = new Vector3 (0, 645, 0);
		}

		// raceState = RaceState.Race; // skip Ready/Set/Go

		Resources.UnloadUnusedAssets ();
	}


	void MarkAnswer (int answerIndex, Evaluation evaluation)
	{
		Text text;
		switch (answerIndex) {
		case 0:
			text = answerLeft;
			break;
		case 1:
			text = answerUp;
			break;
		case 2:
			text = answerRight;
			break;
		default:
			text = answerDown;
			break;
		}

		if (evaluation == Evaluation.Reset) {
			text.color = new Color (0.56f, 0.19f, 0.466f);

			questionIndex++;
			if (questionIndex >= qac.questions.Length)
				questionIndex = 0;
			PopulateVisualOptions ();

		} else {
			if (evaluation == Evaluation.Correct)
				text.color = Color.yellow;
			else
				text.color = Color.red;
			answerDelayCounter = 0.25f;
			previousAnswerIndex = answerIndex;
		}
	}

	void ProcessAnswer (int answerIndex)
	{
		Text answerText = answerLeft;
		if (answerIndex == 1)
			answerText = answerUp;
		if (answerIndex == 2)
			answerText = answerRight;
		if (answerIndex == 3)
			answerText = answerDown;

		EdibleController edible = Instantiate (ediblePrefab);
		edible.transform.SetParent (canvasRef.transform, false);

		Image answerPastille = edible.GetComponent<Image> ();
		if (answerPastille != null)
			answerPastille.sprite = answerPastilles [answerIndex];


		if (answerIndex == qac.questions [questionIndex].answerIndex) {
			fishPlayer.AddForce ();
			MarkAnswer (answerIndex, Evaluation.Correct);
			edible.Init (fishPlayer, answerText.gameObject, answerDelay, canvasRef);
			doffeGladImage.color = Color.white;
			doffeSadImage.color = Color.clear;
			doffeSadTimer = -1000;
			iControl.PlaySingleSfx (SingleSfx.AnswerCorrect);

			LeanTween.moveLocal (doffeContainer.gameObject, new Vector3[] {
				new Vector3 (0f, doffeContainer.transform.localPosition.y, 0f),
				new Vector3 (0f, 100f, 0f),
				new Vector3 (0f, 100f, 0f),
				new Vector3 (0f, 0f, 0f)
			}, 0.7f).setEase (LeanTweenType.easeOutQuad);
//			LeanTween.moveLocal(doffeContainer.gameObject, new Vector3[]{new Vector3(0f,doffeContainer.transform.localPosition.y,0f),new Vector3(0f,60f,0f),new Vector3(0f,80f,0f),new Vector3(0f,100f,0f),new Vector3(0f,90f,0f),new Vector3(0f,80f,0f),new Vector3(0f,60f,0f),new Vector3(0f,0f,0f)}, 0.7f).setEase(LeanTweenType.easeInQuad);
		} else {
			fishPlayer.ReduceForce ();
			fishEnemy.AddForce ();
			MarkAnswer (answerIndex, Evaluation.Incorrect);
			edible.Init (fishEnemy, answerText.gameObject, answerDelay, canvasRef);
			doffeGladImage.color = Color.clear;
			doffeSadImage.color = Color.white;
			doffeSadTimer = 0.7f;
			iControl.PlaySingleSfx (SingleSfx.AnswerWrong1);

			LeanTween.moveLocal (noffeContainer.gameObject, new Vector3[] {
				new Vector3 (0f, noffeContainer.transform.localPosition.y, 0f),
				new Vector3 (0f, -100f, 0f),
				new Vector3 (0f, -100f, 0f),
				new Vector3 (0f, 0f, 0f)
			}, 0.7f).setEase (LeanTweenType.easeOutQuad);
//			LeanTween.moveLocal(noffeContainer.gameObject, new Vector3[]{new Vector3(0f,noffeContainer.transform.localPosition.y,0f),new Vector3(0f,-60f,0f),new Vector3(0f,-80f,0f),new Vector3(0f,-100f,0f),new Vector3(0f,-90f,0f),new Vector3(0f,-80f,0f),new Vector3(0f,-60f,0f),new Vector3(0f,0f,0f)}, 0.7f).setEase(LeanTweenType.easeInQuad);
		}

		bubblePopper.SetTrigger ("Pop");
	}


	private float GetFPS ()
	{
		fps_timeleft -= Time.deltaTime;
		fps_accum += Time.timeScale / Time.deltaTime;
		++fps_frames_count;

		if (fps_timeleft <= 0.0) {
			float fps = fps_accum / fps_frames_count;

			fps_timeleft = fps_updateInterval;
			fps_accum = 0.0F;
			fps_frames_count = 0;
			return fps;
		}
		return -1;
	}

	void PlayPassedResponse ()
	{
		if (!iControl.IsPlayingSfx (true))
			iControl.PlayRandomFromType (SfxType.IsPassed);
	}

	void PlayEnemyPassedResponse ()
	{
		if (!iControl.IsPlayingSfx (false))
			iControl.PlayRandomFromType (SfxType.EnemyIsPassed);
	}


	void Update ()
	{
		int answered = -1;
		bool bDelayed = false;


		if (Input.GetKeyDown (KeyCode.W)) {
			playerWinsImmediate ();
		}
		if (Input.GetKeyDown (KeyCode.Q)) {
			playerWins ();
		}


		if (fadePlane != null && !hasFadedIn) {
			hasFadedIn = true;
			fadePlane.SetActive (true);
			iTween.ColorTo (fadePlane, new Color (0, 0, 0, 0), 0.5f);
		}
			
		if (answerDelayCounter > 0)
			bDelayed = true;


		float playerProgress = progressMaxWidth * fishPlayer.GetGoalProgress ();
		float enemyProgress = progressMaxWidth * fishEnemy.GetGoalProgress ();

		progressPlayerTransform.sizeDelta = new Vector2 (playerProgress, progressPlayerTransform.rect.height);
		progressEnemyTransform.sizeDelta = new Vector2 (enemyProgress, progressEnemyTransform.rect.height);


		if (raceState == RaceState.Race && isLeadingSet == false && fishEnemy.IsGoing ()) {
			isLeading = playerProgress > enemyProgress ? true : false; 
			isLeadingSet = true;
		}

		if (raceState == RaceState.Race && fishEnemy.IsGoing ()) {

			if (isLeading && enemyProgress > playerProgress) {
				if (!iControl.IsPlayingSfx (false)) {
					isLeading = false;
					float waitTime = iControl.PlayRandomFromType (SfxType.EnemyPassing);
					Invoke ("PlayPassedResponse", waitTime);
				}
			} else if (!isLeading && playerProgress > enemyProgress) {
				if (!iControl.IsPlayingSfx (true)) {
					isLeading = true;
					float waitTime = iControl.PlayRandomFromType (SfxType.Passing);
					Invoke ("PlayEnemyPassedResponse", waitTime);
				}
			}
				
			float relativeDistance = Mathf.Abs (fishPlayer.GetGoalProgress () - fishEnemy.GetGoalProgress ());

			farBehindTimer -= Time.deltaTime;
			if (isLeading && relativeDistance > 0.25 && farBehindTimer < 0) {
				if (!iControl.IsPlayingSfx (true)) {
					iControl.PlayRandomFromType (SfxType.InFront);
					farBehindTimer = 30;
				}
			} else if (!isLeading && relativeDistance > 0.25 && farBehindTimer < 0) {
				if (!iControl.IsPlayingSfx (false)) {
					iControl.PlayRandomFromType (SfxType.Behind);
					farBehindTimer = 30;
				}
			} 
		}

		closeClapTimer -= Time.deltaTime;
		float maxDistance = Mathf.Max (fishPlayer.GetGoalProgress (), fishEnemy.GetGoalProgress ());
		if (maxDistance > 0.85 && closeClapTimer < 0 && raceState == RaceState.Race) {

			SingleSfx[] clapTypes = new SingleSfx[] {
				SingleSfx.CloseLoop1,
				SingleSfx.CloseLoop2,
				SingleSfx.CloseLoop3
			};
			closeClapType = clapTypes [Random.Range (1, 3)];
			iControl.PlaySingleSfx (closeClapType);

			closeClapType = SingleSfx.CloseLoop1;
			iControl.PlaySingleSfx (closeClapType, false, Random.Range (0f, 0.02f));

			float distanceLeft = (1f - maxDistance);
			if (distanceLeft < 0.02f)
				distanceLeft = 0.02f;
			closeClapTimer = distanceLeft * 15;
		}


		doffeSadTimer -= Time.deltaTime;
		if (doffeSadTimer < 0 && doffeSadTimer > -100) {
			doffeGladImage.color = Color.white;
			doffeSadImage.color = Color.clear;
			doffeSadTimer = -1000;
		}

		float fps = GetFPS ();
		if (fps >= 0)
			fpsText.text = (int)fps + " FPS";


		preRaceDelayTimer -= Time.deltaTime;
		if (preRaceDelayTimer > 0 && raceState == RaceState.PreRace) {
			return;
		}

		camInitialZoom -= Time.deltaTime;

		if (!fishPlayer.HasFinishedTrashing ())
			fishPlayer.TrashTalk ();
		else if (!bRaceStarted) {
			raceState = RaceState.Race;
			camInitialZoom = 0;
			countScaler.SetActive (true);
			preRaceDelayTimer = 3;
			bRaceStarted = true;

			iControl.PlaySingleSfx (SingleSfx.Ready, false, 0.6f);
			iControl.PlaySingleSfx (SingleSfx.Set, false, 1.4f);
			iControl.PlaySingleSfx (SingleSfx.Go, false, 2.5f);
		}

/*
		if (camInitialZoom > 900) {
			camInitialZoom = 2.5f;
//			CameraZoom (camInitialZoom);
		}
*/

		if (!bRaceStarted)
			return;


		if (camInitialZoom < 1.5f && responseBox.activeSelf == false && preRaceDelayTimer < 0) {
			responseBox.SetActive (true);
			LeanTween.alphaCanvas (responseCanvas, 1, fadeTimeUI);
			LeanTween.alphaCanvas (backCanvas, 1, 0.2f);
			backCanvas.blocksRaycasts = true;
			fishPlayer.SetIsGoing (true);
			fishEnemy.SetIsGoing (true); 
			bRaceStarted = true;
			countScaler.SetActive (false);
		}

		if (camInitialZoom < 0) {
			CameraZoom (0);
		}

		onOffParticles.transform.position = new Vector3 (onOffParticles.transform.position.x, onOffParticles.transform.position.y, cam.transform.position.z + 40);

		if (fishPlayer.HasReachedGoal () && raceState != RaceState.PostRace && raceState != RaceState.PrePostRace) {
			iControl.StopPlayingSfx (true);
			float waitTime = iControl.PlayRandomFromType (SfxType.WinSfx);
			iControl.PlayRandomFromType (SfxType.Wins, -1, waitTime + 0.2f);

			LeanTween.alphaCanvas (responseCanvas, 0, fadeTimeUI);
			responseCanvas.blocksRaycasts = false;
			LeanTween.alphaCanvas (backCanvas, 0, 0.2f);
			backCanvas.blocksRaycasts = false;
			playerWon = true;
			raceState = RaceState.PrePostRace;
			levelIncreased = iControl.IncreaseFishLevel ();
		}
		if (fishEnemy.HasReachedGoal () && raceState != RaceState.PostRace && raceState != RaceState.PrePostRace) {
			LeanTween.alphaCanvas (responseCanvas, 0, fadeTimeUI);
			responseCanvas.blocksRaycasts = false;
			LeanTween.alphaCanvas (backCanvas, 0, 0.2f);
			backCanvas.blocksRaycasts = false;
			playerWon = false;
			raceState = RaceState.PrePostRace;
			fishPlayer.gameObject.layer = LayerMask.NameToLayer ("Fish1");
			fishEnemy.gameObject.layer = LayerMask.NameToLayer ("Fish2");

			iControl.StopPlayingSfx (false);
			iControl.PlayRandomFromType (SfxType.LoseSfx);
			Invoke ("DelayedEnemyWin", 2f);

			iControl.Save ();
		}
			
		if (raceState == RaceState.PrePostRace && answerDelayCounter < 0) {
			raceState = RaceState.PostRace;
			fishPlayer.SetDancing (playerWon);
			fishEnemy.SetDancing (!playerWon);
			if (skipDance)
				endCounter = 3;
			float velocity = fishEnemy.GetVelocity ();
			if (playerWon)
				velocity = fishPlayer.GetVelocity ();
//			iTween.MoveTo (cam.gameObject, new Vector3 (endLine.transform.position.x + 10 + velocity, 67, -65), 4.0f);
			if (playerWon)
				LeanTween.move (cam.gameObject, new Vector3 (endLine.transform.position.x + 10 + velocity, 67, -65), 4.0f).setEase (LeanTweenType.easeOutExpo);
			else {
				LeanTween.move (cam.gameObject, new Vector3 (endLine.transform.position.x + 10 + velocity, 0, -65), 4.0f).setEase (LeanTweenType.easeOutExpo);
			}
		}

		if (cam.gameObject.transform.position.y >= 66 && onOffSurface1.activeSelf == false) {
			onOffSurface1.SetActive (true);

			GameObject fl = GameObject.Find ("finish_line");
			fl.SetActive (false);

			endLine.gameObject.layer = LayerMask.NameToLayer ("Fish2");
			for (int i = 0; i < clouds.Length; i++) {
				clouds [i].Init (i % 2 == 0 ? true : false, 10 + i * 2, 0.2f + i * 0.1f);
			}
		}


		if (raceState == RaceState.PostRace && endCounter < 0) {
			if (fishEnemy.IsCelebratingAtSurface () || fishPlayer.IsCelebratingAtSurface ())
				endCounter = levelIncreased ? 5 : playerWon ? 5 : 3;
		}

		if (endCounter >= 0) {
			endCounter -= Time.deltaTime;

			if (endCounter < 4f && !victoryTransform.activeSelf) {
				ShowGainedFish ();
			}

			if (endCounter < 0.5f) {

				iTween.ColorTo (fadePlane, new Color (0.05f, 0.5f, 0.6f), 0.3f); // works better (faster) than using LeanTween, for whatever reason

				fishGainCanvasGroup.alpha = (endCounter - 0.1f) * 2;

				Invoke ("GotoLevelSelect", 0.4f);
			}
		}

		answerDelayCounter -= Time.deltaTime;
		if (answerDelayCounter > 0) {
			touchAnswer = -1;
			return;
		}

		if (bDelayed)
			MarkAnswer (previousAnswerIndex, Evaluation.Reset);


		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			answered = 0;
		}
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			answered = 1;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			answered = 2;
		}
		if (Input.GetKeyDown (KeyCode.DownArrow)) {
			answered = 3;
		}

		// For debugging
		if (Input.GetKeyDown (KeyCode.M)) {
			baseFishDebugIndex++;
			if (baseFishDebugIndex >= iControl.GetNofBaseFish ())
				baseFishDebugIndex = 0;
			fishEnemy.SetFishAsBase (baseFishDebugIndex);
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			baseFishDebugIndex--;
			if (baseFishDebugIndex < 0)
				baseFishDebugIndex = iControl.GetNofBaseFish () - 1;
			fishEnemy.SetFishAsBase (baseFishDebugIndex);
		}

		// matches Android Back button, ESC for laptop
		if (Input.GetKeyDown (KeyCode.Escape) && responseCanvas.blocksRaycasts == true) {
			BackButton ();
		}

		if (Input.GetKeyDown (KeyCode.T)) {
			fishPlayer.Turn ();
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			fishEnemy.Turn ();
		}


		if (touchAnswer >= 0) {
			answered = touchAnswer;
			touchAnswer = -1;
		}			

		if (answered >= 0 && raceState == RaceState.Race) {
			ProcessAnswer (answered);
		}
	}


	public void DelayedEnemyWin ()
	{
		float waitTime = iControl.PlayRandomFromType (SfxType.Loses, -1);
		Invoke ("GotoLevelSelect", waitTime + 0.1f);
	}

	public void BackButton ()
	{
		iTween.ColorTo (fadePlane, new Color (0.05f, 0.5f, 0.6f), 0.3f); // works better (faster) than using LeanTween, for whatever reason
		LeanTween.alphaCanvas (responseCanvas, 0, 0.2f);
		responseCanvas.blocksRaycasts = false;
		LeanTween.alphaCanvas (backCanvas, 0, 0.2f);
		backCanvas.blocksRaycasts = false;
		fishGainCanvasGroup.alpha = 0;
		iControl.PlaySingleSfx (SingleSfx.Button2);
		endLine.gameObject.layer = LayerMask.NameToLayer ("Entourage");
		Invoke ("GotoLevelSelect", 0.4f);
	}

	private void GotoLevelSelect ()
	{
		if (returningToMain)
			return;
		
		returningToMain = true;
		fishGainCanvasGroup.alpha = 0;

		iControl.SetLastLevelPlayed (iControl.GetTable ());
		SceneManager.LoadScene ("LevelSelect");
	}

	public void AnswerLeft ()
	{
		touchAnswer = 0;
	}

	public void AnswerUp ()
	{
		touchAnswer = 1;
	}

	public void AnswerRight ()
	{
		touchAnswer = 2;
	}

	public void AnswerDown ()
	{
		touchAnswer = 3;
	}



	private void FlipOnOff (int index)
	{
		switch (index) {
		case 0:
			RenderSettings.fog = !RenderSettings.fog;
			break;
		case 1:
			onOffParticles.SetActive (!onOffParticles.activeSelf);
			break;
		case 2:
			onOffFish.SetActive (!onOffFish.activeSelf);
			onOffJelly.SetActive (!onOffJelly.activeSelf);
			break;
		case 3:
			onOffSurface2.SetActive (!onOffSurface2.activeSelf);
			break;
		case 4:
			onOffBubbles.SetActive (!onOffBubbles.activeSelf);
			break;
		case 5:
			onOffSeabed.SetActive (!onOffSeabed.activeSelf);
			break;
		case 6:
			onOffRocks.SetActive (!onOffRocks.activeSelf);
			break;
		case 7:
			onOffBackdrop.SetActive (!onOffBackdrop.activeSelf);
			break;
		case 8:
			onOffWeed.SetActive (!onOffWeed.activeSelf);
			break;
		}
	}


	public void FlipJustWater ()
	{
		onOffSeabed.SetActive (!onOffSeabed.activeSelf);
		onOffRocks.SetActive (!onOffRocks.activeSelf);
		onOffWeed.SetActive (!onOffWeed.activeSelf);
	}


	public void FlipOnOffOnOff ()
	{
		onoOffButtons.SetActive (!onoOffButtons.activeSelf);
	}

	public void OnOffClick ()
	{
		string selectName = EventSystem.current.currentSelectedGameObject.name;
		int selectIndex = int.Parse (selectName.Substring (0, 1));

		FlipOnOff (selectIndex);
	}


	public void enemyWins ()
	{
		playerWon = false;
		bRaceStarted = true;
		LeanTween.alphaCanvas (responseCanvas, 0, fadeTimeUI);
		responseCanvas.blocksRaycasts = false;
		LeanTween.alphaCanvas (backCanvas, 0, 0.2f);
		backCanvas.blocksRaycasts = false;
		raceState = RaceState.PostRace;
		fishPlayer.SetDancing (false);
		fishEnemy.SetDancing (true);
		float xPos = fishEnemy.transform.position.x;
//		LeanTween.moveLocal (cam.gameObject, new Vector3 (xPos, 0, -65), 4.0f).setEase (LeanTweenType.easeOutExpo);
		fishPlayer.gameObject.layer = LayerMask.NameToLayer ("Fish1");
		fishEnemy.gameObject.layer = LayerMask.NameToLayer ("Fish2");

		float waitTime = iControl.PlayRandomFromType (SfxType.LoseSfx);
		iControl.PlayRandomFromType (SfxType.Loses, -1, 2f); // waitTime

		Invoke ("GotoLevelSelect", waitTime + 0.1f + 2);

		iControl.Save ();
	}

	public void playerWins ()
	{
		playerWon = true;
		bRaceStarted = true;
		LeanTween.alphaCanvas (responseCanvas, 0, fadeTimeUI);
		responseCanvas.blocksRaycasts = false;
		LeanTween.alphaCanvas (backCanvas, 0, 0.2f);
		backCanvas.blocksRaycasts = false;
		raceState = RaceState.PostRace;
		fishPlayer.SetDancing (true);
		fishEnemy.SetDancing (false);
		float xPos = fishPlayer.transform.position.x;
		LeanTween.moveLocal (cam.gameObject, new Vector3 (xPos, 67, -65), 4.0f).setEase (LeanTweenType.easeOutExpo);

		float waitTime = iControl.PlayRandomFromType (SfxType.WinSfx);
		iControl.PlayRandomFromType (SfxType.Wins, -1, waitTime + 0.2f);

		levelIncreased = iControl.IncreaseFishLevel ();
		//Debug.Log ("LEVELINC: " + levelIncreased);
	}

	public void playerWinsImmediate ()
	{
		levelIncreased = iControl.IncreaseFishLevel ();
		iControl.Save ();
		Invoke ("GotoLevelSelect",0);
	}


	void CameraZoom (float zoomTime, float maxZoomChange = 4)
	{
//		float oldZoomLevel = cam.transform.position.z;
		float zoomLevel = -64, maxZoom = -65 + 1;
		float minX, maxX, leftDesiredXPos;
		float posLeft, posRight, posRightRemaining = 0;
		float delta = 0.04f;
		float i;
		float rightMax = 0.63f;

		float fishEnemyLeft = fishEnemy.GetLeftMostPos () - 6;
		float fishEnemyRight = fishEnemy.GetRightMostPos () + 6;
		float fishPlayerLeft = fishPlayer.GetLeftMostPos () - 6;
		float fishPlayerRight = fishPlayer.GetRightMostPos () + 6;

		if (raceState == RaceState.PostRace) // postrace anim is done with iTween
			return;

		// calculate x span of the two fish (if one fish is wider, it may be both minx and maxx)
		minX = Mathf.Min (fishEnemyLeft, fishPlayerLeft);
		maxX = Mathf.Max (fishEnemyRight, fishPlayerRight);
		float fishDistanceX = maxX - minX;

		// calculate the range to search for zooming
		float rangeStart = -64, rangeEnd = maxZoom;
		if (zoomTime == 0) {
			rangeStart = prevCamZoom - maxZoomChange;
			if (rangeStart < -640)
				rangeStart = -640;
			rangeEnd = prevCamZoom + maxZoomChange;
			if (rangeEnd > maxZoom)
				rangeEnd = maxZoom;
		}

		// find best zoomlevel within the range
		int camIterations = 0;
		float minDist = 99999999, minDistValue = prevCamZoom;
		for (i = rangeStart; i <= rangeEnd; i = i + delta) {
			cam.transform.position = new Vector3 (cam.transform.position.x, cam.transform.position.y, i);

			posLeft = cam.ViewportToWorldPoint (new Vector3 (0, 0, -cam.transform.position.z - 0.5f)).x;
			posRight = cam.ViewportToWorldPoint (new Vector3 (rightMax, 0, -cam.transform.position.z - 0.5f)).x;
			posRightRemaining = (posRight - cam.ViewportToWorldPoint (new Vector3 (1, 0, -cam.transform.position.z - 0.5f)).x) / 2;
			float dist = posRight - posLeft;
			if (dist < minDist) {
				minDist = dist;
				minDistValue = i;
			}

			if (dist < fishDistanceX) {
				minDistValue = i - delta;
				break;
			}
			camIterations++;
		}

		if (zoomTime > 0)
			minDistValue = -64; // hardcoded since can't make runtime modified tween work. On the positive side, gamestart looks the same no matter what resolution is used
		
		zoomLevel = prevCamZoom = minDistValue;
//		Debug.Log ("CamIterations: " + camIterations);

		// calculate x position of camera, so that it is 1) between the two fish  2) ...as long as that is not outside x edges
		cam.transform.position = new Vector3 (0, 0, -64); // the next calculation is based on the camera's CURRENT position, and since we move it around, it's temporarily put back to a static "base pos" here
		screenWidthUnits = cam.ViewportToWorldPoint (new Vector3 (rightMax, 0, -zoomLevel)).x * 2;

		leftDesiredXPos = minX + (maxX - minX) / 2 - posRightRemaining;
		if (leftDesiredXPos < camMinimumLeft + screenWidthUnits / 2 - posRightRemaining * 2)
			leftDesiredXPos = camMinimumLeft + screenWidthUnits / 2 - posRightRemaining * 2;
		
		if (leftDesiredXPos < -24) // hardcoded since can't make runtime modified tween work. On the positive side, gamestart looks the same no matter what resolution is used
			leftDesiredXPos = -24;

		if (raceState == RaceState.PostRace)
			endYMove += 25 * Time.deltaTime;

		if (zoomTime == 0) {
			float camYPos = cam.transform.position.y + endYMove;
			if (camYPos > 35)
				camYPos = 35;

			cam.transform.position = new Vector3 (leftDesiredXPos, camYPos, zoomLevel);
		} else {
			LeanTween.moveLocal (cam.gameObject, new Vector3 (leftDesiredXPos, cam.transform.position.y, zoomLevel), zoomTime).setEase (LeanTweenType.easeOutExpo);
		}

//		Debug.Log("Cam Zoom/pos: " + zoomLevel + " " + leftDesired);
	}


	private void ShowGainedFish ()
	{
		if (iControl.GetFishLevel () >= iControl.GetMaxLevel (iControl.GetTable ()) && levelIncreased == false)
			return;

		if (!playerWon || iControl.IsMixedTable(iControl.GetTable()))
			return;

		if (enemySprite)
		{
			Texture tex = enemySprite.texture;
			float aspect = (float)tex.height / (float)tex.width;

			victoryFish.transform.localScale = new Vector3 (1.5f, 1.5f * aspect, 1);
			fishGainImage.sprite = enemySprite;
			fishGainCoverImage.sprite = enemySprite;
		}
		victoryTransform.SetActive (true);


		if (!levelIncreased) {
			GameObject go = GameObject.Find ("VictoryStar");
			go.SetActive (false);

			fishGainImage.fillAmount = iControl.GetRelativeTableProgress (iControl.GetTable ());

		} else {
		}

		// Invoke ("StopWinAnim", 2f); // not working... 	void StopWinAnim() { victoryFishAnimation.enabled = false; }

		LeanTween.alphaCanvas (fishGainCanvasGroup, 1, 1);
	}




	private QAcollection MakeSingleCollection (QACollectionProducer qp, int tableIndex, int levelIndex, int maxQLimit = -1)
	{
		QAcollection qac = null;
		int maxPossibleQ, actualQ;

		switch (tableIndex) {
		case 0:

			int AS = (int)QACollectionProducer.Op2Type.AllowSwitch;

			//första talet , andra talet, exempel 0,10,0,0 då blir det mellan 0 och 10 plus mellan 0 och 0.
			//sista parametern är fall den skall få lov att byta 

				int[][] addIntervals = new int[][] {
				new int[] { 0, 10, 0, 0, AS },
				new int[] { 0, 9, 1, 1, AS },
				new int[] { 0, 8, 2, 2, AS },
				new int[] { 0, 7, 3, 3, AS },
				new int[] { 0, 6, 4, 4, AS },
				new int[] { 0, 5, 5, 5, AS },

				new int[] { 0, 5, 0, 0, (int)QACollectionProducer.Op2Type.Copy },
				new int[] { 2, 4, -1, 1, (int)QACollectionProducer.Op2Type.CopyNoRandom },
				new int[] { 10, 10, 1, 5, AS },
				new int[] { 11, 12, 0, 3, AS },
				new int[] { 6, 9, 6, 9, AS },
				new int[] { 10, 10, 1, 10, AS },

				new int[] { 11, 15, 1, 5, AS },
				new int[] { 20, 20, 1, 5, AS },
				new int[] { 16, 16, 1, 9, AS },
				new int[] { 17, 17, 1, 8, AS },
				new int[] { 18, 18, 1, 7, AS },
				new int[] { 25, 25, 1, 5, AS },

				//	new int[] { 0, 3, 0, 3, AS, 0, 3 },
				new int[] { 19, 19, 1, 11, AS },
				new int[] { 30, 30, 1, 10, AS },
				new int[] { 27, 27, 1, 15, AS },
				new int[] { 39, 39, 1, 11, AS },
				//		new int[] { 1, 10, 1, 10, AS, 1, 10 },

				new int[] { 25, 30, 5, 15, AS },
				new int[] { 31, 35, 5, 15, AS },
				//		new int[] { 5, 15, 5, 15, AS, 5, 15 },
				new int[] { 20, 29, 11, 20, AS },
				new int[] { 36, 40, 5, 15, AS } 
			};


/*			int[][] addIntervals = new int[][] {
				new int[] { 0, 10, 0, 0, AS },
				new int[] { 0, 9, 1, 1, AS },
				new int[] { 0, 8, 2, 2, AS },
				new int[] { 0, 7, 3, 3, AS },
				new int[] { 0, 6, 4, 4, AS },
				new int[] { 0, 5, 5, 5, AS },

				new int[] { 0, 5, 0, 0, (int)QACollectionProducer.Op2Type.Copy },
				new int[] { 2, 4, -1, 1, (int)QACollectionProducer.Op2Type.CopyNoRandom },
				new int[] { 10, 10, 1, 5, AS },
				new int[] { 11, 12, 0, 3, AS },
				new int[] { 6, 9, 6, 9, AS },
				new int[] { 10, 10, 1, 10, AS },

				new int[] { 11, 15, 1, 5, AS },
				new int[] { 20, 20, 1, 5, AS },
				new int[] { 16, 16, 1, 9, AS },
				new int[] { 17, 17, 1, 8, AS },
				new int[] { 18, 18, 1, 7, AS },
				new int[] { 25, 25, 1, 5, AS },

				//	new int[] { 0, 3, 0, 3, AS, 0, 3 },
				new int[] { 19, 19, 1, 11, AS },
				new int[] { 30, 30, 1, 10, AS },
				new int[] { 27, 27, 1, 15, AS },
				new int[] { 39, 39, 1, 11, AS },
				//		new int[] { 1, 10, 1, 10, AS, 1, 10 },

				new int[] { 25, 30, 5, 15, AS },
				new int[] { 31, 35, 5, 15, AS },
				//		new int[] { 5, 15, 5, 15, AS, 5, 15 },
				new int[] { 20, 29, 11, 20, AS },
				new int[] { 36, 40, 5, 15, AS },
				//		new int[] { 5, 30, 5, 30, AS, 5, 30 },

				new int[] { 40, 50, 6, 20, AS },
				new int[] { 51, 60, 11, 30, AS },
				new int[] { 61, 70, 20, 40, AS },
				new int[] { 71, 80, 11, 60, AS },
				new int[] { 15, 50, 15, 50, AS, 15, 50 },
				new int[] { 91, 110, 11, 100, AS },
			};*/

			maxPossibleQ = (addIntervals [levelIndex] [1] - addIntervals [levelIndex] [0] + 1) * (addIntervals [levelIndex] [3] - addIntervals [levelIndex] [2] + 1);
			if (addIntervals [levelIndex].Length > 5)
				maxPossibleQ += maxPossibleQ / 2;
			//			Debug.Log (maxPossibleQ);

			if ((QACollectionProducer.Op2Type)addIntervals [levelIndex] [4] == QACollectionProducer.Op2Type.CopyNoRandom) // ugly fix
				maxPossibleQ = 6;

			actualQ = maxPossibleQ;
			if (actualQ > maxQLimit && maxQLimit > 0)
				actualQ = maxQLimit;

		/*	if (addIntervals [levelIndex].Length > 5)
				qac = qp.ProduceCollection_DoubleOp ("A" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Plus, addIntervals [levelIndex] [0], addIntervals [levelIndex] [1], addIntervals [levelIndex] [2], addIntervals [levelIndex] [3], addIntervals [levelIndex] [5], addIntervals [levelIndex] [6],
					0, -1);
			else
			*/
			qac = qp.ProduceCollection ("A" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Plus, addIntervals [levelIndex] [0], addIntervals [levelIndex] [1], addIntervals [levelIndex] [2], addIntervals [levelIndex] [3],
				(QACollectionProducer.Op2Type)addIntervals [levelIndex] [4], 0, -1);

			break;


		case 1: // Leftover/legacy - Stora Plus

			qac = qp.ProduceCollection ("SHARK", 5, 4, QACollectionProducer.MathOperation.Plus, 666, 666, 111, 222,	QACollectionProducer.Op2Type.AllowSwitch, 0, -1);

			break;

		case 2:

			int[][] minusIntervals = new int[][] {
				new int[] { 0, 10, 0, 0 },
				new int[] { 1, 10, 1, 1 },
				new int[] { 2, 10, 2, 2 },
				new int[] { 3, 10, 3, 3 },
				new int[] { 4, 10, 4, 4 },

				new int[] { 5, 10, 5, 5 },
				new int[] { 6, 12, 6, 6 },
				new int[] { 7, 16, 7, 7 },
				new int[] { 8, 18, 8, 8 },
				new int[] { 9, 20, 9, 9 },
				new int[] { 10, 25, 10, 10 },

				new int[] { 100, 100, 0, 10 },
				new int[] { 100, 100, 10, 20 },
				new int[] { 1000, 1000, 0, 20 },
				new int[] { 1000, 1000, 0, 100 },

		
				new int[] { 12, 25, 11, 11 },
				new int[] { 14, 25, 12, 12 },
				new int[] { 15, 30, 13, 13 },
				new int[] { 15, 30, 14, 15 }

				//new int[] { 5, 10, 0, 3, 0, 2 },

			};

			maxPossibleQ = (minusIntervals [levelIndex] [1] - minusIntervals [levelIndex] [0] + 1) * (minusIntervals [levelIndex] [3] - minusIntervals [levelIndex] [2] + 1);
//			if (minusIntervals [levelIndex].Length > 4) maxPossibleQ *= (minusIntervals [levelIndex] [5] - minusIntervals [levelIndex] [4] + 1);  // ok, but maybe too much (risk of overruning finding a non-copy)?
//			Debug.Log (maxPossibleQ);
			actualQ = maxPossibleQ;
			if (actualQ > maxQLimit && maxQLimit > 0)
				actualQ = maxQLimit;

		/*	if (minusIntervals [levelIndex].Length > 4)
				qac = qp.ProduceCollection_DoubleOp ("S" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Minus, minusIntervals [levelIndex] [0], minusIntervals [levelIndex] [1], minusIntervals [levelIndex] [2], minusIntervals [levelIndex] [3], minusIntervals [levelIndex] [4], minusIntervals [levelIndex] [5],
					0, -1);
			else*/
			qac = qp.ProduceCollection ("S" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Minus, minusIntervals [levelIndex] [0], minusIntervals [levelIndex] [1], minusIntervals [levelIndex] [2], minusIntervals [levelIndex] [3],
				QACollectionProducer.Op2Type.Normal, 0, -1);

			break;


		case 3:

			int[][] multiIntervals = new int[][] {
				new int[] { 0, 0, 0, 10 },

				new int[] { 0, 10, 1, 1 },
				new int[] { 0, 5, 2, 2 },
				new int[] { 6, 10, 2, 2 },
				new int[] { 0, 5, 4, 4 },
				new int[] { 6, 10, 4, 4 },

				new int[] { 0, 5, 5, 5 },
				new int[] { 6, 10, 5, 5 },
				new int[] { 0, 5, 3, 3 },
				new int[] { 6, 10, 3, 3 },
				new int[] { 0, 5, 6, 6 },

//				new int[] { 1, 3, 1, 3, 1, 3 },
				new int[] { 6, 10, 6, 6 },
				new int[] { 0, 5, 9, 9 },
				new int[] { 6, 10, 9, 9 },
//				new int[] { 1, 4, 1, 4, 1, 4 },

				new int[] { 0, 5, 8, 8 },
				new int[] { 6, 10, 8, 8 },
				new int[] { 0, 5, 7, 7 },
				new int[] { 8, 10, 7, 7 },

				new int[] { 0, 10, 10, 10 },
				new int[] { 2, 10, 11, 11 },
				new int[] { 2, 10, 12, 12 },
				new int[] { 2, 10, 13, 13 },
//				new int[] { 2, 7, 2, 7, 2, 7 },
			};

			actualQ = maxPossibleQ = (multiIntervals [levelIndex] [1] - multiIntervals [levelIndex] [0] + 1) * (multiIntervals [levelIndex] [3] - multiIntervals [levelIndex] [2] + 1);
//			if (multiIntervals [levelIndex].Length > 4)
//				actualQ *= (multiIntervals [levelIndex] [5] - multiIntervals [levelIndex] [4] + 1);  // ok, but maybe too much (risk of overrunning finding a non-copy)?
//			else
				actualQ = maxPossibleQ + maxPossibleQ / 2;
			if (actualQ > maxQLimit && maxQLimit > 0)
				actualQ = maxQLimit;

		/*	if (multiIntervals [levelIndex].Length > 4)
				qac = qp.ProduceCollection_DoubleOp ("M" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Multiply, multiIntervals [levelIndex] [0], multiIntervals [levelIndex] [1], multiIntervals [levelIndex] [2], multiIntervals [levelIndex] [3], multiIntervals [levelIndex] [4], multiIntervals [levelIndex] [5],
					0, -1);
			else*/
			qac = qp.ProduceCollection ("M" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Multiply, multiIntervals [levelIndex] [0], multiIntervals [levelIndex] [1], multiIntervals [levelIndex] [2], multiIntervals [levelIndex] [3],
				QACollectionProducer.Op2Type.AllowSwitch, 0, -1);
			
			break;


		case 4:
			
			int[][] divIntervals = new int[][] {
				new int[] { 0, 10, 1, 1 },
				new int[] { 0, 10, 2, 2 },
				new int[] { 0, 20, 3, 3 },
				new int[] { 0, 20, 4, 4 },
				new int[] { 0, 30, 5, 5 },

				new int[] { 0, 42, 6, 6 },
				new int[] { 0, 56, 7, 7 },
				new int[] { 0, 72, 8, 8 },
				new int[] { 0, 90, 9, 9 },
				new int[] { 0, 100, 10, 10 },
				new int[] { 100, 1000, 10, 10 },

				new int[] { 0, 100, 20, 20 },
				new int[] { 0, 1000, 50, 50 }


				//new int[] { 32, 160, 16, 16 },
				//new int[] { 34, 200, 17, 17 },
				//new int[] { 36, 200, 18, 18 },
				//new int[] { 38, 200, 19, 19 },
				//new int[] { 42, 200, 21, 21 },
			};

			actualQ = maxPossibleQ = (divIntervals [levelIndex] [1] - divIntervals [levelIndex] [0]) / (divIntervals [levelIndex] [3]);
			if (actualQ > maxQLimit && maxQLimit > 0)
				actualQ = maxQLimit;

		/*	if (divIntervals [levelIndex].Length > 4)
				qac = qp.ProduceCollection_DoubleOp ("D" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Divide, divIntervals [levelIndex] [0], divIntervals [levelIndex] [1], divIntervals [levelIndex] [2], divIntervals [levelIndex] [3], divIntervals [levelIndex] [4], divIntervals [levelIndex] [5],
					0, -1);
			else*/
			qac = qp.ProduceCollection ("D" + levelIndex, actualQ, 4, QACollectionProducer.MathOperation.Divide, divIntervals [levelIndex] [0], divIntervals [levelIndex] [1], divIntervals [levelIndex] [2], divIntervals [levelIndex] [3],
				QACollectionProducer.Op2Type.Normal, 0, -1);
			
			break;
		}

//		if (Application.isEditor) {
//			qac.DebugPrintCollection ();

//		}
		

		return qac;
	}


	private QAcollection MakeCollection (QACollectionProducer qp)
	{
		QAcollection qac = null;
		QAcollection qac2 = null;

		int tableIndex = iControl.GetTable ();
		int fishLevelIndex = iControl.GetFishLevel ();
		int nofFishInSet = iControl.GetNofFishInSet (tableIndex);


		if (iControl.IsMixedTable (tableIndex)) {
			int nofQ = 0;

			do {
				for (int j = 0; j < iControl.GetNofUnmixedTables (); j++) {
					if (j != 1) { // 1 = gamla Stora Plus
						for (int i = 0; i < iControl.GetAccumulatedLevelIndex (j); i++) {
							if (qac == null)
								qac = MakeSingleCollection (qp, j, i, 1);
							else {
								qac2 = MakeSingleCollection (qp, j, i, 1);
								qac = qp.MergeCollections (qac, qac2, false, "");
							}
							nofQ++;
						}
					}
				}
			} while (nofQ < 24);

			qp.ShuffleCollection (qac);
			return qac;
		}

		if (fishLevelIndex < iControl.GetMaxLevel (tableIndex)) {
			int newQ = (int)(0.55f * 28f);

			int accLevel = iControl.GetAccumulatedLevelIndex (tableIndex);

			qac = MakeSingleCollection (qp, tableIndex, accLevel, 16);

			int nofQ = qac.questions.Length;

			int nofOldQ = Mathf.Clamp ((int)0.35f * nofQ, 4, 666);
			if (accLevel - 1 >= 0) {
				int backLevel = Mathf.Clamp (accLevel - 9, 0, 666);
				int oldQcount = 0;

				do {
					for (int i = accLevel - 1; i >= backLevel; i--) {
						qac2 = MakeSingleCollection (qp, tableIndex, i, 1);
						qac = qp.MergeCollections (qac, qac2, false, "");
						oldQcount++;
					}
				} while (oldQcount < nofOldQ);

				int upLevel = accLevel + 1;
				if (upLevel <= iControl.GetMaxLevel (tableIndex)) {
					qac2 = MakeSingleCollection (qp, tableIndex, upLevel, nofQ <= 6 ? 2 : 3);
					qac = qp.MergeCollections (qac, qac2, false, "");
				}
				upLevel++;
				if (upLevel <= iControl.GetMaxLevel (tableIndex)) {
					qac2 = MakeSingleCollection (qp, tableIndex, upLevel, 1);
					qac = qp.MergeCollections (qac, qac2, false, "");
				}
				upLevel++;
				if (upLevel <= iControl.GetMaxLevel (tableIndex) && nofQ > 8) {
					qac2 = MakeSingleCollection (qp, tableIndex, upLevel, 1);
					qac = qp.MergeCollections (qac, qac2, false, "");
				}
				qp.ShuffleCollection (qac);
			}

		} else
		{
			qac = MakeSingleCollection (qp, tableIndex, 0, 1);
			for (int i = 5; i < iControl.GetAccumulatedMaxLevel (tableIndex); i++)
			{
				qac2 = MakeSingleCollection (qp, tableIndex, i, 1);
				qac = qp.MergeCollections (qac, qac2, false, "");
			}
			qp.ShuffleCollection (qac);
		}

		return qac;
	}

}
