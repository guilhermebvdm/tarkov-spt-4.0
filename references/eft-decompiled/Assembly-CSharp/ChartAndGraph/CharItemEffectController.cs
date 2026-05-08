using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ChartAndGraph;

public class CharItemEffectController : MonoBehaviour
{
	private List<ChartItemEffect> list_0 = new List<ChartItemEffect>();

	private Transform transform_0;

	[CompilerGenerated]
	private bool bool_0;

	[CompilerGenerated]
	private bool bool_1;

	private Vector3 vector3_0;

	public bool Boolean_0
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

	public bool Boolean_1
	{
		[CompilerGenerated]
		get
		{
			return bool_1;
		}
		[CompilerGenerated]
		set
		{
			bool_1 = value;
		}
	}

	public Transform Parent
	{
		get
		{
			if (transform_0 == null)
			{
				transform_0 = base.transform.parent;
			}
			return transform_0;
		}
	}

	public CharItemEffectController()
	{
		Boolean_1 = true;
	}

	public void Start()
	{
		vector3_0 = base.transform.localScale;
	}

	public void method_0()
	{
		vector3_0 = base.transform.localScale;
	}

	public void Update()
	{
		Transform parent = base.transform;
		if (Boolean_0)
		{
			parent = Parent;
			if (parent == null)
			{
				return;
			}
		}
		Vector3 localScale = new Vector3(1f, 1f, 1f);
		if (Boolean_1)
		{
			localScale = vector3_0;
		}
		Vector3 zero = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		for (int i = 0; i < list_0.Count; i++)
		{
			ChartItemEffect chartItemEffect = list_0[i];
			if (!(chartItemEffect == null) && !(chartItemEffect.gameObject == null))
			{
				localScale.x *= chartItemEffect.Vector3_0.x;
				localScale.y *= chartItemEffect.Vector3_0.y;
				localScale.z *= chartItemEffect.Vector3_0.z;
				zero += chartItemEffect.Vector3_1;
				identity *= chartItemEffect.Quaternion_0;
			}
			else
			{
				list_0.RemoveAt(i);
				i--;
			}
		}
		parent.localScale = localScale;
	}

	public void Unregister(ChartItemEffect effect)
	{
		list_0.Remove(effect);
		if (list_0.Count == 0)
		{
			base.enabled = false;
		}
	}

	public void Register(ChartItemEffect effect)
	{
		if (!list_0.Contains(effect))
		{
			if (!base.enabled)
			{
				base.enabled = true;
			}
			list_0.Add(effect);
		}
	}
}
