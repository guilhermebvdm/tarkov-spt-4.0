using System;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

public class GClass709 : LoggerClass
{
	[NonSerialized]
	[CompilerGenerated]
	public static GClass709 Gclass709_0 = new GClass709();

	public static GClass709 Instance
	{
		[CompilerGenerated]
		get
		{
			return Gclass709_0;
		}
	}

	public GClass709()
		: base("ai_decision", LoggerMode.Add)
	{
	}

	public void LogRaycast(RaycastHit hit, string sourceFunction, BotOwner owner = null)
	{
		if (hit.collider == null)
		{
			LogInfo("Raycast. Hit: null. Source: {0} Owner: {1} Time: {2}", sourceFunction, owner?.name, Time.time);
		}
		else
		{
			LogInfo("Raycast. Hit: {0} Layer:{1} Source: {2} Owner: {3} Time: {4}", hit.collider.gameObject.name, hit.collider.gameObject.layer, sourceFunction, owner?.name, Time.time);
		}
	}
}
