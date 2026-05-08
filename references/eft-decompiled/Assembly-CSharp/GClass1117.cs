using System;
using Audio.Data;
using EFT.DataProviding;
using UnityEngine;

public class GClass1117 : GClass1116
{
	[NonSerialized]
	public const float Float_0 = 25f;

	[NonSerialized]
	public const float Float_1 = 30f;

	[NonSerialized]
	public GInterface81 Ginterface81_0;

	[NonSerialized]
	public GInterface82 Ginterface82_0;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public GClass1175 Gclass1175_0;

	public GClass1117(BtrDriverSoundBankContainer soundContainer, Transform speakerTransform)
	{
		GClass3670.CreateData<GClass1175>(EDataLifeTime.Raid);
		Gclass1175_0 = GClass3670.GetData<GClass1175>();
		Gclass1175_0.SetPhrasesData(soundContainer);
		BetterSource source = MonoBehaviourSingleton<BetterAudio>.Instance.GetSource(BetterAudio.AudioSourceGroupType.VehicleInSpeech);
		Ginterface81_0 = new GClass1114(speakerTransform, source);
		Ginterface82_0 = new GClass1115(Ginterface81_0);
		Ginterface82_0.EventReceived += method_1;
	}

	public override void OnUpdate(float dt)
	{
		base.OnUpdate(dt);
		Ginterface81_0.ManualUpdate(dt);
		if (!Ginterface81_0.IsPlaying && Time.realtimeSinceStartup >= Float_2 && Gclass1175_0.TryGetBank(EBtrDriverPhraseTrigger.Mutter, out var bank))
		{
			Ginterface81_0.Play(bank);
			method_2();
		}
	}

	public float method_0()
	{
		return Time.realtimeSinceStartup + UnityEngine.Random.Range(25f, 30f);
	}

	public void method_1()
	{
		method_2();
	}

	public void method_2()
	{
		Float_2 = method_0();
	}

	public override void Dispose()
	{
		Ginterface81_0.Dispose();
		Ginterface82_0.EventReceived -= method_1;
		Ginterface82_0.Dispose();
		base.Dispose();
	}
}
