using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishController : MonoBehaviour
{

	public float minSpeed;
	public float boost;
	public float negboost;
	private Rigidbody2D rigidBody;
	private MeshRenderer meshRenderer = null;
	private float boostMod;
	private float startXpos, endXpos;
	private bool isPlayer = false;
	private bool isGoing = false;
	private bool isDancing = false, isWinner = false, celebratesAtSurface = false;
	public Camera cam;
	private Vector3 orgPos;
	public float sinCounter;
	public float waveSpeed, waveSize;
	private float orgXBound;
	public GameObject bubblesPrefab;
	public List<GameObject> bubblers;
	private float minYPos;
	private float worldWidthHalfW;
	private float swingTimeLeft = -1;

	private float[] bendValue = { 0, 3.14f / 2 };
	private float[] bendDelta = { -0.1f, 0.1f };
	private float[] bendMul = { 13f, 20f };
	public float bendMulMod = 1;
	private float eatTimeLeft = -1;
	private int entourageIndex = 0;
	public Material material;

	public EntourageController[] entourage;
	private float[] entourageXBound = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	private bool bEntourageSet = false;

	private IntermediateController iControl;
	public Sprite baseSprite;

	public ParticleSystem eatParticlesPrefab;
	private ParticleSystem eatParticles;

	MegaBend[] mbs;
	MegaModifyObject mmo;

	public GameObject audienceMemberPrefab;

	private float jumpCounter = 30;

	public GameObject propPoint;
	private float xPropPos = 1;
	private float yPropPos = 0;

	private bool isTurning = false, isTrashing = false, finishedTrashing = false;
	private float turnProgress, turnDir;
	public GameObject entourageContainer;

	public GameObject splashParticlesUp;
	public GameObject splashParticlesDown;


	void Start ()
	{
		rigidBody = GetComponent<Rigidbody2D> ();
		rigidBody.gravityScale = 0;
		meshRenderer = GetComponent<MeshRenderer> ();

		Vector3 pos = cam.ViewportToWorldPoint (new Vector3 (0, 0, -cam.transform.position.z - 0.5f));
		float xMod = meshRenderer.bounds.extents.x / 2; // bounds is always HALF the size of the rect, according to docs
		if (minSpeed == 0)
			isPlayer = true;
		if (isPlayer)
			xMod = 0;
		startXpos = pos.x - xMod;
		transform.position = new Vector3 (startXpos, transform.position.y, pos.z);

		Vector3 oldCamPos = cam.transform.position;
		cam.transform.position = new Vector3 (0, 0, -90);
		worldWidthHalfW = cam.ViewportToWorldPoint (new Vector3 (1, 0, 90 - 0.5f)).x;
		minYPos = -cam.ViewportToWorldPoint (new Vector3 (1, 0, 90 - 0.5f)).y + 15;
		cam.transform.position = oldCamPos;
			
		GameObject fl = GameObject.Find ("finish_line");
		endXpos = fl.transform.position.x;

		boostMod = ((float)Screen.width / (float)Screen.height) / (16.0f / 9.0f);

		orgPos = gameObject.transform.position;

		eatParticles = null;

		mbs = GetComponentsInParent<MegaBend> ();
		mmo = GetComponent<MegaModifyObject> ();

		bubblers = new List<GameObject> ();

		iControl = IntermediateController.instance;

		meshRenderer.material = material;

		if (isPlayer) {
			Sprite sprite = iControl.GetSelectedFishSprite (iControl.GetPlayerFishIndex ());
			if (!sprite)
				sprite = baseSprite;

			material.SetTexture ("_MainTex", sprite.texture);

			float scale = 1.2f;

			FishInfo fi = iControl.GetFishCollectionFish  (iControl.GetPlayerFishIndex ());
			if (fi == null) {
				fi = new FishInfo (GameController.TableType.LillaPlus, 0);
			}
			scale = iControl.GetRelativeTableProgress((int)fi.tableType) + 0.7f;
			//Debug.Log (fi.levelIndex + "  : " + iControl.GetTableLevel ((int)fi.tableType));
			if (fi.levelIndex != iControl.GetTableLevel ((int)fi.tableType)) {
				scale = 0.8f + 0.7f;
			}

			if (iControl.GetTableLevel (iControl.GetTable ()) >= iControl.GetMaxLevel (iControl.GetTable ())) {
				scale = 0.8f + 0.7f;
			}


			if (iControl.IsMixedTable (iControl.GetTable ()))
				SetScale (sprite.texture, 1.2f);
			else
				SetScale (sprite.texture, scale); // scale [fishLevelindex]);

			for (int i = 0; i < iControl.GetNofUnmixedTables (); i++)
				if (iControl.HasEntourage ((GameController.TableType)i))
					AddEntourage ((GameController.TableType)i);

			float rescale = 1 + iControl.GetTableLevel (iControl.GetLastLevelPlayed ()) * 0.14f;
			if (iControl.IsMixedTable (iControl.GetTable ()))
				rescale = 1;
			entourageContainer.transform.localScale = new Vector3 (-3 * rescale, 1, -2 * rescale);
			entourageContainer.transform.parent = transform;

			// material.color = Color.red; // tinting the material
			
		} else {
			Sprite sprite = iControl.GetEnemyFishSprite ();
			if (!sprite)
				sprite = baseSprite;
				
			material.SetTexture ("_MainTex", sprite.texture);
			SetScale (sprite.texture, 1.33f);
		}

		orgXBound = meshRenderer.bounds.extents.x;

		// set this line to skip the trash talk sequence
		//finishedTrashing = true;

		if (isPlayer && !finishedTrashing) {
			transform.position = new Vector3 (-25, transform.position.y, transform.position.z);
			transform.Rotate (new Vector3 (0, 150f, 0), Space.World);
			rigidBody.drag = 1.5f;
		}
	}

	void SetScale (Texture tex, float mod)
	{

		float aspect = (float)tex.height / (float)tex.width;

		float xScale = -3;
		float yScale = xScale * aspect;

		xScale *= mod;
		yScale *= mod;

		transform.localScale = new Vector3 (xScale, 1, yScale);
	}


	public void TrashTalk ()
	{
		if (isTrashing || !isPlayer)
			return;

		isTrashing = true;
		SayShit ();
	}

	private void SayShit ()
	{
		float len = iControl.PlayRandomFromType (SfxType.Start);
		if (len > 1.0f)
			len -= 0.5f;
		Invoke ("SayEnemyShit", len);
	}

	private void SayEnemyShit ()
	{
		float len = iControl.PlayRandomFromType (SfxType.EnemyStart);
		Invoke ("GoBack", len - 0.2f);
	}

	private void GoBack ()
	{
		rigidBody.AddForce (new Vector2 (-6500, 0));
//		Turn ();
		Invoke ("Turn", 0.001f);
	}

	public void Turn ()
	{
		isTurning = true;
		turnProgress = 0;
		turnDir = -1; //transform.rotation.z >= 0.7 ? -1 : 1;
/*
		iTween.EaseType et = iTween.Defaults.easeType;
		iTween.Defaults.easeType = iTween.EaseType.easeOutSine;
		iTween.RotateBy(this.gameObject, new Vector3(0, 0, 0.5f), 1.7f);
		iTween.Defaults.easeType = et;
*/

		Invoke ("FinishTrash", 1f);
	}


	private void FinishTrash ()
	{
		finishedTrashing = true;
		rigidBody.drag = 0.5f;
	}

	public bool HasFinishedTrashing ()
	{
		return finishedTrashing;
	}


	private void TurnProgress (float progress)
	{
		transform.Rotate (new Vector3 (0, 150f * Time.deltaTime * turnDir, 0), Space.World);
		if (transform.rotation.z >= 0.7 || transform.rotation.z <= 0) {
			isTurning = false;
		}

		turnProgress += Time.deltaTime / 3;
	}


	// for debug/props positioning purposes
	public void SetFishAsBase (int index)
	{
		FishInfo fi = iControl.GetFishBaseFish (index);

		iControl.SetTable ((int)fi.tableType);
		iControl.SetTableLevel ((int)fi.tableType, fi.levelIndex - 1);

		Sprite sprite = iControl.GetBaseSprite (index);
		if (sprite != null) {
			material.SetTexture ("_MainTex", sprite.texture);
			SetScale (sprite.texture, 1.33f);
		}
	}

	void InvokeSplashDown() {
		float rescale = 1 + iControl.GetTableLevel (iControl.GetLastLevelPlayed ()) * 0.1f;
		if (iControl.IsMixedTable (iControl.GetTable ()))
			rescale = 1;
		if (!isPlayer)
			rescale = 1.33f;
		splashParticlesDown.transform.localScale *= rescale;
		splashParticlesDown.transform.position = new Vector3(transform.position.x, 66, transform.position.z);
		splashParticlesDown.SetActive (true);
	}


	void FixedUpdate ()
	{
		if (isDancing && ((isPlayer && isWinner) || (!isPlayer && !isWinner))) {
			float rotDirection = 1;
			if (isWinner == false)
				rotDirection = -1;
			if (transform.rotation.z < 0.33f)
				transform.Rotate (new Vector3 (0, 0, 20f * rotDirection * Time.deltaTime), Space.World);
			if (transform.position.y < minYPos && !celebratesAtSurface)
				transform.Translate (new Vector3 (0, 30f * rotDirection * Time.deltaTime, 0), Space.World);
			else {
				if (!celebratesAtSurface && cam.transform.position.y >= 66) {
					for (int i = 0; i < entourageIndex; i++) {
						entourage [i].gameObject.layer = LayerMask.NameToLayer ("Fish1");
					}

					if (isPlayer) {
						Vector3 oldpos = cam.transform.position;
						cam.transform.position = new Vector3 (0, 0, -64);
						float halfW = cam.ViewportToWorldPoint (new Vector3 (2.95f, 0, -65)).x;
						cam.transform.position = oldpos;

						for (int i = 0; i < 1; i++) {
							GameObject audience = (GameObject)Instantiate (audienceMemberPrefab, Vector3.zero, Quaternion.identity);
							//audience.transform.Translate (gameObject.transform.position.x + halfW - i * 20 - 20, minYPos - 5, -0.5f + 10 * i, Space.World);
							audience.transform.Translate (gameObject.transform.position.x - 60.0f, minYPos + 4, -0.5f + 10 * i, Space.World);

							AudienceController ac = audience.GetComponent<AudienceController> ();
							ac.SetTargetX (gameObject.transform.position.x - 30.0f);
						}
					}

					iControl.PlaySingleSfx (SingleSfx.SplashUp);
					float rescale = 1 + iControl.GetTableLevel (iControl.GetLastLevelPlayed ()) * 0.1f;
					if (iControl.IsMixedTable (iControl.GetTable ()))
						rescale = 1;
					if (!isPlayer)
						rescale = 1.33f;
					splashParticlesUp.transform.localScale *= rescale;
					splashParticlesUp.transform.position = new Vector3(transform.position.x, 66, transform.position.z);
					splashParticlesUp.SetActive (true);
					if (isPlayer) {
						iControl.PlaySingleSfx (Random.Range (0, 1) == 0 ? SingleSfx.SplashDown : SingleSfx.SplashDown2, false, 1.5f);
						Invoke("InvokeSplashDown", 1.5f);
						iControl.PlayRandomFromType (SfxType.Cheering, -1, 0.2f);

					}
					celebratesAtSurface = true;
				}

				if (isPlayer) {
					transform.Translate (new Vector3 (0, jumpCounter * rotDirection * Time.deltaTime, 0), Space.World);
					if (jumpCounter < 20)
						transform.Rotate (new Vector3 (0, 0, -90f * Time.deltaTime), Space.World);
					jumpCounter -= 30 * Time.deltaTime;
				}


			}
			return;
		}

		gameObject.transform.position = new Vector3 (gameObject.transform.position.x, orgPos.y + Mathf.Sin (sinCounter) * waveSize, gameObject.transform.position.z);
		sinCounter += waveSpeed * Time.deltaTime;

		if (!isGoing)
			return;

		if (rigidBody.velocity.magnitude < minSpeed) {
			rigidBody.velocity = rigidBody.velocity.normalized * minSpeed;
		}
		if (rigidBody.velocity.x < 0)
			rigidBody.velocity = Vector2.zero;
	}


	void Update ()
	{

		if (transform.position.x < startXpos)
			transform.position = new Vector3 (startXpos, transform.position.y, transform.position.z);

		if (!bEntourageSet && isPlayer == true) {
			bEntourageSet = true;
			for (int i = 0; i < 6; i++) {
				if (entourageIndex <= i)
					entourage [i].gameObject.SetActive (false);
			}
		}

		if (mbs != null && mmo != null) {

			mmo.Reset (); // reset bending, otherwise it's iterative

			int i = 0;
			foreach (MegaBend mb in mbs) {
				mb.angle = Mathf.Sin (bendValue [i]) * bendMul [i] * bendMulMod;
				float eatSwing = 1;
				if (swingTimeLeft > 0)
					eatSwing = 2.5f; // used to be 1.4f;
				bendValue [i] += bendDelta [i] * Time.deltaTime * 50 * eatSwing;
				i++;
				if (isWinner && isDancing && celebratesAtSurface) {
					mb.gizmoRot.x = mb.angle * 10;
				}
			}
		}
		if (isTurning) {
			TurnProgress (turnProgress);
		}

		foreach (GameObject go in bubblers.ToArray()) {
			ParticleSystem ps = go.GetComponent<ParticleSystem> ();
			if (ps && !ps.IsAlive ()) {
				bubblers.Remove (go);
				Destroy (go);
			}
		}

		swingTimeLeft -= Time.deltaTime;

		if (eatTimeLeft > 0) {
			if (eatParticles == null) {
				eatParticles = Instantiate (eatParticlesPrefab);
			}

			eatParticles.transform.position = GetMouthPosition ();

		}
		eatTimeLeft -= Time.deltaTime;

		if (eatTimeLeft < 0.2f && eatParticles != null) {
			var emission = eatParticles.emission;
			emission.enabled = false;
		}

		if (eatTimeLeft < 0 && eatParticles != null) {
			Destroy (eatParticles.gameObject);
			eatParticles = null;
		}

		if (Input.GetKeyDown (KeyCode.C) || Input.GetKeyDown (KeyCode.V) || Input.GetKeyDown (KeyCode.Z) || Input.GetKeyDown (KeyCode.X)) {
			if (propPoint != null) {
				if (Input.GetKeyDown (KeyCode.V))
					xPropPos += 0.04f;
				if (Input.GetKeyDown (KeyCode.C))
					xPropPos -= 0.04f;
				if (Input.GetKeyDown (KeyCode.Z))
					yPropPos += 0.04f;
				if (Input.GetKeyDown (KeyCode.X))
					yPropPos -= 0.04f;
				propPoint.SetActive (true);
				propPoint.transform.position = new Vector3 (transform.position.x + meshRenderer.bounds.extents.x * xPropPos, transform.position.y + meshRenderer.bounds.extents.y * yPropPos, transform.position.z);
				Debug.Log ("Prop Pos: " + xPropPos + ", " + yPropPos);
			}
		}

	}

	public Vector3 GetMouthPosition ()
	{
		if (iControl.IsTexturesSet ()) {
			if (isPlayer) {
				FishInfo fi = iControl.GetFishCollectionFish (iControl.GetPlayerFishIndex ());
				if (fi != null)
					return new Vector3 (transform.position.x + meshRenderer.bounds.extents.x * iControl.GetPropPosition (true, PropType.Mouth, (int)fi.tableType, fi.levelIndex),
						transform.position.y + meshRenderer.bounds.extents.y * iControl.GetPropPosition (false, PropType.Mouth, (int)fi.tableType, fi.levelIndex),
						transform.position.z);
				else
					return transform.position;
			} else {
				return new Vector3 (transform.position.x + meshRenderer.bounds.extents.x * iControl.GetPropPosition (true, PropType.Mouth, iControl.GetTable (), iControl.GetTableLevel (iControl.GetTable ()) + 1),
					transform.position.y + meshRenderer.bounds.extents.y * iControl.GetPropPosition (false, PropType.Mouth, iControl.GetTable (), iControl.GetTableLevel (iControl.GetTable ()) + 1),
					transform.position.z);
			}
		} else {
			return new Vector3 (GetRightMostPos (true), transform.position.y, transform.position.z);
		}
	}

	public void AddForce ()
	{
		rigidBody.AddForce (new Vector2 (boost * boostMod, 0));

		for (int i = 0; i < 2; i++) {
			Vector3 bubblePos = transform.position;
			bubblePos += new Vector3 ((-meshRenderer.bounds.extents.x) + 8 - i * 2, 0, 0);
			GameObject bubbles = (GameObject)Instantiate (bubblesPrefab, bubblePos, Quaternion.identity);
			if (bubbles != null) {
				bubbles.transform.Rotate (new Vector3 (-90, 0, 0));
				Rigidbody rb = bubbles.GetComponent<Rigidbody> ();
				if (rb != null) {
					rb.AddForce (new Vector3 (boost * boostMod + 100, 0, 0));
				}
				ParticleSystem ps = bubbles.GetComponent<ParticleSystem> ();
				if (ps) {
					//ps.randomSeed = (uint)Random.Range (0, 65000);
					ps.startSize = i * 2.5f + 2.5f;
					ps.Simulate (0, true, true);
					ps.Play ();
				}

				if (i == 0)
					bubbles.layer = LayerMask.NameToLayer ("Default");
				bubblers.Add (bubbles);
			}
		}
		swingTimeLeft = 1.5f;
	}

	public void ReduceForce ()
	{
		if (rigidBody.velocity.magnitude > 0)
			rigidBody.AddForce (new Vector2 (-negboost * boostMod, 0));
	}

	public bool HasReachedGoal ()
	{

//		if (transform.position.x  > endXpos - meshRenderer.bounds.extents.x)
		if (transform.position.x > endXpos - orgXBound) // may "look" like not hitting endpos early enough, but corresponds with progress bar
			return true;
		else
			return false;
	}

	public float GetGoalProgress ()
	{
		return Mathf.Clamp ((GetRightMostPos () + worldWidthHalfW) / (endXpos + worldWidthHalfW), 0, 1);
	}

	public float GetLeftMostPos (bool getBendedPos = false)
	{
		if (entourageIndex > 0) {
			int i = entourageIndex - 1;

			if (!getBendedPos)
				return entourage [i].gameObject.transform.position.x - entourageXBound [i];
			else {
				MeshRenderer entourageRender = entourage [i].gameObject.GetComponent<MeshRenderer> ();
				return entourage [i].gameObject.transform.position.x - entourageRender.bounds.extents.x; 
			}
		}

		if (!getBendedPos)
			return transform.position.x - orgXBound; // bounds change with bending transform, but we want camera not to wobble
		else {
			return transform.position.x - meshRenderer.bounds.extents.x; 
		}
	}

	public float GetRightMostPos (bool getBendedPos = false)
	{
		if (!getBendedPos)
			return transform.position.x + orgXBound; // bounds change with bending transform, but we want camera not to wobble
		else {
			return transform.position.x + meshRenderer.bounds.extents.x;
		}
	}

	public bool IsGoing ()
	{
		return isGoing;
	}

	public void SetIsGoing (bool going)
	{
		isGoing = going;
		rigidBody.velocity = new Vector2 (minSpeed, rigidBody.velocity.y);
	}

	public void SetDancing (bool amWinner)
	{
		isDancing = true;
		isWinner = amWinner;
		if (!isPlayer)
			minSpeed = 0;
	}

	public bool IsWinner ()
	{
		return isWinner;
	}

	public bool IsPlayer ()
	{
		return isPlayer;
	}

	public bool IsCelebratingAtSurface ()
	{
		return celebratesAtSurface;
	}

	public void Grow ()
	{
//		iTween.ScaleAdd (gameObject, new Vector3 (-0.1f, 0, -0.1f), 0.3f);  // no growing for now
	}

	public float GetVelocity ()
	{
		return rigidBody.velocity.x;
	}


	public void StartEating (float eatTime)
	{
		bool bToPlay = (!isPlayer || Random.Range (0, 100) < 25);

		if (bToPlay)
			iControl.FadePlayingSfx (isPlayer);
		float waitTime = iControl.PlayRandomFromType (SfxType.Crunch);
		if (bToPlay)
			Invoke ("PlayFed", waitTime);

		this.eatTimeLeft = eatTime;
		if (eatParticles != null) {
			var emission = eatParticles.emission;
			emission.enabled = true;
		}
	}

	private void PlayFed ()
	{
		iControl.PlayRandomFromType (isPlayer ? SfxType.PlayerFed : SfxType.EnemyFed);
	}

	public void AddEntourage (GameController.TableType tableType)
	{
		entourage [entourageIndex].SetSprite (IntermediateController.instance.GetEntourageSprite (tableType));
		entourage [entourageIndex].gameObject.SetActive (true);
		MeshRenderer entourageRender = entourage [entourageIndex].gameObject.GetComponent<MeshRenderer> ();
		entourageXBound [entourageIndex] = entourageRender.bounds.extents.x; 
		entourageIndex++;
	}

}
