using System;
using System.Runtime.CompilerServices;
using Audio.Data;

public class GClass1113 : GInterface81, IDisposable
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public virtual bool IsPlaying
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public void Play(SoundBankWithSettings bank, bool stereo = false)
	{
		OnPlay(bank, stereo);
		IsPlaying = true;
	}

	public void ManualUpdate(float dt = 0f)
	{
		OnUpdate(dt);
	}

	public void Stop()
	{
		OnStop();
		IsPlaying = false;
	}

	public virtual void OnPlay(SoundBankWithSettings bank, bool stereo = false)
	{
	}

	public virtual void OnUpdate(float dt)
	{
	}

	public virtual void OnStop()
	{
	}

	public virtual void Dispose()
	{
	}
}
