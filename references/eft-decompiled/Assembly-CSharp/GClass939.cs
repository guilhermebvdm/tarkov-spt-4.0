using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public abstract class GClass939
{
	public struct Struct144
	{
		public Renderer renderer;

		public float distance;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class590
	{
		public static readonly Class590 class590_0 = new Class590();

		public static Comparison<Struct144> comparison_0;

		public int method_0(Struct144 renderer1, Struct144 renderer2)
		{
			return renderer1.distance.CompareTo(renderer2.distance);
		}
	}

	public static List<Struct144> smethod_0(Vector3 cameraPosition, float maxDistance)
	{
		List<Struct144> list = new List<Struct144>();
		Renderer[] array = GClass870.FindUnityObjectsOfType<Renderer>();
		foreach (Renderer renderer in array)
		{
			if (renderer.isVisible)
			{
				float num = Vector3.Distance(renderer.bounds.center, cameraPosition);
				if (num <= maxDistance)
				{
					Struct144 item = new Struct144
					{
						renderer = renderer,
						distance = num
					};
					list.Add(item);
				}
			}
		}
		list.Sort((Struct144 @struct, Struct144 struct2) => @struct.distance.CompareTo(struct2.distance));
		return list;
	}

	public static string StringOfAllRenderersInFrustum(Vector3 cameraPosition, float maxDistance)
	{
		List<Struct144> list = smethod_0(cameraPosition, maxDistance);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Struct144 item in list)
		{
			stringBuilder.AppendFormat("{0}\t{1}:{2}\n", item.distance, item.renderer.gameObject.scene.name, GClass6.GetFullPath(item.renderer.transform));
		}
		return stringBuilder.ToString();
	}

	public static void SaveAllRenderersInFrustum(string path, Vector3 cameraPosition, float maxDistance)
	{
		string contents = StringOfAllRenderersInFrustum(cameraPosition, maxDistance);
		File.WriteAllText(path, contents);
	}

	public static void PrintAllRenderersInFrustum(Vector3 cameraPosition, float maxDistance)
	{
		string text = StringOfAllRenderersInFrustum(cameraPosition, maxDistance);
		Debug.LogError("In Frustrum:\n" + text);
	}
}
