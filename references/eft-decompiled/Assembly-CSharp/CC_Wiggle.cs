using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Wiggle")]
public class CC_Wiggle : CC_Base
{
	public float timer;

	public float speed = 1f;

	public float scale = 12f;

	public float str = 1f;

	public bool autoTimer = true;

	private static readonly int int_0 = Shader.PropertyToID("_Timer");

	private static readonly int int_1 = Shader.PropertyToID("_Scale");

	private static readonly int int_2 = Shader.PropertyToID("_Str");

	public virtual void Update()
	{
		if (autoTimer)
		{
			timer += speed * Time.deltaTime;
		}
	}

	public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.GetSourceDestination(out source, out destination);
		}
		base.material.SetFloat(int_0, timer);
		base.material.SetFloat(int_1, scale);
		base.material.SetFloat(int_2, str);
		Graphics.Blit(source, destination, base.material);
		if (_ssaaPropagator != null)
		{
			_ssaaPropagator.ReleaseSourceDestination(source, destination);
		}
	}
}
