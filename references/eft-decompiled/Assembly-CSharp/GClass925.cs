using System;
using System.Threading.Tasks;
using Diz.Jobs;
using UnityEngine;

public class GClass925 : GClass924<GClass3677, GClass3685>
{
	[NonSerialized]
	public const float Float_0 = 0.05f;

	[NonSerialized]
	public const float Float_1 = 0.03f;

	[NonSerialized]
	public const float Float_2 = 3f;

	[NonSerialized]
	public static Quaternion Quaternion_0 = new Quaternion(0.1227878f, -0.6963643f, -0.6963643f, -0.1227878f);

	[NonSerialized]
	public IEasyAssets IEasyAssets;

	[NonSerialized]
	public GameObject GameObject_0;

	[NonSerialized]
	public MeshFilter MeshFilter_0;

	[NonSerialized]
	public MeshRenderer MeshRenderer_0;

	[NonSerialized]
	public bool Bool_2;

	public override string Folder => "Clothing";

	public GClass925(IEasyAssets easyAssets)
	{
		IEasyAssets = easyAssets;
	}

	public void Begin()
	{
		GameObject_0 = new GameObject("Preview GO")
		{
			layer = LayerMaskClass.WeaponPreview
		};
		GameObject_0.transform.SetPositionAndRotation(new Vector3(0f, -1.4f, 2f), Quaternion.Euler(new Vector3(-90f, 151f, 0f)));
		MeshFilter_0 = GameObject_0.AddComponent<MeshFilter>();
		MeshRenderer_0 = GameObject_0.AddComponent<MeshRenderer>();
	}

	public void End()
	{
		if (!(GameObject_0 == null))
		{
			UnityEngine.Object.Destroy(GameObject_0);
			GameObject_0 = null;
			MeshRenderer_0 = null;
			MeshFilter_0 = null;
		}
	}

	public GClass3685 GetIcon(GClass3677 clothing, XYCellSizeStruct textureSize)
	{
		int clothingHash = GClass928.GetClothingHash(clothing);
		if (method_7(clothingHash, out var icon))
		{
			return icon;
		}
		icon = new GClass3685(clothingHash);
		if (method_8(clothingHash, out var path))
		{
			method_6(icon, path, textureSize).HandleExceptions();
			return icon;
		}
		method_1(icon, clothing, textureSize, saveToFile: true, requireZeroMip: false).HandleExceptions();
		return icon;
	}

	public override async Task<RenderModelResult> RenderModel(GClass3677 clothing, SpriteFactory spriteFactory)
	{
		GameObject asset = GClass1857.GetAsset<GameObject>(IEasyAssets, clothing.Prefab.path, clothing.Prefab.rcid);
		SkinnedMeshRenderer skinnedMeshRenderer = ((asset != null) ? asset.GetComponentInChildren<SkinnedMeshRenderer>() : null);
		if (skinnedMeshRenderer == null)
		{
			return default(RenderModelResult);
		}
		if (!GameObject_0.activeSelf)
		{
			GameObject_0.SetActive(value: true);
		}
		while (Bool_2)
		{
			await JobScheduler.Yield();
		}
		Bool_2 = true;
		MeshFilter_0.sharedMesh = skinnedMeshRenderer.sharedMesh;
		MeshRenderer_0.sharedMaterial = skinnedMeshRenderer.sharedMaterial;
		Sprite sprite = await spriteFactory(GameObject_0, asset.GetComponent<PreviewPivot>());
		Bool_2 = false;
		return new RenderModelResult
		{
			sprite = sprite,
			zeroMipWasLoaded = true
		};
	}

	public override void PoseModelByPivot(GameObject model, Camera camera, float cameraAspect, PreviewPivot.IconSettings settings)
	{
		model.transform.rotation = settings.rotation;
		model.transform.localPosition = settings.position;
	}

	public override void PoseModelByBounds(GameObject model, Camera camera, in Bounds bounds)
	{
		model.transform.rotation = Quaternion_0;
		Bounds bounds2 = new Bounds(Quaternion_0 * bounds.center, Quaternion_0 * bounds.size);
		model.transform.localPosition = new Vector3(0f - bounds2.center.x - 0.05f, 0f - bounds2.center.y - bounds2.extents.y + camera.orthographicSize + 0.03f, 3f);
	}

	public override Bounds GetBounds(GameObject model)
	{
		return model.GetComponent<MeshFilter>().sharedMesh.bounds;
	}
}
