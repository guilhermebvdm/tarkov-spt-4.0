using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Audio.SpatialSystem;
using UnityEngine;

public class SourceContainerClass : IEnumerable, GInterface86
{
	[NonSerialized]
	public const float Float_0 = 0.15f;

	public const int INVALID_ID = -1;

	[NonSerialized]
	public static ISpatialAudioRoom IspatialAudioRoom_0 = new GClass1149();

	[NonSerialized]
	public List<BetterSource> List_0 = new List<BetterSource>();

	[NonSerialized]
	public Dictionary<int, bool> Dictionary_0 = new Dictionary<int, bool>();

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public Vector3 Vector3_1 = Vector3.zero;

	[NonSerialized]
	public float Float_2 = 1f;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_3;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_4;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_5;

	[NonSerialized]
	[CompilerGenerated]
	public EOcclusionTest EocclusionTest_0;

	[NonSerialized]
	[CompilerGenerated]
	public ISpatialAudioRoom IspatialAudioRoom_1 = IspatialAudioRoom_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_6;

	[CompilerGenerated]
	private Action<BetterSource> action_0;

	[CompilerGenerated]
	private Action action_1;

	[CompilerGenerated]
	private Action action_2;

	[CompilerGenerated]
	private Action<int> action_3;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	public int ID
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public float ObstructionEffect
	{
		[CompilerGenerated]
		get
		{
			return Float_3;
		}
		[CompilerGenerated]
		set
		{
			Float_3 = value;
		}
	}

	public float PropagationEffect
	{
		[CompilerGenerated]
		get
		{
			return Float_4;
		}
		[CompilerGenerated]
		set
		{
			Float_4 = value;
		}
	}

	public float OcclusionEffect
	{
		[CompilerGenerated]
		get
		{
			return Float_5;
		}
		[CompilerGenerated]
		set
		{
			Float_5 = value;
		}
	}

	public EOcclusionTest OcclusionTest
	{
		[CompilerGenerated]
		get
		{
			return EocclusionTest_0;
		}
		[CompilerGenerated]
		set
		{
			EocclusionTest_0 = value;
		}
	}

	public ISpatialAudioRoom CurrentAudioRoom
	{
		[CompilerGenerated]
		get
		{
			return IspatialAudioRoom_1;
		}
		[CompilerGenerated]
		set
		{
			IspatialAudioRoom_1 = value;
		}
	}

	public bool IsValid
	{
		get
		{
			if (Bool_0)
			{
				if (!IsLongTermSupported)
				{
					return List_0.Count > 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsLongTermSupported
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public bool IsPlaying
	{
		get
		{
			foreach (BetterSource item in List_0)
			{
				if (item.PlayBackState == BetterSource.EPlayBackState.Playing)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsOutOfHearingRange
	{
		get
		{
			if (!CameraClass.Exist)
			{
				return true;
			}
			return CameraClass.Instance.SqrDistance(CurrentPosition) > Float_1;
		}
	}

	public Vector3 CurrentPosition => method_0();

	public float MaxDistance
	{
		[CompilerGenerated]
		get
		{
			return Float_6;
		}
		[CompilerGenerated]
		set
		{
			Float_6 = value;
		}
	}

	public float SqrMaxDistance => Float_1;

	public int Count => List_0.Count;

	public BetterSource this[int i] => List_0[i];

	public Bounds SourceBounds => new Bounds(CurrentPosition, Vector3.one * 0.15f);

	public float MaxOcclusionQualityFactor
	{
		get
		{
			return Float_2;
		}
		set
		{
			Float_2 = Mathf.Clamp01(value);
		}
	}

	public bool UseQualityCompression
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public event Action<BetterSource> OnSourceAdded
	{
		[CompilerGenerated]
		add
		{
			Action<BetterSource> action = action_0;
			Action<BetterSource> action2;
			do
			{
				action2 = action;
				Action<BetterSource> value2 = (Action<BetterSource>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<BetterSource> action = action_0;
			Action<BetterSource> action2;
			do
			{
				action2 = action;
				Action<BetterSource> value2 = (Action<BetterSource>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action OnSourceRemoved
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

	public event Action OnSourcePlayed
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

	public event Action<int> OnContainerEmpty
	{
		[CompilerGenerated]
		add
		{
			Action<int> action = action_3;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<int> action = action_3;
			Action<int> action2;
			do
			{
				action2 = action;
				Action<int> value2 = (Action<int>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Initialize(Transform parentTransform, EOcclusionTest test, int id, bool isLongTermSupported = false)
	{
		OcclusionTest = test;
		ID = id;
		IsLongTermSupported = isLongTermSupported;
		Transform_0 = parentTransform;
		Vector3_0 = parentTransform.position;
		Bool_0 = true;
	}

	public void AddSource(BetterSource source, bool limit = false)
	{
		if (!Bool_0)
		{
			Debug.LogWarning("Container not initialized!");
		}
		else if (!List_0.Contains(source))
		{
			source.OnReleased += RemoveSource;
			source.OnPlayed += method_1;
			List_0.Add(source);
			Dictionary_0[source.GetInstanceID()] = limit;
			method_2();
			action_0?.Invoke(source);
		}
	}

	public void RemoveSource(BetterSource source)
	{
		if (List_0.Contains(source))
		{
			source.OnReleased -= RemoveSource;
			source.OnPlayed -= method_1;
			List_0.Remove(source);
			Dictionary_0.Remove(source.GetInstanceID());
			method_2();
			action_1?.Invoke();
			if (List_0.Count == 0)
			{
				action_3?.Invoke(ID);
			}
		}
	}

	public bool IsSourceAllowedLimit(int sourceInstanceID)
	{
		bool value;
		return Dictionary_0.TryGetValue(sourceInstanceID, out value) && value;
	}

	public bool IsOutOfHearingRangeWithOffset(Vector3 targetPos, float offset = 5f)
	{
		offset *= offset;
		return Vector3.SqrMagnitude(CurrentPosition - targetPos) > Float_1 + offset;
	}

	public void SetPositionOffset(Vector3 offset)
	{
		Vector3_1 = offset;
	}

	public Vector3 method_0()
	{
		if (!(Transform_0 == null))
		{
			return Transform_0.position + Vector3_1;
		}
		return Vector3_0;
	}

	public void method_1()
	{
		action_2?.Invoke();
	}

	public void method_2()
	{
		MaxDistance = float.MinValue;
		for (int i = 0; i < List_0.Count; i++)
		{
			BetterSource betterSource = List_0[i];
			if (betterSource.MaxDistance > MaxDistance)
			{
				MaxDistance = betterSource.MaxDistance;
				Float_1 = MaxDistance * MaxDistance;
			}
		}
	}

	public void Clear()
	{
		for (int num = List_0.Count - 1; num >= 0; num--)
		{
			RemoveSource(List_0[num]);
		}
		CurrentAudioRoom = IspatialAudioRoom_0;
		OcclusionTest = EOcclusionTest.None;
		OcclusionEffect = 0f;
		PropagationEffect = 0f;
		ObstructionEffect = 0f;
		Transform_0 = null;
		Vector3_0 = Vector3.zero;
		Bool_0 = false;
		MaxDistance = 0f;
		Float_1 = 0f;
		IsLongTermSupported = true;
		UseQualityCompression = false;
		Float_2 = 1f;
	}

	public Vector3 GetCachedPosition()
	{
		return Vector3_0;
	}

	public void UpdateCachedPosition()
	{
		Vector3_0 = method_0();
	}

	public void UpdateOcclusionData(GInterface86 newOcclusionData)
	{
		OcclusionEffect = newOcclusionData.OcclusionEffect;
		ObstructionEffect = newOcclusionData.ObstructionEffect;
		PropagationEffect = newOcclusionData.PropagationEffect;
	}

	public bool IsOutOfHearingRangeWithOffset(float offset = 0f)
	{
		if (!CameraClass.Exist)
		{
			return true;
		}
		float num = offset * offset;
		return CameraClass.Instance.SqrDistance(CurrentPosition) > Float_1 + num;
	}

	public IEnumerator GetEnumerator()
	{
		return List_0.GetEnumerator();
	}

	public IEnumerator GetEnumerator_1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		//ILSpy generated this explicit interface implementation from .override directive in GetEnumerator_1
		return this.GetEnumerator_1();
	}
}
