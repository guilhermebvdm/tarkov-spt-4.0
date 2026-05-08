using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

public class SmallPhysicsSystem : MonoBehaviour
{
	private const float float_0 = 0.2f;

	private const float float_1 = 5f;

	private static SmallPhysicsSystem smallPhysicsSystem_0;

	private static bool bool_0 = false;

	private static object object_0 = new object();

	private List<SmallPhysicsObject> list_0 = new List<SmallPhysicsObject>(300);

	private float float_2;

	private int int_0;

	private float float_3 = 30f;

	private float float_4;

	private float float_5;

	private int int_1;

	public static SmallPhysicsSystem Instance
	{
		get
		{
			if (bool_0)
			{
				return null;
			}
			lock (object_0)
			{
				if (smallPhysicsSystem_0 == null)
				{
					GameObject obj = new GameObject();
					smallPhysicsSystem_0 = obj.AddComponent<SmallPhysicsSystem>();
					obj.name = "SmallPhysicsSystem";
				}
				return smallPhysicsSystem_0;
			}
		}
	}

	public void Awake()
	{
		float_5 = 25f;
		float_2 = Time.realtimeSinceStartup;
	}

	public void AddObject(SmallPhysicsObject smallPhysicsObject)
	{
		list_0.Add(smallPhysicsObject);
	}

	public void RemoveObject(SmallPhysicsObject smallPhysicsObject)
	{
		list_0.Remove(smallPhysicsObject);
	}

	public void Update()
	{
		int_0++;
		if (int_1 == 0)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (!(realtimeSinceStartup - float_2 >= 0.2f))
			{
				return;
			}
			float_3 = (float)int_0 / (realtimeSinceStartup - float_2);
			int_0 = 0;
			float_2 = realtimeSinceStartup;
			float_4 = (float)list_0.Count / 0.2f / float_3;
		}
		List<IPlayer> registeredPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers;
		int num = int_1;
		while (int_1 < list_0.Count && !((float)(int_1 - num) > float_4))
		{
			SmallPhysicsObject smallPhysicsObject = list_0[int_1];
			foreach (IPlayer item in registeredPlayers)
			{
				if (item != null && !((item.Position - smallPhysicsObject.gameObject.transform.position).sqrMagnitude >= float_5))
				{
					smallPhysicsObject.UpdatePhysics();
					break;
				}
			}
			int_1++;
		}
		if (int_1 + 1 >= list_0.Count)
		{
			int_1 = 0;
		}
	}

	public void OnApplicationQuit()
	{
		bool_0 = true;
	}
}
