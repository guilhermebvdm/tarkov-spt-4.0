using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIDoorsHolder : MonoBehaviour
{
	[CompilerGenerated]
	public class Class144
	{
		public string portalDoorId;

		public bool method_0(NavMeshDoorLink x)
		{
			return x.DoorId == portalDoorId;
		}
	}

	public List<NavMeshDoorLink> DoorsLinks;

	public static AIDoorsHolder CreateOfFind(AIVoxelesData parentData)
	{
		AIDoorsHolder aIDoorsHolder = Object.FindObjectOfType<AIDoorsHolder>();
		if (aIDoorsHolder == null)
		{
			aIDoorsHolder = new GameObject("AIDoorsHolder").AddComponent<AIDoorsHolder>();
		}
		aIDoorsHolder.transform.SetParent(parentData.transform);
		aIDoorsHolder.transform.SetParent(null);
		return aIDoorsHolder;
	}

	public NavMeshDoorLink TryFindDoor(string portalDoorId)
	{
		if (DoorsLinks == null)
		{
			return null;
		}
		return DoorsLinks.FirstOrDefault((NavMeshDoorLink x) => x.DoorId == portalDoorId);
	}
}
