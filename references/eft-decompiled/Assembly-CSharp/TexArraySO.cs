using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TextureArray", menuName = "Scriptable objects/TextureArray")]
[PreferBinarySerialization]
public class TexArraySO : ScriptableObject
{
	[Delayed]
	[SerializeField]
	private int _width = 256;

	[Delayed]
	[SerializeField]
	private int _height = 256;

	[SerializeField]
	private TextureFormat _format = TextureFormat.ARGB32;

	[SerializeField]
	private bool _mips = true;

	[SerializeField]
	private Texture2D[] _textures;

	[SerializeField]
	[HideInInspector]
	private Texture2DArray _textureArray;

	[SerializeField]
	[HideInInspector]
	private bool _hadMips;

	public static void smethod_0(Texture2DArray textureArray, Texture2D[] textures)
	{
		for (int i = 0; i < textures.Length; i++)
		{
			Color32[] colors = smethod_1(textures[i], textureArray.width, textureArray.height);
			textureArray.SetPixels32(colors, i);
		}
		textureArray.Apply(updateMipmaps: true, makeNoLongerReadable: true);
	}

	public static Color32[] smethod_1(Texture2D texture, int width, int height)
	{
		RenderTexture active = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
		Graphics.Blit(texture, temporary);
		RenderTexture.active = temporary;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, recalculateMipMaps: false);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: false);
		Color32[] pixels = texture2D.GetPixels32();
		Object.DestroyImmediate(texture2D);
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return pixels;
	}

	public bool method_0(Texture2D[] textures, ref string errorMessage, ref InfoMessageType? messageType)
	{
		if (textures.Length < 1)
		{
			errorMessage = " Must be at least 1 texture";
			messageType = InfoMessageType.Error;
			return false;
		}
		HashSet<Texture2D> hashSet = new HashSet<Texture2D>();
		int num = 0;
		while (true)
		{
			if (num < textures.Length)
			{
				if (hashSet.Contains(textures[num]))
				{
					break;
				}
				hashSet.Add(textures[num]);
				num++;
				continue;
			}
			return true;
		}
		errorMessage = $"The array contains entries of the same texture '{textures[num].name}'";
		messageType = InfoMessageType.Error;
		return false;
	}

	public bool method_1(Texture2D[] textures, ref string errorMessage, ref InfoMessageType? messageType)
	{
		new HashSet<Texture2D>();
		int num = 0;
		while (true)
		{
			if (num < textures.Length)
			{
				if (textures[num].width < _width || textures[num].height < _height)
				{
					break;
				}
				num++;
				continue;
			}
			return true;
		}
		errorMessage = $"'{textures[num].name}' texture have smaller resolution than the array, it will be scaled up";
		messageType = InfoMessageType.Warning;
		return false;
	}
}
