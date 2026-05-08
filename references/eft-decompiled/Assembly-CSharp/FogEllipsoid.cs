using UnityEngine;

[ExecuteInEditMode]
public class FogEllipsoid : MonoBehaviour
{
	public enum Blend
	{
		Additive,
		Multiplicative
	}

	public Blend m_Blend;

	public float m_Density = 1f;

	[GAttribute13(0f)]
	public float m_Radius = 1f;

	[GAttribute13(0f)]
	public float m_Stretch = 2f;

	[Range(0f, 1f)]
	public float m_Feather = 0.7f;

	[Range(0f, 1f)]
	public float m_NoiseAmount;

	public float m_NoiseSpeed = 1f;

	[GAttribute13(0f)]
	public float m_NoiseScale = 1f;

	private bool bool_0;

	public void method_0()
	{
		if (!bool_0)
		{
			bool_0 = LightManager<FogEllipsoid>.Add(this);
		}
	}

	public void OnEnable()
	{
		method_0();
	}

	public void Update()
	{
		method_0();
	}

	public void OnDisable()
	{
		LightManager<FogEllipsoid>.Remove(this);
		bool_0 = false;
	}

	public void OnDrawGizmosSelected()
	{
		Matrix4x4 identity = Matrix4x4.identity;
		Transform transform = base.transform;
		identity.SetTRS(transform.position, transform.rotation, new Vector3(1f, m_Stretch, 1f));
		Gizmos.matrix = identity;
		Gizmos.DrawWireSphere(Vector3.zero, m_Radius);
	}
}
