using System;
using System.Runtime.CompilerServices;
using CommonAssets.Scripts.Game.GameTriggers.Handlers;
using UnityEngine;

public class SyncableAudioMuteState : ISyncAble
{
	[SerializeField]
	private HandlerAudioSourceMute[] _handlersAudioSourceMute;

	private bool? nullable_0;

	public void Start()
	{
		if (_handlersAudioSourceMute == null)
		{
			return;
		}
		HandlerAudioSourceMute[] handlersAudioSourceMute = _handlersAudioSourceMute;
		foreach (HandlerAudioSourceMute handlerAudioSourceMute in handlersAudioSourceMute)
		{
			if (!(handlerAudioSourceMute == null))
			{
				handlerAudioSourceMute.OnMuteStateChanged += delegate(bool nextState)
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
		if (_handlersAudioSourceMute != null && nullable_0.HasValue)
		{
			HandlerAudioSourceMute[] handlersAudioSourceMute = _handlersAudioSourceMute;
			for (int i = 0; i < handlersAudioSourceMute.Length; i++)
			{
				handlersAudioSourceMute[i].ApplyState(nullable_0.Value);
			}
		}
	}

	[CompilerGenerated]
	public void method_2(bool nextState)
	{
		nullable_0 = nextState;
	}
}
