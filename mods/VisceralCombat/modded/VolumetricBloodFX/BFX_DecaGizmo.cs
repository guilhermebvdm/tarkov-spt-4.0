using UnityEngine;
using Object = UnityEngine.Object;

public class BFX_DecaGizmo : MonoBehaviour
{
	private Transform t;

	private void OnDrawGizmos()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)t == (Object)null)
		{
			t = ((Component)this).transform;
		}
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.15f);
		Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
		Gizmos.DrawCube(Vector3.zero, Vector3.one);
		Gizmos.color = new Color(0.19215687f, 8f / 15f, 1f, 0.95f);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.color = new Color(0.95f, 0.2f, 0.2f, 0.85f);
		Gizmos.DrawRay(t.position, t.up);
	}
}
