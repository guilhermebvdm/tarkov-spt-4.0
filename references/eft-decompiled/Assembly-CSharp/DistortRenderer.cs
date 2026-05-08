using UnityEngine;

public class DistortRenderer : MonoBehaviour
{
	private Renderer renderer_0;

	private Material material_0;

	public Renderer Renderer
	{
		get
		{
			if (renderer_0 != null)
			{
				return renderer_0;
			}
			method_0();
			return renderer_0;
		}
	}

	public Material MeterialToRender
	{
		get
		{
			if (material_0 != null)
			{
				return material_0;
			}
			method_0();
			return material_0;
		}
	}

	public void Start()
	{
		if (renderer_0 == null || material_0 == null)
		{
			method_0();
		}
	}

	public void method_0()
	{
		renderer_0 = GetComponent<Renderer>();
		renderer_0.enabled = GetComponent<ParticleSystem>() != null;
		Material material = new Material(GClass872.Find("Hidden/ClipAll"));
		material.hideFlags = HideFlags.HideAndDontSave;
		material_0 = new Material(renderer_0.sharedMaterial);
		renderer_0.sharedMaterial = material;
		DistortCameraFX.AddRenderer(this);
	}

	public void OnDestroy()
	{
		DistortCameraFX.RemoveRenderer(this);
	}

	public void OnEnable()
	{
		DistortCameraFX.AddRenderer(this);
	}

	public void OnDisable()
	{
		DistortCameraFX.RemoveRenderer(this);
	}
}
