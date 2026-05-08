using UnityEngine;

public class Shard : MonoBehaviour
{
	private MeshFilter meshFilter_0;

	private MeshCollider meshCollider_0;

	public Mesh Mesh => meshFilter_0.mesh;

	public void Awake()
	{
		base.gameObject.AddComponent<MeshRenderer>();
		base.gameObject.AddComponent<Rigidbody>();
		meshFilter_0 = base.gameObject.AddComponent<MeshFilter>();
		meshCollider_0 = base.gameObject.AddComponent<MeshCollider>();
		meshCollider_0.convex = true;
		meshFilter_0.mesh = new Mesh
		{
			name = "Shard Mesh"
		};
		base.gameObject.SetActive(value: false);
	}

	public void method_0()
	{
		base.transform.parent = null;
		base.gameObject.SetActive(value: true);
	}

	public void method_1(Vector3[] verts, int[] tris, Vector2[] uvs)
	{
		int num = 0;
		Vector3[] array = new Vector3[tris.Length];
		Vector2[] array2 = new Vector2[tris.Length];
		int[] array3 = new int[tris.Length];
		for (int i = 0; i < tris.Length; i += 3)
		{
			array[num] = verts[tris[i]];
			array3[num] = num;
			array2[num] = new Vector2(1f, 0f);
			int num2 = num + 1;
			array[num2] = verts[tris[i + 1]];
			array3[num2] = num2;
			array2[num2] = new Vector2(0f, 0f);
			int num3 = num2 + 1;
			array[num3] = verts[tris[i + 2]];
			array3[num3] = num3;
			array2[num3] = new Vector2(1f, 1f);
			num = num3 + 1;
		}
		Mesh.vertices = array;
		Mesh.triangles = array3;
		Mesh.uv = array2;
		Mesh.RecalculateNormals();
		meshCollider_0.sharedMesh = Mesh;
		method_0();
	}

	public static Shard smethod_0(GameObject parent, Vector3[] newVertices, int[] newTriangles, Vector2[] newUVs)
	{
		Shard nextShard = ShardPool.NextShard;
		if (nextShard == null)
		{
			Debug.LogError("Not enough shards left in pool.");
			return null;
		}
		nextShard.name = "Split shard";
		nextShard.method_1(newVertices, newTriangles, newUVs);
		nextShard.GetComponent<Renderer>().material = parent.GetComponent<Renderer>().material;
		if (parent.GetComponent<Rigidbody>() != null)
		{
			nextShard.GetComponent<Rigidbody>().mass = parent.GetComponent<Rigidbody>().mass;
			nextShard.GetComponent<Rigidbody>().velocity = parent.GetComponent<Rigidbody>().velocity;
		}
		return nextShard;
	}
}
