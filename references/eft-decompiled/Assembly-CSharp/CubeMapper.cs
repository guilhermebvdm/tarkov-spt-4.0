using UnityEngine;

public class CubeMapper : MonoBehaviour
{
	[SerializeField]
	protected string _weaponShaderNameToReplace = "p0/Reflective/Bumped Specular SMap";

	protected ShaderDataReplacer _sdr;

	private static readonly int int_0 = Shader.PropertyToID("_Cube");

	private static readonly int int_1 = Shader.PropertyToID("_ReflectColor");

	private static readonly int int_2 = Shader.PropertyToID("_Specularness");

	private static readonly int int_3 = Shader.PropertyToID("_Glossness");

	public virtual void Update()
	{
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			method_0();
		}
	}

	public void method_0()
	{
		_sdr = GClass870.FindUnityObjectOfType<ShaderDataReplacer>();
		if (_sdr == null)
		{
			return;
		}
		Renderer[] componentsInChildren = base.transform.parent.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer.name != "MuzzleJetCombinedMesh"))
			{
				continue;
			}
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				if (material.shader.name == _sdr._shaderNameToReplaceData || material.shader.name == "Rain/Bumped Specular SMap Wet")
				{
					material.SetTexture(int_0, _sdr._cubemap);
					material.SetColor(int_1, _sdr._reflectColor);
					material.SetFloat(int_2, _sdr._specularness);
					material.SetFloat(int_3, _sdr._glossness);
				}
			}
		}
	}
}
