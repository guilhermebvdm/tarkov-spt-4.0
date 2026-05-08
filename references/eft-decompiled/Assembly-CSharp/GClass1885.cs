using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;

public class GClass1885 : IDisposable
{
	public enum ERemoveDelayReason
	{
		NoOneToSpawn,
		CantBeRemoved,
		Timeout
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1161
	{
		public static readonly Class1161 class1161_0 = new Class1161();

		public static Func<GInterface175, int> func_0;

		public int method_0(GInterface175 delayedParams)
		{
			return delayedParams.Count;
		}
	}

	[NonSerialized]
	public List<GInterface175> List_0 = new List<GInterface175>();

	[NonSerialized]
	public List<GInterface175> List_1 = new List<GInterface175>();

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0;

	[NonSerialized]
	[CompilerGenerated]
	public List<GInterface175> List_2 = new List<GInterface175>(LocalBotSettingsProviderClass.Core.MAX_SIMPLE_BOTS_IN_DELAY);

	public List<GInterface175> DelaysModels
	{
		[CompilerGenerated]
		get
		{
			return List_2;
		}
	}

	public int WaitCount => DelaysModels.Sum((GInterface175 delayedParams) => delayedParams.Count);

	public GClass1885()
	{
		CancellationTokenSource_0 = new CancellationTokenSource();
		Singleton<BotEventHandler>.Instance.OnStopBotSpawn += method_1;
		method_0(CancellationTokenSource_0.Token).HandleExceptions();
	}

	public void Add(GInterface175 simpleBotSpawnSpawnDelayResult)
	{
		if (WaitCount < DelaysModels.Capacity && !simpleBotSpawnSpawnDelayResult.Data.SpawnStopped)
		{
			DelaysModels.Add(simpleBotSpawnSpawnDelayResult);
		}
	}

	public void Remove(GInterface175 delayedBotsInfo, ERemoveDelayReason reason)
	{
		List_1.Add(delayedBotsInfo);
	}

	public async Task method_0(CancellationToken token)
	{
		while (true)
		{
			List_0.Clear();
			method_2();
			_ = DelaysModels.Count;
			foreach (GInterface175 delaysModel in DelaysModels)
			{
				if (smethod_1(delaysModel, out var reason))
				{
					Remove(delaysModel, reason);
				}
				else if (smethod_0(delaysModel))
				{
					List_0.Add(delaysModel);
				}
			}
			if (List_0.Count > 0)
			{
				foreach (GInterface175 item in List_0)
				{
					item.OnDelayFinished();
				}
			}
			method_2();
			token.ThrowIfCancellationRequested();
			await Task.Delay(TimeSpan.FromSeconds(3.0), token);
		}
	}

	public static bool smethod_0(GInterface175 delayModel)
	{
		bool flag = Singleton<AbstractGame>.Instance.GameTimer.PastTime.TotalMilliseconds > 0.0;
		bool num = Time.time - delayModel.CreatedTime > 3f;
		bool flag2 = Time.time - delayModel.LastCheckTime > 1f;
		delayModel.LastCheckTime = Time.time;
		return num && flag2 && flag;
	}

	public static bool smethod_1(GInterface175 delayModel, out ERemoveDelayReason reason)
	{
		if (delayModel.Count != 0 && delayModel.Data.Count != 0)
		{
			if (!delayModel.CanBeRemoved)
			{
				reason = ERemoveDelayReason.CantBeRemoved;
				return false;
			}
			reason = ERemoveDelayReason.Timeout;
			return Time.time - delayModel.CreatedTime > (float)LocalBotSettingsProviderClass.Core.SIMPLE_BOTS_IN_DELAY_PERIOD;
		}
		reason = ERemoveDelayReason.NoOneToSpawn;
		return true;
	}

	public void method_1()
	{
		List_1.AddRange(DelaysModels);
	}

	public void method_2()
	{
		if (List_1.Count <= 0)
		{
			return;
		}
		foreach (GInterface175 item in List_1)
		{
			DelaysModels.Remove(item);
		}
		List_1.Clear();
	}

	public void Dispose()
	{
		CancellationTokenSource_0.Cancel();
	}
}
