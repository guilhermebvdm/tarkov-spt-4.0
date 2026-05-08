using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class StuckFPS : MonoBehaviour
{
	[SerializeField]
	private int _minFramerate = 60;

	[SerializeField]
	private int _maxFramerate = 60;

	private RenderTexture renderTexture_0;

	private float float_0;

	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (renderTexture_0 == null)
		{
			renderTexture_0 = new RenderTexture(source.width, source.height, source.depth)
			{
				name = "Last Frame"
			};
		}
		int num = Random.Range(_minFramerate, _maxFramerate);
		if (Time.time - float_0 > 1f / (float)num)
		{
			Graphics.Blit(source, renderTexture_0);
			float_0 = Time.time;
		}
		Graphics.Blit(renderTexture_0, destination);
	}

	public void OnDestroy()
	{
		Object.DestroyImmediate(renderTexture_0);
	}
}
