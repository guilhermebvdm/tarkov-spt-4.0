using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

public abstract class GClass660
{
	public enum EOrderType
	{
		First,
		Last
	}

	[NonSerialized]
	public static PlayerLoopSystem PlayerLoopSystem_0;

	static GClass660()
	{
		PlayerLoopSystem_0 = PlayerLoop.GetDefaultPlayerLoop();
	}

	public static bool smethod_0(PlayerLoopSystem root, Type type, out PlayerLoopSystem playerLoopSystem)
	{
		if (root.type == type)
		{
			playerLoopSystem = root;
			return true;
		}
		if (root.subSystemList != null)
		{
			PlayerLoopSystem[] subSystemList = root.subSystemList;
			for (int i = 0; i < subSystemList.Length; i++)
			{
				if (smethod_0(subSystemList[i], type, out playerLoopSystem))
				{
					return true;
				}
			}
		}
		playerLoopSystem = default(PlayerLoopSystem);
		return false;
	}

	public static bool FindParentPlayerLoopSystem(PlayerLoopSystem root, Type type, out PlayerLoopSystem playerLoopSystem, out int index)
	{
		playerLoopSystem = default(PlayerLoopSystem);
		index = -1;
		if (root.type == type)
		{
			Debug.LogError("No parent for " + root.type);
			return false;
		}
		if (root.subSystemList != null)
		{
			for (int i = 0; i < root.subSystemList.Length; i++)
			{
				PlayerLoopSystem root2 = root.subSystemList[i];
				if (!(root2.type == type))
				{
					if (FindParentPlayerLoopSystem(root2, type, out playerLoopSystem, out index))
					{
						return true;
					}
					continue;
				}
				playerLoopSystem = root;
				index = i;
				return true;
			}
		}
		return false;
	}

	public static bool smethod_1(PlayerLoopSystem searchRoot, PlayerLoopSystem playerLoopSystem)
	{
		if (searchRoot.type == playerLoopSystem.type)
		{
			Debug.LogError("root == playerLoopSystem");
			return false;
		}
		if (searchRoot.subSystemList != null)
		{
			for (int i = 0; i < searchRoot.subSystemList.Length; i++)
			{
				PlayerLoopSystem searchRoot2 = searchRoot.subSystemList[i];
				if (!(searchRoot2.type == playerLoopSystem.type))
				{
					if (smethod_1(searchRoot2, playerLoopSystem))
					{
						return true;
					}
					continue;
				}
				searchRoot.subSystemList[i] = playerLoopSystem;
				return true;
			}
		}
		return false;
	}

	public static bool smethod_2(PlayerLoopSystem playerLoopSystem, PlayerLoopSystem[] subSystemList)
	{
		playerLoopSystem.subSystemList = subSystemList;
		if (smethod_1(PlayerLoopSystem_0, playerLoopSystem))
		{
			PlayerLoop.SetPlayerLoop(PlayerLoopSystem_0);
			return true;
		}
		Debug.LogError("Cant apply changes to edited loop system: " + playerLoopSystem.type.FullName);
		return false;
	}

	public static void InsertAsSubSystem(Type rootType, PlayerLoopSystem newSystem, EOrderType orderType)
	{
		if (smethod_0(PlayerLoopSystem_0, rootType, out var playerLoopSystem))
		{
			List<PlayerLoopSystem> list = ((playerLoopSystem.subSystemList != null) ? playerLoopSystem.subSystemList.ToList() : new List<PlayerLoopSystem>());
			switch (orderType)
			{
			case EOrderType.First:
				list.Insert(0, newSystem);
				break;
			case EOrderType.Last:
				list.Add(newSystem);
				break;
			}
			smethod_2(playerLoopSystem, list.ToArray());
		}
		else
		{
			Debug.LogError("Cant find root system: " + rootType.FullName);
		}
	}

	public static void InsertAsSiblingSystem(Type siblingType, PlayerLoopSystem newSystem, EOrderType orderType)
	{
		if (FindParentPlayerLoopSystem(PlayerLoopSystem_0, siblingType, out var playerLoopSystem, out var index))
		{
			List<PlayerLoopSystem> list = ((playerLoopSystem.subSystemList != null) ? playerLoopSystem.subSystemList.ToList() : new List<PlayerLoopSystem>());
			int index2 = -1;
			switch (orderType)
			{
			case EOrderType.First:
				index2 = index;
				break;
			case EOrderType.Last:
				index2 = index + 1;
				break;
			}
			list.Insert(index2, newSystem);
			smethod_2(playerLoopSystem, list.ToArray());
		}
		else
		{
			Debug.LogError("Cant find parent system for: " + siblingType.FullName);
		}
	}

	public static bool smethod_3(PlayerLoopSystem searchRoot, Type removeSystemType)
	{
		if (searchRoot.type == removeSystemType)
		{
			Debug.LogError("root == removeSystemType");
			return false;
		}
		List<PlayerLoopSystem> list = ((searchRoot.subSystemList != null) ? searchRoot.subSystemList.ToList() : new List<PlayerLoopSystem>());
		int num = 0;
		while (true)
		{
			if (num < list.Count)
			{
				PlayerLoopSystem searchRoot2 = list[num];
				if (!(searchRoot2.type == removeSystemType))
				{
					if (smethod_3(searchRoot2, removeSystemType))
					{
						break;
					}
					num++;
					continue;
				}
				list.RemoveAt(num);
				searchRoot.subSystemList = list.ToArray();
				if (smethod_2(searchRoot, list.ToArray()))
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	public static void RemoveLoopSystem(Type removeSystemType)
	{
		if (smethod_3(PlayerLoopSystem_0, removeSystemType))
		{
			PlayerLoop.SetPlayerLoop(PlayerLoopSystem_0);
		}
		else
		{
			Debug.LogError("Cant delete system type: " + removeSystemType.FullName);
		}
	}

	public static bool FindPlayerLoopSystem(Type type, out PlayerLoopSystem playerLoopSystem)
	{
		return smethod_0(PlayerLoopSystem_0, type, out playerLoopSystem);
	}

	public static void PrintAllPlayerLoopSystems()
	{
		StringBuilder stringBuilder = new StringBuilder();
		smethod_4(PlayerLoopSystem_0, stringBuilder);
		Debug.LogFormat("PlayerLoopSystems:\n{0}", stringBuilder);
	}

	public static void smethod_4(PlayerLoopSystem playerLoopSystem, StringBuilder output, string indent = "")
	{
		if (playerLoopSystem.subSystemList != null)
		{
			PlayerLoopSystem[] subSystemList = playerLoopSystem.subSystemList;
			for (int i = 0; i < subSystemList.Length; i++)
			{
				PlayerLoopSystem playerLoopSystem2 = subSystemList[i];
				output.AppendFormat("{0}{1}\n", indent, playerLoopSystem2.type.FullName);
				smethod_4(playerLoopSystem2, output, indent + "\t");
			}
		}
	}

	public static void ResetGameLoop()
	{
		PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
	}
}
