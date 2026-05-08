using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public abstract class GClass1882<T> : GInterface175 where T : GClass1882<T>
{
	[NonSerialized]
	public string String_0;

	public readonly Action<T> ContinuationCallback;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotCreationDataClass BotCreationDataClass;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	public int Count
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public BotCreationDataClass Data
	{
		[CompilerGenerated]
		get
		{
			return BotCreationDataClass;
		}
	}

	public bool CanBeRemoved
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public float CreatedTime
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
	}

	public float LastCheckTime
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
		[CompilerGenerated]
		set
		{
			Float_1 = value;
		}
	}

	public GClass1882(int count, BotCreationDataClass creationData, Action<T> callback)
	{
		Int_0 = count;
		BotCreationDataClass = creationData;
		ContinuationCallback = callback;
		Bool_0 = creationData.IsValidSpawnType(WildSpawnType.marksman);
		LastCheckTime = (Float_0 = Time.time);
		if (GClass398.Instance.IsTraceEnable())
		{
			String_0 = Environment.StackTrace;
		}
	}

	public void OnDelayFinished()
	{
		ContinuationCallback?.Invoke((T)this);
	}

	public override string ToString()
	{
		return "creationDataId:" + Data.Id;
	}
}
