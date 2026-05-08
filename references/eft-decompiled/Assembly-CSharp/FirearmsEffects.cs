using System;
using UnityEngine;

public class FirearmsEffects : MonoBehaviour
{
	protected MuzzleManager _muzzleManager;

	public GameObject[] Jets
	{
		get
		{
			if (!(_muzzleManager != null))
			{
				return Array.Empty<GameObject>();
			}
			return _muzzleManager.MuzzleJets;
		}
	}

	public void StartFireEffects(bool isVisible, float sqrCameraDistance)
	{
		if (base.enabled && EFTHardSettings.Instance.SHOT_EFFECTS_ENABLED)
		{
			_muzzleManager.Shot(isVisible, sqrCameraDistance);
		}
	}

	public void StartLauncherEffects()
	{
		if (base.enabled && EFTHardSettings.Instance.SHOT_EFFECTS_ENABLED)
		{
			_muzzleManager.LauncherShot();
		}
	}

	public void Init(Transform hierarchy)
	{
		_muzzleManager = GClass6.GetComponentInChildrenActiveIgnoreFirstLevel<MuzzleManager>(base.gameObject.transform);
		if (_muzzleManager != null)
		{
			_muzzleManager.Hierarchy = hierarchy;
			if (_muzzleManager.JetMaterial != null)
			{
				_muzzleManager.JetMaterial = new Material(_muzzleManager.JetMaterial);
			}
			else
			{
				Debug.LogError("JetMaterial == null");
			}
		}
		UpdateMuzzle();
	}

	public void UpdateMuzzle()
	{
		if (base.enabled && !(_muzzleManager == null))
		{
			_muzzleManager.method_4();
			_muzzleManager.UpdateJetsAndFumes();
		}
	}
}
