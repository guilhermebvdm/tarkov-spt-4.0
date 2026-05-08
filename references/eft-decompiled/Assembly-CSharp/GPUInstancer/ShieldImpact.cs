using UnityEngine;

namespace GPUInstancer;

public class ShieldImpact : MonoBehaviour
{
	private float float_0;

	private Material material_0;

	private static readonly int int_0 = Shader.PropertyToID("_ImpactTime");

	private static readonly int int_1 = Shader.PropertyToID("_ImpactPosition");

	public void Awake()
	{
		material_0 = base.transform.Find("ImpactShield").GetComponent<MeshRenderer>().material;
	}

	public void Update()
	{
		if (float_0 > 0f)
		{
			float_0 -= Time.deltaTime * 1000f;
			if (float_0 < 0f)
			{
				float_0 = 0f;
			}
			material_0.SetFloat(int_0, float_0);
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		ContactPoint[] contacts = collision.contacts;
		foreach (ContactPoint contactPoint in contacts)
		{
			material_0.SetVector(int_1, base.transform.InverseTransformPoint(contactPoint.point));
			float_0 = 500f;
			material_0.SetFloat(int_0, float_0);
		}
	}
}
