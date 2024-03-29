using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class NonUIButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
{
	public bool bDown = false;
	private Image spriteRenderer;
//	private SpriteRenderer spriteRenderer;

	void Start () {
		GameObject go = GameObject.FindGameObjectWithTag ("OverlayPlay");
		spriteRenderer = go.GetComponent<Image> ();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		spriteRenderer.color = Color.gray;
		bDown = true;
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
		if (bDown)
			spriteRenderer.color = Color.gray;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
		if (bDown)
			spriteRenderer.color = Color.white;
    } 

	public void OnPointerUp(PointerEventData eventData)
	{
		spriteRenderer.color = Color.white;
		bDown = false;
	}

	public void OnDrag(PointerEventData eventData) // without implementing this, onpointerup gets called as soon as we drag
	{
	}
}
