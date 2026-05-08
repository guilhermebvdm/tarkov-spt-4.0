using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public abstract class TOD_ImageEffect : MonoBehaviour
{
	private static readonly int int_0 = Shader.PropertyToID("_MainTex");

	protected Camera cam;

	private TOD_Sky tod_Sky_0;

	private bool bool_0;

	private bool bool_1;

	public TOD_Sky Sky
	{
		get
		{
			return tod_Sky_0;
		}
		set
		{
			tod_Sky_0 = value;
		}
	}

	public Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			Debug.Log("Missing shader in " + ToString());
			base.enabled = false;
			return null;
		}
		if (!shader.isSupported)
		{
			Debug.LogError("The shader " + shader.ToString() + " on effect " + ToString() + " is not supported on this platform!");
			base.enabled = false;
			return null;
		}
		return new Material(shader)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	public void Awake()
	{
		bool_0 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
		bool_1 = SystemInfo.SupportsRenderTextureFormat(RuntimeUtilities.defaultHDRRenderTextureFormat);
		if (!cam)
		{
			cam = GetComponent<Camera>();
		}
		if (!tod_Sky_0)
		{
			tod_Sky_0 = MonoBehaviourSingleton<TOD_Sky>.Instance;
		}
	}

	public bool CheckSupport(bool needDepth = false, bool needHdr = false)
	{
		if (!cam)
		{
			return false;
		}
		if ((bool)tod_Sky_0 && tod_Sky_0.Initialized)
		{
			if (!SystemInfo.supportsImageEffects)
			{
				Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
				base.enabled = false;
				return false;
			}
			if (needDepth && !bool_0)
			{
				Debug.LogWarning("The image effect " + ToString() + " has been disabled as it requires a depth texture.");
				base.enabled = false;
				return false;
			}
			if (needHdr && !bool_1)
			{
				Debug.LogWarning("The image effect " + ToString() + " has been disabled as it requires HDR.");
				base.enabled = false;
				return false;
			}
			if (needDepth)
			{
				cam.depthTextureMode |= DepthTextureMode.Depth;
			}
			if (needHdr)
			{
				cam.allowHDR = true;
			}
			return true;
		}
		return false;
	}

	public void DrawBorder(RenderTexture dest, Material material)
	{
		RenderTexture.active = dest;
		bool flag = true;
		GL.PushMatrix();
		GL.LoadOrtho();
		for (int i = 0; i < material.passCount; i++)
		{
			material.SetPass(i);
			float y;
			float y2;
			if (flag)
			{
				y = 1f;
				y2 = 0f;
			}
			else
			{
				y = 0f;
				y2 = 1f;
			}
			float x = 0f + 1f / ((float)dest.width * 1f);
			float y3 = 0f;
			float y4 = 1f;
			GL.Begin(7);
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			float x2 = 1f - 1f / ((float)dest.width * 1f);
			x = 1f;
			y3 = 0f;
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(x2, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(x2, y4, 0.1f);
			x = 1f;
			y3 = 0f;
			y4 = 0f + 1f / ((float)dest.height * 1f);
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			x = 1f;
			y3 = 1f - 1f / ((float)dest.height * 1f);
			y4 = 1f;
			GL.TexCoord2(0f, y);
			GL.Vertex3(0f, y3, 0.1f);
			GL.TexCoord2(1f, y);
			GL.Vertex3(x, y3, 0.1f);
			GL.TexCoord2(1f, y2);
			GL.Vertex3(x, y4, 0.1f);
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, y4, 0.1f);
			GL.End();
		}
		GL.PopMatrix();
	}

	public void CustomBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr = 0)
	{
		RenderTexture.active = dest;
		fxMaterial.SetTexture(int_0, source);
		GL.PushMatrix();
		GL.LoadOrtho();
		fxMaterial.SetPass(passNr);
		GL.Begin(7);
		GL.MultiTexCoord2(0, 0f, 0f);
		GL.Vertex3(0f, 0f, 3f);
		GL.MultiTexCoord2(0, 1f, 0f);
		GL.Vertex3(1f, 0f, 2f);
		GL.MultiTexCoord2(0, 1f, 1f);
		GL.Vertex3(1f, 1f, 1f);
		GL.MultiTexCoord2(0, 0f, 1f);
		GL.Vertex3(0f, 1f, 0f);
		GL.End();
		GL.PopMatrix();
	}

	public TOD_ImageEffect()
	{
	}
}
