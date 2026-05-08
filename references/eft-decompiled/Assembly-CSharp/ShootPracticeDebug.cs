using UnityEngine;

public class ShootPracticeDebug : MonoBehaviour
{
	public bool AlwaysDraw = true;

	public Color color = Color.yellow;

	public void OnDrawGizmosSelected()
	{
		if (!AlwaysDraw)
		{
			method_0();
		}
	}

	public void OnDrawGizmos()
	{
		if (AlwaysDraw)
		{
			method_0();
		}
	}

	public void method_0()
	{
		Gizmos.color = color;
		Gizmos.DrawSphere(base.transform.position, 0.2f);
		Gizmos.DrawWireSphere(base.transform.position, 0.21f);
	}
}
