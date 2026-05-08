using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EFT.UI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tab : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[CompilerGenerated]
	private Action<Tab, bool> action_0;

	[SerializeField]
	protected GameObject _normalVersion;

	[SerializeField]
	protected GameObject _selectedVersion;

	[SerializeField]
	protected CanvasGroup _canvasGroup;

	[SerializeField]
	protected Image _targetImage;

	[SerializeField]
	protected Sprite _hoverSprite;

	[SerializeField]
	protected GameObject _hoverGraphic;

	[SerializeField]
	protected GameObject _idleGraphic;

	public LocalizedText LocalizedText;

	public bool Interactable = true;

	protected Sprite _normalSprite;

	private bool bool_0;

	protected bool _uiSelected;

	protected GInterface486 Controller;

	public virtual bool CanHandlePointerClick
	{
		get
		{
			if (!bool_0)
			{
				return Interactable;
			}
			return false;
		}
	}

	public event Action<Tab, bool> OnSelectionChanged
	{
		[CompilerGenerated]
		add
		{
			Action<Tab, bool> action = action_0;
			Action<Tab, bool> action2;
			do
			{
				action2 = action;
				Action<Tab, bool> value2 = (Action<Tab, bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Tab, bool> action = action_0;
			Action<Tab, bool> action2;
			do
			{
				action2 = action;
				Action<Tab, bool> value2 = (Action<Tab, bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Awake()
	{
		OnAwake();
	}

	public virtual void OnAwake()
	{
		if (_targetImage != null)
		{
			_normalSprite = _targetImage.sprite;
		}
		LocalizedText = GetComponent<LocalizedText>();
	}

	public virtual void Init(GInterface486 controller)
	{
		Controller = controller;
	}

	public void OnPointerClick([NotNull] PointerEventData eventData)
	{
		if (CanHandlePointerClick)
		{
			HandlePointerClick(bool_0 || _uiSelected);
		}
	}

	public virtual void HandlePointerClick(bool isSelectedNow)
	{
		action_0?.Invoke(this, !isSelectedNow);
	}

	public virtual void Select(bool sendCallback = true, bool uiOnly = false)
	{
		UpdateVisual(selected: true, uiOnly);
		if (sendCallback)
		{
			Controller?.Show();
		}
	}

	public virtual async Task<bool> Deselect()
	{
		bool flag;
		if (flag = Controller != null)
		{
			flag = !(await Controller.TryHide());
		}
		if (flag)
		{
			return false;
		}
		UpdateVisual(selected: false);
		return true;
	}

	public virtual void UpdateVisual(bool selected, bool uiOnly = false)
	{
		if (!uiOnly)
		{
			bool_0 = selected;
		}
		_uiSelected = selected;
		method_0();
	}

	public virtual void vmethod_0(bool active)
	{
		if (_targetImage != null)
		{
			GClass3839.ChangeImageAlpha(_targetImage, active ? 1f : 0.15f);
		}
		Interactable = active;
		if (_canvasGroup != null)
		{
			GClass856.SetUnlockStatus(_canvasGroup, active);
		}
	}

	public void method_0()
	{
		_normalVersion.gameObject.SetActive(!bool_0 && !_uiSelected);
		_selectedVersion.gameObject.SetActive(bool_0 || _uiSelected);
	}

	public virtual void OnPointerEnter([NotNull] PointerEventData eventData)
	{
		Hover(isHovered: true);
	}

	public virtual void OnPointerExit([NotNull] PointerEventData eventData)
	{
		Hover(isHovered: false);
	}

	public virtual void Hover(bool isHovered)
	{
		if (Interactable)
		{
			if (_targetImage != null)
			{
				_targetImage.sprite = (isHovered ? _hoverSprite : _normalSprite);
			}
			if (_hoverGraphic != null)
			{
				_hoverGraphic.SetActive(isHovered);
			}
			if (_idleGraphic != null)
			{
				_idleGraphic.SetActive(!isHovered);
			}
		}
	}
}
