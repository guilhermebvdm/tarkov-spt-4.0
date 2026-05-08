using System.Collections;
using UnityEngine;

public class CultistSignEffect : HalloweenEffect
{
	private static readonly int int_0 = Shader.PropertyToID("_Dissolve");

	private MaterialPropertyBlock materialPropertyBlock_0;

	private MeshRenderer meshRenderer_0;

	private bool bool_0;

	[SerializeField]
	[Range(0f, 1f)]
	private float _dissolvePower;

	[SerializeField]
	[Range(1f, 5f)]
	private float _interpolateTime = 1f;

	public void Initialize()
	{
		meshRenderer_0 = GetComponent<MeshRenderer>();
		materialPropertyBlock_0 = new MaterialPropertyBlock();
		method_0();
	}

	public void SetValue(bool value)
	{
		bool_0 = value;
	}

	public override IEnumerator RunProcess()
	{
		float a = (bool_0 ? 1 : 0);
		float b = ((!bool_0) ? 1 : 0);
		float num = 0f;
		while (num < _interpolateTime)
		{
			num += Time.deltaTime;
			_dissolvePower = Mathf.Lerp(a, b, Mathf.Clamp01(num / _interpolateTime));
			materialPropertyBlock_0.SetFloat(int_0, _dissolvePower);
			meshRenderer_0.SetPropertyBlock(materialPropertyBlock_0);
			yield return null;
		}
	}

	public void OnDestroy()
	{
		Abort();
		method_0();
	}

	public void method_0()
	{
		_dissolvePower = 1f;
		if (materialPropertyBlock_0 != null)
		{
			materialPropertyBlock_0.SetFloat(int_0, _dissolvePower);
		}
		if (meshRenderer_0 != null)
		{
			meshRenderer_0.SetPropertyBlock(materialPropertyBlock_0);
		}
	}
}
