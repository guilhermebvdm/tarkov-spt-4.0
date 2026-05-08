using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class GClass2073
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	public struct Struct331
	{
		public GClass2073 gclass2073_0;

		public Transform camTr;
	}

	[NonSerialized]
	public const int Int_0 = 50;

	[NonSerialized]
	public Vector3 Vector3_0 = new Vector3(-0.2f, -0.45f, 0f);

	[NonSerialized]
	public Vector3 Vector3_1 = new Vector3(0.2f, 0.2f, 0.2f);

	[NonSerialized]
	public Vector3 Vector3_2 = new Vector3(-0.2f, 0.2f, 0.2f);

	[NonSerialized]
	public Vector3 Vector3_3 = new Vector3(10f, -3f, 0f);

	[NonSerialized]
	public Vector3 Vector3_4 = new Vector3(10f, 3f, 0f);

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Dictionary<Transform, (Vector3 position, Vector3 rotation, bool isRight)> Dictionary_0 = new Dictionary<Transform, (Vector3, Vector3, bool)>();

	public void ValidateAllLights(IEnumerable<Transform> tactical, Transform camTr)
	{
		foreach (Transform item in Dictionary_0.Keys.ToList())
		{
			if (item == null)
			{
				Dictionary_0.Remove(item);
			}
		}
		Transform[] array = (tactical as Transform[]) ?? tactical.ToArray();
		Transform[] array2 = Dictionary_0.Keys.Except(array).ToArray();
		foreach (Transform tactical2 in array2)
		{
			RemoveLight(tactical2);
		}
		array2 = array;
		foreach (Transform tactical3 in array2)
		{
			AddLight(tactical3, camTr);
		}
	}

	public void AddLight(Transform tactical, Transform camTr)
	{
		Struct331 struct331_ = default(Struct331);
		struct331_.gclass2073_0 = this;
		struct331_.camTr = camTr;
		Light[] componentsInChildren = tactical.GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren)
		{
			method_1(light.transform, ref struct331_);
		}
		LaserBeam[] componentsInChildren2 = tactical.GetComponentsInChildren<LaserBeam>(includeInactive: true);
		foreach (LaserBeam laserBeam in componentsInChildren2)
		{
			method_1(laserBeam.transform, ref struct331_);
		}
		MeshRenderer[] componentsInChildren3 = tactical.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = false;
		}
	}

	public void RemoveLight(Transform tactical)
	{
		Light[] componentsInChildren = tactical.GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light light in componentsInChildren)
		{
			method_2(light.transform);
		}
		LaserBeam[] componentsInChildren2 = tactical.GetComponentsInChildren<LaserBeam>(includeInactive: true);
		foreach (LaserBeam laserBeam in componentsInChildren2)
		{
			method_2(laserBeam.transform);
		}
		MeshRenderer[] componentsInChildren3 = tactical.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].enabled = false;
		}
	}

	public void SetPause(bool pause)
	{
		Bool_0 = pause;
		if (!Bool_0)
		{
			return;
		}
		foreach (KeyValuePair<Transform, (Vector3, Vector3, bool)> item in Dictionary_0)
		{
			item.Deconstruct(out var key, out var value);
			(Vector3, Vector3, bool) tuple = value;
			Transform transform = key;
			var (position, localEulerAngles, _) = tuple;
			if (transform.gameObject.activeInHierarchy)
			{
				transform.position = position;
				transform.localEulerAngles = localEulerAngles;
			}
		}
	}

	public void SetCameraPositionOffset(Vector3 newOffset)
	{
		Vector3_1 = newOffset;
		Vector3_2 = new Vector3(0f - newOffset.x, newOffset.y, newOffset.z);
	}

	public void SetCameraRotationOffset(Vector3 newOffset)
	{
		Vector3_3 = newOffset;
		Vector3_4 = new Vector3(newOffset.x, 0f - newOffset.y, newOffset.z);
	}

	public void SetFoVDelta(Vector3 delta)
	{
		Vector3_0 = delta;
	}

	public bool method_0(Transform origin, Transform obj)
	{
		if (origin == null)
		{
			return true;
		}
		Vector3 lhs = obj.position - origin.position;
		return new Vector3(Vector3.Dot(lhs, origin.right.normalized), Vector3.Dot(lhs, origin.up.normalized), Vector3.Dot(lhs, origin.forward.normalized)).x > 0f;
	}

	public void Update(Vector3 targetPos, Vector3 targetRot)
	{
		if (Bool_0)
		{
			return;
		}
		int num = (int)CameraClass.Instance.Fov;
		int num2 = Mathf.Clamp(50 - num, 0, 50);
		foreach (KeyValuePair<Transform, (Vector3, Vector3, bool)> item2 in Dictionary_0)
		{
			item2.Deconstruct(out var key, out var value);
			(Vector3, Vector3, bool) tuple = value;
			Transform transform = key;
			bool item = tuple.Item3;
			if (transform.gameObject.activeInHierarchy)
			{
				transform.position = targetPos;
				transform.Translate(item ? Vector3_1 : Vector3_2);
				Vector3 vector = (item ? Vector3_3 : Vector3_4);
				transform.eulerAngles = targetRot + vector + Vector3_0 * num2;
			}
		}
	}

	[CompilerGenerated]
	public void method_1(Transform tr, ref Struct331 struct331_0)
	{
		if (!Dictionary_0.ContainsKey(tr))
		{
			Dictionary_0[tr] = (tr.localPosition, tr.localEulerAngles, method_0(struct331_0.camTr, tr));
		}
	}

	[CompilerGenerated]
	public void method_2(Transform tr)
	{
		if (Dictionary_0.TryGetValue(tr, out (Vector3, Vector3, bool) value))
		{
			(tr.localPosition, tr.localEulerAngles, _) = value;
		}
		Dictionary_0.Remove(tr);
	}
}
