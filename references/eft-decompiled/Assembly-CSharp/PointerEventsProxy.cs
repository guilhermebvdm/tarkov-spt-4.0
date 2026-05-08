using Diz.Binding;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

[UsedImplicitly]
public class PointerEventsProxy : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private readonly BindableEvent bindableEvent_0 = new BindableEvent();

	private readonly BindableEvent bindableEvent_1 = new BindableEvent();

	public IBindableEvent OnPointerEnter => bindableEvent_0;

	public IBindableEvent OnPointerExit => bindableEvent_1;

	public void OnPointerEnter(PointerEventData eventData)
	{
		bindableEvent_0.Invoke();
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPointerEnter
		this.OnPointerEnter(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		bindableEvent_1.Invoke();
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		//ILSpy generated this explicit interface implementation from .override directive in OnPointerExit
		this.OnPointerExit(eventData);
	}
}
