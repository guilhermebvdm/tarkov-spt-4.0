using UnityEngine;

public class SpeedTreeTest : MonoBehaviour
{
	public Shader BShader;

	public void Start()
	{
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		int subMeshCount = componentsInChildren[0].sharedMesh.subMeshCount;
		Mesh sharedMesh = GClass976.Combine(componentsInChildren, Vector3.zero, subMeshCount);
		base.gameObject.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
		Material[] sharedMaterials = componentsInChildren[0].GetComponent<Renderer>().sharedMaterials;
		base.gameObject.AddComponent<MeshRenderer>().sharedMaterials = sharedMaterials;
		Material[] array = sharedMaterials;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].shader = BShader;
		}
		MeshFilter[] array2 = componentsInChildren;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].gameObject.SetActive(value: false);
		}
	}

	public static void smethod_0(Mesh[] meshes, Renderer renderer = null)
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			smethod_1(meshes[i], "submesh " + i, renderer, i);
		}
	}

	public static GameObject smethod_1(Mesh mesh, string name, Renderer renderer = null, int submesh = -1)
	{
		GameObject obj = new GameObject(name);
		obj.AddComponent<MeshFilter>().sharedMesh = mesh;
		MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
		if (renderer != null)
		{
			if (submesh == -1)
			{
				meshRenderer.sharedMaterials = renderer.sharedMaterials;
				return obj;
			}
			meshRenderer.sharedMaterial = renderer.sharedMaterials[submesh];
		}
		return obj;
	}
}
