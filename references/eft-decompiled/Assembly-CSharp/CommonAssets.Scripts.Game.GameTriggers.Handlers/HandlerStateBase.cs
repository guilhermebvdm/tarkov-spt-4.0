using System;
using UnityEngine;

namespace CommonAssets.Scripts.Game.GameTriggers.Handlers;

public abstract class HandlerStateBase : MonoBehaviour
{
	public abstract event Action<bool> StateChanged;

	public abstract void ApplyState(bool state);

	public HandlerStateBase()
	{
	}
}
