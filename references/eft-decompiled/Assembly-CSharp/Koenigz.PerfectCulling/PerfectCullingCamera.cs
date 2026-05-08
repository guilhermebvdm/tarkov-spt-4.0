using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Koenigz.PerfectCulling.EFT;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling;

[RequireComponent(typeof(Camera))]
public class PerfectCullingCamera : MonoBehaviour
{
	public static List<PerfectCullingCamera> AllCameras = new List<PerfectCullingCamera>();

	[CompilerGenerated]
	private bool bool_0 = true;

	[CompilerGenerated]
	private Vector3 vector3_0;

	private static int int_0 = -1;

	private static int int_1 = -1;

	private static int int_2 = -1;

	private Camera camera_0;

	private global::VisibilityIndicesClass<int> gclass1222_0 = new global::VisibilityIndicesClass<int>(4096);

	private global::VisibilityIndicesClass<int> gclass1222_1 = new global::VisibilityIndicesClass<int>(4096);

	private JobHandle? nullable_0;

	public bool VisualizeFrustumCulling
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

	public Vector3 ObservePosition
	{
		[CompilerGenerated]
		get
		{
			return vector3_0;
		}
		[CompilerGenerated]
		set
		{
			vector3_0 = value;
		}
	}

	public void Awake()
	{
		camera_0 = GetComponent<Camera>();
	}

	public void OnEnable()
	{
		ObservePosition = base.transform.position;
		method_5();
		AllCameras.Add(this);
		RenderPipelineManager.beginCameraRendering += method_0;
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_4));
	}

	public void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= method_0;
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_4));
		method_5();
		AllCameras.Remove(this);
	}

	public void Update()
	{
		ObservePosition = base.transform.position;
	}

	public void method_0(ScriptableRenderContext context, Camera camera)
	{
		method_4(camera);
	}

	public void method_1(Camera camera)
	{
	}

	public void method_2()
	{
		gclass1222_0 = new global::VisibilityIndicesClass<int>(4096);
		gclass1222_1 = new global::VisibilityIndicesClass<int>(4096);
	}

	public void method_3()
	{
		JobHandle? jobHandle = nullable_0;
		if (jobHandle.HasValue)
		{
			JobHandle valueOrDefault = jobHandle.GetValueOrDefault();
			bool num = PerfectCullingCrossSceneSampler.Instance?.FinishJob(valueOrDefault) ?? true;
			nullable_0 = null;
			if (num)
			{
				int_1 = int_2;
			}
		}
		gclass1222_0.Clear();
		gclass1222_1.Clear();
		int frameHashNoOrientation = PerfectCullingCrossSceneVolume.GetFrameHashNoOrientation(ObservePosition, gclass1222_0, gclass1222_1);
		if (int_1 == frameHashNoOrientation)
		{
			return;
		}
		for (int i = 0; i < gclass1222_1.Count; i++)
		{
			PerfectCullingCrossSceneGroup.AllCrossGroups[gclass1222_1.Buffer[i]].counterMainThread++;
		}
		if (PerfectCullingCrossSceneSampler.Instance != null)
		{
			int_2 = frameHashNoOrientation;
			nullable_0 = PerfectCullingCrossSceneSampler.Instance.ScheduleJob(gclass1222_0, gclass1222_1);
			if (!nullable_0.HasValue)
			{
				int_1 = int_2;
			}
		}
		else
		{
			int_1 = frameHashNoOrientation;
			nullable_0 = null;
		}
	}

	public void method_4(Camera camera)
	{
		if (camera != camera_0)
		{
			method_1(camera);
		}
		else
		{
			method_3();
		}
	}

	public void method_5()
	{
		int_1 = -1;
	}
}
