using System;
using System.Runtime.InteropServices;
using EFT;
using UnityEngine;

[Serializable]
[StructLayout(LayoutKind.Auto)]
public struct SeasonGrass
{
	[SerializeField]
	[HideInInspector]
	public ResourceKey _textureKey;

	[SerializeField]
	[HideInInspector]
	public ResourceKey _bumpKey;

	public Color DetailHealthyColor;

	public Color DetailDryColor;

	public Color WindWaveTintColor;

	public ResourceKey TextureKey => _textureKey;

	public ResourceKey BumpKey => _bumpKey;
}
