using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntourageController : MonoBehaviour {

	private Rigidbody2D rigidBody;
	private bool isDancing = false;

	private float [] bendValue = { 0, 3.14f/2 };
	private float [] bendDelta = { -0.07f, 0.07f };
	private float [] bendMul = { 10f, 16f };
	public float bendMulMod = 1;

	private MegaBend[] mbs;
	private MegaModifyObject mmo;

	void Start () {
		mbs = GetComponentsInParent<MegaBend> ();
		mmo = GetComponent<MegaModifyObject> ();

		int startMod = Random.Range (0,100);
		bendValue [0] += startMod * bendDelta [0];
		bendValue [1] += startMod * bendDelta [1];
	}
		
	void Update () {
		if (mbs != null && mmo != null) {

			mmo.Reset (); // reset bending, otherwise it's iterative

			int i = 0;
			foreach (MegaBend mb in mbs) {
				if (i < 2) {
					mb.angle = Mathf.Sin (bendValue [i]) * bendMul [i] * bendMulMod;
					bendValue [i] += bendDelta [i] * Time.deltaTime * 50;
					i++;
					if (isDancing) {
						mb.gizmoRot.x = mb.angle * 10;
					}
				}
			}
		}
	}

	public void SetDancing() {
		isDancing = true;
	}

	public void SetSprite(Sprite sprite) {
		IntermediateController iControl = IntermediateController.instance;

		Material eMat = new Material(Shader.Find("Standard NoFog"));
		if (eMat == null || sprite == null)
			return;

		GetComponent<Renderer>().material = eMat;
		eMat.SetTexture("_MainTex", sprite.texture);

		// change shader to cutout (see http://sassybot.com/blog/swapping-rendering-mode-in-unity-5-0/ )
		eMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
		eMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
		eMat.SetInt("_ZWrite", 1);
		eMat.EnableKeyword("_ALPHATEST_ON");
		eMat.DisableKeyword("_ALPHABLEND_ON");
		eMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		eMat.renderQueue = 2450;
		eMat.SetFloat ("_Glossiness", 0.20f);

		if (iControl.GetTable() == 5)
			eMat.color = iControl.IntColor(231, 201, 229);
		else
			eMat.color = iControl.IntColor(201, 228, 230);
	}
}
