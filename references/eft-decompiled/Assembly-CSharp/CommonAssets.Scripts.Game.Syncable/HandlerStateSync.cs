using System;
using System.Runtime.CompilerServices;
using CommonAssets.Scripts.Game.GameTriggers.Handlers;
using UnityEngine;

namespace CommonAssets.Scripts.Game.Syncable;

public class HandlerStateSync : ISyncAble
{
	[SerializeField]
	private HandlerStateBase[] _handlerStates;

	private bool? nullable_0;

	public void Start()
	{
		if (_handlerStates == null || _handlerStates.Length == 0)
		{
			return;
		}
		HandlerStateBase[] handlerStates = _handlerStates;
		foreach (HandlerStateBase handlerStateBase in handlerStates)
		{
			if (!(handlerStateBase == null))
			{
				handlerStateBase.StateChanged += delegate(bool nextState)
				{
					nullable_0 = nextState;
				};
			}
		}
	}

	public override void Serialize(GInterface131 writerStream)
	{
		writerStream.Write(nullable_0.HasValue);
		if (nullable_0.HasValue)
		{
			writerStream.Write(nullable_0.Value);
		}
	}

	public override void Deserialize(IDataReader readerStream)
	{
		if (!readerStream.ReadBool())
		{
			return;
		}
		nullable_0 = readerStream.ReadBool();
		try
		{
			method_1();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public void method_1()
	{
		if (_handlerStates != null && nullable_0.HasValue)
		{
			HandlerStateBase[] handlerStates = _handlerStates;
			for (int i = 0; i < handlerStates.Length; i++)
			{
				handlerStates[i].ApplyState(nullable_0.Value);
			}
		}
	}

	[CompilerGenerated]
	public void method_2(bool nextState)
	{
		nullable_0 = nextState;
	}
}
