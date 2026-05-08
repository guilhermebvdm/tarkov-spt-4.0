using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IconShadow : MonoBehaviour
{
	[SerializeField]
	private Color _shadowColor = Color.black;

	[SerializeField]
	private Vector2 _shadowOffset = new Vector2(-5f, 5f);

	private Material material_0;

	private Material material_1;

	private Vector4 vector4_0;

	private Camera camera_0;

	private static readonly int int_0 = Shader.PropertyToID("_TextureSize");

	private static readonly int int_1 = Shader.PropertyToID("_ShadowOffset");

	private static readonly int int_2 = Shader.PropertyToID("_Color");

	private static readonly int int_3 = Shader.PropertyToID("_Shadow");

	public Material Material_0
	{
		get
		{
			if (material_0 != null)
			{
				return material_0;
			}
			Shader shader = GClass872.Find("p0/IconShadow");
			if (shader == null || !shader.isSupported)
			{
				base.enabled = false;
			}
			material_0 = ((shader != null) ? new Material(shader) : null);
			if (material_0 == null)
			{
				base.enabled = false;
			}
			else
			{
				material_0.SetVector(int_0, vector4_0);
				material_0.SetVector(int_1, new Vector4(_shadowOffset.x, _shadowOffset.y));
				material_0.SetVector(int_2, _shadowColor);
			}
			return material_0;
		}
	}

	public Material Material_1
	{
		get
		{
			if (material_1 != null)
			{
				return material_1;
			}
			Shader shader = GClass872.Find("p0/IconShadowBlit");
			if (shader == null || !shader.isSupported)
			{
				base.enabled = false;
			}
			material_1 = ((shader != null) ? new Material(shader) : null);
			if (material_1 == null)
			{
				base.enabled = false;
			}
			return material_1;
		}
	}

	public void SetTexDimension(int width, int height)
	{
		vector4_0 = new Vector4(width, height);
	}

	public void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		if (camera_0 == null)
		{
			camera_0 = GetComponent<Camera>();
		}
		RenderTexture temporary = RenderTexture.GetTemporary(camera_0.targetTexture.width, camera_0.targetTexture.height);
		RenderTexture temporary2 = RenderTexture.GetTemporary(camera_0.targetTexture.width / 4, camera_0.targetTexture.height / 4);
		Graphics.Blit(source, temporary, Material_0);
		for (int i = 0; i < 8; i++)
		{
			Graphics.Blit(temporary, temporary2);
			Graphics.Blit(temporary2, temporary);
		}
		Material_1.SetTexture(int_3, temporary);
		Graphics.Blit(source, dest, Material_1);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
