using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace Audio.AmbientSubsystem.AmbientSplineEmitter.SplineSoundEmitter;

public class AmbientPlayerGroupSplineEmitter : AbstractSplineMappedEmitter
{
	[Serializable]
	public class SoundPlayerConfig
	{
		public BaseAmbientSoundPlayer soundPlayer;

		public Vector2 spreadRange = new Vector2(0f, 1f);
	}

	[SerializeField]
	private List<SoundPlayerConfig> _configs;

	[CompilerGenerated]
	private Action action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Vector3 vector3_0;

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
		foreach (SoundPlayerConfig config in _configs)
		{
			config.soundPlayer.Played += method_0;
			config.soundPlayer.Stopped += method_1;
			config.soundPlayer.StartPlay += method_2;
		}
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
		foreach (SoundPlayerConfig config in _configs)
		{
			config.soundPlayer.transform.position = newPos;
		}
	}

	public override void SetSpreadRange(Vector2 spreadRange)
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			config.spreadRange = spreadRange;
		}
	}

	public override void UpdateSpread(float value)
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			value = Mathf.Clamp(value, config.spreadRange.x, config.spreadRange.y);
			config.soundPlayer.UpdateSpread(value);
		}
	}

	public override void UpdateSpatialBlend(float value)
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			config.soundPlayer.UpdateSpatialBlend(value);
		}
	}

	public override void ScaleMaxDistance(float value)
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			config.soundPlayer.ScaleMaxDistance(value);
		}
	}

	public override void FadeOut(float fadeOutSec)
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			config.soundPlayer.FadeOut(fadeOutSec);
		}
	}

	public override void FadeIn(float fadeInSec)
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			config.soundPlayer.FadeIn(fadeInSec);
		}
	}

	public override void OnDestroy()
	{
		foreach (SoundPlayerConfig config in _configs)
		{
			if (!(config?.soundPlayer == null))
			{
				config.soundPlayer.Played -= method_0;
				config.soundPlayer.Stopped -= method_1;
				config.soundPlayer.StartPlay -= method_2;
			}
		}
		base.OnDestroy();
	}
}
