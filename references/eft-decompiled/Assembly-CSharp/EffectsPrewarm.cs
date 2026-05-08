using System;
using System.Linq;
using Systems.Effects;
using Comfort.Common;
using EFT.Ballistics;
using UnityEngine;

public class EffectsPrewarm : MonoBehaviour
{
	private GameObject gameObject_0;

	private Camera camera_0;

	private RenderTexture renderTexture_0;

	public void Start()
	{
		method_0();
		method_1();
		method_2();
	}

	public void method_0()
	{
		if (gameObject_0 == null)
		{
			gameObject_0 = new GameObject("EffectsPrewarmCamera", typeof(Camera));
			gameObject_0.transform.SetParent(base.transform);
			gameObject_0.transform.position = new Vector3(0f, -1000f, 0f);
		}
		camera_0 = gameObject_0.GetComponent<Camera>();
		renderTexture_0 = new RenderTexture(camera_0.pixelWidth, camera_0.pixelHeight, 24);
		camera_0.name = "Effects Prewarm Camera RT";
		camera_0.targetTexture = renderTexture_0;
	}

	public void method_1()
	{
		ParticleSystem[] array = GClass870.FindUnityObjectsOfType<ParticleSystem>();
		Vector3 position = camera_0.transform.position + camera_0.transform.forward;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Emit(new ParticleSystem.EmitParams
			{
				position = position,
				startLifetime = 1f,
				velocity = Vector3.zero,
				startColor = Color.white,
				startSize = 1f
			}, 1);
		}
		foreach (MaterialType item in Enum.GetValues(typeof(MaterialType)).Cast<MaterialType>())
		{
			Singleton<Effects>.Instance.PrewarmEmit(item, null, position, Vector3.one);
		}
	}

	public void method_2()
	{
		UnityEngine.Object.DestroyImmediate(gameObject_0);
		UnityEngine.Object.DestroyImmediate(renderTexture_0);
	}
}
