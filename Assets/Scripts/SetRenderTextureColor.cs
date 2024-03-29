using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SetRenderTextureColor : MonoBehaviour {
	/*
	 *      
	 * 
	 */
	public RawImage myRawImage;
	public RenderTexture renderTexture; // renderTextuer that you will be rendering stuff on
 
	Texture2D texture;


	 
	// Use this for initialization
	void Start () {
		renderTexture.Release ();

	}
 

	// Update is called once per frame
	void Update () {

		if (renderTexture.IsCreated ()) {
			myRawImage.color = new Color (1f,1f, 1f, 1f);
			Destroy (this);
		 
		}
	 
	}



 
}


