using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GPUInstancer;

public class GrassInitialization : MonoBehaviour
{
	[SerializeField]
	private Transform GrassParent;

	[SerializeField]
	private GPUInstancerPrefab prefab;

	[SerializeField]
	private List<GameObject> PrefabList = new List<GameObject>();

	[SerializeField]
	private List<Color> DryColor = new List<Color>();

	[SerializeField]
	private List<Color> HealthyColor = new List<Color>();

	[SerializeField]
	private List<Color> WinterColors = new List<Color>();

	[SerializeField]
	private List<Color> SeasonsColors = new List<Color>();

	[SerializeField]
	private string FileName = "CityGrassPrefabs.pcl";

	private const string string_0 = "_ArrayIndexBuffer";

	public static string String_0 => Application.streamingAssetsPath + "/Grass/";

	public string String_1 => String_0 + FileName;

	public void Awake()
	{
		byte[] array = File.ReadAllBytes(String_1);
		GStruct115 gStruct = new GStruct115
		{
			bytes = array
		};
		int num = Marshal.SizeOf(typeof(GStruct116));
		int num2 = array.Length / num;
		int[] array2 = new int[num2];
		Matrix4x4[] array3 = new Matrix4x4[num2];
		for (int i = 0; i < num2; i++)
		{
			array3[i] = gStruct.prefabs[i].matrix;
			array2[i] = gStruct.prefabs[i].arrayIndex;
		}
		prefab.prefabPrototype.enableRuntimeModifications = true;
		GPUInstancerPrefabManager[] components = base.gameObject.GetComponents<GPUInstancerPrefabManager>();
		foreach (GPUInstancerPrefabManager obj in components)
		{
			obj.SetColorBuffers(DryColor, HealthyColor);
			GClass1257.InitializeWithMatrix4x4Array(obj, prefab.prefabPrototype, array3);
			GClass1257.SetInstanceCount(obj, prefab.prefabPrototype, num2);
			GClass1257.DefineAndAddVariationFromArray(obj, prefab.prefabPrototype, "_ArrayIndexBuffer", array2);
		}
	}

	public void method_0(ESeason season)
	{
		byte[] array = File.ReadAllBytes(String_1);
		GStruct115 gStruct = new GStruct115
		{
			bytes = array
		};
		int num = Marshal.SizeOf(typeof(GStruct116));
		int num2 = array.Length / num;
		int[] array2 = new int[num2];
		Matrix4x4[] array3 = new Matrix4x4[num2];
		for (int i = 0; i < num2; i++)
		{
			array3[i] = gStruct.prefabs[i].matrix;
			array2[i] = gStruct.prefabs[i].arrayIndex;
		}
		prefab.prefabPrototype.enableRuntimeModifications = true;
		GPUInstancerPrefabManager[] components = base.gameObject.GetComponents<GPUInstancerPrefabManager>();
		List<Color> dryColor = DryColor;
		List<Color> healthyColor = HealthyColor;
		switch (season)
		{
		default:
			throw new ArgumentOutOfRangeException("season", season, null);
		case ESeason.Autumn:
		case ESeason.Spring:
		case ESeason.AutumnLate:
			dryColor = (healthyColor = SeasonsColors);
			break;
		case ESeason.Winter:
		case ESeason.SpringEarly:
			dryColor = (healthyColor = WinterColors);
			break;
		case ESeason.Summer:
			break;
		}
		foreach (GPUInstancerPrefabManager obj in components)
		{
			obj.GeneratePrototypesForWinter();
			obj.InitializeRuntimeDataAndBuffers();
			obj.SetColorBuffers(dryColor, healthyColor);
			GClass1257.InitializeWithMatrix4x4Array(obj, prefab.prefabPrototype, array3);
			GClass1257.SetInstanceCount(obj, prefab.prefabPrototype, num2);
			GClass1257.DefineAndAddVariationFromArray(obj, prefab.prefabPrototype, "_ArrayIndexBuffer", array2);
		}
	}
}
