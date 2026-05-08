using GPUInstancer;
using UnityEngine;

public class BoidController : MonoBehaviour
{
	public abstract class GClass874
	{
		public static readonly int BP_boidsData = Shader.PropertyToID("boidsData");

		public static readonly int BP_bufferSize = Shader.PropertyToID("bufferSize");

		public static readonly int BP_controllerTransform = Shader.PropertyToID("controllerTransform");

		public static readonly int BP_controllerVelocity = Shader.PropertyToID("controllerVelocity");

		public static readonly int BP_controllerVelocityVariation = Shader.PropertyToID("controllerVelocityVariation");

		public static readonly int BP_controllerRotationCoeff = Shader.PropertyToID("controllerRotationCoeff");

		public static readonly int BP_controllerNeighborDist = Shader.PropertyToID("controllerNeighborDist");

		public static readonly int BP_time = Shader.PropertyToID("time");

		public static readonly int BP_deltaTime = Shader.PropertyToID("deltaTime");

		public static readonly int BP_noiseTexture = Shader.PropertyToID("noiseTexture");
	}

	public int spawnCount = 10;

	public float spawnRadius = 4f;

	[Range(0.1f, 20f)]
	public float velocity = 6f;

	[Range(0f, 0.9f)]
	public float velocityVariation = 0.5f;

	[Range(0.1f, 20f)]
	public float rotationCoeff = 4f;

	[Range(0.1f, 10f)]
	public float neighborDist = 2f;

	private Matrix4x4[] matrix4x4_0;

	private Vector4[] vector4_0;

	public Transform centerTransform;

	public Texture2D noiseTexture;

	private GPUInstancerPrefabManager gpuinstancerPrefabManager_0;

	private ComputeShader computeShader_0;

	private string string_0 = "colorBuffer";

	private float[] float_0 = new float[16];

	private ComputeBuffer computeBuffer_0;

	public void Start()
	{
		matrix4x4_0 = new Matrix4x4[spawnCount];
		vector4_0 = new Vector4[spawnCount];
		for (int i = 0; i < spawnCount; i++)
		{
			Spawn(i);
		}
		gpuinstancerPrefabManager_0 = Object.FindObjectOfType<GPUInstancerPrefabManager>();
		if (gpuinstancerPrefabManager_0 != null && gpuinstancerPrefabManager_0.prototypeList.Count > 0)
		{
			GPUInstancerPrefabPrototype prototype = (GPUInstancerPrefabPrototype)gpuinstancerPrefabManager_0.prototypeList[0];
			GClass1257.InitializeWithMatrix4x4Array(gpuinstancerPrefabManager_0, prototype, matrix4x4_0);
			GClass1257.DefineAndAddVariationFromArray(gpuinstancerPrefabManager_0, prototype, string_0, vector4_0);
			computeBuffer_0 = GClass1257.GetTransformDataBuffer(gpuinstancerPrefabManager_0, prototype);
		}
	}

	public void Spawn(int index)
	{
		matrix4x4_0[index] = Matrix4x4.TRS(centerTransform.position + Random.insideUnitSphere * spawnRadius, Random.rotation, Vector3.one);
		vector4_0[index] = Random.ColorHSV();
	}

	public void Update()
	{
		if (computeShader_0 == null)
		{
			computeShader_0 = Resources.Load<ComputeShader>("GPUIBoids");
		}
		if (!(computeShader_0 == null) && computeBuffer_0 != null)
		{
			GClass1274.Matrix4x4ToFloatArray(centerTransform.localToWorldMatrix, float_0);
			computeShader_0.SetBuffer(0, GClass874.BP_boidsData, computeBuffer_0);
			computeShader_0.SetTexture(0, GClass874.BP_noiseTexture, noiseTexture);
			computeShader_0.SetInt(GClass874.BP_bufferSize, spawnCount);
			computeShader_0.SetFloats(GClass874.BP_controllerTransform, float_0);
			computeShader_0.SetFloat(GClass874.BP_controllerVelocity, velocity);
			computeShader_0.SetFloat(GClass874.BP_controllerVelocityVariation, velocityVariation);
			computeShader_0.SetFloat(GClass874.BP_controllerRotationCoeff, rotationCoeff);
			computeShader_0.SetFloat(GClass874.BP_controllerNeighborDist, neighborDist);
			computeShader_0.SetFloat(GClass874.BP_time, Time.time);
			computeShader_0.SetFloat(GClass874.BP_deltaTime, Time.deltaTime);
			computeShader_0.Dispatch(0, Mathf.CeilToInt((float)spawnCount / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
		}
	}
}
