using System;
using System.Threading;
using UnityEngine;

public class GClass1519<T> : IProgress<T>
{
	[NonSerialized]
	public Action<T> Action_0;

	[NonSerialized]
	public CancellationToken CancellationToken_0;

	public GClass1519(Action<T> handler, CancellationToken cancellationToken = default(CancellationToken))
	{
		Action_0 = handler;
		CancellationToken_0 = cancellationToken;
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
