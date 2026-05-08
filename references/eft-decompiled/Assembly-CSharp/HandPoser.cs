using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using RootMotion.FinalIK;
using UnityEngine;

public class HandPoser : Poser
{
	[Serializable]
	[CompilerGenerated]
	public class Class538
	{
		public static readonly Class538 class538_0 = new Class538();

		public static Func<Transform, Quaternion> func_0;

		public static Func<Transform, Quaternion> func_1;

		public Quaternion method_0(Transform x)
		{
			return x.localRotation;
		}

		public Quaternion method_1(Transform x)
		{
			return x.localRotation;
		}
	}

	private Transform[] transform_0;

	private Transform[] transform_1;

	private GInterface26 ginterface26_0;

	private GInterface26 ginterface26_1;

	private Transform[] transform_2;

	public float GripWeight;

	public float IKTresholdDistance = 0.15f;

	public bool IgnoreIndexFinger;

	public Transform[] Transform_0
	{
		get
		{
			if (transform_0 == null)
			{
				InitChildren();
			}
			return transform_0;
		}
	}

	[ContextMenu("Save as left")]
	public void method_0()
	{
		EFTHardSettings.Instance.LEFT_HAND_QTS = (from x in GetComponentsInChildren<Transform>()
			select x.localRotation).ToArray();
	}

	[ContextMenu("Save as right")]
	public void method_1()
	{
		EFTHardSettings.Instance.RIGHT_HAND_QTS = (from x in GetComponentsInChildren<Transform>()
			select x.localRotation).ToArray();
	}

	public void InitChildren()
	{
		transform_0 = GetComponentsInChildren<Transform>();
	}

	public IEnumerator Lerp2TargetRoutine(Quaternion[] qts, float t = 2f, float str = 0.5f)
	{
		t = Mathf.Max(1f, t);
		float num = Time.time + t;
		while (Time.time < num)
		{
			for (int i = 0; i < Transform_0.Length; i++)
			{
				Transform obj = Transform_0[i];
				obj.localRotation = Quaternion.Lerp(obj.localRotation, qts[i], 1f - (num - Time.time) / t * str);
			}
			yield return null;
		}
	}

	public void Lerp2Target(Quaternion[] qts, float t = 2f, float str = 0.5f)
	{
		StartCoroutine(Lerp2TargetRoutine(qts, t));
	}

	public static int NumParents(Transform child, Transform parent, int index = 0)
	{
		if (!(child == null) && !(child == parent))
		{
			return NumParents(child.parent, parent, ++index);
		}
		return index;
	}

	public override void FixTransforms()
	{
	}

	public void Start()
	{
		InitChildren();
	}

	public void MapGrip(Transform t)
	{
		transform_1 = t.GetComponentsInChildren<Transform>();
	}

	public void MapIkPalm(GripPose t)
	{
		ginterface26_0 = t;
		if (ginterface26_0 == null)
		{
			transform_2 = null;
		}
		else if (!ginterface26_0.IsCached)
		{
			transform_2 = t.GetComponentsInChildren<Transform>();
		}
	}

	public void ClearMap()
	{
		transform_1 = null;
	}

	public void SetGrip(GInterface26 grip)
	{
		ginterface26_1 = grip;
	}

	public void ManualUpdate(bool skip = false)
	{
		if (!(weight <= 0.001f))
		{
			SimpleGrip();
		}
	}

	public void SimpleGrip(bool skip = false)
	{
		if (Transform_0 == null || Transform_0.Length == 0)
		{
			return;
		}
		int num = (skip ? 2 : Transform_0.Length);
		if (ginterface26_0 == null)
		{
			for (int i = 1; i < num; i++)
			{
				Quaternion localRotation = Quaternion.Lerp(Transform_0[i].localRotation, method_2(i), weight);
				Transform_0[i].localRotation = localRotation;
			}
			return;
		}
		float value = Vector3.Distance(Transform_0[0].position, ginterface26_0.Position);
		float t = Mathf.InverseLerp(IKTresholdDistance, 0.01f, value);
		for (int j = 1; j < num; j++)
		{
			Quaternion localRotation2 = Quaternion.Lerp(Transform_0[j].localRotation, Quaternion.Lerp(method_2(j), ginterface26_0.IsCached ? ginterface26_0[j] : transform_2[j].localRotation, t), weight);
			Transform_0[j].localRotation = localRotation2;
		}
	}

	public Quaternion method_2(int index)
	{
		if (IgnoreIndexFinger && (index == 5 || index == 6))
		{
			return transform_1[index].localRotation;
		}
		if (ginterface26_1 == null)
		{
			return transform_1[index].localRotation;
		}
		if (GripWeight >= 1f)
		{
			return ginterface26_1[index];
		}
		return Quaternion.Lerp(transform_1[index].localRotation, ginterface26_1[index], GripWeight);
	}

	public override void AutoMapping()
	{
		throw new NotImplementedException();
	}

	public override void StoreDefaultState()
	{
		throw new NotImplementedException();
	}

	public void RemoveGripMapping()
	{
		transform_1 = null;
	}
}
