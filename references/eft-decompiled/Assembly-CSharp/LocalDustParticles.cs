using UnityEngine;

[ExecuteInEditMode]
public class LocalDustParticles : MonoBehaviour
{
	public LocalDustParticlesParent PurticlesParent;

	public int Count = 100;

	public Color ParticlesColor = Color.white;

	public float AlphaRandomness;

	public float SizeRandomness;

	public float SideSpeed;

	private Matrix4x4 matrix4x4_0;

	private int int_0;

	private float float_0;

	private Color color_0;

	private float float_1;

	private float float_2;

	public void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Color particlesColor = ParticlesColor;
		particlesColor.a *= 0.1f;
		Gizmos.color = particlesColor;
		Gizmos.DrawCube(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));
		particlesColor.a = 0.5f;
		Gizmos.color = particlesColor;
		Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f));
		if (!(PurticlesParent == null) && (matrix4x4_0 != base.transform.localToWorldMatrix || int_0 != Count || float_0 != SideSpeed || color_0 != ParticlesColor || float_1 != AlphaRandomness || float_2 != SizeRandomness))
		{
			color_0 = ParticlesColor;
			float_0 = SideSpeed;
			int_0 = Count;
			matrix4x4_0 = base.transform.localToWorldMatrix;
			float_1 = AlphaRandomness;
			float_2 = SizeRandomness;
			PurticlesParent.GenerateMesh();
		}
	}

	public void OnEnable()
	{
	}
}
