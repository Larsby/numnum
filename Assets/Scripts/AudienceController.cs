using UnityEngine;
using System.Collections;

public class AudienceController : MonoBehaviour
{

	private float orgX, deltaX;
	float rotateValue = 0;
	float addTime = 0;

	void Start ()
	{
		/*
		transform.Rotate (new Vector3 (-90, 0, 0), Space.World);
		transform.Rotate (new Vector3 (0, 0, 55), Space.World);

		rotateValue = Random.Range (0.0f, 3.14f);
		*/
	}

	public void SetTargetX (float x)
	{
		orgX = transform.position.x;
		deltaX = x - orgX;
	}

	void Update ()
	{
		
		rotateValue += 8 * Time.deltaTime;

		addTime += Time.deltaTime;
		if (addTime > 1f / 2f)
			addTime = 1f / 2f;

		transform.rotation = Quaternion.identity;
	
		transform.Rotate (new Vector3 (0, 180.0f, Mathf.Sin (rotateValue) * 15), Space.World);
		//transform.position = new Vector3 (orgX + addTime * deltaX * 0.5f, transform.position.y, transform.position.z);
		transform.position = new Vector3 (orgX + addTime * deltaX * 2f, transform.position.y, transform.position.z);


	}
}
