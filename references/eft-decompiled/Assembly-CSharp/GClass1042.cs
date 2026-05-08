using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GClass1042
{
	public class Class739
	{
		public Vector3[] Vertices;

		public Vector2[] Uv;

		public Vector4[] Tangents;

		public Vector3[] Normals;

		public int[] Triangles;
	}

	[NonSerialized]
	public static Dictionary<Material, Mesh> Dictionary_0 = new Dictionary<Material, Mesh>();

	[NonSerialized]
	public static Dictionary<Material, Class739> Dictionary_1 = new Dictionary<Material, Class739>();

	public static void Add(GameObject obj, bool saveCollider, bool dontDesttroyComponents = true)
	{
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		MeshFilter component2 = obj.GetComponent<MeshFilter>();
		if (!(component == null) && !(component2 == null))
		{
			Mesh sharedMesh = component2.sharedMesh;
			Material sharedMaterial = component.sharedMaterial;
			Mesh mesh;
			Class739 @class;
			if (Dictionary_0.ContainsKey(sharedMaterial) && Dictionary_1.ContainsKey(sharedMaterial))
			{
				mesh = Dictionary_0[sharedMaterial];
				@class = Dictionary_1[sharedMaterial];
			}
			else
			{
				mesh = smethod_0(component);
				Dictionary_0.Add(sharedMaterial, mesh);
				@class = new Class739();
				Dictionary_1.Add(sharedMaterial, @class);
			}
			smethod_3(mesh, sharedMesh, obj.transform.localToWorldMatrix, @class);
			if (dontDesttroyComponents)
			{
				smethod_2(obj, saveCollider);
			}
			else
			{
				smethod_1(obj, saveCollider);
			}
		}
		else
		{
			Debug.LogWarning("Cant bake mesh in " + obj.name + ". Renderer of mesh filter not found");
		}
	}

	public static void Reset()
	{
		Dictionary_0.Clear();
	}

	public static Mesh smethod_0(Renderer renderer)
	{
		GameObject gameObject = new GameObject("Baked_mesh_" + renderer.name);
		gameObject.transform.position = Vector3.zero;
		gameObject.transform.rotation = Quaternion.identity;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		Mesh result = (meshFilter.sharedMesh = new Mesh
		{
			name = "AdditiveMeshBakeManager CreateGameObject"
		});
		meshRenderer.sharedMaterials = renderer.sharedMaterials;
		return result;
	}

	public static void smethod_1(GameObject obj, bool saveCollider)
	{
		Component[] components = obj.GetComponents<Component>();
		if (!saveCollider)
		{
			UnityEngine.Object.Destroy(obj);
			return;
		}
		Component[] array = components;
		foreach (Component component in array)
		{
			if (!(component is Transform) && !(component is Collider))
			{
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	public static void smethod_2(GameObject obj, bool saveCollider)
	{
		if (!saveCollider)
		{
			obj.SetActive(value: false);
			return;
		}
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = true;
		}
		Renderer component2 = obj.GetComponent<Renderer>();
		if (component2 != null)
		{
			component2.enabled = false;
		}
		LODGroup component3 = obj.GetComponent<LODGroup>();
		if (component3 != null)
		{
			component3.enabled = false;
		}
		MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
	}

	public static void smethod_3(Mesh commonMesh, Mesh meshToBake, Matrix4x4 modelMatrix, Class739 meshData)
	{
		int num = commonMesh.vertices.Length;
		int num2 = 0;
		Vector3[] array;
		Vector2[] array2;
		Vector4[] array3;
		Vector3[] array4;
		int[] array5;
		if (meshData.Vertices != null)
		{
			array = new Vector3[meshData.Vertices.Length + meshToBake.vertexCount];
			array2 = new Vector2[meshData.Uv.Length + meshToBake.uv.Length];
			array3 = new Vector4[meshData.Tangents.Length + meshToBake.tangents.Length];
			array4 = new Vector3[meshData.Normals.Length + meshToBake.normals.Length];
			array5 = new int[meshData.Triangles.Length + meshToBake.triangles.Length];
		}
		else
		{
			array = new Vector3[meshToBake.vertexCount];
			array2 = new Vector2[meshToBake.uv.Length];
			array3 = new Vector4[meshToBake.tangents.Length];
			array4 = new Vector3[meshToBake.normals.Length];
			array5 = new int[meshToBake.triangles.Length];
		}
		if (meshData.Vertices != null)
		{
			num2 = meshData.Triangles.Length;
			for (int i = 0; i < num; i++)
			{
				array[i] = meshData.Vertices[i];
				array2[i] = meshData.Uv[i];
				array3[i] = meshData.Tangents[i];
				array4[i] = meshData.Normals[i];
			}
			for (int j = 0; j < num2; j++)
			{
				array5[j] = meshData.Triangles[j];
			}
		}
		for (int k = 0; k < meshToBake.vertexCount; k++)
		{
			int num3 = k + num;
			array[num3] = modelMatrix.MultiplyPoint(meshToBake.vertices[k]);
			array2[num3] = meshToBake.uv[k];
			array3[num3] = modelMatrix.MultiplyVector(meshToBake.tangents[k]);
			array4[num3] = modelMatrix.MultiplyVector(meshToBake.normals[k]);
		}
		for (int l = 0; l < meshToBake.triangles.Length; l++)
		{
			array5[l + num2] = meshToBake.triangles[l] + num;
		}
		commonMesh.Clear();
		commonMesh.vertices = array;
		commonMesh.uv = array2;
		commonMesh.tangents = array3;
		commonMesh.normals = array4;
		commonMesh.triangles = array5;
		commonMesh.RecalculateBounds();
		meshData.Vertices = array.Clone() as Vector3[];
		meshData.Uv = array2.Clone() as Vector2[];
		meshData.Tangents = array3.Clone() as Vector4[];
		meshData.Normals = array4.Clone() as Vector3[];
		meshData.Triangles = array5.Clone() as int[];
	}
}
