using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class PhotoCamera : MonoBehaviour
{
	public delegate void GDelegate35(PhotoCamera sender);

	private RenderTexture renderTexture_0;

	private Camera camera_0;

	private bool bool_0;

	private bool bool_1;

	private int int_0;

	private Vector3 vector3_0 = new Vector3(0f, 0f, 5f);

	[CompilerGenerated]
	private GDelegate35 gdelegate35_0;

	private Action<Texture> action_0;

	private GameObject gameObject_0;

	private Matrix4x4 matrix4x4_0;

	private Light light_0;

	public event GDelegate35 OnTextureRenderFinished
	{
		[CompilerGenerated]
		add
		{
			GDelegate35 gDelegate = gdelegate35_0;
			GDelegate35 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate35 value2 = (GDelegate35)Delegate.Combine(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate35_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
		[CompilerGenerated]
		remove
		{
			GDelegate35 gDelegate = gdelegate35_0;
			GDelegate35 gDelegate2;
			do
			{
				gDelegate2 = gDelegate;
				GDelegate35 value2 = (GDelegate35)Delegate.Remove(gDelegate2, value);
				gDelegate = Interlocked.CompareExchange(ref gdelegate35_0, value2, gDelegate2);
			}
			while ((object)gDelegate != gDelegate2);
		}
	}

	public void Awake()
	{
		int_0 = LayerMask.NameToLayer("PhotoLayer");
		camera_0 = base.gameObject.AddComponent<Camera>();
		camera_0.backgroundColor = new Color(1f, 0f, 0f, 0f);
		camera_0.orthographic = true;
		camera_0.cullingMask = 1 << int_0;
		camera_0.enabled = false;
		light_0 = base.gameObject.AddComponent<Light>();
		light_0.type = LightType.Directional;
		light_0.color = Color.white;
		light_0.enabled = false;
		matrix4x4_0 = Matrix4x4.TRS(base.transform.position + vector3_0, Quaternion.identity, Vector3.one);
		Debug.Log(matrix4x4_0);
	}

	public void MakeTexture(GameObject gObject, Action<Texture> callback, int cameraOrthoSize, Vector2 renderTextureSize)
	{
		camera_0.enabled = true;
		renderTexture_0 = new RenderTexture(Mathf.FloorToInt(GetComponent<Camera>().pixelWidth * 2), Mathf.FloorToInt(GetComponent<Camera>().pixelHeight * 2), 24);
		renderTexture_0.name = "PhotoCamera RT";
		camera_0.targetTexture = renderTexture_0;
		camera_0.orthographicSize = 0.5f;
		gameObject_0 = gObject;
		action_0 = callback;
		bool_0 = true;
	}

	public void OnPreCull()
	{
		if (bool_0)
		{
			light_0.enabled = true;
			method_0(gameObject_0.transform);
			bool_0 = false;
			bool_1 = true;
		}
	}

	public void method_0(Transform t)
	{
		foreach (Transform item in t)
		{
			method_0(item);
			MeshFilter component = item.gameObject.GetComponent<MeshFilter>();
			MeshRenderer component2 = item.gameObject.GetComponent<MeshRenderer>();
			if (!(component != null) || !(component2 != null))
			{
				continue;
			}
			if (component.sharedMesh.subMeshCount == 1)
			{
				Debug.Log(component.sharedMesh);
				Matrix4x4 matrix = matrix4x4_0 * item.localToWorldMatrix;
				Graphics.DrawMesh(component.sharedMesh, matrix, component2.sharedMaterial, int_0, camera_0);
				Debug.Log(" m1 " + item.localToWorldMatrix.ToString());
				Matrix4x4 matrix4x = matrix4x4_0;
				Debug.Log(" m2 " + matrix4x.ToString());
				Debug.Log(" result " + (matrix4x4_0 * item.localToWorldMatrix).ToString());
				Debug.Log(matrix.GetColumn(3));
			}
			else
			{
				for (int i = 0; i < component.sharedMesh.subMeshCount; i++)
				{
					Debug.Log(component.sharedMesh?.ToString() + " complex " + component.sharedMesh.subMeshCount);
					int num = ((i < component2.sharedMaterials.Length) ? i : (component2.sharedMaterials.Length - 1));
					Graphics.DrawMesh(method_1(component.sharedMesh.GetTriangles(i), component.sharedMesh), item.localToWorldMatrix * camera_0.worldToCameraMatrix * camera_0.projectionMatrix, component2.sharedMaterials[num], int_0, camera_0);
				}
			}
		}
	}

	public Mesh method_1(int[] triangles, Mesh originalMesh)
	{
		Mesh mesh = new Mesh();
		mesh.name = "PhotoCamera GetMeshFromSubmesh";
		mesh.Clear();
		mesh.vertices = originalMesh.vertices;
		mesh.uv = originalMesh.uv;
		mesh.uv2 = originalMesh.uv2;
		mesh.uv2 = originalMesh.uv2;
		mesh.colors = originalMesh.colors;
		mesh.normals = originalMesh.normals;
		mesh.triangles = triangles;
		mesh.tangents = originalMesh.tangents;
		return mesh;
	}

	public void OnPostRender()
	{
		if (bool_1)
		{
			light_0.enabled = false;
			action_0(renderTexture_0);
			if (gdelegate35_0 != null)
			{
				gdelegate35_0(this);
			}
			GetComponent<Camera>().targetTexture = null;
			GetComponent<Camera>().enabled = false;
			bool_1 = false;
		}
	}
}
