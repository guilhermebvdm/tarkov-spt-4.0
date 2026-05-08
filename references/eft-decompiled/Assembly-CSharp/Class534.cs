using System;
using System.Collections.Generic;
using UnityEngine;

public class Class534
{
	public class Class535
	{
		public string Name;

		public float Time = -1f;

		public Class535(string name, float time)
		{
			Name = name;
			Time = time;
		}
	}

	[NonSerialized]
	public Animation Animation_0;

	[NonSerialized]
	public LinkedList<Class535> LinkedList_0 = new LinkedList<Class535>();

	[NonSerialized]
	public Class535 Class535_0;

	[NonSerialized]
	public AnimationState AnimationState_0;

	[NonSerialized]
	public float Float_0;

	public float FadeLength = 0.1f;

	public bool WillChange
	{
		get
		{
			if (AnimationState_0 == null)
			{
				return true;
			}
			return AnimationState_0.time >= Float_0;
		}
	}

	public float Delta
	{
		get
		{
			if (AnimationState_0 == null)
			{
				return 0f;
			}
			return AnimationState_0.time - Float_0;
		}
	}

	public Class534(Animation animation)
	{
		Animation_0 = animation;
	}

	public void Add(string name, float time = -1f)
	{
		Animation_0[name].wrapMode = WrapMode.ClampForever;
		LinkedList_0.AddLast(new Class535(name, time));
	}

	public void Reset()
	{
		if (AnimationState_0 != null)
		{
			Animation_0.Rewind(AnimationState_0.name);
			AnimationState_0 = null;
		}
	}

	public void Update()
	{
		if (LinkedList_0.First != null)
		{
			float num = ((!(AnimationState_0 != null) || !AnimationState_0.enabled) ? 0f : ((LinkedList_0.First.Value.Time == -1f) ? (AnimationState_0.time - Float_0) : (Time.time - LinkedList_0.First.Value.Time)));
			if (num >= 0f)
			{
				Class535_0 = LinkedList_0.First.Value;
				LinkedList_0.RemoveFirst();
				AnimationState_0 = Animation_0[Class535_0.Name];
				Animation_0[Class535_0.Name].time = num;
				Animation_0.CrossFade(Class535_0.Name, FadeLength, PlayMode.StopSameLayer);
				Float_0 = AnimationState_0.length;
			}
		}
	}

	public void method_0()
	{
		if (LinkedList_0.First != null)
		{
			float num = ((!(AnimationState_0 != null) || !AnimationState_0.enabled) ? 0f : (AnimationState_0.time - Float_0));
			if (num >= 0f)
			{
				Class535_0 = LinkedList_0.First.Value;
				LinkedList_0.RemoveFirst();
				AnimationState_0 = Animation_0[Class535_0.Name];
				Animation_0[Class535_0.Name].time = num;
				Animation_0.Play(Class535_0.Name);
				Float_0 = AnimationState_0.length;
			}
		}
	}
}
