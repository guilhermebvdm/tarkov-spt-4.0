using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class AstroidGenerator : MonoBehaviour
{
	[Range(0f, 200000f)]
	public int count = 50000;

	public List<GPUInstancerPrefab> asteroidObjects = new List<GPUInstancerPrefab>();

	public GPUInstancerPrefabManager prefabManager;

	public Transform centerTransform;

	private List<GPUInstancerPrefab> list_0 = new List<GPUInstancerPrefab>();

	private int int_0;

	private Vector3 vector3_0;

	private Vector3 vector3_1;

	private Quaternion quaternion_0;

	private Vector3 vector3_2;

	private Vector3 vector3_3;

	private GPUInstancerPrefab gpuinstancerPrefab_0;

	private GameObject gameObject_0;

	private float float_0;

	private int int_1;

	private int int_2 = 3;

	public void Awake()
	{
		int_0 = 0;
		vector3_0 = centerTransform.position;
		vector3_1 = Vector3.zero;
		quaternion_0 = Quaternion.identity;
		vector3_2 = Vector3.zero;
		vector3_3 = Vector3.one;
		float_0 = 1f;
		gameObject_0 = new GameObject("Asteroids");
		gameObject_0.transform.position = vector3_0;
		gameObject_0.transform.parent = base.gameObject.transform;
		int_1 = ((count < 5000) ? 1 : (count / 2500));
		int num = ((count % int_1 > 0) ? (int_1 - 1) : int_1);
		list_0.Clear();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < Mathf.FloorToInt((float)count / (float)int_1); j++)
			{
				list_0.Add(method_1(vector3_0, i));
			}
		}
		if (num != int_1)
		{
			for (int k = 0; k < count - Mathf.FloorToInt((float)count / (float)int_1) * num; k++)
			{
				list_0.Add(method_1(vector3_0, int_1));
			}
		}
		Debug.Log("Instantiated " + int_0 + " objects.");
	}

	public void Start()
	{
		if (prefabManager != null && prefabManager.gameObject.activeSelf && prefabManager.enabled)
		{
			GClass1257.RegisterPrefabInstanceList(prefabManager, list_0);
			GClass1257.InitializeGPUInstancer(prefabManager);
		}
	}

	public void method_0(Vector3 center, int column, float radius)
	{
		float num = UnityEngine.Random.value * 360f;
		vector3_1.x = center.x + radius * Mathf.Sin(num * (MathF.PI / 180f));
		vector3_1.y = center.y - (float)column * (float)int_2 / 2f + (float)(column * int_2) + UnityEngine.Random.Range(0f, 1f);
		vector3_1.z = center.z + radius * Mathf.Cos(num * (MathF.PI / 180f));
	}

	public GPUInstancerPrefab method_1(Vector3 center, int column)
	{
		method_0(center, column - Mathf.FloorToInt((float)int_1 / 2f), UnityEngine.Random.Range(80f, 150f));
		quaternion_0 = Quaternion.FromToRotation(Vector3.forward, center - vector3_1);
		gpuinstancerPrefab_0 = UnityEngine.Object.Instantiate(asteroidObjects[UnityEngine.Random.Range(0, asteroidObjects.Count)], vector3_1, quaternion_0);
		gpuinstancerPrefab_0.transform.parent = gameObject_0.transform;
		vector3_2.x = UnityEngine.Random.Range(-180f, 180f);
		vector3_2.y = UnityEngine.Random.Range(-180f, 180f);
		vector3_2.z = UnityEngine.Random.Range(-180f, 180f);
		gpuinstancerPrefab_0.transform.localRotation = Quaternion.Euler(vector3_2);
		float_0 = UnityEngine.Random.Range(0.3f, 1.2f);
		vector3_3.x = float_0;
		vector3_3.y = float_0;
		vector3_3.z = float_0;
		gpuinstancerPrefab_0.transform.localScale = vector3_3;
		int_0++;
		return gpuinstancerPrefab_0;
	}
}
