using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.InventoryLogic;
using PlayerIcons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arena.UI;

public class FaceCardView : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private ArenaToggleStateToImageColor _arenaToggleStateToImageColorDictionary = new ArenaToggleStateToImageColor();

	[SerializeField]
	private List<FaceCardColorNode> _stateColors = new List<FaceCardColorNode>();

	[SerializeField]
	private Image[] _glows;

	[SerializeField]
	private Image[] _shadows;

	[SerializeField]
	private PlayerIconImage _playerIconImage;

	[CompilerGenerated]
	private Action<int> action_0;

	private int int_0;

	private bool bool_0;

	public event Action<int> Event_0
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = action_0;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = action_0;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Init(string faceName, InventoryEquipment inventoryEquipment, GClass2197 customization, ToggleGroup toggleGroup, int callbackValue, Action<int> callback, bool isSelected)
	{
		_label.text = faceName;
		_playerIconImage.SetPresetIcon(customization, inventoryEquipment);
		_toggle.group = toggleGroup;
		int_0 = callbackValue;
		method_3(callback);
		_toggle.onValueChanged.RemoveListener(method_0);
		_toggle.isOn = isSelected;
		_toggle.onValueChanged.AddListener(method_0);
		method_2((!isSelected) ? EToggleState.Deselected : EToggleState.Selected);
		bool_0 = isSelected;
	}

	public void method_0(bool isSelected)
	{
		bool_0 = isSelected;
		if (isSelected)
		{
			action_0?.Invoke(int_0);
		}
		method_2((!isSelected) ? EToggleState.Deselected : EToggleState.Selected);
	}

	public void method_1(bool isEnter)
	{
		if (!bool_0)
		{
			if (isEnter)
			{
				method_2(EToggleState.Hovered);
			}
			else
			{
				method_2((!bool_0) ? EToggleState.Deselected : EToggleState.Selected);
			}
		}
	}

	public void method_2(EToggleState toggleState)
	{
		foreach (FaceCardColorNode stateColor in _stateColors)
		{
			if (stateColor.State == toggleState)
			{
				Image[] glows = _glows;
				for (int i = 0; i < glows.Length; i++)
				{
					glows[i].color = stateColor.GlowColor;
				}
				glows = _shadows;
				for (int i = 0; i < glows.Length; i++)
				{
					glows[i].color = stateColor.ShadowColor;
				}
			}
		}
	}

	public void method_3(Action<int> listener)
	{
		action_0 = null;
		Event_0 += listener;
	}
}
