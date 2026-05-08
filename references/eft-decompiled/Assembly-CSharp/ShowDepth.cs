using UnityEngine;

public class ShowDepth : MonoBehaviour
{
	private Material material_0;

	public void Start()
	{
		material_0 = new Material(GClass872.Find("Hidden/ShowDepth"));
	}

	public void OnRenderImage(RenderTexture s, RenderTexture d)
	{
		Graphics.Blit(s, d, material_0);
	}
}
