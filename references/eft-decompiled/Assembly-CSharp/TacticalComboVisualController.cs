using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

public class TacticalComboVisualController : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class431
	{
		public static readonly Class431 class431_0 = new Class431();

		public static Predicate<Transform> predicate_0;

		public bool method_0(Transform mod)
		{
			return mod.name == "mode_000";
		}
	}

	public const string DISABLED_TRANSFORM_NAME = "mode_000";

	public const string LIGHT_BEAM_TRANSFORM_NAME = "mode_";

	private readonly List<Transform> list_0 = new List<Transform>();

	public LightComponent LightMod;

	private Transform transform_0;

	[SerializeField]
	private float _shadowNearPlaneShift = 0.05f;

	private Light[] light_0;

	private LaserBeam[] laserBeam_0;

	public void Awake()
	{
		Init();
	}

	public void Init()
	{
		light_0 = GetComponentsInChildren<Light>(includeInactive: true);
		laserBeam_0 = GetComponentsInChildren<LaserBeam>(includeInactive: true);
		if (list_0.Count != 0)
		{
			return;
		}
		List<Transform> list = TransformHelperClass.smethod_4(base.transform, "mode_", false);
		if (list != null && list.Count != 0)
		{
			transform_0 = list.Find((Transform mod) => mod.name == "mode_000");
			if (transform_0 == null)
			{
				Debug.LogErrorFormat(base.transform, "There is no {0} in {1}", "mode_000", base.transform.name);
			}
			else
			{
				list.Remove(transform_0);
				list_0.AddRange(list);
			}
		}
		else
		{
			Debug.LogErrorFormat(base.transform, "There is no '{0}XXX' transforms in {1}", "mode_", base.transform.name);
		}
	}

	public void UpdateBeams(bool isYourPlayer = false)
	{
		if (LightMod != null)
		{
			if (transform_0 != null)
			{
				transform_0.gameObject.SetActive(!LightMod.IsActive);
			}
			for (int i = 0; i < list_0.Count; i++)
			{
				list_0[i].gameObject.SetActive(LightMod.IsActive && LightMod.SelectedMode == i);
			}
			for (int j = 0; j < laserBeam_0.Length; j++)
			{
				laserBeam_0[j].SetOwner(isYourPlayer);
			}
		}
	}

	public void SetPointOfView(EPointOfView pointOfView)
	{
	}
}
