using System;
using UnityEngine;

namespace PlayerIcons;

[Serializable]
public class MeshMaterialSetting
{
	public MeshType meshType;

	public bool isChanged;

	[Range(0.01f, 10f)]
	public float specularityMesh;

	[Range(0.01f, 10f)]
	public float glossnessMesh;
}
