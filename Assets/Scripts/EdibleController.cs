using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EdibleController : MonoBehaviour {

	private float activeTimeLeft1 = 0, activeTimeLeft2 = 0, timeStart;
	public FishController trackingObject = null;
	public Canvas canvasRef;
	private RectTransform rt;
	private const float minSize = 0.0f;
	private Vector3 startPos;
	private float startWidth, startHeight;
	private bool startedEating = false;

	void Start () {
		rt = GetComponent<RectTransform>();
		rt.transform.position = startPos;
		startWidth = rt.sizeDelta.x;
		startHeight = rt.sizeDelta.y;
	}

	public void Init(FishController fish, GameObject start, float time, Canvas canvasUsed) {
		activeTimeLeft1 = activeTimeLeft2 = timeStart = time;
		canvasRef = canvasUsed;
		trackingObject = fish;
		gameObject.SetActive (true);
		startPos = start.transform.position;
	}

	void Update () {
		if (activeTimeLeft1 < -0.3f)
			return;

		activeTimeLeft1 -= Time.deltaTime;
		if (activeTimeLeft1 <= -0.3f) {
			if (trackingObject != null)
				trackingObject.Grow ();
			Destroy (gameObject);
			return;
		}

		activeTimeLeft2 -= Time.deltaTime * 2.0f;

		// We need anchored positions or everything is messed up;

		RectTransform CanvasRect = canvasRef.GetComponent<RectTransform>();

		Vector3 ViewportPosition=Camera.main.WorldToViewportPoint(trackingObject.GetMouthPosition());

		Vector2 WorldObject_ScreenPosition=new Vector2(
			((ViewportPosition.x*CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
			((ViewportPosition.y*CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

		float relTime = activeTimeLeft2 / timeStart;
		if (relTime < minSize)
			relTime = minSize;

		if (relTime < minSize + 0.2f && !startedEating) {
//			IntermediateController.instance.FadePlayingSfx (trackingObject.IsPlayer());

			trackingObject.StartEating (0.6f);
//			gameObject.SetActive (false);
			gameObject.transform.position = new Vector3 (10000,0,0); // we want this object to be destroyed later, if set active to false then Update is no longer called
			startedEating = true;
		}

		rt.sizeDelta = new Vector2 ((relTime-0.08f) * startWidth, (relTime-0.08f) * startHeight);

		float xSpan = rt.anchoredPosition.x - WorldObject_ScreenPosition.x;
		float ySpan = rt.anchoredPosition.y - WorldObject_ScreenPosition.y;

		relTime = 1.1f - (activeTimeLeft1 / timeStart);
		rt.anchoredPosition=new Vector2(rt.anchoredPosition.x - xSpan*relTime, rt.anchoredPosition.y - ySpan*relTime);
	}
}
