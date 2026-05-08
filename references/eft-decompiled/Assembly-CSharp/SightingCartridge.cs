using System.Threading.Tasks;
using Comfort.Common;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SightingCartridge : MonoBehaviour
{
	public Transform WeaponHierarchy;

	private Transform transform_0;

	private Vector3 vector3_0;

	private LineRenderer lineRenderer_0;

	public float GizmoSize = 0.1f;

	private GameObject gameObject_0;

	private static readonly int int_0 = Shader.PropertyToID("_Color");

	public async Task Start()
	{
		IOperation<IAssetsManager> operation = await GClass1805.CreateAssetsManager();
		if (!operation.Failed)
		{
			AssetsManagerSingletonClass.Manager = operation.Result;
			lineRenderer_0 = GetComponent<LineRenderer>();
			lineRenderer_0.startWidth = 0.01f;
			lineRenderer_0.endWidth = 0.05f;
			gameObject_0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			gameObject_0.transform.localScale = new Vector3(GizmoSize, GizmoSize, GizmoSize);
			gameObject_0.GetComponent<Renderer>().material.SetColor(int_0, Color.red);
			Object.Destroy(gameObject_0.GetComponent<Collider>());
		}
	}

	public void Update()
	{
		if (WeaponHierarchy != null)
		{
			method_0(WeaponHierarchy);
			WeaponHierarchy = null;
		}
		if (transform_0 != null)
		{
			lineRenderer_0.SetPosition(0, transform_0.position);
			lineRenderer_0.SetPosition(1, transform_0.position - transform_0.up * 600f);
			Vector3 direction = -transform_0.up;
			if (Physics.Raycast(transform_0.position, direction, out var hitInfo, 600f))
			{
				gameObject_0.transform.position = hitInfo.point;
				gameObject_0.transform.localScale = new Vector3(GizmoSize, GizmoSize, GizmoSize);
			}
		}
	}

	public void method_0(Transform hier)
	{
		transform_0 = TransformHelperClass.FindTransformRecursive(hier, "fireport");
	}
}
