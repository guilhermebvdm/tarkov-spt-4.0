using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
public class RendererSelectorTest : MonoBehaviour
{
	private Collider collider_0;

	private Plane[] plane_0 = new Plane[6];

	[SerializeField]
	private bool _updateRendersEveryFrame;

	[SerializeField]
	private bool _updatePlanesEveryFrame = true;

	[SerializeField]
	private bool _updateSelectionEveryFrame;

	private ICollection<MeshRenderer> icollection_0;

	[SerializeField]
	private List<MeshRenderer> _selectedRenderers = new List<MeshRenderer>();

	public void Awake()
	{
		collider_0 = GetComponent<Collider>();
	}

	public void Update()
	{
		if (!(collider_0 == null))
		{
			if (_updateRendersEveryFrame)
			{
				method_0();
			}
			if (_updatePlanesEveryFrame)
			{
				method_1();
			}
			if (_updateSelectionEveryFrame)
			{
				method_2();
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
		foreach (MeshRenderer selectedRenderer in _selectedRenderers)
		{
			Gizmos.DrawCube(selectedRenderer.bounds.center, selectedRenderer.bounds.size);
		}
	}

	public void method_0()
	{
		icollection_0 = GClass842.GetAllComponentsOfType<MeshRenderer>(includeInactive: false);
	}

	public void method_1()
	{
		Bounds colliderBoundsWithoutRotation = GClass842.GetColliderBoundsWithoutRotation(collider_0);
		Quaternion rotation = collider_0.transform.rotation;
		GClass823.ConvertBoundsToPlanesNonAlloc(colliderBoundsWithoutRotation, rotation, normalsIn: true, plane_0);
	}

	public void method_2()
	{
		_selectedRenderers.Clear();
		foreach (MeshRenderer item in icollection_0)
		{
			if (GeometryUtility.TestPlanesAABB(plane_0, item.bounds))
			{
				_selectedRenderers.Add(item);
			}
		}
	}
}
