using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Interactive;
using UnityEngine;

public class BotDoorsController : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class151
	{
		public static readonly Class151 class151_0 = new Class151();

		public static Func<NavMeshDoorLink, bool> func_0;

		public bool method_0(NavMeshDoorLink x)
		{
			return x != null;
		}
	}

	[CompilerGenerated]
	public class Class152
	{
		public NavMeshDoorLink link;

		public bool method_0(Door x)
		{
			return x.Id == link.DoorId;
		}
	}

	[CompilerGenerated]
	public class Class153
	{
		public Vector3 pos;

		public Door method_0(Door x1, Door x2)
		{
			if ((x1.transform.position - pos).magnitude > (x2.transform.position - pos).magnitude)
			{
				return x2;
			}
			return x1;
		}
	}

	public List<NavMeshDoorLink> _navMeshDoorLinks;

	public void method_0()
	{
		foreach (NavMeshDoorLink navMeshDoorLink in _navMeshDoorLinks)
		{
			if (navMeshDoorLink.Door != null)
			{
				navMeshDoorLink.SetAllCarvers(val: false);
				EDoorState doorState = navMeshDoorLink.Door.DoorState;
				if (doorState - 1 > EDoorState.Locked && doorState == EDoorState.Open)
				{
					navMeshDoorLink.Carver_Opened.carving = true;
				}
			}
		}
	}

	public void RefreshData(BotsController bc)
	{
		if (_navMeshDoorLinks == null)
		{
			_navMeshDoorLinks = (from x in UnityEngine.Object.FindObjectsOfType<NavMeshDoorLink>()
				where x != null
				select x).ToList();
		}
		Door[] source = GClass870.FindUnityObjectsOfType<Door>();
		foreach (NavMeshDoorLink link in _navMeshDoorLinks)
		{
			Door door = source.FirstOrDefault((Door x) => x.Id == link.DoorId);
			if (door == null)
			{
				Vector3 pos = link.transform.position;
				door = source.Aggregate((Door x1, Door x2) => ((x1.transform.position - pos).magnitude > (x2.transform.position - pos).magnitude) ? x2 : x1);
				_ = (link.transform.position - door.transform.position).sqrMagnitude;
			}
			link.Init(bc);
			link.SetDoor(door, withSubscribe: true);
		}
		method_0();
	}

	public void PreCalcRefresh()
	{
		_navMeshDoorLinks = new List<NavMeshDoorLink>();
		foreach (Transform item in base.transform)
		{
			UnityEngine.Object.DestroyImmediate(item.gameObject);
		}
	}

	public void Update()
	{
		if (_navMeshDoorLinks != null)
		{
			for (int i = 0; i < _navMeshDoorLinks.Count; i++)
			{
				_navMeshDoorLinks[i].ManualUpdate();
			}
		}
	}

	public static BotDoorsController CreateOrFind(bool doErrorMsg)
	{
		BotDoorsController botDoorsController = UnityEngine.Object.FindObjectOfType<BotDoorsController>();
		if (botDoorsController != null)
		{
			return botDoorsController;
		}
		botDoorsController = new GameObject("BotDoorsController").AddComponent<BotDoorsController>();
		BotZone botZone = UnityEngine.Object.FindObjectOfType<BotZone>();
		if (botZone != null)
		{
			botDoorsController.transform.SetParent(botZone.transform.parent);
		}
		return botDoorsController;
	}
}
