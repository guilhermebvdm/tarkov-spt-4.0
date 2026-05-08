using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using PlayerIcons;
using UnityEngine;

public class SonicInfoClass
{
	[CompilerGenerated]
	public class Class736
	{
		public MeshType meshType;

		public bool method_0(MeshMaterialSetting m)
		{
			return m.meshType == meshType;
		}
	}

	[NonSerialized]
	public int Int_0 = Shader.PropertyToID("_Specularness");

	[NonSerialized]
	public int Int_1 = Shader.PropertyToID("_Glossness");

	[NonSerialized]
	public List<Material> List_0 = new List<Material>();

	[NonSerialized]
	public PlayerIconCreatorSettings PlayerIconCreatorSettings_0;

	public SonicInfoClass(PlayerIconCreatorSettings settings)
	{
		PlayerIconCreatorSettings_0 = settings;
	}

	public void ChangeMaterials(PlayerBody playerBody)
	{
		for (int i = 0; i < playerBody.MeshTransform.childCount; i++)
		{
			Transform child = playerBody.MeshTransform.GetChild(i);
			MeshType meshType = (child.name.Contains("head") ? MeshType.Face : MeshType.Body);
			method_0(child, meshType);
		}
		method_0(playerBody.PlayerBones.Head.Original, MeshType.Helmet);
		method_0(playerBody.PlayerBones.Weapon_Root_Anim, MeshType.Weapon);
	}

	public void method_0(Transform child, MeshType meshType)
	{
		MeshMaterialSetting meshMaterialSetting = PlayerIconCreatorSettings_0.meshMaterialSettings.FirstOrDefault((MeshMaterialSetting m) => m.meshType == meshType);
		if (!meshMaterialSetting.isChanged)
		{
			return;
		}
		Renderer[] componentsInChildren = child.GetComponentsInChildren<Renderer>();
		foreach (Renderer obj in componentsInChildren)
		{
			List_0.Clear();
			obj.GetMaterials(List_0);
			foreach (Material item in List_0)
			{
				item.SetFloat(Int_1, meshMaterialSetting.specularityMesh);
				item.SetFloat(Int_0, meshMaterialSetting.glossnessMesh);
			}
		}
	}
}
