using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Comfort.Common;
using Unity.Jobs;
using UnityEngine;

public class ObservedCullingManager : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Struct164 : IJob
	{
		public static readonly CullingManager.GClass996 JobParameters = new CullingManager.GClass996
		{
			FovMultiplicator = 1.5f
		};

		public static ObservedCullingManager Instance;

		public void Execute()
		{
			if (Instance == null || JobParameters.CameraInstance == null || !JobParameters.IsCameraEnabled)
			{
				return;
			}
			GClass2380 cameraFrustrum = JobParameters.CameraFrustrum;
			int count = Instance.gclass998_0.Count;
			for (int i = 0; i < count; i++)
			{
				Struct165 value = Instance.gclass998_0[i];
				float sqrMagnitude = (JobParameters.CameraPosition - value.Data.CullingObject.SafeMultithreadedPosition).sqrMagnitude;
				bool flag = cameraFrustrum.IntersectsSphere(ref value.Sphere);
				value.Data.VisibilityData.InFpsFrustum = flag;
				value.Data.VisibilityData.CurrentCameraDistanceSqr = sqrMagnitude;
				value.Data.CullingObject.SqrCameraDistance = sqrMagnitude;
				value.Data.VisibilityData.IsCulledByDistance = sqrMagnitude > value.Data.CullingDistanceSqr;
				value.Data.JobVisibilityFlag = true;
				if (value.Data.VisibilityData.IsCulledByDistance && (Instance.IsUsingGrid || !flag))
				{
					value.Data.JobVisibilityFlag = false;
				}
				Instance.gclass998_0[i] = value;
			}
		}
	}

	public struct Struct165
	{
		public BoundingSphere Sphere;

		public CullingManager.GStruct69 Data;
	}

	private Camera camera_0;

	private GClass998<Struct165> gclass998_0;

	private JobHandle jobHandle_0;

	private List<GClass999> list_0 = new List<GClass999>();

	[CompilerGenerated]
	private bool bool_0;

	public bool IsUsingGrid
	{
		[CompilerGenerated]
		get
		{
			return bool_0;
		}
		[CompilerGenerated]
		set
		{
			bool_0 = value;
		}
	}

	public void Awake()
	{
		gclass998_0 = new GClass998<Struct165>(512);
		Struct164.Instance = this;
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_5));
		Singleton<ObservedCullingManager>.Create(this);
		IsUsingGrid = GClass1238.Instance != null;
		GClass1238.OnCullingGridCreated -= method_1;
		if (!IsUsingGrid)
		{
			GClass1238.OnCullingGridCreated += method_1;
		}
	}

	public void Update()
	{
		method_3();
		method_4();
		method_0();
		method_2();
	}

	public int Register(GClass999 o)
	{
		Struct165 item = new Struct165
		{
			Sphere = new BoundingSphere(o.Position, o.Radius),
			Data = new CullingManager.GStruct69
			{
				CullingDistanceSqr = o.CullDistanceSqr,
				CullingObject = o,
				VisibilityData = new CullingManager.GStruct70
				{
					CullingByDistanceOnly = o.CullByDistanceOnly
				},
				JobVisibilityFlag = true
			}
		};
		list_0.Add(o);
		return gclass998_0.Add(item);
	}

	public void Unregister(GClass999 o)
	{
		gclass998_0.Data[o.Index].Data.Reset();
		gclass998_0.Remove(o.Index);
		list_0.Remove(o);
	}

	public void method_0()
	{
		int count = gclass998_0.Count;
		for (int i = 0; i < count; i++)
		{
			gclass998_0[i].Data.CullingObject.CustomUpdate();
		}
	}

	public void method_1()
	{
		IsUsingGrid = true;
		foreach (GClass999 item in list_0)
		{
			item.SetAutocullingWorkingStatus(isWorking: true);
		}
		GClass1238.OnCullingGridCreated -= method_1;
	}

	public void method_2()
	{
		if (Struct164.JobParameters.UpdateParameters())
		{
			jobHandle_0 = default(Struct164).Schedule();
			JobHandle.ScheduleBatchedJobs();
		}
	}

	public void method_3()
	{
		jobHandle_0.Complete();
	}

	public void method_4()
	{
		int count = gclass998_0.Count;
		for (int i = 0; i < count; i++)
		{
			CullingManager.GStruct69 data = gclass998_0[i].Data;
			data.CullingObject.SetVisibility(data.JobVisibilityFlag);
			data.CullingObject.IsVisibleByCamera = data.VisibilityData.InFpsFrustum;
		}
	}

	public void method_5(Camera cam)
	{
		if (!(cam == null) && !(cam == camera_0) && cam == CameraClass.Instance.Camera)
		{
			method_6(cam);
		}
	}

	public void UpdateSphere(GInterface55 o)
	{
		UpdateSphere(o, o.Index);
	}

	public void UpdateSphere(GInterface55 o, int index)
	{
		Struct165 dataByIndex = gclass998_0.GetDataByIndex(index);
		dataByIndex.Sphere.radius = o.Radius;
		dataByIndex.Sphere.position = o.Position;
		gclass998_0.SetDataByIndex(index, dataByIndex);
	}

	public void method_6(Camera cam)
	{
		camera_0 = cam;
		Struct164.JobParameters.CameraInstance = camera_0;
	}

	public void OnDestroy()
	{
		GClass1238.OnCullingGridCreated -= method_1;
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_5));
		Struct164.Instance = null;
		method_6(null);
		list_0.Clear();
		gclass998_0.Dispose();
		Singleton<ObservedCullingManager>.Release(this);
	}
}
