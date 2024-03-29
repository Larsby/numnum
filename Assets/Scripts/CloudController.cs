using UnityEngine;
using System.Collections;

public class CloudController : MonoBehaviour {

	private float delta;
	public Camera cam;
	private SpriteRenderer spriteRenderer;

	void Start () {
	}

	public void Init(bool fromLeft, float speed, float scale) {
		float halfW;

		delta = speed;
		this.gameObject.SetActive (true);

		Vector3 oldpos = cam.transform.position;
		cam.transform.position = new Vector3(cam.transform.position.x, 0, -90);

		if (fromLeft)
			halfW = cam.ViewportToWorldPoint (new Vector3 (1, 0, -65)).x;
		else
			halfW = cam.ViewportToWorldPoint (new Vector3 (0, 0, -65)).x;
		cam.transform.position = oldpos;

		spriteRenderer = GetComponent<SpriteRenderer> ();
		gameObject.transform.position = new Vector3 (halfW + spriteRenderer.bounds.extents.x * (fromLeft? -1 : 1.2f), transform.position.y, transform.position.z);

		if (!fromLeft)
			delta = -delta;
	}

	void Update () {
		gameObject.transform.position = new Vector3 (transform.position.x + delta * Time.deltaTime, transform.position.y, transform.position.z);
	}
}
