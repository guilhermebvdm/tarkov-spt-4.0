using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class PlaceAndScreenObject : MonoBehaviour
{
	private List<Quaternion> list_0;

	private GameObject gameObject_0;

	[SerializeField]
	private Camera _camera;

	public void Init(List<Vector3> screenshotAngles)
	{
		if (!_camera)
		{
			_camera = Camera.main;
		}
		list_0 = new List<Quaternion>();
		foreach (Vector3 screenshotAngle in screenshotAngles)
		{
			list_0.Add(Quaternion.Euler(screenshotAngle));
		}
	}

	public void PlaceObject(GameObject prefab)
	{
		if (gameObject_0 != null)
		{
			Object.DestroyImmediate(gameObject_0);
			gameObject_0 = null;
		}
		gameObject_0 = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
		gameObject_0.name = prefab.name;
	}

	public string TakeScreenshot(string filePath, int texWidth, int texHeight)
	{
		if (gameObject_0 == null)
		{
			return null;
		}
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}
		string text = string.Empty;
		List<string> list = new List<string>();
		for (int i = 0; i < list_0.Count; i++)
		{
			gameObject_0.transform.rotation = list_0[i];
			GClass926.CenterByBounds(gameObject_0, _camera, 1f, new PreviewPivot.IconSettings());
			string text2 = $"{filePath}/{gameObject_0.name}{i}.png";
			list.Add(text2);
			method_0(text2, texWidth, texHeight);
			if (string.IsNullOrEmpty(text))
			{
				text = text2;
			}
		}
		return text;
	}

	public void ClearScene()
	{
		if (gameObject_0 != null)
		{
			Object.DestroyImmediate(gameObject_0);
		}
	}

	public void method_0(string filePath, int texWidth, int texHeight)
	{
		_camera.clearFlags = CameraClearFlags.Color;
		_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
		_camera.useOcclusionCulling = false;
		RenderTexture renderTexture = new RenderTexture(texWidth, texHeight, 24)
		{
			name = "PlaceAndScreenObject RT"
		};
		_camera.targetTexture = renderTexture;
		Texture2D obj = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, mipChain: false)
		{
			name = "ScreenshotPlaceAndScreen"
		};
		_camera.Render();
		RenderTexture.active = renderTexture;
		obj.ReadPixels(new Rect(0f, 0f, texWidth, texHeight), 0, 0);
		_camera.targetTexture = null;
		RenderTexture.active = null;
		Object.DestroyImmediate(renderTexture);
		byte[] bytes = obj.EncodeToPNG();
		File.WriteAllBytes(filePath, bytes);
	}
}
