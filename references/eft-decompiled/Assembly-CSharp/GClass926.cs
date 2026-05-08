using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Diz.Jobs;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using EFT.UI.WeaponModding;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

public class GClass926 : GClass924<Item, GClass929>
{
	public struct Struct124
	{
		[NonSerialized]
		[CompilerGenerated]
		public Texture2D Texture2D_0;

		[NonSerialized]
		[CompilerGenerated]
		public int Int_0;

		public readonly Texture2D Tex2d
		{
			[CompilerGenerated]
			get
			{
				return Texture2D_0;
			}
		}

		public int RequestsCount
		{
			[CompilerGenerated]
			readonly get
			{
				return Int_0;
			}
			[CompilerGenerated]
			set
			{
				Int_0 = value;
			}
		}

		public Struct124(Texture2D tex2d)
		{
			Texture2D_0 = tex2d;
			RequestsCount = 0;
		}

		public Struct124 IncRequestCount()
		{
			Struct124 result = new Struct124(Tex2d);
			result.RequestsCount = RequestsCount + 1;
			return result;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class573
	{
		public static readonly Class573 class573_0 = new Class573();

		public static Func<Item, string> func_0;

		public string method_0(Item x)
		{
			return x.Prefab.path;
		}
	}

	[NonSerialized]
	public IEasyAssets IEasyAssets;

	[NonSerialized]
	public PoolManagerClass PoolManagerClass;

	[NonSerialized]
	public static int Int_3;

	[NonSerialized]
	public static float Float_0;

	public override string Folder => string.Empty;

	public GClass926(IEasyAssets easyAssets, PoolManagerClass objectsFactory)
	{
		IEasyAssets = easyAssets;
		PoolManagerClass = objectsFactory;
	}

	public GClass929 GetItemIcon(Item item, in XYCellSizeStruct size, bool forcedGeneration = false)
	{
		int itemHash = GClass928.GetItemHash(item);
		GClass929 icon;
		bool flag = method_7(itemHash, out icon);
		if (!forcedGeneration && flag && (GClass2340.InRaid || !icon.IsGeneratedInRaid))
		{
			return icon;
		}
		icon = new GClass929(itemHash)
		{
			IsGeneratedInRaid = GClass2340.InRaid
		};
		if (!forcedGeneration && method_8(itemHash, out var path))
		{
			method_6(icon, path, size).HandleExceptions();
			return icon;
		}
		method_1(icon, item, size, saveToFile: true, requireZeroMip: true).HandleExceptions();
		return icon;
	}

	public override void PoseModelByPivot(GameObject model, Camera camera, float cameraAspect, PreviewPivot.IconSettings settings)
	{
		model.transform.rotation = settings.rotation;
		CenterByBounds(model, camera, cameraAspect, settings);
	}

	public override void PoseModelByBounds(GameObject model, Camera camera, in Bounds bounds)
	{
		camera.orthographicSize = bounds.extents.y;
	}

	public static void CenterByBounds(GameObject model, Camera camera, float cameraAspect, PreviewPivot.IconSettings iconSettings)
	{
		Bounds bounds = WeaponPreview.GetBounds(model);
		camera.orthographic = true;
		float num = bounds.extents.x / bounds.extents.y;
		Vector3 position = model.transform.position;
		position -= bounds.center;
		Vector3 position2 = new Vector3(position.x, position.y + camera.transform.position.y, position.z);
		if (iconSettings.hasOffset)
		{
			position2 += iconSettings.position;
		}
		model.transform.position = position2;
		if (num > cameraAspect)
		{
			camera.orthographicSize = bounds.extents.x / cameraAspect / iconSettings.boundsScale;
		}
		else
		{
			camera.orthographicSize = bounds.extents.y / iconSettings.boundsScale;
		}
	}

	public async Task<bool> method_11(GameObject obj)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			List<string> value2;
			using (CollectionPool<List<string>, string>.Get(out value2))
			{
				HashSet<string> value3;
				using (CollectionPool<HashSet<string>, string>.Get(out value3))
				{
					List<Struct124> value4;
					using (CollectionPool<List<Struct124>, Struct124>.Get(out value4))
					{
						MeshRenderer[] array = componentsInChildren;
						foreach (MeshRenderer obj2 in array)
						{
							value.Clear();
							value2.Clear();
							obj2.GetSharedMaterials(value);
							foreach (Material item in value)
							{
								if (!item)
								{
									continue;
								}
								item.GetTexturePropertyNames(value2);
								foreach (string item2 in value2)
								{
									Texture texture = item.GetTexture(item2);
									if ((bool)texture && texture is Texture2D texture2D && texture.dimension == TextureDimension.Tex2D && !value3.Contains(texture2D.name))
									{
										texture2D.requestedMipmapLevel = 0;
										texture2D.minimumMipmapLevel = 0;
										value4.Add(new Struct124(texture2D));
										value3.Add(texture2D.name);
									}
								}
							}
						}
						bool result = true;
						while (value4.Count > 0)
						{
							for (int num = value4.Count - 1; num >= 0; num--)
							{
								if (value4[num].Tex2d.IsRequestedMipmapLevelLoaded())
								{
									value4.RemoveAt(num);
								}
								else
								{
									value4[num] = value4[num].IncRequestCount();
									if (value4[num].RequestsCount >= 200)
									{
										Debug.LogError("Mip 0 waiting timeout: " + value4[num].Tex2d.name);
										value4.RemoveAt(num);
										result = false;
									}
								}
							}
							await Task.Yield();
						}
						return result;
					}
				}
			}
		}
	}

	public async Task method_12(GameObject obj)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		List<Material> list = new List<Material>();
		List<string> list2 = new List<string>();
		new List<Struct124>();
		HashSet<string> hashSet = new HashSet<string>();
		MeshRenderer[] array = componentsInChildren;
		foreach (MeshRenderer obj2 in array)
		{
			list.Clear();
			list2.Clear();
			obj2.GetSharedMaterials(list);
			foreach (Material item in list)
			{
				if (item == null)
				{
					continue;
				}
				item.GetTexturePropertyNames(list2);
				foreach (string item2 in list2)
				{
					if (hashSet.Contains(item2))
					{
						continue;
					}
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

	public override async Task<RenderModelResult> RenderModel(Item item, SpriteFactory spriteFactory)
	{
		Item item2 = GClass3380.CloneVisibleItem(item);
		global::DependencyGraphClass<IEasyBundle>.GClass1661 gClass = GClass1857.Retain(IEasyAssets, from x in GClass3380.GetAllItems(item2)
			select x.Prefab.path);
		await GClass1857.LoadBundles(gClass);
		while (!gClass.Finished)
		{
			await JobScheduler.Yield();
		}
		GameObject gameObject = await PoolManagerClass.CreateCleanLootPrefabAsync(item2);
		if (Interlocked.Increment(ref Int_3) == 1)
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
		await Task.Yield();
		bool zeroMipWasLoaded = await method_11(gameObject);
		TransformHelperClass.SetLayersRecursively(gameObject, LayerMaskClass.WeaponPreview);
		PreviewPivot component = gameObject.GetComponent<PreviewPivot>();
		Sprite sprite = await spriteFactory(gameObject, component);
		if (Interlocked.Decrement(ref Int_3) == 0)
		{
			GameGraphicsClass.RestoreMemoryBudget();
			Float_0 = 0f;
			if (CameraClass.Exist)
			{
				CameraClass.Instance.IncreaseStreamingMipMapBias(-2f);
			}
		}
		AssetPoolObject.ReturnToPool(gameObject);
		gClass.Release();
		return new RenderModelResult
		{
			sprite = ((component.Icon.overrideIcon != null) ? component.Icon.overrideIcon : sprite),
			zeroMipWasLoaded = zeroMipWasLoaded
		};
	}
}
