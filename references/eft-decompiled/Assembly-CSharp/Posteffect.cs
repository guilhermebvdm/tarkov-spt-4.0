using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class Posteffect : MonoBehaviour
{
	[SerializeField]
	protected Shader EffectShader;

	[SerializeField]
	protected string EffectValueName = "Effect amount";

	[Range(0f, 1f)]
	[SerializeField]
	private float _effectAmount;

	protected Material EffectMaterial;

	public float EffectAmount
	{
		get
		{
			return _effectAmount;
		}
		set
		{
			_effectAmount = Mathf.Clamp01(value);
		}
	}

	public virtual void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (EffectShader == null)
		{
			Debug.Log("Post effect shader is not set up!");
			base.enabled = false;
		}
		else if (!EffectShader.isSupported)
		{
			Debug.Log(EffectShader.name + " is not supported");
			base.enabled = false;
		}
		EffectMaterial = new Material(EffectShader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destanation)
	{
		if (Math.Abs(EffectAmount) < Mathf.Epsilon)
		{
			GClass860.BlitOrCopy(source, destanation);
			return;
		}
		EffectMaterial.SetFloat(EffectValueName, EffectAmount);
		Graphics.Blit(source, destanation, EffectMaterial);
	}
}
