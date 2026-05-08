using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using EFT.GlobalEvents;
using UnityEngine;
using UnityEngine.Audio;

public class GClass1196 : GInterface108
{
	[NonSerialized]
	public const string String_0 = "Audio/Events/InfectedEvent/ZombieAttackEventScreams";

	[NonSerialized]
	public const int Int_0 = 10;

	[NonSerialized]
	public const float Float_0 = 100f;

	[NonSerialized]
	public HashSet<Vector3> HashSet_0 = new HashSet<Vector3>();

	[NonSerialized]
	public Vector2 Vector2_0 = new Vector2(0.1f, 1f);

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0;

	[NonSerialized]
	public SoundBank SoundBank_0;

	[NonSerialized]
	public Action Action_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool IsRunning
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

	public void Run()
	{
		if (IsRunning)
		{
			Debug.Log("HalloweenInfectedEventAudioPlayer is already running");
			return;
		}
		IsRunning = true;
		method_0().HandleExceptions();
	}

	public async Task method_0()
	{
		CancellationTokenSource_0 = new CancellationTokenSource();
		CancellationToken token = CancellationTokenSource_0.Token;
		try
		{
			SoundBank_0 = await method_1("Audio/Events/InfectedEvent/ZombieAttackEventScreams", token);
			Action_0 = (Action)Delegate.Combine(Action_0, GlobalEventHandlerClass.Instance.SubscribeOnEvent<BotsCrowdSpawnEvent>(method_2));
		}
		catch (OperationCanceledException)
		{
			Debug.Log("HalloweenInfectedEventAudioPlayer stopped on Init.");
		}
		finally
		{
			if (token.IsCancellationRequested)
			{
				Debug.Log("HalloweenInfectedEventAudioPlayer initialization cancelled.");
			}
		}
	}

	public async Task<SoundBank> method_1(string path, CancellationToken cancellationToken)
	{
		ResourceRequest resourceRequest = Resources.LoadAsync<SoundBank>(path);
		while (!resourceRequest.isDone)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				cancellationToken.ThrowIfCancellationRequested();
			}
			await Task.Yield();
		}
		cancellationToken.ThrowIfCancellationRequested();
		return resourceRequest.asset as SoundBank;
	}

	public void Stop()
	{
		if (IsRunning)
		{
			try
			{
				CancellationTokenSource_0?.Cancel();
				CancellationTokenSource_0?.Dispose();
			}
			catch (ObjectDisposedException)
			{
				Debug.Log("HalloweenInfectedEventAudioPlayer token source is disposed");
			}
			Action_0?.Invoke();
			IsRunning = false;
		}
	}

	public void method_2(BotsCrowdSpawnEvent spawnEvent)
	{
		Vector3 position = GamePlayerOwner.MyPlayer.Position;
		string profileID = spawnEvent.ProfileID;
		IPlayerOwner everExistedBridgeByProfileID = Singleton<GameWorld>.Instance.GetEverExistedBridgeByProfileID(profileID);
		Vector3 position2 = everExistedBridgeByProfileID.iPlayer.Position;
		float num = Vector3.Distance(position, position2);
		float radius = Mathf.Lerp(100f, 1f, Mathf.Clamp01(num / 100f));
		HashSet<Vector3> hashSet = method_3(position2, radius, 10);
		EnvironmentManager instance = EnvironmentManager.Instance;
		EnvironmentType environment = EnvironmentType.Outdoor;
		if (instance != null)
		{
			environment = (everExistedBridgeByProfileID.iPlayer.IsYourPlayer ? instance.Environment : instance.GetPlayerCurrentEnvironmentType(profileID));
		}
		foreach (Vector3 item in hashSet)
		{
			method_4(item, num, environment);
		}
	}

	public HashSet<Vector3> method_3(Vector3 center, float radius, int pointsCount)
	{
		HashSet_0.Clear();
		for (int i = 0; i < pointsCount; i++)
		{
			float f = MathF.PI * 2f * (float)i / (float)pointsCount;
			float num = Mathf.Cos(f) * radius;
			float num2 = Mathf.Sin(f) * radius;
			Vector3 item = new Vector3(center.x + num, center.y, center.z + num2);
			HashSet_0.Add(item);
		}
		return HashSet_0;
	}

	public void method_4(Vector3 position, float distanceToMyPlayer, EnvironmentType environment)
	{
		if (SoundBank_0.Environments.Length == 0)
		{
			Debug.LogError("HalloweenInfectedEventAudioPlayer sound bank _zombieCrowdSoundBank is empty");
			return;
		}
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		if (instance == null)
		{
			Debug.LogError("BetterAudio not exist!");
			return;
		}
		AudioMixerGroup forceMixerGroup = ((environment == EnvironmentType.Indoor) ? instance.CommonAmbientInEffectsMixer : instance.CommonAmbientOutEffectsMixer);
		float delay = TransformHelperClass.Random(Vector2_0);
		instance.PlayAtPointDelayed(position, SoundBank_0, SoundBank_0.SourceType, distanceToMyPlayer, delay, 1f, distanceToMyPlayer, environment, EOcclusionTest.None, spatialize: false, forceMixerGroup);
	}
}
