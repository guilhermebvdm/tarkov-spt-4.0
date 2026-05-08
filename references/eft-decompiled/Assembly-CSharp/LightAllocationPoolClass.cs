using System;
using System.Collections.Generic;
using UnityEngine;

public class LightAllocationPoolClass
{
	public class Class712
	{
		public Light LightScript;

		public GameObject LightObject;

		public Transform LightTransform;

		public float DeathTime;

		public float RangeFallof;

		public float IntensityFallof;

		public void Start(float time, float range, float intensity, Vector3 position, Color color, bool shadows = false)
		{
			DeathTime = Time.time + time;
			RangeFallof = range / time;
			IntensityFallof = intensity / time;
			LightScript.enabled = true;
			LightScript.range = range;
			LightScript.intensity = intensity;
			LightTransform.position = position;
			LightScript.color = color;
			LightScript.shadows = (shadows ? LightShadows.Hard : LightShadows.None);
		}

		public void Update(float deltaTime)
		{
			LightScript.intensity -= IntensityFallof * deltaTime;
			LightScript.range -= RangeFallof * deltaTime;
		}
	}

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public LinkedList<Class712> LinkedList_0 = new LinkedList<Class712>();

	[NonSerialized]
	public LinkedList<Class712> LinkedList_1 = new LinkedList<Class712>();

	[NonSerialized]
	public const int Int_0 = 100;

	[NonSerialized]
	public const float Float_0 = 1.5f;

	[NonSerialized]
	public const float Float_1 = 0.1f;

	[NonSerialized]
	public const float Float_2 = 5f;

	public void Init()
	{
		LinkedList_0 = new LinkedList<Class712>();
		LinkedList_1 = new LinkedList<Class712>();
		FillFreeLights();
	}

	public void Destroy()
	{
		foreach (Class712 item in LinkedList_0)
		{
			UnityEngine.Object.DestroyImmediate(item.LightObject);
		}
		foreach (Class712 item2 in LinkedList_1)
		{
			UnityEngine.Object.DestroyImmediate(item2.LightObject);
		}
		LinkedList_0.Clear();
		LinkedList_1.Clear();
	}

	public void Update()
	{
		if (LinkedList_1.Count == 0)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		LinkedListNode<Class712> linkedListNode = LinkedList_1.First;
		while (linkedListNode != null)
		{
			Class712 value = linkedListNode.Value;
			if (value.DeathTime < Time.time)
			{
				value.LightScript.enabled = false;
				LinkedListNode<Class712> node = linkedListNode;
				linkedListNode = linkedListNode.Next;
				LinkedList_1.Remove(node);
				LinkedList_0.AddLast(node);
			}
			else
			{
				linkedListNode = linkedListNode.Next;
				value.Update(deltaTime);
			}
		}
	}

	public void Add(Vector3 position, Color color, float range = 1.5f, float intensity = 5f, float time = 0.1f, bool shadows = false)
	{
		Class712 @class;
		if (LinkedList_0.Count == 0)
		{
			@class = CreateLightStruct();
			LinkedList_1.AddLast(@class);
		}
		else
		{
			LinkedListNode<Class712> first = LinkedList_0.First;
			LinkedList_0.Remove(first);
			LinkedList_1.AddLast(first);
			@class = first.Value;
		}
		@class.Start(time, range, intensity, position, color, shadows);
	}

	public void FillFreeLights()
	{
		for (int i = 0; i < 100; i++)
		{
			Class712 value = CreateLightStruct();
			LinkedList_0.AddLast(value);
		}
	}

	public Class712 CreateLightStruct()
	{
		Class712 @class = new Class712();
		@class.LightObject = new GameObject("LightOfPool", typeof(Light));
		Transform transform = CreateOrGetLightParent().transform;
		@class.LightObject.transform.SetParent(transform);
		@class.LightTransform = @class.LightObject.transform;
		@class.LightScript = @class.LightObject.GetComponent<Light>();
		@class.LightScript.enabled = false;
		return @class;
	}

	public GameObject CreateOrGetLightParent()
	{
		if (GameObject_0 != null)
		{
			return GameObject_0;
		}
		GameObject_0 = new GameObject("LightOfPool Parent");
		UnityEngine.Object.DontDestroyOnLoad(GameObject_0);
		return GameObject_0;
	}
}
