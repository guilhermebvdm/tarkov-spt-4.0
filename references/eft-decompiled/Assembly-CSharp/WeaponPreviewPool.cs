using System.Collections.Generic;
using System.Linq;
using EFT.UI.WeaponModding;
using JetBrains.Annotations;
using UnityEngine;

public class WeaponPreviewPool : MonoBehaviour
{
	[SerializeField]
	private WeaponPreview _weaponPreviewTemplate;

	[SerializeField]
	private int _count = 8;

	private readonly List<WeaponPreview> list_0 = new List<WeaponPreview>();

	public void Awake()
	{
		for (int i = 0; i < _count; i++)
		{
			WeaponPreview weaponPreview = Object.Instantiate(_weaponPreviewTemplate, base.transform, worldPositionStays: false);
			weaponPreview.name = $"WeaponPreview_{i + 1}";
			weaponPreview.transform.localPosition = new Vector3(0f, 10f * (float)(i + 1), 0f);
			list_0.Add(weaponPreview);
			weaponPreview.gameObject.SetActive(value: false);
		}
	}

	[CanBeNull]
	public WeaponPreview GetWeaponPreview()
	{
		WeaponPreview weaponPreview = list_0.FirstOrDefault();
		if (weaponPreview == null)
		{
			Debug.LogError("Preview is null");
			return null;
		}
		list_0.Remove(weaponPreview);
		weaponPreview.Init();
		return weaponPreview;
	}

	public void ReturnToPool(WeaponPreview weaponPreview)
	{
		weaponPreview.gameObject.SetActive(value: false);
		list_0.Add(weaponPreview);
	}
}
