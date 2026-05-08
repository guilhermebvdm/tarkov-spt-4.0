using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
public class TOD_Components : MonoBehaviour
{
	public GameObject Sun;

	public GameObject Moon;

	public GameObject Atmosphere;

	public GameObject Clear;

	public GameObject Space;

	public GameObject Light;

	[CompilerGenerated]
	private Transform transform_0;

	[CompilerGenerated]
	private Transform transform_1;

	[CompilerGenerated]
	private Transform transform_2;

	[CompilerGenerated]
	private Transform transform_3;

	[CompilerGenerated]
	private Transform transform_4;

	[CompilerGenerated]
	private Renderer renderer_0;

	[CompilerGenerated]
	private Renderer renderer_1;

	[CompilerGenerated]
	private Renderer renderer_2;

	[CompilerGenerated]
	private Renderer renderer_3;

	[CompilerGenerated]
	private Renderer renderer_4;

	[CompilerGenerated]
	private Renderer renderer_5;

	[CompilerGenerated]
	private MeshFilter meshFilter_0;

	[CompilerGenerated]
	private MeshFilter meshFilter_1;

	[CompilerGenerated]
	private MeshFilter meshFilter_2;

	[CompilerGenerated]
	private MeshFilter meshFilter_3;

	[CompilerGenerated]
	private MeshFilter meshFilter_4;

	[CompilerGenerated]
	private MeshFilter meshFilter_5;

	[CompilerGenerated]
	private Material material_0;

	[CompilerGenerated]
	private Material material_1;

	[CompilerGenerated]
	private Material material_2;

	[CompilerGenerated]
	private Material material_3;

	[CompilerGenerated]
	private Material material_4;

	[CompilerGenerated]
	private Material material_5;

	[CompilerGenerated]
	private Light light_0;

	[CompilerGenerated]
	private TOD_Sky tod_Sky_0;

	[CompilerGenerated]
	private TOD_Time tod_Time_0;

	[CompilerGenerated]
	private TOD_Camera tod_Camera_0;

	[CompilerGenerated]
	private TOD_Rays tod_Rays_0;

	[CompilerGenerated]
	private TOD_Scattering tod_Scattering_0;

	[CompilerGenerated]
	private MBOIT_Scattering mboit_Scattering_0;

	public Transform DomeTransform
	{
		[CompilerGenerated]
		get
		{
			return transform_0;
		}
		[CompilerGenerated]
		set
		{
			transform_0 = value;
		}
	}

	public Transform SunTransform
	{
		[CompilerGenerated]
		get
		{
			return transform_1;
		}
		[CompilerGenerated]
		set
		{
			transform_1 = value;
		}
	}

	public Transform MoonTransform
	{
		[CompilerGenerated]
		get
		{
			return transform_2;
		}
		[CompilerGenerated]
		set
		{
			transform_2 = value;
		}
	}

	public Transform LightTransform
	{
		[CompilerGenerated]
		get
		{
			return transform_3;
		}
		[CompilerGenerated]
		set
		{
			transform_3 = value;
		}
	}

	public Transform SpaceTransform
	{
		[CompilerGenerated]
		get
		{
			return transform_4;
		}
		[CompilerGenerated]
		set
		{
			transform_4 = value;
		}
	}

	public Renderer SpaceRenderer
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

	public Renderer AtmosphereRenderer
	{
		[CompilerGenerated]
		get
		{
			return renderer_1;
		}
		[CompilerGenerated]
		set
		{
			renderer_1 = value;
		}
	}

	public Renderer ClearRenderer
	{
		[CompilerGenerated]
		get
		{
			return renderer_2;
		}
		[CompilerGenerated]
		set
		{
			renderer_2 = value;
		}
	}

	public Renderer CloudRenderer
	{
		[CompilerGenerated]
		get
		{
			return renderer_3;
		}
		[CompilerGenerated]
		set
		{
			renderer_3 = value;
		}
	}

	public Renderer SunRenderer
	{
		[CompilerGenerated]
		get
		{
			return renderer_4;
		}
		[CompilerGenerated]
		set
		{
			renderer_4 = value;
		}
	}

	public Renderer MoonRenderer
	{
		[CompilerGenerated]
		get
		{
			return renderer_5;
		}
		[CompilerGenerated]
		set
		{
			renderer_5 = value;
		}
	}

	public MeshFilter SpaceMeshFilter
	{
		[CompilerGenerated]
		get
		{
			return meshFilter_0;
		}
		[CompilerGenerated]
		set
		{
			meshFilter_0 = value;
		}
	}

	public MeshFilter AtmosphereMeshFilter
	{
		[CompilerGenerated]
		get
		{
			return meshFilter_1;
		}
		[CompilerGenerated]
		set
		{
			meshFilter_1 = value;
		}
	}

	public MeshFilter ClearMeshFilter
	{
		[CompilerGenerated]
		get
		{
			return meshFilter_2;
		}
		[CompilerGenerated]
		set
		{
			meshFilter_2 = value;
		}
	}

	public MeshFilter CloudMeshFilter
	{
		[CompilerGenerated]
		get
		{
			return meshFilter_3;
		}
		[CompilerGenerated]
		set
		{
			meshFilter_3 = value;
		}
	}

	public MeshFilter SunMeshFilter
	{
		[CompilerGenerated]
		get
		{
			return meshFilter_4;
		}
		[CompilerGenerated]
		set
		{
			meshFilter_4 = value;
		}
	}

	public MeshFilter MoonMeshFilter
	{
		[CompilerGenerated]
		get
		{
			return meshFilter_5;
		}
		[CompilerGenerated]
		set
		{
			meshFilter_5 = value;
		}
	}

	public Material SpaceMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_0;
		}
		[CompilerGenerated]
		set
		{
			material_0 = value;
		}
	}

	public Material AtmosphereMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_1;
		}
		[CompilerGenerated]
		set
		{
			material_1 = value;
		}
	}

	public Material ClearMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_2;
		}
		[CompilerGenerated]
		set
		{
			material_2 = value;
		}
	}

	public Material CloudMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_3;
		}
		[CompilerGenerated]
		set
		{
			material_3 = value;
		}
	}

	public Material SunMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_4;
		}
		[CompilerGenerated]
		set
		{
			material_4 = value;
		}
	}

	public Material MoonMaterial
	{
		[CompilerGenerated]
		get
		{
			return material_5;
		}
		[CompilerGenerated]
		set
		{
			material_5 = value;
		}
	}

	public Light LightSource
	{
		[CompilerGenerated]
		get
		{
			return light_0;
		}
		[CompilerGenerated]
		set
		{
			light_0 = value;
		}
	}

	public TOD_Sky Sky
	{
		[CompilerGenerated]
		get
		{
			return tod_Sky_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Sky_0 = value;
		}
	}

	public TOD_Time Time
	{
		[CompilerGenerated]
		get
		{
			return tod_Time_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Time_0 = value;
		}
	}

	public TOD_Camera Camera
	{
		[CompilerGenerated]
		get
		{
			return tod_Camera_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Camera_0 = value;
		}
	}

	public TOD_Rays Rays
	{
		[CompilerGenerated]
		get
		{
			return tod_Rays_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Rays_0 = value;
		}
	}

	public TOD_Scattering Scattering
	{
		[CompilerGenerated]
		get
		{
			return tod_Scattering_0;
		}
		[CompilerGenerated]
		set
		{
			tod_Scattering_0 = value;
		}
	}

	public MBOIT_Scattering MBOITScattering
	{
		[CompilerGenerated]
		get
		{
			return mboit_Scattering_0;
		}
		[CompilerGenerated]
		set
		{
			mboit_Scattering_0 = value;
		}
	}

	public void Initialize()
	{
		DomeTransform = GetComponent<Transform>();
		Sky = GetComponent<TOD_Sky>();
		Time = GetComponent<TOD_Time>();
		if ((bool)Space)
		{
			SpaceTransform = Space.GetComponent<Transform>();
			SpaceRenderer = Space.GetComponent<Renderer>();
			SpaceMaterial = SpaceRenderer.sharedMaterial;
			SpaceMeshFilter = Space.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Space reference not set.");
		}
		if ((bool)Atmosphere)
		{
			AtmosphereRenderer = Atmosphere.GetComponent<Renderer>();
			AtmosphereMaterial = AtmosphereRenderer.sharedMaterial;
			AtmosphereMeshFilter = Atmosphere.GetComponent<MeshFilter>();
			if (AtmosphereMeshFilter != null && AtmosphereMeshFilter.sharedMesh != null)
			{
				AtmosphereMeshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);
			}
		}
		else
		{
			Debug.LogError("Atmosphere reference not set.");
		}
		if ((bool)Clear)
		{
			ClearRenderer = Clear.GetComponent<Renderer>();
			ClearMaterial = ClearRenderer.sharedMaterial;
			ClearMeshFilter = Clear.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Clear reference not set.");
		}
		if ((bool)Light)
		{
			LightTransform = Light.GetComponent<Transform>();
			LightSource = Light.GetComponent<Light>();
		}
		else
		{
			Debug.LogError("Light reference not set.");
		}
		if ((bool)Sun)
		{
			SunTransform = Sun.GetComponent<Transform>();
			SunRenderer = Sun.GetComponent<Renderer>();
			SunMaterial = SunRenderer.sharedMaterial;
			SunMeshFilter = Sun.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Sun reference not set.");
		}
		if ((bool)Moon)
		{
			MoonTransform = Moon.GetComponent<Transform>();
			MoonRenderer = Moon.GetComponent<Renderer>();
			MoonMaterial = MoonRenderer.sharedMaterial;
			MoonMeshFilter = Moon.GetComponent<MeshFilter>();
			if (Application.isPlaying)
			{
				MoonMeshFilter.sharedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
			}
		}
		else
		{
			Debug.LogError("Moon reference not set.");
		}
	}
}
