using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1120
{
	[CompilerGenerated]
	public class Class786
	{
		public Action<Exception> onError;

		public void method_0(Exception ex)
		{
			if (onError != null)
			{
				onError(ex);
			}
			else
			{
				Debug.LogException(ex);
			}
		}
	}

	[NonSerialized]
	public GClass1121 Gclass1121_0;

	[NonSerialized]
	public GClass1119 Gclass1119_0;

	public GClass1120()
	{
		Gclass1121_0 = new GClass1121();
		Gclass1119_0 = new GClass1119();
	}

	public void Write(SerializableAudioRoutesBakeData bakeDataSo, string pathToSave)
	{
		Gclass1121_0.Write(bakeDataSo, pathToSave);
	}

	public Coroutine ReadAndWriteToDataContainerAsync(MonoBehaviour runner, string bakeDataPath, GClass1138 dataContainer, CancellationToken cancellationToken, Action onDone = null, Action<Exception> onError = null)
	{
		if (runner == null)
		{
			throw new ArgumentNullException("runner");
		}
		return runner.StartCoroutine(Gclass1119_0.ReadAndWriteToDataContainerCoroutine(bakeDataPath, dataContainer, cancellationToken, onDone, delegate(Exception ex)
		{
			if (onError != null)
			{
				onError(ex);
			}
			else
			{
				Debug.LogException(ex);
			}
		}));
	}
}
