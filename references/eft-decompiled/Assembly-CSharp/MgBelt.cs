using System;
using System.Collections.Generic;
using BezierSplineTools;
using UnityEngine;

public class MgBelt : MonoBehaviour
{
	public class GClass913
	{
		public float Shift;

		public float Decay;

		public const float Cap = 3f;
	}

	[SerializeField]
	private BezierCurve Curve1;

	[SerializeField]
	private float _sleepCooldown = 4f;

	public Rigidbody[] KeyElemetns;

	public Collider[] KeyElementsColliders;

	public List<Transform> Links = new List<Transform>();

	public float WaveAmplitude;

	public AnimationCurve AmplitudeCurve;

	public AnimationCurve WeightCurve;

	private readonly List<GClass913> list_0 = new List<GClass913>();

	private float float_0 = float.PositiveInfinity;

	private bool bool_0;

	private bool bool_1 = true;

	private Transform transform_0;

	private int int_0;

	private bool bool_2;

	private bool bool_3;

	public bool On
	{
		get
		{
			return bool_1;
		}
		set
		{
			bool_1 = value;
			if (value)
			{
				float_0 = float.PositiveInfinity;
				bool_0 = false;
				method_0(isPhysicsEnabled: true);
			}
			else
			{
				float_0 = Time.time + _sleepCooldown;
				bool_0 = true;
			}
		}
	}

	public void method_0(bool isPhysicsEnabled)
	{
		if (isPhysicsEnabled && bool_2 && On)
		{
			if (!bool_3)
			{
				Rigidbody[] keyElemetns = KeyElemetns;
				for (int i = 0; i < keyElemetns.Length; i++)
				{
					EFTPhysicsClass.GClass745.SupportRigidbody(keyElemetns[i], 0f);
				}
				bool_3 = true;
			}
		}
		else if (bool_3)
		{
			Rigidbody[] keyElemetns = KeyElemetns;
			for (int i = 0; i < keyElemetns.Length; i++)
			{
				EFTPhysicsClass.GClass745.UnsupportRigidbody(keyElemetns[i]);
			}
			bool_3 = false;
		}
	}

	public void SetVisible(bool isVisible)
	{
		if (bool_2 != isVisible)
		{
			bool_2 = isVisible;
			method_0(bool_2);
		}
	}

	public void SetPivotPoint(Transform t)
	{
		transform_0 = t;
	}

	public void Init()
	{
		Curve1.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		method_0(isPhysicsEnabled: true);
		On = false;
		int num = KeyElemetns.Length;
		KeyElementsColliders = new Collider[num];
		for (int i = 0; i < num; i++)
		{
			KeyElementsColliders[i] = KeyElemetns[i].GetComponent<Collider>();
		}
		Curve1.points = new Vector3[num];
	}

	public void LateUpdate()
	{
		if (!bool_0 && !On)
		{
			return;
		}
		if ((bool)transform_0)
		{
			KeyElemetns[0].transform.SetPositionAndRotation(transform_0.position, transform_0.rotation);
		}
		if (Curve1 != null)
		{
			for (int i = 0; i < KeyElemetns.Length; i++)
			{
				Curve1.points[i] = KeyElemetns[i].position;
			}
		}
		for (int j = 0; j < Links.Count && Links[j].gameObject.activeInHierarchy; j++)
		{
			int num = (int)((float)Links.Count / 3f);
			int num2 = j / num;
			int num3 = num2 + 1;
			Rigidbody rigidbody = KeyElemetns[num2];
			Rigidbody rigidbody2 = KeyElemetns[num3];
			float num4 = (float)j / (float)Links.Count;
			Vector3 point = Curve1.GetPoint(num4);
			float t = (float)(j % num) / (float)num;
			if (list_0.Count > 0)
			{
				Vector3 normalized = Vector3.Lerp(rigidbody.transform.forward, rigidbody2.transform.forward, t).normalized;
				float num5 = float.MinValue;
				foreach (GClass913 item in list_0)
				{
					num5 = Math.Max(num5, AmplitudeCurve.Evaluate(num4 - item.Shift) * WeightCurve.Evaluate(num4 * item.Decay));
				}
				point += num5 * WaveAmplitude * normalized;
			}
			Links[j].SetPositionAndRotation(point, Quaternion.Lerp(rigidbody.rotation, rigidbody2.rotation, t));
		}
		for (int num6 = list_0.Count - 1; num6 >= 0; num6--)
		{
			GClass913 gClass = list_0[num6];
			if (gClass.Shift > GClass842.GetDuration(AmplitudeCurve))
			{
				list_0.RemoveAt(num6);
			}
			else
			{
				gClass.Shift += Time.deltaTime;
			}
		}
		if (bool_0 && Time.time > float_0)
		{
			bool_0 = false;
			method_0(isPhysicsEnabled: false);
		}
	}

	public void AddWave()
	{
		list_0.Add(new GClass913
		{
			Shift = 0f,
			Decay = UnityEngine.Random.Range(1f, 2f)
		});
	}

	public void DisplayAll(int displayElements)
	{
		for (int i = 0; i < Links.Count; i++)
		{
			Links[i].gameObject.SetActive(i < displayElements);
		}
	}

	public void DisplayDelta(int displayElements)
	{
		for (int i = int_0; i < Links.Count && i < displayElements; i++)
		{
			Links[i].gameObject.SetActive(value: true);
			int_0 = i;
		}
		int num = Links.Count / (KeyElementsColliders.Length - 1);
		for (int j = 1; j < KeyElementsColliders.Length && displayElements > num * j; j++)
		{
			KeyElementsColliders[j].isTrigger = false;
		}
	}
}
