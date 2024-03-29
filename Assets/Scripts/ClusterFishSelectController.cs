using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ClusterFishSelectController : MonoBehaviour
{

	private IntermediateController iControl;
	public GameObject interMediateControllerPrefab;

	public Button fishButtonPrefab;
	public GameObject grid;
	private AsyncOperation async = null;
	private GameObject oldSelectedButton = null;
	private Image oldSelectedImage = null;

	private float[] bendValue = { 0, 3.14f / 2 };
	private float[] bendDelta = { -0.1f, 0.1f };
	private float[] bendMul = { 13f, 20f };
	private float[] fishScale = { 0.40f, 0.5f, 0.6f, 0.70f, 0.8f, 0.9f, 1.0f };
	public float bendMulMod = 1;

	public Sprite baseSprite;

	private Button startSelect = null;
	private bool firstUpdate = true;

	public Button coverButton;
	private Vector3 oldScale;
	private Vector3 oldPos;

	MegaBend[] mbs;
	MegaModifyObject mmo;

	public GameObject fishAnimPlane;
	public GameObject fadePlane;
	public GameObject loadIcon;
	private float loadIconHeight;
	private float loadTimer = 1000;
	private bool shownLoadIcon = false;
	private bool acceptButtonInput = false;

	void Awake ()
	{

		iControl = IntermediateController.instance;
		if (iControl == null)
		{
			Instantiate (interMediateControllerPrefab, Vector3.zero, Quaternion.identity);
			iControl = IntermediateController.instance;
		}
	}

	void Start ()
	{
		bool selectedFishFound = false;
		int i;

		mbs = fishAnimPlane.GetComponents<MegaBend> ();
		mmo = fishAnimPlane.GetComponent<MegaModifyObject> ();

		for (i = 0; i < iControl.GetNofBaseFish (); i++)
		{
			Button button = Instantiate (fishButtonPrefab, grid.transform, false) as Button;
			button.interactable = false;

			//int scaleIndex = i % 7;
			//button.GetComponent<RectTransform> ().localScale = new Vector3 (fishScale [scaleIndex], fishScale [scaleIndex], 1.0f);

			button.GetComponent<RectTransform> ().localScale = new Vector3 (0.9f, 0.9f, 1.0f);

			int index = iControl.GetFishInCollectionIndex (i);
			if (index >= 0)
				button.interactable = true;
			else
				button.GetComponent<RectTransform> ().localScale = new Vector3 (0.7f, 0.7f, 1.0f);

			FishInfo fi = iControl.GetFishBaseFish (i);
			if (fi.levelIndex >= iControl.GetTableLevel ((int)fi.tableType) && !selectedFishFound && !iControl.IsMixedTable (iControl.GetTable ()))
			{
				startSelect = button;
				selectedFishFound = true;
			}

/*			if (iControl.isFishWon (i)) // only show if fish was just won
				startSelect = button;
*/

			Image img = button.GetComponent<Image> ();
			Sprite sprite = iControl.GetBaseSprite (i);
			if (sprite != null)
				img.sprite = sprite;
			else
			{
				img.enabled = false;
			}

			button.onClick.AddListener (() =>
			{
				fishSelectOnClickEvent (index);
			}); // I don't get why I have to create index from i, but otherwise it always thinks "i" is 3 (i.e. at end of loop)

			fadePlane.SetActive (true);
			iTween.ColorTo (fadePlane, new Color (0, 0, 0, 0), 0.3f);

		}

		iControl.ResetFishWonLevel ();

		loadIcon.SetActive (true);
		SpriteRenderer renderer = loadIcon.GetComponent<SpriteRenderer> ();
		Vector3 loadIconPos = Camera.main.ViewportToWorldPoint (new Vector3 (1, 0, 30));
		loadIconHeight = renderer.bounds.extents.y;
		loadIconPos = new Vector3 (loadIconPos.x - renderer.bounds.extents.x - 1.5f, loadIconPos.y - loadIconHeight, loadIconPos.z);
		loadIcon.transform.position = loadIconPos;
	}


	void Update ()
	{

		loadTimer += Time.deltaTime;
		if (loadTimer >= 2f && loadTimer < 100 && !shownLoadIcon)
		{
			iTween.MoveTo (loadIcon, new Vector3 (loadIcon.transform.position.x, loadIcon.transform.position.y + loadIconHeight * 2 + 1f, loadIcon.transform.position.z), 0.5f);
			shownLoadIcon = true;
		}
			
		if (startSelect != null && !firstUpdate)
		{
			EventSystem.current.SetSelectedGameObject (startSelect.gameObject);
			startSelect.onClick.Invoke ();
			startSelect = null;
		}
		firstUpdate = false;

		if (async != null)
		{
			if (async.isDone || async.progress == 0.9f)
			{ // idiotic API where 0.9f means "finished" (see https://docs.unity3d.com/ScriptReference/AsyncOperation-allowSceneActivation.html)
				iTween.MoveTo (loadIcon, new Vector3 (loadIcon.transform.position.x, loadIcon.transform.position.y - loadIconHeight * 2 + 1f, loadIcon.transform.position.z), 0.2f);
				async.allowSceneActivation = true;
			}
		}

		if (mbs != null && mmo != null && oldSelectedButton != null && mmo.mesh != null)
		{

			mmo.Reset (); // reset bending, otherwise it's iterative

			int i = 0;
			foreach (MegaBend mb in mbs)
			{
				mb.angle = Mathf.Sin (bendValue [i]) * bendMul [i] * bendMulMod;
				bendValue [i] += bendDelta [i] * Time.deltaTime * 50;
				i++;
			}
		}

		// matches Android Back button, ESC for laptop
		if (Input.GetKeyDown (KeyCode.Escape))
		{
			BackButton ();
		}


	}

	public void fishSelectOk ()
	{

		if (!acceptButtonInput)
			return;
		acceptButtonInput = false;

		iControl.PlaySingleSfx (SingleSfx.PlayWithFish);

		iTween.ColorTo (fadePlane, new Color (0.05f, 0.5f, 0.6f), 0.3f); // works better (faster) than using LeanTween, for whatever reason
		HideCoverButtonP (true);
		bendMulMod = 1;
		Invoke ("fishSelectOkStep2", 0.4f);
	}

	private void fishSelectOkStep2 ()
	{
		Application.backgroundLoadingPriority = ThreadPriority.Normal;
		if (iControl.GetTable () == 5) // mix
			async = SceneManager.LoadSceneAsync ("Race_mikael_alt2");
		else if (iControl.GetTable () == 2) // minus
			async = SceneManager.LoadSceneAsync ("Race_minus");
		else if (iControl.GetTable () == 3) // multi
			async = SceneManager.LoadSceneAsync ("Race_multi");
		else if (iControl.GetTable () == 4) // div
			async = SceneManager.LoadSceneAsync ("Race_div");
		else
			async = SceneManager.LoadSceneAsync ("Race_fredrik_test");
		async.allowSceneActivation = false;
		async.priority = 1000;
		loadTimer = 0;

		//Larsby jag undrar varför denna är här?
		StartCoroutine (WaitForLoad ());
	}


	public void fishClickAlreadySelected (int index)
	{
		if (LeanTween.isTweening (fishAnimPlane))
			return;
		acceptButtonInput = true;

		iControl.PlaySingleSfx (SingleSfx.ChooseFish);
		Invoke ("PlayDelayed", 0.6f);

		Sprite sprite = iControl.GetSelectedFishSprite (index);
		if (sprite == null)
			sprite = baseSprite;

		SetScale (sprite.texture, 1);
	}

	void PlayDelayed ()
	{
		iControl.PlayRandomFromType (SfxType.FishSelected);
	}

	public void fishSelectOnClickEvent (int index)
	{
		if (LeanTween.isTweening (fishAnimPlane))
			return;		

		iControl.PlaySingleSfx (SingleSfx.ChooseFish);
		Invoke ("PlayDelayed", 0.6f);

		iControl.SetPlayerFishIndex (index);


		if (oldSelectedButton)
			oldSelectedButton.SetActive (false);
		if (oldSelectedImage)
			oldSelectedImage.enabled = true;

		GameObject obj = EventSystem.current.currentSelectedGameObject;
		Button[] buttons = obj.GetComponentsInChildren<Button> (true);
		if (buttons != null)
		{
			oldSelectedButton = buttons [1].gameObject;
			oldSelectedButton.SetActive (true);

			oldSelectedImage = buttons [0].GetComponent<Image> ();
			oldSelectedImage.enabled = false;

			buttons [1].onClick.RemoveAllListeners ();
			buttons [1].onClick.AddListener (() =>
			{
				fishClickAlreadySelected (index);
			});
		}

		fishAnimPlane.transform.position = obj.transform.position;
		fishAnimPlane.SetActive (true);


		MeshRenderer meshRenderer = fishAnimPlane.GetComponent<MeshRenderer> ();
		Sprite sprite = iControl.GetSelectedFishSprite (index);
		if (sprite == null)
			sprite = baseSprite;

		bendValue [0] = 0;
		bendValue [1] = 3.14f / 2;

		SetScale (sprite.texture, 1);
		meshRenderer.material.SetTexture ("_MainTex", sprite.texture);

		acceptButtonInput = true;
	}


	// This whole function is extremely ad hoc. I don't know how to properly calculate this...

	void SetScale (Texture tex, float mod)
	{

		float xScale, yScale, zScale;
		float scaleMod = 0.35f, adHoc = 11;
		float toScaleMod = 4.6f;

		if (tex.width > tex.height)
		{
			float aspect = (float)tex.height / (float)tex.width;
			float aspectRev = 1 - (aspect - 0.5f);

			xScale = -aspectRev * 7;
			if (xScale < -7)
				xScale = -7;
			zScale = xScale * aspect;
		} else
		{
			float aspect = (float)tex.width / (float)tex.height;

			zScale = -4;
			xScale = zScale * aspect;
		}

		xScale *= mod * scaleMod * adHoc;
		yScale = adHoc;
		zScale *= mod * scaleMod * adHoc;

		fishAnimPlane.transform.localScale = new Vector3 (xScale, yScale, zScale);


		oldScale = fishAnimPlane.transform.localScale;
		oldPos = fishAnimPlane.transform.position;


		LeanTween.scale (fishAnimPlane, new Vector3 (xScale * toScaleMod, yScale * toScaleMod, zScale * toScaleMod), 1.0f).setEase (LeanTweenType.easeOutExpo);

		Vector3 midScreen = Camera.main.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 90)); // z at 0, since camera is at -90
		LeanTween.move (fishAnimPlane, midScreen, 1.0f).setEase (LeanTweenType.easeOutExpo);

		coverButton.gameObject.SetActive (true);

		CanvasGroup canvasGroup = coverButton.GetComponent<CanvasGroup> ();
		LeanTween.alphaCanvas (canvasGroup, 1, 0.5f);
		canvasGroup.alpha = 0;
	}


	public void HideCoverButton ()
	{
		if (!acceptButtonInput)
			return;
		if (!LeanTween.isTweening (fishAnimPlane))
			acceptButtonInput = false;
		
		HideCoverButtonP (false);
	}

	public void BackButton ()
	{
		iTween.ColorTo (fadePlane, new Color (0.05f, 0.5f, 0.6f), 0.3f); // works better (faster) than using LeanTween, for whatever reason
		iControl.PlaySingleSfx (SingleSfx.Button2);
		Invoke ("BackStep2", 0.4f);
	}

	private void BackStep2 ()
	{
		SceneManager.LoadScene ("LevelSelect");
	}

	public void HideCoverButtonP (bool endAnim)
	{

		if (LeanTween.isTweening (fishAnimPlane) && endAnim == false)
			return;

		if (!endAnim)
			iControl.PlaySingleSfx (SingleSfx.Button2);

		if (!endAnim)
		{
			LeanTween.scale (fishAnimPlane, oldScale, 0.5f).setEase (LeanTweenType.easeOutExpo);
			LeanTween.move (fishAnimPlane, oldPos, 0.5f).setEase (LeanTweenType.easeOutExpo);
		} else
		{
			LeanTween.scale (fishAnimPlane, oldScale * 1.8f, 0.3f).setEase (LeanTweenType.linear);
			Vector3 offScreen = Camera.main.ViewportToWorldPoint (new Vector3 (1.1f, 0.5f, 90));
			LeanTween.move (fishAnimPlane, offScreen, 0.3f).setEase (LeanTweenType.linear);
		}

		CanvasGroup canvasGroup = coverButton.GetComponent<CanvasGroup> ();
		LeanTween.alphaCanvas (canvasGroup, 0, 0.3f);
		Invoke ("DisableCoverButton", 0.3f);
	}

	private void DisableCoverButton ()
	{
		coverButton.gameObject.SetActive (false);
	}


	private IEnumerator WaitForLoad ()
	{

		while (!async.isDone)
		{
			yield return new WaitForSeconds (0.05f);
		}
	}

}
