using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EFT;
using EFT.UI;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Screenshot360 : MonoBehaviour
{
	public class Class596
	{
		public GameObject HiddenGameObject;

		public bool IsActive;

		public Class596(GameObject obj)
		{
			HiddenGameObject = obj;
			if (HiddenGameObject != null)
			{
				IsActive = HiddenGameObject.activeSelf;
				HiddenGameObject.SetActive(value: false);
			}
		}

		public void AfterScreenshot()
		{
			HiddenGameObject?.SetActive(IsActive);
		}
	}

	public string _saveToFilePath = "";

	private RenderTexture renderTexture_0;

	private Camera camera_0;

	private static List<Class596> list_0 = new List<Class596>();

	public void method_0()
	{
		PrismEffects component = GetComponent<PrismEffects>();
		if ((bool)component)
		{
			component.useVignette = false;
		}
		CC_ContrastVignette component2 = GetComponent<CC_ContrastVignette>();
		if ((bool)component2)
		{
			component2.enabled = false;
		}
		ChromaticAberration component3 = GetComponent<ChromaticAberration>();
		if ((bool)component3)
		{
			component3.Shift = 0f;
		}
		SSAA component4 = GetComponent<SSAA>();
		if ((bool)component4)
		{
			component4.enabled = false;
		}
	}

	public void Setup(int resolution, string saveToFile, Vector3 direction, Vector3 up, int cameraDepth, PrismEffects masterPrismEffect, PostProcessLayer ppLayer)
	{
		method_0();
		_saveToFilePath = saveToFile;
		camera_0 = GetComponent<Camera>();
		renderTexture_0 = new RenderTexture(resolution, resolution, 32, RenderTextureFormat.ARGB32);
		camera_0.targetTexture = renderTexture_0;
		camera_0.fieldOfView = 90f;
		camera_0.transform.LookAt(camera_0.transform.position + direction, up);
		camera_0.depth = cameraDepth;
		camera_0.GetComponent<SSAAImpl>();
		if (masterPrismEffect != null && masterPrismEffect.useExposure)
		{
			PrismEffects component = GetComponent<PrismEffects>();
			if (component != null && component.useExposure)
			{
				masterPrismEffect.AddDependantEffectExposure(component);
			}
		}
		if (!(ppLayer != null))
		{
			return;
		}
		PostProcessBundle bundle = ppLayer.GetBundle<AutoExposure>();
		if (bundle != null && bundle.settings.enabled.value)
		{
			PostProcessLayer component2 = GetComponent<PostProcessLayer>();
			if (component2 != null)
			{
				ppLayer.AddDependedPostProcessExposure(component2);
			}
		}
	}

	public void ReadAndStoreResults()
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = renderTexture_0;
		int width = renderTexture_0.width;
		Texture2D texture2D = new Texture2D(width, width, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, width, width), 0, 0);
		texture2D.Apply();
		RenderTexture.active = null;
		camera_0.targetTexture = null;
		byte[] bytes = texture2D.EncodeToPNG();
		new FileInfo(_saveToFilePath).Directory.Create();
		File.WriteAllBytes(_saveToFilePath, bytes);
		RenderTexture.active = active;
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	public static IEnumerator smethod_0(Screenshot360[] comp360, int counter)
	{
		yield return new WaitForEndOfFrame();
		if (counter > 0)
		{
			comp360[0].StartCoroutine(smethod_0(comp360, counter - 1));
			yield break;
		}
		for (int i = 0; i < 6; i++)
		{
			comp360[i].ReadAndStoreResults();
		}
		smethod_5();
	}

	public static Camera smethod_1()
	{
		GameObject gameObject = GameObject.Find("FPS Camera");
		if (gameObject == null)
		{
			Debug.LogError("FPS Camera not found! Can't make 360 screenshot");
			return null;
		}
		return gameObject.GetComponent<Camera>();
	}

	public static void Make360Screenshot(int cubemapSize, bool alignToHorizon)
	{
		Camera camera = smethod_1();
		if (camera == null)
		{
			return;
		}
		GameObject[] array = new GameObject[6]
		{
			UnityEngine.Object.Instantiate(camera.gameObject),
			UnityEngine.Object.Instantiate(camera.gameObject),
			UnityEngine.Object.Instantiate(camera.gameObject),
			UnityEngine.Object.Instantiate(camera.gameObject),
			UnityEngine.Object.Instantiate(camera.gameObject),
			UnityEngine.Object.Instantiate(camera.gameObject)
		};
		Screenshot360[] array2 = new Screenshot360[6];
		for (int i = 0; i < 6; i++)
		{
			array2[i] = array[i].AddComponent<Screenshot360>();
		}
		Vector3 vector = camera.transform.forward;
		Vector3 vector2 = camera.transform.right;
		Vector3 vector3 = camera.transform.up;
		if (alignToHorizon)
		{
			vector = new Vector3(vector.x, 0f, vector.z).normalized;
			vector2 = new Vector3(vector2.x, 0f, vector2.z).normalized;
			vector3 = Vector3.Cross(vector, vector2);
		}
		Debug.Log("360degree Screenshots will be saved to [" + Application.dataPath + "/Backgrounds/] ...");
		array2[0].Setup(cubemapSize, Application.dataPath + "/Backgrounds/Forward.png", vector, camera.transform.up, -100, null, null);
		PrismEffects component = array[0].GetComponent<PrismEffects>();
		PostProcessLayer component2 = array[0].GetComponent<PostProcessLayer>();
		if (component2 != null)
		{
			PostProcessBundle bundle = component2.GetBundle<AutoExposure>();
			if (bundle != null && bundle.settings.enabled.value)
			{
				component2.storeLastExposureTexture = true;
			}
		}
		array2[1].Setup(cubemapSize, Application.dataPath + "/Backgrounds/Right.png", vector2, vector3, -99, component, component2);
		array2[2].Setup(cubemapSize, Application.dataPath + "/Backgrounds/Left.png", -vector2, vector3, -98, component, component2);
		array2[3].Setup(cubemapSize, Application.dataPath + "/Backgrounds/Back.png", -vector, vector3, -97, component, component2);
		array2[4].Setup(cubemapSize, Application.dataPath + "/Backgrounds/Up.png", vector3, -vector, -96, component, component2);
		array2[5].Setup(cubemapSize, Application.dataPath + "/Backgrounds/Down.png", -vector3, vector, -95, component, component2);
		smethod_4();
		array2[0].StartCoroutine(smethod_0(array2, 2));
	}

	public static void smethod_2(Type monoBehaviourType)
	{
		UnityEngine.Object obj = UnityEngine.Object.FindObjectOfType(monoBehaviourType);
		if (obj != null)
		{
			list_0.Add(new Class596((obj as MonoBehaviour).gameObject));
		}
	}

	public static void smethod_3(GameObject go)
	{
		list_0.Add(new Class596(go));
	}

	public static void smethod_4()
	{
		Player player = UnityEngine.Object.FindObjectOfType<Player>();
		if (player != null)
		{
			smethod_3(player.HandsController.WeaponRoot.gameObject);
		}
		smethod_2(typeof(SimpleCharacterController));
		smethod_2(typeof(EftBattleUIScreen));
		smethod_2(typeof(ExtractionTimersPanel));
	}

	public static void smethod_5()
	{
		foreach (Class596 item in list_0)
		{
			item.AfterScreenshot();
		}
		list_0.Clear();
	}
}
