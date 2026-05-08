using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer;

public class PrefabsWithoutGameObjects : MonoBehaviour
{
	public GPUInstancerPrefabManager prefabManager;

	public GPUInstancerPrefab prefab;

	public int bufferSize = 10000;

	public Button addSphere;

	public Button removeSphere;

	public Button clearSphere;

	public Text sphereCountText;

	public Text positionUpdateFrequencyText;

	public Text scaleUpdateFrequencyText;

	public Text colorUpdateFrequencyText;

	private Matrix4x4[] matrix4x4_0;

	private int int_0;

	private float float_0 = 1f;

	private float float_1 = 1f;

	private float float_2 = 1f;

	private Vector4[] vector4_0;

	private string string_0 = "colorBuffer";

	public void Awake()
	{
		if (bufferSize < 1000)
		{
			bufferSize = 1000;
		}
		matrix4x4_0 = new Matrix4x4[bufferSize];
		method_3(1000);
		GClass1257.InitializeWithMatrix4x4Array(prefabManager, prefab.prefabPrototype, matrix4x4_0);
		GClass1257.SetInstanceCount(prefabManager, prefab.prefabPrototype, int_0);
		vector4_0 = new Vector4[bufferSize];
		for (int i = 0; i < bufferSize; i++)
		{
			vector4_0[i] = Random.ColorHSV();
		}
		GClass1257.DefineAndAddVariationFromArray(prefabManager, prefab.prefabPrototype, string_0, vector4_0);
		method_5();
		StartCoroutine(method_0());
		StartCoroutine(method_1());
		StartCoroutine(method_2());
	}

	public IEnumerator method_0()
	{
		while (true)
		{
			if (int_0 > 100 && float_0 > 0f)
			{
				int num = Random.Range(0, int_0 - 100);
				for (int i = num; i < num + 100; i++)
				{
					Vector3 vector = Random.insideUnitSphere * 20f;
					matrix4x4_0[i].SetColumn(3, new Vector4(vector.x, vector.y, vector.z, matrix4x4_0[i][3, 3]));
				}
				GClass1257.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab.prefabPrototype, matrix4x4_0, num, num, 100);
			}
			yield return new WaitForSeconds(1f - float_0 + 0.01f);
		}
	}

	public IEnumerator method_1()
	{
		while (true)
		{
			if (int_0 > 100 && float_1 > 0f)
			{
				int num = Random.Range(0, int_0 - 100);
				for (int i = num; i < num + 100; i++)
				{
					Matrix4x4 matrix4x = matrix4x4_0[i];
					Vector3 pos = matrix4x.GetColumn(3);
					Quaternion q = Quaternion.LookRotation(matrix4x.GetColumn(2), matrix4x.GetColumn(1));
					Vector3 s = Vector3.one * Random.Range(0.5f, 1.5f);
					matrix4x4_0[i] = Matrix4x4.TRS(pos, q, s);
				}
				GClass1257.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab.prefabPrototype, matrix4x4_0, num, num, 100);
			}
			yield return new WaitForSeconds(1f - float_1 + 0.01f);
		}
	}

	public IEnumerator method_2()
	{
		while (true)
		{
			if (int_0 > 100 && float_2 > 0f)
			{
				int num = Random.Range(0, int_0 - 100);
				for (int i = num; i < num + 100; i++)
				{
					vector4_0[i] = Random.ColorHSV();
				}
				GClass1257.UpdateVariationFromArray(prefabManager, prefab.prefabPrototype, string_0, vector4_0, num, num, 100);
			}
			yield return new WaitForSeconds(1f - float_2 + 0.01f);
		}
	}

	public void method_3(int instanceCount)
	{
		int num = int_0;
		int_0 += instanceCount;
		for (int i = num; i < int_0; i++)
		{
			matrix4x4_0[i] = Matrix4x4.TRS(Random.insideUnitSphere * 20f, Quaternion.identity, Vector3.one * Random.Range(0.5f, 1.5f));
		}
	}

	public void method_4(int instanceCount)
	{
		int num = int_0;
		int_0 -= instanceCount;
		for (int i = int_0; i < num; i++)
		{
			matrix4x4_0[i] = Matrix4x4.zero;
		}
	}

	public void method_5()
	{
		if (int_0 + 1000 > bufferSize)
		{
			addSphere.interactable = false;
		}
		else
		{
			addSphere.interactable = true;
		}
		if (int_0 - 1000 < 0)
		{
			removeSphere.interactable = false;
		}
		else
		{
			removeSphere.interactable = true;
		}
		if (int_0 <= 0)
		{
			clearSphere.interactable = false;
		}
		else
		{
			clearSphere.interactable = true;
		}
		sphereCountText.text = "Sphere Count: " + int_0;
	}

	public void SetPositionUpdateFrequency(float updateInterval)
	{
		float_0 = updateInterval;
		if (float_0 <= 0f)
		{
			positionUpdateFrequencyText.text = "Updating positions cancelled";
		}
		else
		{
			positionUpdateFrequencyText.text = "Updating positions every " + (1f - float_0 + 0.01f).ToString("0.00") + " seconds";
		}
	}

	public void SetScaleUpdateFrequency(float updateInterval)
	{
		float_1 = updateInterval;
		if (float_1 <= 0f)
		{
			scaleUpdateFrequencyText.text = "Updating scales cancelled";
		}
		else
		{
			scaleUpdateFrequencyText.text = "Updating scales every " + (1f - float_1 + 0.01f).ToString("0.00") + " seconds";
		}
	}

	public void SetColorUpdateFrequency(float updateInterval)
	{
		float_2 = updateInterval;
		if (float_2 <= 0f)
		{
			colorUpdateFrequencyText.text = "Updating colors cancelled";
		}
		else
		{
			colorUpdateFrequencyText.text = "Updating colors every " + (1f - float_2 + 0.01f).ToString("0.00") + " seconds";
		}
	}

	public void AddSpheres()
	{
		int num = int_0;
		method_3(1000);
		method_5();
		GClass1257.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab.prefabPrototype, matrix4x4_0, num, num, 1000);
		GClass1257.SetInstanceCount(prefabManager, prefab.prefabPrototype, int_0);
	}

	public void RemoveSpheres()
	{
		method_4(1000);
		method_5();
		GClass1257.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab.prefabPrototype, matrix4x4_0, int_0, int_0, 1000);
		GClass1257.SetInstanceCount(prefabManager, prefab.prefabPrototype, int_0);
	}

	public void ClearSpheres()
	{
		int_0 = 0;
		method_5();
		matrix4x4_0 = new Matrix4x4[bufferSize];
		GClass1257.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prefab.prefabPrototype, matrix4x4_0);
		GClass1257.SetInstanceCount(prefabManager, prefab.prefabPrototype, int_0);
	}
}
