using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class CustomLight : MonoBehaviour, GInterface51
{
	private Renderer renderer_0;

	public void Awake()
	{
		renderer_0 = GetComponent<Renderer>();
		renderer_0.enabled = false;
		LightManager.Add(this);
	}

	public Renderer method_0()
	{
		if (renderer_0 != null)
		{
			return renderer_0;
		}
		renderer_0 = GetComponent<Renderer>();
		return renderer_0;
	}

	public void OnEnable()
	{
		LightManager.Add(this);
	}

	public void OnDisable()
	{
		LightManager.Remove(this);
	}

	public void OnDestroy()
	{
		OnDisable();
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawIcon(base.transform.position, GClass842.True ? "AreaLight Gizmo" : "PointLight Gizmo", allowScaling: true);
	}

	public void Draw(CommandBuffer buffer, Plane[] frustrumPlanes, Camera cam)
	{
		if (GeometryUtility.TestPlanesAABB(frustrumPlanes, renderer_0.bounds))
		{
			buffer.DrawRenderer(renderer_0, renderer_0.sharedMaterial);
		}
	}
}
