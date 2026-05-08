using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Koenigz.PerfectCulling.EFT;

public class PerfectCullingMaterialSettings : MonoBehaviour
{
	[CompilerGenerated]
	public class Class841
	{
		public Material mat;

		public bool method_0(Material x)
		{
			return mat == x;
		}
	}

	[CompilerGenerated]
	public class Class842
	{
		public Material mat;

		public bool method_0(Material x)
		{
			return mat == x;
		}
	}

	[SerializeField]
	private bool _ignoreBatchedWindowsFilter;

	[SerializeField]
	private Material[] _forceOpaqueMaterials = Array.Empty<Material>();

	[SerializeField]
	private Material[] _forceTransparentMaterials = Array.Empty<Material>();

	private static PerfectCullingMaterialSettings perfectCullingMaterialSettings_0;

	public bool IgnoreBatchedWindowsFilter => _ignoreBatchedWindowsFilter;

	public Material[] ForceOpaqueMaterials => _forceOpaqueMaterials;

	public Material[] ForceTransparentMaterials => _forceTransparentMaterials;

	public static PerfectCullingMaterialSettings Instance
	{
		get
		{
			if (perfectCullingMaterialSettings_0 == null)
			{
				perfectCullingMaterialSettings_0 = UnityEngine.Object.FindObjectOfType<PerfectCullingMaterialSettings>();
			}
			return perfectCullingMaterialSettings_0;
		}
	}

	public static bool IsMaterialForcedOpaque(Material mat)
	{
		if ((bool)Instance)
		{
			return Array.Find(perfectCullingMaterialSettings_0._forceOpaqueMaterials, (Material x) => mat == x);
		}
		return false;
	}

	public static bool IsMaterialForcedTransparent(Material mat)
	{
		if ((bool)Instance)
		{
			return Array.Find(perfectCullingMaterialSettings_0._forceTransparentMaterials, (Material x) => mat == x);
		}
		return false;
	}
}
