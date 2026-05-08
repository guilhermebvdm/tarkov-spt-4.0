using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public abstract class BaseRandomAmbientSoundPlayer : BaseAmbientSoundPlayer, GInterface105
{
	private const float float_0 = 0.01f;

	[Header("Random Settings")]
	[Space]
	[SerializeField]
	private Vector2 _randomTimeRange = new Vector2(15f, 20f);

	private float float_1;

	private float float_2;

	private bool bool_2;

	public abstract SoundBank CurrentBank { get; set; }

	public override void Awake()
	{
		base.Awake();
		float_1 = Time.realtimeSinceStartup;
	}

	public override AudioClip GetClip()
	{
		if (!(CurrentBank == null))
		{
			return CurrentBank.PickSingleClip(0);
		}
		return base.EmptyClip;
	}

	public virtual float GetRandomDelay()
	{
		return Random.Range(_randomTimeRange.x, _randomTimeRange.y);
	}

	public override void OnPlay(float dt)
	{
		if (Time.realtimeSinceStartup > float_1 && !bool_2)
		{
			base.OnPlay(dt);
			bool_2 = true;
			float_2 = Time.realtimeSinceStartup + GetRandomDelay();
		}
		if (bool_2 && Time.realtimeSinceStartup >= float_2)
		{
			AudioClip clip = GetClip();
			float length = clip.length;
			if (CanPlay())
			{
				base.Source.pitch = GetPitch();
				base.Source.clip = clip;
				base.Source.Play();
			}
			bool_2 = false;
			float_1 = Time.realtimeSinceStartup + length + 0.01f;
		}
	}

	public virtual void ChangeSoundBank(SoundBank newSoundBank)
	{
		bool isPlaying = base.IsPlaying;
		if (isPlaying)
		{
			Stop(force: true);
		}
		CurrentBank = newSoundBank;
		if (isPlaying)
		{
			Play();
		}
	}

	public BaseRandomAmbientSoundPlayer()
	{
	}
}
