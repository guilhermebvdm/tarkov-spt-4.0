using System;
using System.Threading;
using UnityEngine;

public class GClass948<T> : IProgress<T>
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public CancellationToken CancellationToken_0;

	[NonSerialized]
	public GInterface47<T> Ginterface47_0;

	[NonSerialized]
	public Action<T> Action_0;

	public GClass948(int systemCount, GInterface47<T> numericOps, Action<T> handler, CancellationToken cancellationToken = default(CancellationToken))
	{
		Int_0 = systemCount;
		Ginterface47_0 = numericOps;
		Action_0 = handler;
		CancellationToken_0 = cancellationToken;
		Gparam_0 = new T[systemCount];
	}

	public void UpdateProgress(int systemIndex, T progressValue)
	{
		if (systemIndex >= 0 && systemIndex < Int_0)
		{
			CancellationToken cancellationToken_ = CancellationToken_0;
			if (cancellationToken_.IsCancellationRequested)
			{
				return;
			}
			Gparam_0[systemIndex] = progressValue;
			T x = default(T);
			try
			{
				T[] gparam_ = Gparam_0;
				foreach (T y in gparam_)
				{
					x = Ginterface47_0.Add(x, y);
				}
				x = Ginterface47_0.Divide(x, Int_0);
				Action_0(x);
				return;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return;
			}
		}
		Debug.LogError($"Invalid system index: {systemIndex}");
	}

	public void Report(T value)
	{
		CancellationToken cancellationToken_ = CancellationToken_0;
		if (!cancellationToken_.IsCancellationRequested)
		{
			try
			{
				Action_0(value);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
