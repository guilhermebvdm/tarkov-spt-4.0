using System.Collections.Generic;
using EFT.UI;
using Sirenix.OdinInspector;
using UnityEngine;

public class StatedUIButtonAnimation : SerializedMonoBehaviour, GInterface473, GInterface474<EButtonAnimationState>
{
	[SerializeField]
	private Dictionary<EButtonAnimationState, GameObject[]> _states;

	public void TransitionToState(EButtonAnimationState state)
	{
		method_0(state);
	}

	public void SetState(EButtonAnimationState state)
	{
		method_0(state);
	}

	public void method_0(EButtonAnimationState state)
	{
		foreach (KeyValuePair<EButtonAnimationState, GameObject[]> state2 in _states)
		{
			state2.Deconstruct(out var key, out var value);
			EButtonAnimationState num = key;
			GameObject[] array = value;
			if (num != state && array != null)
			{
				value = array;
				for (int i = 0; i < value.Length; i++)
				{
					value[i].SetActive(value: false);
				}
			}
		}
		if (_states.TryGetValue(state, out var value2))
		{
			GameObject[] value = value2;
			for (int i = 0; i < value.Length; i++)
			{
				value[i].SetActive(value: true);
			}
		}
	}

	public void Stop()
	{
	}
}
