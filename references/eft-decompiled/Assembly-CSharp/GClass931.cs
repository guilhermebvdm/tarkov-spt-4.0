using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass931
{
	public struct Struct128
	{
		public Texture2D tex2d;

		public int requestsCount;

		public bool loggedError;
	}

	[NonSerialized]
	public static float Float_0;

	[NonSerialized]
	public static int Int_0;

	[NonSerialized]
	public static List<Struct128> List_0 = new List<Struct128>();

	[NonSerialized]
	public static List<Material> List_1 = new List<Material>();

	[NonSerialized]
	public static List<string> List_2 = new List<string>();

	[NonSerialized]
	public static List<string> List_3 = new List<string>();

	public async Task Prepare()
	{
		if (Interlocked.Increment(ref Int_0) == 1)
		{
			Float_0 = GameGraphicsClass.IncreaseMemoryBudgetForIcon();
			if (CameraClass.Exist)
			{
				CameraClass.Instance.IncreaseStreamingMipMapBias(2f);
			}
		}
		else
		{
			while (Float_0 == 0f)
			{
				await Task.Yield();
			}
		}
	}

	public void Restore()
	{
		if (Interlocked.Decrement(ref Int_0) == 0)
		{
			GameGraphicsClass.RestoreMemoryBudget();
			Float_0 = 0f;
			if (CameraClass.Exist)
			{
				CameraClass.Instance.IncreaseStreamingMipMapBias(-2f);
			}
		}
	}

	public async Task<bool> RequestMipZeroFast(GameObject obj)
	{
		method_0(obj);
		int num = 0;
		while (num < 50)
		{
			await Task.Yield();
			bool flag = false;
			for (int i = 0; i < List_0.Count; i++)
			{
				if (!List_0[i].tex2d.IsRequestedMipmapLevelLoaded())
				{
					num++;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return num < 50;
	}

	public async Task<bool> RequestMipZero(GameObject obj)
	{
		method_0(obj);
		bool flag = true;
		bool result = false;
		while (flag)
		{
			await Task.Yield();
			flag = false;
			result = true;
			for (int i = 0; i < List_0.Count; i++)
			{
				if (!List_0[i].tex2d.IsRequestedMipmapLevelLoaded())
				{
					result = false;
					Struct128 value = List_0[i];
					if (value.requestsCount < 200)
					{
						value.requestsCount++;
						List_0[i] = value;
						flag = true;
						break;
					}
					if (!value.loggedError)
					{
						Debug.LogError("Mip 0 waiting timeout: " + value.tex2d.name);
						value.loggedError = true;
						List_0[i] = value;
					}
				}
			}
		}
		return result;
	}

	public void method_0(GameObject obj)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		List_0.Clear();
		List_3.Clear();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer obj2 in array)
		{
			List_1.Clear();
			List_2.Clear();
			obj2.GetSharedMaterials(List_1);
			foreach (Material item in List_1)
			{
				if (item == null)
				{
					continue;
				}
				item.GetTexturePropertyNames(List_2);
				foreach (string item2 in List_2)
				{
					Texture texture = item.GetTexture(item2);
					if (!(texture == null) && texture is Texture2D texture2D && texture.dimension == TextureDimension.Tex2D)
					{
						string name = texture2D.name;
						if (!List_3.Contains(name) && texture2D != null)
						{
							texture2D.requestedMipmapLevel = 0;
							texture2D.minimumMipmapLevel = 0;
							List_0.Add(new Struct128
							{
								tex2d = texture2D,
								requestsCount = 0,
								loggedError = false
							});
							List_3.Add(name);
						}
					}
				}
			}
		}
	}

	public async Task ResetRequestedMip(GameObject obj)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer obj2 in componentsInChildren)
		{
			List_1.Clear();
			List_2.Clear();
			obj2.GetSharedMaterials(List_1);
			foreach (Material item in List_1)
			{
				if (item == null)
				{
					continue;
				}
				item.GetTexturePropertyNames(List_2);
				foreach (string item2 in List_2)
				{
					Texture texture = item.GetTexture(item2);
					if (!(texture == null) && texture.dimension == TextureDimension.Tex2D)
					{
						Texture2D texture2D = (Texture2D)texture;
						if (texture2D != null)
						{
							texture2D.ClearRequestedMipmapLevel();
							texture2D.ClearMinimumMipmapLevel();
						}
					}
				}
			}
		}
		await Task.Yield();
	}
}
