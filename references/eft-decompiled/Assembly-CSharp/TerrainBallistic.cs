using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.Ballistics;
using UnityEngine;

public class TerrainBallistic : BaseBallistic
{
	[CompilerGenerated]
	public class Class327
	{
		public ESurfaceSound terrainMaterial;

		public bool method_0(BallisticCollider c)
		{
			return c.SurfaceSound == terrainMaterial;
		}
	}

	[CompilerGenerated]
	public class Class328
	{
		public TerrainBallistic terrainBallistic_0;

		public int i;

		public Func<BallisticCollider, bool> func_0;

		public bool method_0(BallisticCollider c)
		{
			return c.SurfaceSound == terrainBallistic_0.TerrainMaterials[i];
		}
	}

	public TerrainData TerrainData;

	public TextAsset TextureMixDataAsset;

	private static GameObject gameObject_0;

	private int int_0;

	private Vector3 vector3_0;

	private BallisticCollider[] ballisticCollider_0;

	private GClass795 gclass795_0;

	public ESurfaceSound[] TerrainMaterials;

	[CompilerGenerated]
	private static readonly MaterialType[] materialType_0 = new MaterialType[6]
	{
		MaterialType.Asphalt,
		MaterialType.Soil,
		MaterialType.GrassHigh,
		MaterialType.Concrete,
		MaterialType.Gravel,
		MaterialType.Water
	};

	public static MaterialType[] MaterialType_0
	{
		[CompilerGenerated]
		get
		{
			return materialType_0;
		}
	}

	public void Awake()
	{
		int_0++;
		if (gameObject_0 == null)
		{
			gameObject_0 = new GameObject("Terrain Ballistic");
			gameObject_0.isStatic = true;
			MaterialType[] array = MaterialType_0;
			foreach (MaterialType typeOfMaterial in array)
			{
				BallisticCollider ballisticCollider = gameObject_0.AddComponent<BallisticCollider>();
				ballisticCollider.TypeOfMaterial = typeOfMaterial;
				ballisticCollider.Associate(ballisticCollider.TypeOfMaterial);
				ApplyPresetValues(ballisticCollider);
			}
		}
		BallisticCollider[] components = gameObject_0.GetComponents<BallisticCollider>();
		if (TerrainData == null)
		{
			Terrain component = GetComponent<Terrain>();
			if (component != null)
			{
				TerrainData = component.terrainData;
			}
			else
			{
				Debug.LogError("Can't find terrain data: " + this, this);
			}
		}
		int alphamapLayers = TerrainData.alphamapLayers;
		int num = TerrainMaterials.Length;
		if (num < alphamapLayers)
		{
			num = alphamapLayers;
			ballisticCollider_0 = new BallisticCollider[num];
		}
		else
		{
			ballisticCollider_0 = new BallisticCollider[num];
		}
		for (int j = 0; j < num; j++)
		{
			ESurfaceSound terrainMaterial = ((j < TerrainMaterials.Length) ? TerrainMaterials[j] : ESurfaceSound.Soil);
			ballisticCollider_0[j] = components.FirstOrDefault((BallisticCollider c) => c.SurfaceSound == terrainMaterial);
			if (ballisticCollider_0[j] == null)
			{
				ballisticCollider_0[j] = components.First();
			}
		}
		if (TextureMixDataAsset != null)
		{
			gclass795_0 = GClass795.LoadFromAsset(TextureMixDataAsset);
		}
		vector3_0 = base.transform.position;
	}

	public void OnValidate()
	{
		if (gameObject_0 == null)
		{
			return;
		}
		BallisticCollider[] components = gameObject_0.GetComponents<BallisticCollider>();
		ballisticCollider_0 = new BallisticCollider[TerrainMaterials.Length];
		int i;
		for (i = 0; i < TerrainMaterials.Length; i++)
		{
			ballisticCollider_0[i] = components.FirstOrDefault((BallisticCollider c) => c.SurfaceSound == TerrainMaterials[i]);
			if (ballisticCollider_0[i] == null)
			{
				ballisticCollider_0[i] = components.First();
			}
		}
	}

	public void OnDestroy()
	{
		int_0--;
		if (int_0 < 1)
		{
			UnityEngine.Object.Destroy(gameObject_0);
		}
	}

	public override ESurfaceSound GetSurfaceSound(Vector3 pos)
	{
		return Get(pos).SurfaceSound;
	}

	public override BallisticCollider Get(Vector3 pos)
	{
		int num = method_0(pos);
		if (num > ballisticCollider_0.Length)
		{
			Debug.LogErrorFormat("{0} has unassigned collision texture #{1} ", base.gameObject.name, num);
			return ballisticCollider_0.FirstOrDefault((BallisticCollider b) => b.SurfaceSound == SurfaceSound);
		}
		return ballisticCollider_0[num];
	}

	public int method_0(Vector3 worldPos)
	{
		if (gclass795_0 != null)
		{
			return gclass795_0.GetTextureMix(worldPos, vector3_0);
		}
		return GClass795.GetMainTexture(worldPos, TerrainData, vector3_0);
	}

	public static void ApplyPresetValues(BallisticCollider bCollider)
	{
		BallisticPreset[] colliderPresets = EFTHardSettings.Instance.ColliderPresets;
		int typeOfMaterial = (int)bCollider.TypeOfMaterial;
		if (typeOfMaterial >= colliderPresets.Length)
		{
			Debug.LogError("Preset is missing");
			return;
		}
		BallisticPreset ballisticPreset = colliderPresets[typeOfMaterial];
		if (ballisticPreset != null)
		{
			bCollider.TypeOfMaterial = ballisticPreset.MaterialType;
			bCollider.PenetrationLevel = ballisticPreset[0];
			bCollider.PenetrationChance = ballisticPreset[1];
			bCollider.RicochetChance = ballisticPreset[2];
			bCollider.FragmentationChance = ballisticPreset[3];
			bCollider.TrajectoryDeviationChance = ballisticPreset[4];
			bCollider.TrajectoryDeviation = ballisticPreset[5];
		}
	}

	public static bool IsNoneDefault(BallisticCollider bCollider)
	{
		BallisticPreset[] colliderPresets = EFTHardSettings.Instance.ColliderPresets;
		int typeOfMaterial = (int)bCollider.TypeOfMaterial;
		if (typeOfMaterial >= colliderPresets.Length)
		{
			Debug.LogError("Preset is missing");
			return true;
		}
		BallisticPreset ballisticPreset = colliderPresets[typeOfMaterial];
		if (ballisticPreset == null)
		{
			return true;
		}
		if (Mathf.Approximately(bCollider.PenetrationLevel, ballisticPreset[0]) && Mathf.Approximately(bCollider.PenetrationChance, ballisticPreset[1]) && Mathf.Approximately(bCollider.RicochetChance, ballisticPreset[2]) && Mathf.Approximately(bCollider.FragmentationChance, ballisticPreset[3]) && Mathf.Approximately(bCollider.TrajectoryDeviationChance, ballisticPreset[4]))
		{
			return !Mathf.Approximately(bCollider.TrajectoryDeviation, ballisticPreset[5]);
		}
		return true;
	}

	[CompilerGenerated]
	public bool method_1(BallisticCollider b)
	{
		return b.SurfaceSound == SurfaceSound;
	}
}
