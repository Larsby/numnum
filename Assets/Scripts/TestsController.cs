using UnityEngine;
using System.Collections;

public class TestsController : MonoBehaviour {

	void Start () {
		MeshRenderer mr = GetComponent<MeshRenderer> ();
		mr.sortingLayerName = "Fish";
		mr.sortingOrder = 20;
		
		Debug.Log("A");
	}

}
