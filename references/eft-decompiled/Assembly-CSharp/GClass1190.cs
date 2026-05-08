using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Audio.AmbientSubsystem.PathMoverStrategy;
using UnityEngine;

public abstract class GClass1190 : GInterface106
{
	[NonSerialized]
	public MonoBehaviour MonoBehaviour_0;

	[NonSerialized]
	public GInterface110 Ginterface110_0;

	[NonSerialized]
	public Vector3[] Vector3_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public Vector3 Vector3_1 = Vector3.zero;

	[NonSerialized]
	public float[] Float_2;

	[CompilerGenerated]
	private Action action_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0 = true;

	public bool IsCompleted
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

	public abstract EMovementStrategy MovementStrategy { get; }

	public event Action MovementCompleted
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

	public GClass1190(MonoBehaviour behaviour, GInterface110 posTranslator, Vector3[] points, float[] segmentLengths, float totalLength)
	{
		MonoBehaviour_0 = behaviour;
		Ginterface110_0 = posTranslator;
		Vector3_0 = points;
		Float_0 = totalLength;
		Float_2 = segmentLengths;
	}

	public void SetSpeed(float duration)
	{
		Float_1 = duration;
	}

	public void Execute()
	{
		IsCompleted = false;
		ResetParameters();
		OnExecute(method_0);
	}

	public void method_0()
	{
		if (!IsCompleted)
		{
			IsCompleted = true;
			action_0?.Invoke();
		}
	}

	public abstract void OnExecute(Action onDone = null);

	public void Stop(bool resetPosition = false)
	{
		ResetParameters();
		method_0();
	}

	public virtual void ResetParameters(bool resetPosition = false)
	{
	}

	public Vector3 method_1()
	{
		if (Int_0 >= Vector3_0.Length)
		{
			method_3();
		}
		Vector3_1 = Vector3_0[Int_0];
		Int_0++;
		return Vector3_1;
	}

	public Vector3 method_2(float arcLength)
	{
		arcLength = Mathf.Clamp(arcLength, 0f, Float_0);
		float num = 0f;
		int num2 = 0;
		while (true)
		{
			if (num2 < Float_2.Length)
			{
				if (!(arcLength > num + Float_2[num2]))
				{
					break;
				}
				num += Float_2[num2];
				num2++;
				continue;
			}
			Vector3_1 = Vector3_0[Vector3_0.Length - 1];
			return Vector3_1;
		}
		float t = (arcLength - num) / Float_2[num2];
		int num3 = num2 * 3;
		Vector3_1 = GClass1280.GetPoint(Vector3_0[num3], Vector3_0[num3 + 1], Vector3_0[num3 + 2], Vector3_0[num3 + 3], t);
		return Vector3_1;
	}

	public void method_3()
	{
		for (int num = Vector3_0.Length - 1; num > 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num + 1);
			Vector3 vector = Vector3_0[num];
			Vector3_0[num] = Vector3_0[num2];
			Vector3_0[num2] = vector;
		}
		Int_0 = 0;
	}
}
