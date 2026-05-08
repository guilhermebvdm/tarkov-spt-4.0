using UnityEngine;

public class WeaponCubeMapper : CubeMapper
{
	private float float_0;

	private static readonly int int_4 = Shader.PropertyToID("_WeaponDirection");

	private static readonly int int_5 = Shader.PropertyToID("_WeaponDelta");

	public override void Update()
	{
		float num = Vector3.Dot(-base.transform.up, -Vector3.up);
		Shader.SetGlobalFloat(int_4, num);
		Shader.SetGlobalFloat(int_5, num - float_0);
		float_0 = num;
		base.Update();
	}
}
