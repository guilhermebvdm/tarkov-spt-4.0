using System;
using UnityEngine;

namespace MirzaBeig.Shaders.ImageEffects;

[Serializable]
[ExecuteInEditMode]
public class Sharpen : IEBase
{
	[Range(-2f, 2f)]
	public float strength = 0.5f;

	[Range(0f, 8f)]
	public float edgeMult = 0.2f;

	private static readonly int _strength = Shader.PropertyToID("_strength");

	private static readonly int _edgeMult = Shader.PropertyToID("_edgeMult");

	public new void Awake()
	{
		base.shader = GClass872.Find("Hidden/Mirza Beig/Image Effects/Sharpen");
	}

	public new void Start()
	{
	}

	public new void Update()
	{
	}

	public new void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetFloat(_strength, strength);
		base.material.SetFloat(_edgeMult, edgeMult);
		blit(source, destination);
	}
}
