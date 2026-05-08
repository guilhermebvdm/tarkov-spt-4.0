using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Audio.SpatialSystem;
using UnityEngine;

public class GClass1139
{
	[CompilerGenerated]
	public class Class790
	{
		public GClass1139 gclass1139_0;

		public GClass1138 dataContainer;

		public void method_0()
		{
			gclass1139_0.method_0(dataContainer);
		}
	}

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public GClass1120 Gclass1120_0;

	[NonSerialized]
	public TaskCompletionSource<bool> TaskCompletionSource_0;

	[NonSerialized]
	public Coroutine Coroutine_0;

	[NonSerialized]
	public MonoBehaviour MonoBehaviour_0;

	[CompilerGenerated]
	private Action action_0;

	public event Action OnDataLoaded
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

	public GClass1139(string dataPath, MonoBehaviour runner)
	{
		String_0 = Path.Combine(Application.streamingAssetsPath, dataPath);
		Gclass1120_0 = new GClass1120();
		MonoBehaviour_0 = runner;
	}

	public async Task LoadDataAsync(CancellationToken cancellationToken)
	{
		TaskCompletionSource_0 = new TaskCompletionSource<bool>();
		GClass1138 dataContainer = GClass3670.GetData<GClass1138>();
		dataContainer.Clear();
		method_1(dataContainer);
		method_2(String_0, dataContainer, cancellationToken, delegate
		{
			method_0(dataContainer);
		});
		try
		{
			await TaskCompletionSource_0.Task.Await(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			TaskCompletionSource_0?.TrySetCanceled();
			throw;
		}
		catch (Exception exception)
		{
			TaskCompletionSource_0?.TrySetException(exception);
			throw;
		}
	}

	public void method_0(GClass1138 dataContainer)
	{
		if (dataContainer != null && !dataContainer.IsDisposed)
		{
			dataContainer.BuildRoomPairIndex();
			TaskCompletionSource_0?.TrySetResult(result: true);
			Coroutine_0 = null;
			action_0?.Invoke();
		}
		else
		{
			TaskCompletionSource_0?.TrySetCanceled();
			Coroutine_0 = null;
		}
	}

	public void method_1(GClass1138 container)
	{
		Dictionary<short, BaseSpatialAudioPortal> portalsByID = container.PortalsByID;
		Dictionary<string, BaseSpatialAudioPortal> interactivePortalsByID = container.InteractivePortalsByID;
		Dictionary<short, SpatialAudioRoom> roomsByID = container.RoomsByID;
		BaseSpatialAudioPortal[] array = UnityEngine.Object.FindObjectsOfType<BaseSpatialAudioPortal>();
		SpatialAudioRoom[] array2 = UnityEngine.Object.FindObjectsOfType<SpatialAudioRoom>();
		BaseSpatialAudioPortal[] array3 = array;
		foreach (BaseSpatialAudioPortal baseSpatialAudioPortal in array3)
		{
			if ((object)baseSpatialAudioPortal == null)
			{
				continue;
			}
			portalsByID[baseSpatialAudioPortal.ID] = baseSpatialAudioPortal;
			container.AddStaticPortalData(baseSpatialAudioPortal);
			if (!(baseSpatialAudioPortal is IIdentifiable identifiable) || identifiable.Ids.Count <= 0)
			{
				continue;
			}
			foreach (string id in identifiable.Ids)
			{
				interactivePortalsByID[id] = baseSpatialAudioPortal;
			}
		}
		SpatialAudioRoom[] array4 = array2;
		foreach (SpatialAudioRoom spatialAudioRoom in array4)
		{
			if ((object)spatialAudioRoom != null)
			{
				roomsByID[spatialAudioRoom.ID] = spatialAudioRoom;
			}
		}
	}

	public void method_2(string path, GClass1138 container, CancellationToken cancellationToken, Action onDone = null)
	{
		Coroutine_0 = Gclass1120_0.ReadAndWriteToDataContainerAsync(MonoBehaviour_0, path, container, cancellationToken, onDone, delegate(Exception ex)
		{
			TaskCompletionSource_0?.TrySetException(ex);
		});
	}

	public void StopLoadCoroutine()
	{
		if (Coroutine_0 != null)
		{
			try
			{
				MonoBehaviour_0.StopCoroutine(Coroutine_0);
			}
			catch (Exception arg)
			{
				GClass722.Instance.LogWarn($"Failed to StopCoroutine: {arg}");
			}
			Coroutine_0 = null;
		}
	}

	[CompilerGenerated]
	public void method_3(Exception ex)
	{
		TaskCompletionSource_0?.TrySetException(ex);
	}
}
