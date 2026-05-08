using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GPUInstancer;

public class SpaceshipMobileJoystick : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler
{
	[HideInInspector]
	public Vector3 inputDirection;

	private Image image_0;

	private Image image_1;

	private Vector2 vector2_0;

	public void Start()
	{
		image_0 = GetComponent<Image>();
		image_1 = base.transform.GetChild(0).GetComponent<Image>();
		inputDirection = Vector3.zero;
	}

	public virtual void OnDrag(PointerEventData data)
	{
		vector2_0 = Vector2.zero;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(image_0.rectTransform, data.position, data.pressEventCamera, out vector2_0))
		{
			inputDirection.x = vector2_0.x / image_0.rectTransform.sizeDelta.x * 2f + (float)((image_0.rectTransform.pivot.x == 1f) ? 1 : (-1));
			inputDirection.z = vector2_0.y / image_0.rectTransform.sizeDelta.y * 2f + (float)((image_0.rectTransform.pivot.y == 1f) ? 1 : (-1));
			inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);
			image_1.rectTransform.anchoredPosition = new Vector3(inputDirection.x * (image_0.rectTransform.sizeDelta.x / 3f), inputDirection.z * (image_0.rectTransform.sizeDelta.y / 3f));
		}
	}

	public virtual void OnPointerDown(PointerEventData data)
	{
		OnDrag(data);
	}

	public virtual void OnPointerUp(PointerEventData data)
	{
		inputDirection = Vector3.zero;
		image_1.rectTransform.anchoredPosition = Vector3.zero;
	}
}
