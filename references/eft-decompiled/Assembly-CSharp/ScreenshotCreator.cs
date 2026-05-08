using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class ScreenshotCreator : MonoBehaviour
{
	[Serializable]
	public class CameraObject
	{
		public enum Hotkey
		{
			Hotkey,
			A,
			B,
			C,
			D,
			E,
			F,
			G,
			H,
			I,
			J,
			K,
			L,
			M,
			N,
			O,
			P,
			Q,
			R,
			S,
			T,
			U,
			V,
			W,
			X,
			Y,
			Z
		}

		public GameObject cam;

		public bool deleteQuestion;

		public Hotkey hotkey;
	}

	public enum FileType
	{
		PNG,
		JPG
	}

	public enum CaptureMethod
	{
		CaptureScreenshot,
		RenderTexture,
		Cutout
	}

	[HideInInspector]
	public Color signatureColor = new Color(1f, 0f, 0.5f);

	public List<CameraObject> list = new List<CameraObject>();

	[HideInInspector]
	public bool foldoutSettings;

	[Tooltip("The name of your screenshot or screenshot session. Camera name and current date will be added automatically.")]
	public string customName = "";

	public string customDirectory = "";

	[HideInInspector]
	public int lastCamID;

	[HideInInspector]
	public Camera lastCam;

	public bool includeCamName = true;

	public bool includeDate = true;

	public bool includeResolution = true;

	public FileType fileType;

	public CaptureMethod captureMethod;

	public bool singleCamera;

	public float renderSizeMultiplier = 1f;

	public int captureSizeMultiplier = 1;

	public Vector2 cutoutPosition;

	public Vector2 cutoutSize;

	private GUIStyle guistyle_0;

	public bool applicationPath;

	private Vector2 vector2_0;

	private Vector2 vector2_1;

	private float float_0;

	public void Create()
	{
		list.Add(new CameraObject());
	}

	public void RequestDelete(int id)
	{
		list[id].deleteQuestion = true;
	}

	public void Delete(int id)
	{
		list.Remove(list[id]);
		if (list.Count == 0)
		{
			Create();
		}
	}

	public void Awake()
	{
		if (list.Count == 0)
		{
			Create();
			list[0].cam = Camera.main.gameObject;
		}
	}

	public void LateUpdate()
	{
		if (!Input.anyKeyDown)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].hotkey == CameraObject.Hotkey.Hotkey || !Input.GetKeyDown(list[i].hotkey.ToString().ToLower()))
			{
				continue;
			}
			if (list[i] != null)
			{
				if (captureMethod == CaptureMethod.RenderTexture)
				{
					Camera component = list[i].cam.GetComponent<Camera>();
					if (component == null)
					{
						CaptureScreenshots(i, fallback: true);
					}
					else
					{
						CaptureRenderTexture(component, i);
					}
				}
				else if (captureMethod == CaptureMethod.CaptureScreenshot)
				{
					CaptureScreenshots(i, fallback: false);
				}
				else
				{
					StartCoroutine(CaptureCutout(i));
				}
				lastCam = list[lastCamID].cam.GetComponent<Camera>();
			}
			else
			{
				Debug.Log("Screenshot by Hotkey (" + list[i].hotkey.ToString() + ") could not be created! Camera not available.");
			}
		}
	}

	public void method_0(int id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].cam != null)
			{
				list[i].cam.SetActive(value: false);
			}
		}
		list[id].cam.SetActive(value: true);
	}

	public string getSaveDirectory()
	{
		string text = ((customDirectory != "") ? customDirectory : "Screenshots");
		if (applicationPath)
		{
			return Application.persistentDataPath + "/" + text + "/";
		}
		return Directory.GetCurrentDirectory() + "/" + text + "/";
	}

	public string method_1()
	{
		string saveDirectory = getSaveDirectory();
		if (!Directory.Exists(saveDirectory))
		{
			Directory.CreateDirectory(saveDirectory);
		}
		return saveDirectory;
	}

	public void method_2()
	{
		guistyle_0 = new GUIStyle(GUI.skin.box);
		int num = 16;
		Color[] array = new Color[256];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				if (i != 0 && i != num - 1 && j != 0 && j != num - 1)
				{
					array[i * num + j] = new Color(1f, 1f, 1f, 0.1f);
				}
				else
				{
					array[i * num + j] = Color.white;
				}
			}
		}
		Texture2D texture2D = new Texture2D(num, num);
		texture2D.name = "ScreenshotCreator";
		texture2D.SetPixels(array);
		texture2D.Apply();
		guistyle_0.normal.background = texture2D;
	}

	public void method_3()
	{
		cutoutPosition.x = Mathf.Clamp(cutoutPosition.x, cutoutSize.x / 2f, (float)Screen.width - cutoutSize.x / 2f);
		cutoutPosition.y = Mathf.Clamp(cutoutPosition.y, cutoutSize.y / 2f, (float)Screen.height - cutoutSize.y / 2f);
		cutoutSize.x = Mathf.Clamp(cutoutSize.x, 0f, Screen.width);
		cutoutSize.y = Mathf.Clamp(cutoutSize.y, 0f, Screen.height);
	}

	public void OnGUI()
	{
		if (vector2_0.x == cutoutPosition.x && vector2_0.y == cutoutPosition.y && vector2_1.x == cutoutSize.x && vector2_1.y == cutoutSize.y)
		{
			float_0 -= Time.deltaTime;
		}
		else
		{
			float_0 = 1f;
		}
		vector2_0 = cutoutPosition;
		vector2_1 = cutoutSize;
		if (!(float_0 <= 0f) && captureMethod == CaptureMethod.Cutout)
		{
			method_2();
			method_3();
			GUI.Box(new Rect(cutoutPosition.x - cutoutSize.x / 2f, cutoutPosition.y - cutoutSize.y / 2f, cutoutSize.x, cutoutSize.y), "", guistyle_0);
		}
	}

	public void CaptureCutoutVoid(int id)
	{
		if (singleCamera)
		{
			method_0(id);
		}
		StartCoroutine(CaptureCutout(id));
	}

	public IEnumerator CaptureCutout(int id)
	{
		yield return new WaitForEndOfFrame();
		string text = method_1() + getFileName(id);
		method_4();
		method_3();
		int num = (int)(cutoutPosition.x - cutoutSize.x / 2f);
		int num2 = (int)((float)Screen.height - cutoutPosition.y - cutoutSize.y / 2f);
		int num3 = (int)cutoutSize.x;
		int num4 = (int)cutoutSize.y;
		Texture2D texture2D = new Texture2D(num3, num4, TextureFormat.RGB24, mipChain: false);
		texture2D.name = "ScreenshotCreator";
		texture2D.ReadPixels(new Rect(num, num2, num3, num4), 0, 0);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		UnityEngine.Object.Destroy(texture2D);
		File.WriteAllBytes(text, bytes);
		Debug.Log("[ScreenshotCreator] Cutout Screenshot (" + num3 + "x" + num4 + " at " + num + "," + num2 + ") saved to: " + text);
	}

	public void method_4()
	{
		if (cutoutSize.x <= 8f || cutoutSize.y <= 8f)
		{
			Debug.Log("[ScreenshotCreator] A size of less than 8x8 pixels for Cutout has been detected!");
			if (Screen.width >= 500 && Screen.height >= 500)
			{
				Debug.Log("[ScreenshotCreator] Reset to " + Screen.width + "x" + Screen.height + " pixels!");
				cutoutSize = new Vector2(Screen.width, Screen.height);
			}
			else
			{
				Debug.Log("[ScreenshotCreator] Reset to 500x500 pixels!");
				cutoutSize = new Vector2(500f, 500f);
			}
		}
	}

	public void CaptureScreenshots(int id, bool fallback)
	{
		if (singleCamera)
		{
			method_0(id);
		}
		string text = method_1() + getFileName(id);
		ScreenCapture.CaptureScreenshot(text, captureSizeMultiplier);
		if (fallback)
		{
			Debug.Log("Fallback to Application.CaptureScreenshot because a GameObject without Camera (or Camera group) was used. Screenshot saved to: " + text);
		}
		else
		{
			Debug.Log("Screenshot saved to: " + text);
		}
	}

	public void CaptureRenderTexture(Camera attachedCam, int id)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].cam != null)
			{
				list[i].cam.SetActive(value: false);
			}
		}
		list[id].cam.SetActive(value: true);
		string text = method_1() + getFileName(id);
		int num = (int)((float)attachedCam.pixelWidth * renderSizeMultiplier);
		int num2 = (int)((float)attachedCam.pixelHeight * renderSizeMultiplier);
		RenderTexture renderTexture = new RenderTexture(num, num2, 24);
		renderTexture.name = "ScreenshotCreater RT";
		attachedCam.targetTexture = renderTexture;
		Texture2D obj = new Texture2D(num, num2, TextureFormat.RGB24, mipChain: false)
		{
			name = "ScreenshotCreator"
		};
		attachedCam.Render();
		RenderTexture.active = renderTexture;
		obj.ReadPixels(new Rect(0f, 0f, num, num2), 0, 0);
		attachedCam.targetTexture = null;
		RenderTexture.active = null;
		UnityEngine.Object.DestroyImmediate(renderTexture);
		byte[] bytes = obj.EncodeToPNG();
		File.WriteAllBytes(text, bytes);
		Debug.Log("Screenshot saved to: " + text);
	}

	public string getFileName(int camID)
	{
		string text = "";
		text = ((!(customName != "")) ? (text + Application.dataPath.Split("/"[0])[^2]) : (text + customName));
		if (includeCamName)
		{
			text += "_";
			if (camID >= 0 && camID < list.Count && list[camID] != null && !(list[camID].cam == null))
			{
				text += list[camID].cam.name;
				lastCamID = camID;
			}
			else
			{
				text += "CameraName";
				lastCamID = 0;
			}
		}
		if (includeDate)
		{
			text += "_";
			text += EFTDateTimeClass.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		}
		if (includeResolution)
		{
			text += "_";
			text += getResolution();
		}
		if (fileType == FileType.JPG)
		{
			text += ".jpg";
		}
		else if (fileType == FileType.PNG)
		{
			text += ".png";
		}
		return text;
	}

	public string getResolution()
	{
		if (lastCam == null || list[lastCamID].cam != lastCam.gameObject)
		{
			if (list[lastCamID].cam != null)
			{
				lastCam = list[lastCamID].cam.GetComponentInChildren<Camera>();
			}
			else
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != null && !(list[i].cam == null))
					{
						lastCam = list[i].cam.GetComponentInChildren<Camera>();
						if (lastCam != null)
						{
							break;
						}
					}
				}
			}
		}
		if (lastCam == null)
		{
			return "-x-";
		}
		if (captureMethod == CaptureMethod.RenderTexture)
		{
			return (int)((float)lastCam.pixelWidth * renderSizeMultiplier) + "x" + (int)((float)lastCam.pixelHeight * renderSizeMultiplier);
		}
		return lastCam.pixelWidth * captureSizeMultiplier + "x" + lastCam.pixelHeight * captureSizeMultiplier;
	}
}
