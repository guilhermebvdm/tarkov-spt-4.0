using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class GClass1002
{
	public struct GStruct71
	{
		public string orderName;

		public int id;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class703
	{
		public static readonly Class703 class703_0 = new Class703();

		public static Func<GStruct71, string> func_0;

		public string method_0(GStruct71 t)
		{
			return t.orderName;
		}
	}

	public static void AddAtFirstCommandBuffer(this Camera cam, CameraEvent evn, CommandBuffer cmd)
	{
		CommandBuffer[] commandBuffers = cam.GetCommandBuffers(evn);
		cam.RemoveCommandBuffers(evn);
		cam.AddCommandBuffer(evn, cmd);
		for (int i = 0; i < commandBuffers.Length; i++)
		{
			cam.AddCommandBuffer(evn, commandBuffers[i]);
		}
	}

	public static void SortCommandBuffers(this Camera cam, CameraEvent evn)
	{
		CommandBuffer[] commandBuffers = cam.GetCommandBuffers(evn);
		List<GStruct71> list = new List<GStruct71>();
		for (int i = 0; i < commandBuffers.Length; i++)
		{
			string name = commandBuffers[i].name;
			int num = name.IndexOf('+');
			num = ((num < 0) ? name.Length : num);
			string orderName = name.Substring(num);
			GStruct71 item = new GStruct71
			{
				orderName = orderName,
				id = i
			};
			list.Add(item);
		}
		List<GStruct71> list2 = list.OrderBy((GStruct71 t) => t.orderName).ToList();
		cam.RemoveCommandBuffers(evn);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			int id = list2[num2].id;
			cam.AddCommandBuffer(evn, commandBuffers[id]);
		}
	}

	public static void SetKeyword(this Material mat, string key, bool enable)
	{
		if (enable)
		{
			mat.EnableKeyword(key);
		}
		else
		{
			mat.DisableKeyword(key);
		}
	}
}
