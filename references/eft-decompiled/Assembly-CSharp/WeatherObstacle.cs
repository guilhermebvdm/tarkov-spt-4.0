using System.Runtime.CompilerServices;
using EFT.EnvironmentEffect;
using UnityEngine;

[GAttribute8(-100)]
[DisallowMultipleComponent]
public class WeatherObstacle : MonoBehaviour
{
	private static WeatherObstacle weatherObstacle_0;

	[CompilerGenerated]
	private MeshCollider meshCollider_0;

	private Mesh mesh_0;

	public static WeatherObstacle Instance
	{
		get
		{
			return weatherObstacle_0;
		}
		set
		{
			if (weatherObstacle_0 == null)
			{
				weatherObstacle_0 = value;
			}
			else if ((object)weatherObstacle_0 != value)
			{
				Debug.LogError("WeatherObstacle instance already set to " + weatherObstacle_0.GetType().Name);
			}
		}
	}

	public MeshCollider MeshCollider
	{
		[CompilerGenerated]
		get
		{
			return meshCollider_0;
		}
		[CompilerGenerated]
		set
		{
			meshCollider_0 = value;
		}
	}

	public void Awake()
	{
		mesh_0 = method_0();
		MeshCollider = GClass6.GetOrAddComponent<MeshCollider>(base.gameObject);
		MeshCollider.sharedMesh = mesh_0;
		method_1();
	}

	public void Start()
	{
		Instance = this;
	}

	public Mesh method_0()
	{
		DryPlane[] componentsInChildren = base.gameObject.GetComponentsInChildren<DryPlane>();
		CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			array[i] = new CombineInstance
			{
				mesh = componentsInChildren[i].Mesh,
				transform = componentsInChildren[i].transform.localToWorldMatrix
			};
		}
		Mesh mesh = new Mesh();
		mesh.name = "WeatherObstacle GetMesh";
		mesh.CombineMeshes(array, mergeSubMeshes: true, useMatrices: true);
		return mesh;
	}

	public void method_1()
	{
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			Object.Destroy(base.transform.GetChild(num).gameObject);
		}
	}

	[ContextMenu("Combine")]
	public void method_2()
	{
		weatherObstacle_0 = this;
		mesh_0 = method_0();
		MeshCollider = GClass6.GetOrAddComponent<MeshCollider>(base.gameObject);
		MeshCollider.sharedMesh = mesh_0;
	}

	public void OnDestroy()
	{
		if (MeshCollider != null)
		{
			Object.Destroy(MeshCollider);
		}
		if ((object)weatherObstacle_0 == this)
		{
			weatherObstacle_0 = null;
		}
	}
}
