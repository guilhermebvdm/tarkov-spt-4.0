using System;
using System.Collections.Generic;
using Audio.Data;
using UnityEngine;

public class GClass1114 : GClass1113
{
	public readonly struct Struct202(SoundBankWithSettings bank, bool stereo)
	{
		public readonly SoundBankWithSettings Bank = bank;

		public readonly bool Stereo = stereo;
	}

	[NonSerialized]
	public const float Float_0 = 1f;

	[NonSerialized]
	public Queue<Struct202> Queue_0 = new Queue<Struct202>();

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public BetterSource BetterSource_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public int Int_0 = -1;

	public override bool IsPlaying => BetterSource_0.PlayBackState == BetterSource.EPlayBackState.Playing;

	public GClass1114(Transform speakerTransform, BetterSource source)
	{
		Transform_0 = speakerTransform;
		BetterSource_0 = source;
		BetterSource_0.SetParent(Transform_0, worldPositionStay: false);
		BetterSource_0.SetActive(active: true);
	}

	public override void OnPlay(SoundBankWithSettings bank, bool stereo = false)
	{
		Struct202 item = new Struct202(bank, stereo);
		Queue_0.Enqueue(item);
	}

	public override void OnUpdate(float dt)
	{
		if (Queue_0.Count == 0)
		{
			return;
		}
		Struct202 @struct = Queue_0.Peek();
		if (!(Time.realtimeSinceStartup < Float_1) && (BetterSource_0.PlayBackState != BetterSource.EPlayBackState.Playing || @struct.Bank.Importance > Int_0))
		{
			Struct202 struct2 = Queue_0.Dequeue();
			Int_0 = struct2.Bank.Importance;
			if (struct2.Bank.TryGetRandomClip(out var clip))
			{
				BetterSource_0.source1.transform.rotation = Transform_0.rotation;
				BetterSource_0.SetRolloff(@struct.Bank.Rolloff);
				BetterSource_0.Play(clip, null, 1f, struct2.Bank.BaseVolume, struct2.Stereo, oneShot: false);
				Float_1 = Time.realtimeSinceStartup + clip.length + 1f;
			}
			if (Queue_0.Count == 0)
			{
				Int_0 = -1;
			}
		}
	}

	public override void OnStop()
	{
		BetterSource_0.Stop();
		Int_0 = -1;
	}

	public override void Dispose()
	{
		Stop();
		Queue_0.Clear();
		BetterSource_0.Release();
		base.Dispose();
	}
}
