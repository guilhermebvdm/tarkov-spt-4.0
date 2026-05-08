using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter;

public class AmbientPlayerSplineMappedEmitter : AbstractSplineMappedEmitter
{
	[SerializeField]
	private BaseAmbientSoundPlayer _soundPlayer;

	[SerializeField]
	private Vector2 _spreadRange = new Vector2(0f, 1f);

	private Transform transform_0;

	[CompilerGenerated]
	private Vector3 vector3_0;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	public override Vector3 EmitterPosition
	{
		[CompilerGenerated]
		get
		{
			return vector3_0;
		}
		[CompilerGenerated]
		set
		{
			vector3_0 = value;
		}
	}

	public override event Action StartEmit
	{
		[CompilerGenerated]
		add
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_0;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public override event Action Emitted
	{
		[CompilerGenerated]
		add
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_1;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public override event Action Stopped
	{
		[CompilerGenerated]
		add
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action action = action_2;
			Action action2;
			do
			{
				action2 = action;
				Action value2 = (Action)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public override void OnAwake()
	{
		_soundPlayer.Played += method_0;
		_soundPlayer.Stopped += method_1;
		_soundPlayer.StartPlay += method_2;
		transform_0 = _soundPlayer.transform;
		base.OnAwake();
	}

	public void method_0()
	{
		action_1?.Invoke();
	}

	public void method_1()
	{
		action_2?.Invoke();
	}

	public void method_2()
	{
		action_0?.Invoke();
	}

	public override void Translate(Vector3 newPos)
	{
		EmitterPosition = newPos;
		transform_0.position = newPos;
	}

	public override void SetSpreadRange(Vector2 spreadRange)
	{
		_spreadRange = spreadRange;
	}

	public override void UpdateSpread(float value)
	{
		value = Mathf.Clamp(value, _spreadRange.x, _spreadRange.y);
		_soundPlayer.UpdateSpread(value);
	}

	public override void UpdateSpatialBlend(float value)
	{
		_soundPlayer.UpdateSpatialBlend(value);
	}

	public override void ScaleMaxDistance(float value)
	{
		_soundPlayer.ScaleMaxDistance(value);
	}

	public override void FadeOut(float fadeOutSec)
	{
		_soundPlayer.FadeOut(fadeOutSec);
	}

	public override void FadeIn(float fadeInSec)
	{
		_soundPlayer.FadeIn(fadeInSec);
	}

	public override void OnDestroy()
	{
		if (_soundPlayer != null)
		{
			_soundPlayer.Played -= method_0;
			_soundPlayer.Stopped -= action_2;
		}
		base.OnDestroy();
	}
}
