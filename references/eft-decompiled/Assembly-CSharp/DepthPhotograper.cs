using System;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Profiling;

public class DepthPhotograper : MonoBehaviour
{
	private static DepthPhotograper depthPhotograper_0;

	[SerializeField]
	private PowOfTwoDimensions _depthTextureDimension = PowOfTwoDimensions._4096;

	[SerializeField]
	private float _yBias;

	private float float_0;

	private float float_1;

	private Vector3 vector3_0;

	private Camera camera_0;

	private Material material_0;

	[SerializeField]
	private RenderTexture _depthRT;

	private RenderTexture renderTexture_0;

	private static readonly int int_0 = Shader.PropertyToID("_MapStart");

	private static readonly int int_1 = Shader.PropertyToID("_MapScale");

	private static readonly int int_2 = Shader.PropertyToID("_WeatherDepthMap");

	private const float float_2 = 100f;

	private int int_3 = 11;

	private CustomSampler customSampler_0 = CustomSampler.Create("CheckIfCameraUnderRain");

	private Bounds bounds_0;

	public static DepthPhotograper Instance => depthPhotograper_0;

	public float Single_0 => Math.Max(bounds_0.size.x, bounds_0.size.z);

	public void Start()
	{
		depthPhotograper_0 = this;
	}

	[ContextMenu("Render")]
	public void Render()
	{
		if (Singleton<LevelSettings>.Instance != null)
		{
			bounds_0 = Singleton<LevelSettings>.Instance.RainBounds;
		}
		else
		{
			Debug.LogError("---- There is no instance of LevelSettings!------");
		}
		int_3 = LayerMask.NameToLayer("Terrain");
		material_0 = new Material(GClass872.Find("Hidden/Black"));
		method_0();
		renderTexture_0 = new RenderTexture((int)_depthTextureDimension, (int)_depthTextureDimension, 16)
		{
			name = "Depth Camera Render Texture",
			format = RenderTextureFormat.Depth,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear
		};
		method_1();
		method_2();
		method_3();
		UnityEngine.Object.DestroyImmediate(material_0);
		UnityEngine.Object.DestroyImmediate(camera_0.gameObject);
		UnityEngine.Object.DestroyImmediate(renderTexture_0);
	}

	public void method_0()
	{
		int depthTextureDimension = (int)_depthTextureDimension;
		if ((bool)_depthRT)
		{
			if (_depthRT.width == depthTextureDimension && _depthRT.height == depthTextureDimension)
			{
				return;
			}
			_depthRT.Release();
			GClass972.SafeDestroy(_depthRT);
			_depthRT = null;
		}
		_depthRT = new RenderTexture(depthTextureDimension, depthTextureDimension, 0, RenderTextureFormat.RFloat)
		{
			name = "Depth Render Texture",
			wrapMode = TextureWrapMode.Clamp
		};
	}

	public bool CheckIfCameraUnderRain(Transform cameraTransform)
	{
		WeatherObstacle instance = WeatherObstacle.Instance;
		if (!(cameraTransform == null) && !(instance == null) && !(instance.MeshCollider == null))
		{
			Vector3 position = cameraTransform.position;
			Vector3 origin = new Vector3(position.x, position.y + 100f, position.z);
			Ray ray = new Ray(origin, Vector3.down);
			RaycastHit hitInfo;
			return !instance.MeshCollider.Raycast(ray, out hitInfo, 100f);
		}
		return false;
	}

	public void method_1()
	{
		GameObject gameObject = new GameObject("DepthCamera");
		gameObject.transform.SetParent(base.transform);
		camera_0 = gameObject.AddComponent<Camera>();
		camera_0.enabled = false;
		camera_0.aspect = 1f;
		camera_0.orthographic = true;
		camera_0.orthographicSize = 0.5f * Single_0;
		camera_0.cullingMask = 1 << int_3;
		camera_0.nearClipPlane = 0.1f;
		camera_0.farClipPlane = bounds_0.size.y;
		camera_0.useOcclusionCulling = false;
		camera_0.clearFlags = CameraClearFlags.Color;
		camera_0.renderingPath = RenderingPath.Forward;
		camera_0.depthTextureMode = DepthTextureMode.Depth;
		camera_0.targetTexture = renderTexture_0;
		camera_0.transform.position = new Vector3(bounds_0.center.x, bounds_0.max.y, bounds_0.center.z);
		camera_0.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
	}

	public void method_2()
	{
		Terrain[] array = GClass870.FindUnityObjectsOfType<Terrain>();
		bool[] array2 = new bool[array.Length];
		DisablerTerrainCullingObject[] array3 = new DisablerTerrainCullingObject[array.Length];
		float num = -1000f;
		for (int i = 0; i < array.Length; i++)
		{
			Terrain terrain = array[i];
			array2[i] = terrain.drawHeightmap;
			array3[i] = terrain.GetComponent<DisablerTerrainCullingObject>();
			terrain.drawHeightmap = array3[i] != null || array2[i];
			terrain.transform.position += Vector3.up * num;
		}
		WeatherObstacle instance = WeatherObstacle.Instance;
		if (instance != null)
		{
			Graphics.DrawMesh(instance.MeshCollider.sharedMesh, Matrix4x4.identity, material_0, LayerMask.NameToLayer("Terrain"));
		}
		else
		{
			Debug.LogError("WeatherObstacle.Instance is null");
		}
		camera_0.Render();
		camera_0.targetTexture = null;
		for (int j = 0; j < array.Length; j++)
		{
			Terrain obj = array[j];
			obj.drawHeightmap = array2[j];
			obj.transform.position -= Vector3.up * num;
		}
		RenderTexture active = RenderTexture.active;
		Graphics.Blit(renderTexture_0, _depthRT);
		RenderTexture.active = active;
	}

	public void method_3()
	{
		float single_ = Single_0;
		Vector3 vector = new Vector3(bounds_0.center.x - 0.5f * single_, bounds_0.min.y + _yBias, bounds_0.center.z - 0.5f * single_);
		Vector3 vector2 = new Vector3(1f / single_, 1f / bounds_0.size.y, 1f / single_);
		Shader.SetGlobalVector(int_0, vector);
		Shader.SetGlobalVector(int_1, vector2);
		Shader.SetGlobalTexture(int_2, _depthRT);
	}

	public void OnDestroy()
	{
		depthPhotograper_0 = null;
		UnityEngine.Object.Destroy(_depthRT);
	}
}
