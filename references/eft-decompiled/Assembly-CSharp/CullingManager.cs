using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT.Weather;
using Unity.Jobs;
using UnityEngine;

[ExecuteInEditMode]
public class CullingManager : MonoBehaviour
{
	public struct GStruct69
	{
		public GInterface55 CullingObject;

		public float CullingDistanceSqr;

		public GStruct70 VisibilityData;

		public bool JobVisibilityFlag;

		public void Reset()
		{
			CullingObject = null;
			CullingDistanceSqr = 0f;
			VisibilityData.Reset();
		}

		public override string ToString()
		{
			return $"CullingDistanceSqr: {CullingDistanceSqr}\nCullingObject.IsVisible: {CullingObject.IsVisible}" + VisibilityData.ToString();
		}
	}

	public struct GStruct70
	{
		public bool InOpticFructum;

		public bool InFpsFrustum;

		public bool IsCulledByDistance;

		public float CurrentCameraDistanceSqr;

		public bool IsAimingOn;

		public bool CullingByDistanceOnly;

		public bool IsObjectVisible()
		{
			if (CullingByDistanceOnly)
			{
				return !IsCulledByDistance;
			}
			if (!IsCulledByDistance && InFpsFrustum)
			{
				return true;
			}
			return InOpticFructum;
		}

		public void Reset()
		{
			InOpticFructum = false;
			InFpsFrustum = false;
			IsCulledByDistance = true;
			CurrentCameraDistanceSqr = 0f;
			IsAimingOn = false;
		}

		public override string ToString()
		{
			return $"InOpticFructum: {InOpticFructum}\nInFpsFrustum: {InFpsFrustum}\nIsCulledByDistance: {IsCulledByDistance}\nCurrentCameraDistanceSqr: {CurrentCameraDistanceSqr}\nIsAimingOn: {IsAimingOn}\nCullingByDistanceOnly: {CullingByDistanceOnly}\nIsObjectVisible: {IsObjectVisible()}";
		}
	}

	public class GClass996
	{
		public GClass2380 CameraFrustrum = new GClass2380();

		public Camera CameraInstance;

		public bool IsCameraEnabled;

		public Vector3 CameraPosition;

		public bool IsOpticCamera;

		public float FovMultiplicator = 1f;

		public void Clear()
		{
			CameraFrustrum = null;
			CameraInstance = null;
		}

		public bool UpdateParameters()
		{
			if (CameraInstance == null)
			{
				return false;
			}
			CameraPosition = CameraInstance.transform.position;
			IsCameraEnabled = CameraInstance.enabled;
			IsOpticCamera = CameraClass.Instance?.OpticCameraManager?.Camera == CameraInstance;
			CameraFrustrum.Update(CameraInstance.transform.position, CameraInstance.transform.rotation, CameraInstance.fieldOfView * FovMultiplicator, CameraInstance.nearClipPlane, CameraInstance.farClipPlane, CameraInstance.aspect);
			return true;
		}
	}

	public struct Struct163 : IJob
	{
		public static readonly Dictionary<int, GClass996> JobParameters = new Dictionary<int, GClass996>();

		public int JobId;

		public void Execute()
		{
			if (JobParameters != null && JobParameters.ContainsKey(JobId))
			{
				method_0(JobParameters[JobId]);
			}
		}

		public void method_0(GClass996 parameters)
		{
			try
			{
				if (Instance == null || Instance.gstruct69_0 == null || Instance.boundingSphere_0 == null || Instance.list_0 == null || !parameters.IsCameraEnabled)
				{
					return;
				}
				GClass2380 cameraFrustrum = parameters.CameraFrustrum;
				GStruct69[] gstruct69_ = Instance.gstruct69_0;
				BoundingSphere[] boundingSphere_ = Instance.boundingSphere_0;
				List<int> list_ = Instance.list_0;
				int count = list_.Count;
				for (int i = 0; i < count; i++)
				{
					int num = list_[i];
					if (gstruct69_ != null && gstruct69_.Length > num)
					{
						float sqrMagnitude = (parameters.CameraPosition - gstruct69_[num].CullingObject.SafeMultithreadedPosition).sqrMagnitude;
						bool flag = cameraFrustrum.IntersectsSphere(ref boundingSphere_[num]);
						gstruct69_[num].VisibilityData.InOpticFructum = gstruct69_[num].VisibilityData.InOpticFructum || (flag && parameters.IsOpticCamera);
						gstruct69_[num].VisibilityData.InFpsFrustum = flag;
						gstruct69_[num].VisibilityData.CurrentCameraDistanceSqr = sqrMagnitude;
						gstruct69_[num].CullingObject.SqrCameraDistance = sqrMagnitude;
						gstruct69_[num].VisibilityData.IsCulledByDistance = sqrMagnitude > gstruct69_[num].CullingDistanceSqr;
						gstruct69_[num].JobVisibilityFlag = gstruct69_[num].VisibilityData.IsObjectVisible() && gstruct69_[num].CullingObject.IsAutocullVisible;
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private static CullingManager cullingManager_0;

	[CompilerGenerated]
	private static Action action_0;

	[CompilerGenerated]
	private static Action action_1;

	[SerializeField]
	private Camera _debugCamera;

	private const int int_0 = 10000;

	private int int_1;

	private readonly Dictionary<Camera, CullingGroup> dictionary_0 = new Dictionary<Camera, CullingGroup>();

	private readonly Queue<int> queue_0 = new Queue<int>(10000);

	private readonly BoundingSphere[] boundingSphere_0 = new BoundingSphere[10000];

	private readonly List<int> list_0 = new List<int>(10000);

	private readonly GStruct69[] gstruct69_0 = new GStruct69[10000];

	private static readonly List<GInterface55> list_1 = new List<GInterface55>();

	private Camera camera_0;

	private bool bool_0;

	private List<JobHandle> list_2;

	public static CullingManager Instance
	{
		get
		{
			return cullingManager_0;
		}
		set
		{
			cullingManager_0 = value;
			if (cullingManager_0 != null)
			{
				action_0?.Invoke();
			}
			else
			{
				action_1?.Invoke();
			}
		}
	}

	public static event Action OnInstanceCreated
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

	public static event Action OnInstanceDestroyed
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

	public void Awake()
	{
		if (cullingManager_0 != null)
		{
			Debug.LogWarning("Several CullingManager in scenes!", base.gameObject);
			if (Application.isPlaying)
			{
				Debug.LogError("Several CullingManager in scenes!", base.gameObject);
				UnityEngine.Object.DestroyImmediate(cullingManager_0);
			}
		}
		Instance = this;
		method_0();
		method_1();
	}

	public void Update()
	{
		method_11();
		int count = list_0.Count;
		for (int i = 0; i < count; i++)
		{
			int num = list_0[i];
			gstruct69_0[num].CullingObject.SetVisibility(gstruct69_0[num].JobVisibilityFlag);
			gstruct69_0[num].VisibilityData.InOpticFructum = false;
			if (gstruct69_0[num].CullingObject.IsAutocullVisible)
			{
				gstruct69_0[num].CullingObject.CustomUpdate();
			}
		}
		method_10();
	}

	public void method_0()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(method_4));
		method_3();
		method_7();
	}

	public void method_1()
	{
		for (int i = 0; i < list_1.Count; i++)
		{
			list_1[i].Register();
		}
		list_1.Clear();
	}

	public int Register(GInterface55 cullableObject)
	{
		if (list_0.Count >= 10000)
		{
			Debug.LogError($"Maximum number ({10000}) of active spheres indices exceeded ", base.gameObject);
			return -1;
		}
		int num;
		if (queue_0.Count > 0)
		{
			num = queue_0.Dequeue();
		}
		else
		{
			num = int_1;
			int_1++;
			foreach (KeyValuePair<Camera, CullingGroup> item in dictionary_0)
			{
				item.Value.SetBoundingSphereCount(int_1);
			}
		}
		list_0.Add(num);
		UpdateSphere(cullableObject, num);
		method_2(cullableObject, num);
		return num;
	}

	public static void AddEarlyObject(GInterface55 cullableObject)
	{
		if (!list_1.Contains(cullableObject))
		{
			list_1.Add(cullableObject);
		}
	}

	public static void RemoveEarlyObject(GInterface55 cullableObject)
	{
		list_1.Remove(cullableObject);
	}

	public void method_2(GInterface55 cullableObject, int index)
	{
		gstruct69_0[index].CullingDistanceSqr = cullableObject.CullDistanceSqr;
		gstruct69_0[index].CullingObject = cullableObject;
		gstruct69_0[index].VisibilityData.CullingByDistanceOnly = cullableObject.CullByDistanceOnly;
		if (dictionary_0.Count == 0)
		{
			gstruct69_0[index].CullingObject.SetVisibility(isVisible: false);
			return;
		}
		foreach (KeyValuePair<Camera, CullingGroup> item in dictionary_0)
		{
			Camera key = item.Key;
			GClass2380 cameraFrustrum = Struct163.JobParameters[key.GetInstanceID()].CameraFrustrum;
			if (!(key == null) && key.enabled && key.gameObject.activeInHierarchy)
			{
				_ = item.Value;
				bool flag = key.name.Contains("optic");
				float num = Vector3.SqrMagnitude(key.transform.position - gstruct69_0[index].CullingObject.ClearTransformPosition);
				bool flag2 = cameraFrustrum.IntersectsSphere(ref boundingSphere_0[index]);
				gstruct69_0[index].VisibilityData.IsCulledByDistance = !flag && num > gstruct69_0[index].CullingDistanceSqr;
				if (flag)
				{
					gstruct69_0[index].VisibilityData.InOpticFructum = flag2;
				}
				else
				{
					gstruct69_0[index].VisibilityData.InFpsFrustum = flag2;
				}
				gstruct69_0[index].CullingObject.SetVisibility(gstruct69_0[index].VisibilityData.IsObjectVisible() && gstruct69_0[index].CullingObject.IsAutocullVisible);
				if (key == CameraClass.Instance.Camera)
				{
					gstruct69_0[index].CullingObject.SqrCameraDistance = num;
				}
			}
		}
	}

	public void Unregister(GInterface55 o)
	{
		if (list_1.Contains(o))
		{
			list_1.Remove(o);
		}
		if (!queue_0.Contains(o.Index))
		{
			queue_0.Enqueue(o.Index);
		}
		gstruct69_0[o.Index].Reset();
		list_0.Remove(o.Index);
	}

	public void method_3()
	{
		queue_0.Clear();
		list_0.Clear();
		method_6();
		Array.Clear(boundingSphere_0, 0, boundingSphere_0.Length);
		Array.Clear(gstruct69_0, 0, gstruct69_0.Length);
		camera_0 = null;
		int_1 = 0;
	}

	public void Reload()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_4));
		method_3();
		method_0();
	}

	public void method_4(Camera cam)
	{
		if (bool_0 || cam == null)
		{
			return;
		}
		bool flag = cam == CameraClass.Instance.Camera;
		bool flag2 = false;
		bool flag3 = false;
		if (WeatherController.Instance != null && WeatherController.Instance.PlayerCamera != null)
		{
			flag3 = cam == WeatherController.Instance.PlayerCamera;
		}
		bool flag4 = cam == _debugCamera;
		if (flag3 || flag || flag4 || flag2)
		{
			if (flag2)
			{
				camera_0 = cam;
			}
			if (!dictionary_0.ContainsKey(cam))
			{
				CullingGroup cullingGroup = new CullingGroup();
				cullingGroup.targetCamera = cam;
				cullingGroup.SetBoundingSpheres(boundingSphere_0);
				cullingGroup.SetBoundingSphereCount(int_1);
				cullingGroup.enabled = false;
				method_8(cam);
				dictionary_0.Add(cam, cullingGroup);
			}
		}
	}

	public void method_5(Camera cam, CullingGroup cullingGroup, bool isOpticCam)
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			int num = list_0[i];
			bool isVisible = gstruct69_0[num].CullingObject.IsVisible;
			gstruct69_0[num].VisibilityData.InOpticFructum &= camera_0 != null && camera_0.enabled;
			float num2 = Vector3.SqrMagnitude(cam.transform.position - gstruct69_0[num].CullingObject.ClearTransformPosition);
			bool flag = cullingGroup.IsVisible(num);
			if (isOpticCam)
			{
				gstruct69_0[num].VisibilityData.InOpticFructum = flag;
			}
			else
			{
				gstruct69_0[num].VisibilityData.InFpsFrustum = flag;
				gstruct69_0[num].VisibilityData.CurrentCameraDistanceSqr = num2;
				gstruct69_0[num].CullingObject.SqrCameraDistance = num2;
			}
			gstruct69_0[num].VisibilityData.IsCulledByDistance = !isOpticCam && num2 > gstruct69_0[num].CullingDistanceSqr;
			bool visibility;
			if (isVisible != (visibility = gstruct69_0[num].VisibilityData.IsObjectVisible() && gstruct69_0[num].CullingObject.IsAutocullVisible))
			{
				gstruct69_0[num].CullingObject.SetVisibility(visibility);
			}
		}
	}

	public float GetCameraDistanceSqr(int index)
	{
		if (gstruct69_0 == null)
		{
			return 0f;
		}
		return gstruct69_0[index].VisibilityData.CurrentCameraDistanceSqr;
	}

	public bool IsOpticEnabled()
	{
		if (camera_0 != null)
		{
			return camera_0.enabled;
		}
		return false;
	}

	public void UpdateSphere(GInterface55 cullingObject, int index)
	{
		boundingSphere_0[index].radius = cullingObject.Radius;
		boundingSphere_0[index].position = cullingObject.Position;
	}

	public void UpdateSphere(GInterface55 cullingObject)
	{
		UpdateSphere(cullingObject, cullingObject.Index);
	}

	public void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(method_4));
		method_6();
	}

	public void OnDestroy()
	{
		method_3();
		Instance = null;
	}

	public void method_6()
	{
		foreach (KeyValuePair<Camera, CullingGroup> item in dictionary_0)
		{
			if (item.Value != null)
			{
				item.Value.Dispose();
			}
		}
		dictionary_0.Clear();
		method_9();
	}

	public void LockState(bool lockState)
	{
		bool_0 = lockState;
	}

	public void ForceEnable(bool enable)
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			int num = list_0[i];
			gstruct69_0[num].CullingObject.SetVisibility(enable);
		}
	}

	public void method_7()
	{
		list_2 = new List<JobHandle>();
	}

	public void method_8(Camera cam)
	{
		GClass996 gClass = new GClass996();
		gClass.CameraInstance = cam;
		Struct163.JobParameters[cam.GetInstanceID()] = gClass;
		Debug.Log($"Added camera for job culling {cam.gameObject.name}, total={Struct163.JobParameters.Count}");
	}

	public void method_9()
	{
		if (Struct163.JobParameters == null)
		{
			return;
		}
		foreach (KeyValuePair<int, GClass996> jobParameter in Struct163.JobParameters)
		{
			jobParameter.Value.Clear();
		}
		Struct163.JobParameters.Clear();
	}

	public void method_10()
	{
		if (Struct163.JobParameters == null || Struct163.JobParameters.Count == 0)
		{
			return;
		}
		Struct163 jobData = default(Struct163);
		foreach (KeyValuePair<int, GClass996> jobParameter in Struct163.JobParameters)
		{
			if (jobParameter.Value.UpdateParameters())
			{
				jobData.JobId = jobParameter.Value.CameraInstance.GetInstanceID();
				list_2.Add(jobData.Schedule());
			}
		}
		JobHandle.ScheduleBatchedJobs();
	}

	public void method_11()
	{
		foreach (JobHandle item in list_2)
		{
			item.Complete();
		}
		list_2.Clear();
	}

	public void method_12()
	{
		int count = list_0.Count;
		for (int i = 0; i < count; i++)
		{
			_ = list_0[i];
		}
	}
}
