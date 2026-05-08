using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class GClass995
{
	[Serializable]
	[CompilerGenerated]
	public class Class693
	{
		public static readonly Class693 class693_0 = new Class693();

		public static Action<bool> action_0;

		public void method_0(bool b)
		{
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[CompilerGenerated]
	private Action<bool> action_0 = delegate
	{
	};

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public XYCellSizeStruct[] Gstruct29_0;

	[NonSerialized]
	public LinkedList<InteractionReciver> LinkedList_0 = new LinkedList<InteractionReciver>();

	public bool SomethingClosely
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public event Action<bool> OnChangeCloseness
	{
		[CompilerGenerated]
		add
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<bool> action = action_0;
			Action<bool> action2;
			do
			{
				action2 = action;
				Action<bool> value2 = (Action<bool>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public GClass995(Transform transform, float radius)
	{
		Transform_0 = transform;
		Float_0 = radius;
		Gstruct29_0 = method_0(radius);
	}

	public void Update()
	{
		Vector3 position = Transform_0.position;
		int hash = InteractionReciver.GetHash(position);
		if (hash != Int_0)
		{
			Int_0 = hash;
			XYCellSizeStruct coordinates = InteractionReciver.GetCoordinates(position);
			LinkedList_0.Clear();
			XYCellSizeStruct[] gstruct29_ = Gstruct29_0;
			for (int i = 0; i < gstruct29_.Length; i++)
			{
				InteractionReciver.FillRecivers(gstruct29_[i] + coordinates, LinkedList_0);
			}
		}
		bool flag = false;
		foreach (InteractionReciver item in LinkedList_0)
		{
			if (Vector3.Distance(position, item.Position) <= Float_0)
			{
				flag = true;
				break;
			}
		}
		if (flag != SomethingClosely)
		{
			SomethingClosely = flag;
			action_0(SomethingClosely);
		}
	}

	public XYCellSizeStruct[] method_0(float radius)
	{
		LinkedList<XYCellSizeStruct> linkedList = new LinkedList<XYCellSizeStruct>();
		radius += 1.4143f;
		int num = (int)radius + 1;
		for (int i = -num; i < num; i++)
		{
			for (int j = -num; j < num; j++)
			{
				if (Mathf.Sqrt(j * j + i * i) < radius)
				{
					linkedList.AddLast(new XYCellSizeStruct(j, i));
				}
			}
		}
		XYCellSizeStruct[] array = new XYCellSizeStruct[linkedList.Count];
		linkedList.CopyTo(array, 0);
		return array;
	}
}
