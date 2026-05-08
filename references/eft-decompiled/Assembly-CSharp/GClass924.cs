using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Diz.Jobs;
using EFT.UI.WeaponModding;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class GClass924<T, U> : IDisposable where U : GInterface39
{
	public delegate Task<Sprite> SpriteFactory(GameObject model, PreviewPivot pivot);

	public struct RenderModelResult
	{
		public Sprite sprite;

		public bool zeroMipWasLoaded;
	}

	public struct Struct115
	{
		public Color AmbientEquatorColor;

		public Color AmbientGroundColor;

		public float AmbientIntensity;

		public Color AmbientLight;

		public AmbientMode AmbientMode;

		public Color AmbientSkyColor;

		public bool Fog;

		public static Struct115 Store()
		{
			return new Struct115
			{
				AmbientEquatorColor = RenderSettings.ambientEquatorColor,
				AmbientGroundColor = RenderSettings.ambientGroundColor,
				AmbientIntensity = RenderSettings.ambientIntensity,
				AmbientLight = RenderSettings.ambientLight,
				AmbientMode = RenderSettings.ambientMode,
				AmbientSkyColor = RenderSettings.ambientSkyColor,
				Fog = RenderSettings.fog
			};
		}

		public static void Reset()
		{
			RenderSettings.ambientEquatorColor = Color.black;
			RenderSettings.ambientGroundColor = Color.black;
			RenderSettings.ambientIntensity = 0f;
			RenderSettings.ambientLight = Color.black;
			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientSkyColor = Color.black;
			RenderSettings.fog = false;
		}

		public void Restore()
		{
			RenderSettings.ambientEquatorColor = AmbientEquatorColor;
			RenderSettings.ambientGroundColor = AmbientGroundColor;
			RenderSettings.ambientIntensity = AmbientIntensity;
			RenderSettings.ambientLight = AmbientLight;
			RenderSettings.ambientMode = AmbientMode;
			RenderSettings.ambientSkyColor = AmbientSkyColor;
			RenderSettings.fog = Fog;
		}
	}

	[CompilerGenerated]
	public class Class569
	{
		public GClass924<T, U> gclass924_0;

		public XYCellSizeStruct size;

		public async Task<Sprite> method_0(GameObject model, PreviewPivot pivot)
		{
			await gclass924_0.method_0();
			return gclass924_0.method_4(model, in size, pivot);
		}
	}

	[CompilerGenerated]
	public class Class570
	{
		public GClass924<T, U> gclass924_0;

		public XYCellSizeStruct size;

		public async Task<Sprite> method_0(GameObject model, PreviewPivot pivot)
		{
			await gclass924_0.method_0();
			while (gclass924_0.Bool_0)
			{
				await JobScheduler.Yield();
			}
			gclass924_0.Bool_0 = true;
			await JobScheduler.Yield();
			Sprite result = gclass924_0.method_4(model, in size, pivot);
			await JobScheduler.Yield();
			gclass924_0.Bool_0 = false;
			return result;
		}
	}

	[NonSerialized]
	public const int Int_0 = 2;

	[NonSerialized]
	public Dictionary<int, U> Dictionary_0 = new Dictionary<int, U>(1024);

	[NonSerialized]
	public Dictionary<int, int> Dictionary_1 = new Dictionary<int, int>(1024);

	[NonSerialized]
	[CompilerGenerated]
	public GInterface40 Ginterface40_0 = new GClass933();

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public int? Nullable_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public Camera Camera_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public IconShadow IconShadow_0;

	[NonSerialized]
	public Light[] Light_0;

	[NonSerialized]
	public CommandBuffer CommandBuffer_0;

	[NonSerialized]
	public CommandBuffer CommandBuffer_1;

	[NonSerialized]
	public GClass931 Gclass931_0 = new GClass931();

	public virtual GInterface40 ShaderReplacer
	{
		[CompilerGenerated]
		get
		{
			return Ginterface40_0;
		}
	}

	public abstract string Folder { get; }

	public virtual string IconCameraName => "IconCamera";

	public virtual Camera RenderCamera => Camera_0;

	public GClass924()
	{
		string path = Path.Combine(GClass930.Path, BackendConfigAbstractClass.Config.MatchingVersion ?? "Editor");
		String_0 = Path.Combine(path, Folder);
		String_1 = Path.Combine(String_0, "index.json");
	}

	public async Task Init()
	{
		method_10();
		Dictionary_1.Clear();
		if (!File.Exists(String_1))
		{
			return;
		}
		try
		{
			using StreamReader streamReader = File.OpenText(String_1);
			JsonParserClass.PopulateJson(await streamReader.ReadToEndAsync(), Dictionary_1);
			if (Dictionary_1.Count < 1)
			{
				Debug.LogError("Error: Icon cache index is present but empty");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Could not parse Icon cache index file: " + ex.Message);
		}
		Int_2 = ((Dictionary_1.Values.Count > 0) ? Dictionary_1.Values.Max() : 0);
	}

	public void ClearMemoryCache()
	{
		foreach (U value in Dictionary_0.Values)
		{
			smethod_1(value);
		}
		Dictionary_0.Clear();
		Dictionary_1.Clear();
		if (Directory.Exists(String_0))
		{
			Directory.Delete(String_0, recursive: true);
		}
	}

	public async Task method_0()
	{
		if (Camera_0 != null)
		{
			return;
		}
		if (!Bool_1)
		{
			Bool_1 = true;
			ResourceRequest resourceRequest = Resources.LoadAsync<Camera>(IconCameraName);
			await GClass841.Await(resourceRequest);
			Camera_0 = UnityEngine.Object.Instantiate((Camera)resourceRequest.asset);
			Camera_0.name = Folder + IconCameraName;
			Camera_0.enabled = false;
			UnityEngine.Object.DontDestroyOnLoad(Camera_0.gameObject);
			Camera_0.clearFlags = CameraClearFlags.Depth;
			Camera_0.transform.position = new Vector3(0f, -10f, -1f);
			IconShadow_0 = Camera_0.GetComponent<IconShadow>();
			Light_0 = Camera_0.GetComponentsInParent<Light>();
			StreamingController streamingController = Camera_0.gameObject.GetComponent<StreamingController>();
			if (streamingController == null)
			{
				streamingController = Camera_0.gameObject.AddComponent<StreamingController>();
			}
			streamingController.streamingMipmapBias = -100f;
			Bool_1 = false;
			await JobScheduler.Yield();
		}
		else
		{
			while (Bool_1)
			{
				await JobScheduler.Yield();
			}
		}
	}

	public async Task<Sprite> GetItemSpriteAsync(T item, XYCellSizeStruct size)
	{
		return (await RenderModel(item, async delegate(GameObject model, PreviewPivot pivot)
		{
			await method_0();
			return method_4(model, in size, pivot);
		})).sprite;
	}

	public async Task method_1(U icon, T item, XYCellSizeStruct size, bool saveToFile, bool requireZeroMip)
	{
		Int_1++;
		Dictionary_0[icon.Hash] = icon;
		RenderModelResult renderModelResult = await RenderModel(item, async delegate(GameObject model, PreviewPivot pivot)
		{
			await method_0();
			while (Bool_0)
			{
				await JobScheduler.Yield();
			}
			Bool_0 = true;
			await JobScheduler.Yield();
			Sprite result = method_4(model, in size, pivot);
			await JobScheduler.Yield();
			Bool_0 = false;
			return result;
		});
		if (renderModelResult.sprite != null)
		{
			smethod_1(icon);
			Sprite sprite = renderModelResult.sprite;
			icon.Sprite = sprite;
			icon.Sprite.texture.filterMode = FilterMode.Trilinear;
			icon.Changed.Invoke();
			if ((!requireZeroMip) ? saveToFile : (saveToFile && renderModelResult.zeroMipWasLoaded))
			{
				await method_5(icon);
			}
		}
		else
		{
			Debug.LogError("Something went wrong! Sprite for " + icon.Hash + " was not created!");
		}
		Int_1 = Mathf.Max(Int_1 - 1, 0);
		if (Int_1 <= 0)
		{
			if (Nullable_0.HasValue)
			{
				QualitySettings.streamingMipmapsMaxLevelReduction = Nullable_0.Value;
				Nullable_0 = null;
			}
			if (Dictionary_1.Count > 0)
			{
				method_10();
				File.WriteAllText(String_1, JsonParserClass.ToJson(Dictionary_1));
			}
		}
	}

	public void method_2(GameObject model, in XYCellSizeStruct textureSize, PreviewPivot previewPivot)
	{
		float num = (float)textureSize.X / (float)textureSize.Y;
		model.transform.SetParent(Camera_0.transform);
		if (previewPivot != null)
		{
			model.transform.localPosition = Vector3.zero;
			model.transform.rotation = Quaternion.identity;
			model.transform.localScale = Vector3.one;
			PoseModelByPivot(model, Camera_0, num, previewPivot.Icon);
		}
		else
		{
			Camera_0.orthographic = true;
			PoseModelByBounds(model, Camera_0, GetBounds(model));
		}
		Camera_0.aspect = num;
	}

	public Texture2D method_3(in XYCellSizeStruct size)
	{
		int x = size.X;
		int width = x * 2;
		int y = size.Y;
		int height = y * 2;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 8);
		temporary.name = "IconCreator TextureDouble";
		Camera_0.gameObject.SetActive(value: true);
		Camera_0.targetTexture = temporary;
		Camera_0.clearFlags = CameraClearFlags.Color;
		Camera_0.backgroundColor = new Color(0f, 0f, 0f, 0f);
		Camera_0.useOcclusionCulling = false;
		IconShadow_0.SetTexDimension(width, height);
		RenderTexture temporary2 = RenderTexture.GetTemporary(x, y);
		GClass860.ClearTexture(temporary2);
		Camera_0.Render();
		Graphics.Blit(temporary, temporary2);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = temporary2;
		Texture2D texture2D = smethod_0(x, y);
		texture2D.ReadPixels(new Rect(0f, 0f, Camera_0.pixelWidth, Camera_0.pixelHeight), 0, 0, recalculateMipMaps: false);
		texture2D.Apply();
		RenderTexture.active = active;
		Camera_0.targetTexture = null;
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
		Camera_0.gameObject.SetActive(value: false);
		return texture2D;
	}

	public static Texture2D smethod_0(int width, int height)
	{
		return new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false)
		{
			filterMode = FilterMode.Trilinear,
			name = "Icon"
		};
	}

	public Sprite method_4(GameObject model, in XYCellSizeStruct size, PreviewPivot previewPivot)
	{
		if (model == null)
		{
			return null;
		}
		Struct115 @struct = Struct115.Store();
		Struct115.Reset();
		ShaderReplacer.Replace(model);
		method_2(model, in size, previewPivot);
		Light[] light_ = Light_0;
		for (int i = 0; i < light_.Length; i++)
		{
			light_[i].enabled = true;
		}
		model.SetActive(value: true);
		Texture2D texture = method_3(in size);
		model.SetActive(value: false);
		ShaderReplacer.Restore();
		@struct.Restore();
		return smethod_2(texture);
	}

	public async Task method_5(U icon)
	{
		method_10();
		Dictionary_1[icon.Hash] = ++Int_2;
		using FileStream fileStream = File.OpenWrite(method_9(Int_2));
		byte[] array = icon.Sprite.texture.EncodeToPNG();
		await fileStream.WriteAsync(array, 0, array.Length);
	}

	public async Task method_6(U icon, string iconPath, XYCellSizeStruct size)
	{
		Dictionary_0[icon.Hash] = icon;
		Texture2D texture2D = smethod_0(size.X, size.Y);
		using (FileStream fileStream = File.OpenRead(iconPath))
		{
			byte[] array = ArrayPool<byte>.Shared.Rent((int)fileStream.Length);
			await fileStream.ReadAsync(array, 0, array.Length);
			texture2D.LoadImage(array);
			ArrayPool<byte>.Shared.Return(array);
		}
		Sprite sprite = smethod_2(texture2D);
		icon.Sprite = sprite;
		icon.Sprite.texture.filterMode = FilterMode.Trilinear;
		icon.Changed.Invoke();
	}

	public bool method_7(int hash, out U icon)
	{
		return Dictionary_0.TryGetValue(hash, out icon);
	}

	public bool method_8(int hash, out string path)
	{
		path = null;
		if (!Dictionary_1.TryGetValue(hash, out var value))
		{
			return false;
		}
		path = method_9(value);
		return File.Exists(path);
	}

	public string method_9(int fileId)
	{
		return Path.Combine(String_0, fileId + ".png");
	}

	public abstract Task<RenderModelResult> RenderModel(T item, SpriteFactory spriteFactory);

	public abstract void PoseModelByPivot(GameObject model, Camera camera, float cameraAspect, PreviewPivot.IconSettings settings);

	public abstract void PoseModelByBounds(GameObject model, Camera camera, in Bounds bounds);

	public virtual Bounds GetBounds(GameObject model)
	{
		return WeaponPreview.GetBounds(model);
	}

	public static void smethod_1(U icon)
	{
		if (icon.Sprite != null)
		{
			UnityEngine.Object.Destroy(icon.Sprite.texture);
			UnityEngine.Object.Destroy(icon.Sprite);
		}
	}

	public virtual void Dispose()
	{
		if (!(Camera_0 == null))
		{
			Camera_0.RemoveAllCommandBuffers();
			if (CommandBuffer_0 != null)
			{
				CommandBuffer_0.Dispose();
				CommandBuffer_0 = null;
			}
			if (CommandBuffer_1 != null)
			{
				CommandBuffer_1.Dispose();
				CommandBuffer_1 = null;
			}
			UnityEngine.Object.DestroyImmediate(Camera_0.gameObject);
			Camera_0 = null;
		}
	}

	public static Sprite smethod_2(Texture2D texture)
	{
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0f, 0f), 100f);
	}

	public void method_10()
	{
		if (!Directory.Exists(String_0))
		{
			Directory.CreateDirectory(String_0);
		}
	}
}
