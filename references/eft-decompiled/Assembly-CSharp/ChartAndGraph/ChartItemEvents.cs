using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ChartAndGraph;

public class ChartItemEvents : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, Interface6
{
	[Serializable]
	public class Event : UnityEvent<GameObject>
	{
	}

	[SerializeField]
	private ChartItemTextBlend _textBlend;

	private bool bool_0;

	private bool bool_1;

	private IInternalUse iinternalUse_0;

	private object object_0;

	IInternalUse Interface6.Parent
	{
		get
		{
			return iinternalUse_0;
		}
		set
		{
			iinternalUse_0 = value;
		}
	}

	object Interface6.UserData
	{
		get
		{
			return object_0;
		}
		set
		{
			object_0 = value;
		}
	}

	public void OnMouseEnter()
	{
		if (!bool_0)
		{
			_textBlend.Grow();
		}
		if (iinternalUse_0 != null)
		{
			iinternalUse_0.InternalItemHovered(object_0);
		}
		bool_0 = true;
	}

	public void OnMouseExit()
	{
		if (bool_0)
		{
			_textBlend.Shrink();
		}
		if (iinternalUse_0 != null)
		{
			iinternalUse_0.InternalItemLeave(object_0);
		}
		bool_0 = false;
	}

	public void OnMouseDown()
	{
		if (!bool_1)
		{
			_textBlend.Grow();
		}
		if (iinternalUse_0 != null)
		{
			iinternalUse_0.InternalItemSelected(object_0);
		}
		bool_1 = true;
	}

	public void OnMouseUp()
	{
		bool_1 = false;
	}

	public void OnPointerEnter([CanBeNull] PointerEventData eventData)
	{
		OnMouseEnter();
	}

	public void OnPointerExit([CanBeNull] PointerEventData eventData)
	{
		OnMouseExit();
	}

	public void OnPointerDown([CanBeNull] PointerEventData eventData)
	{
		OnMouseDown();
	}

	public void OnPointerUp([CanBeNull] PointerEventData eventData)
	{
		OnMouseUp();
	}
}
