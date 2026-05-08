using System.Runtime.CompilerServices;
using UnityEngine;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[GAttribute8(-100)]
public class StencilShadow : MonoBehaviour
{
	public Color Ambient = new Color(0f, 0f, 0f, 1f);

	public AmbientLight.CullingSettings Culling;

	[CompilerGenerated]
	private Renderer renderer_0;

	[CompilerGenerated]
	private Bounds bounds_0;

	public int DesiredDrawPriority = -1;

	[Range(0f, 1f)]
	public float FogAttenuation;

	[SerializeField]
	[HideInInspector]
	private int _drawPriority = -1;

	private Mesh mesh_0;

	public Renderer Renderer
	{
		[CompilerGenerated]
		get
		{
			return renderer_0;
		}
		[CompilerGenerated]
		set
		{
			renderer_0 = value;
		}
	}

	public virtual Bounds Bounds
	{
		[CompilerGenerated]
		get
		{
			return bounds_0;
		}
		[CompilerGenerated]
		set
		{
			bounds_0 = value;
		}
	}

	public int ActualDrawPriority => _drawPriority;

	[ContextMenu("Awake")]
	public void Awake()
	{
		if (Culling != null)
		{
			Culling.Update();
		}
		MeshFilter component = GetComponent<MeshFilter>();
		if (component != null)
		{
			mesh_0 = component.sharedMesh;
		}
		Renderer = GetComponent<Renderer>();
		if (!(Renderer == null))
		{
			Bounds = Renderer.bounds;
			if (base.isActiveAndEnabled)
			{
				AmbientLight.AddStencilShadow(this);
			}
		}
	}

	public void OnEnable()
	{
		Awake();
	}

	public void OnDisable()
	{
		AmbientLight.RemoveStencilShadow(this);
	}

	public void OnValidate()
	{
		Awake();
	}

	public void OnDestroy()
	{
		AmbientLight.RemoveStencilShadow(this);
	}
}
