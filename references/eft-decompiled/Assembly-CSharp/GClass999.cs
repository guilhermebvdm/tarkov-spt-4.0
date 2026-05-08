using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using Koenigz.PerfectCulling.EFT;
using UnityEngine;

public class GClass999 : GInterface55, IDisposable, IAutocullAutomated
{
	[CompilerGenerated]
	private Action<bool> action_0;

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3 Vector3_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0 = -1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_2;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	[NonSerialized]
	public bool Bool_3;

	[NonSerialized]
	public bool Bool_4;

	[NonSerialized]
	public bool Bool_5;

	[NonSerialized]
	public bool Bool_6;

	public float Radius
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
	}

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			return Vector3_0;
		}
		[CompilerGenerated]
		set
		{
			Vector3_0 = value;
		}
	}

	public int Index
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

	public float CullDistanceSqr
	{
		[CompilerGenerated]
		get
		{
			return Float_1;
		}
	}

	public bool CullByDistanceOnly
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

	public float SqrCameraDistance
	{
		[CompilerGenerated]
		get
		{
			return Float_2;
		}
		[CompilerGenerated]
		set
		{
			Float_2 = value;
		}
	}

	public bool IsVisible
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

	public bool IsVisibleByCamera
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

	public Vector3 ClearTransformPosition => Position;

	public Vector3 SafeMultithreadedPosition => Position;

	public bool IsAutocullVisible
	{
		get
		{
			return Bool_5;
		}
		set
		{
			Bool_5 = value;
			method_0();
		}
	}

	public Bounds AutocullObjectBounds => new Bounds(Position, Vector3.one * Radius);

	public bool IsDynamicCullingObject => true;

	public event Action<bool> OnVisibilityChanged
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass999(Transform o, float radius, float cullingDistance)
	{
		Transform_0 = o;
		Float_0 = radius;
		Float_1 = cullingDistance * cullingDistance;
	}

	public void SetAutocullingWorkingStatus(bool isWorking)
	{
		Bool_6 = isWorking;
	}

	public void CustomUpdate()
	{
		Position = Transform_0.position;
		if (Singleton<ObservedCullingManager>.Instantiated && Index != -1)
		{
			Singleton<ObservedCullingManager>.Instance.UpdateSphere(this);
		}
	}

	public void Register()
	{
		if (Singleton<ObservedCullingManager>.Instantiated)
		{
			Index = Singleton<ObservedCullingManager>.Instance.Register(this);
			SetAutocullingWorkingStatus(Singleton<ObservedCullingManager>.Instance.IsUsingGrid);
		}
		GClass1237.AddDynamic(this);
	}

	public void method_0()
	{
		if (Bool_6)
		{
			if (Bool_4)
			{
				IsVisible = true;
			}
			else
			{
				IsVisible = Bool_5 && IsVisibleByCamera;
			}
		}
		else
		{
			IsVisible = Bool_4;
		}
		if (IsVisible != Bool_3)
		{
			action_0?.Invoke(IsVisible);
			Bool_3 = IsVisible;
		}
	}

	public void SetVisibility(bool isVisible)
	{
		Bool_4 = isVisible;
		method_0();
	}

	public void Dispose()
	{
		if (Singleton<ObservedCullingManager>.Instantiated && Index != -1)
		{
			Singleton<ObservedCullingManager>.Instance.Unregister(this);
		}
		GClass1237.RemoveDynamic(this);
		Index = -1;
	}
}
