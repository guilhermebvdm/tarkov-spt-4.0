using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GClass986
{
	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	public RenderTexture RenderTexture_0;

	[NonSerialized]
	public float Float_1 = 1f;

	[NonSerialized]
	public Vector2 Vector2_0 = new Vector2(0.01f, 0.4f);

	[NonSerialized]
	public float Float_2 = 10f;

	[NonSerialized]
	public float Float_3 = 10f;

	[NonSerialized]
	public int Int_0 = 20;

	[NonSerialized]
	public const float Float_4 = 1.2f;

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public Queue<GClass987> Queue_0 = new Queue<GClass987>();

	[NonSerialized]
	public List<GClass987> List_0 = new List<GClass987>();

	[NonSerialized]
	public List<GClass987> List_1 = new List<GClass987>();

	[NonSerialized]
	public Transform Transform_0;

	[NonSerialized]
	public Camera Camera_0;

	[NonSerialized]
	public Material Material_0;

	[NonSerialized]
	public const float Float_5 = 0.15f;

	[NonSerialized]
	public float Float_6;

	[NonSerialized]
	public bool Bool_1 = true;

	[NonSerialized]
	public float Float_7 = 1f;

	[NonSerialized]
	public float Float_8 = 1f;

	[NonSerialized]
	public float Float_9 = 1f;

	public float Intensity
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public GClass986(AnimationCurve scaleCurve, RenderTexture dropsTexture, int dropsAmount, float rainDropsDelay, Vector2 dropsScale, float dropLifetime, float dropLifetimeWithoutGlass, int maxDropsAtOnce, Camera targetCamera, Material dropMaterial, float offGlassScaleMultiplier)
	{
		UpdateValues(scaleCurve, dropsTexture, dropsAmount, rainDropsDelay, dropsScale, dropLifetime, dropLifetimeWithoutGlass, maxDropsAtOnce, targetCamera, dropMaterial, offGlassScaleMultiplier);
	}

	public void UpdateValues(AnimationCurve scaleCurve, RenderTexture dropsTexture, int dropsAmount, float rainDropsDelay, Vector2 dropsScale, float dropLifetime, float dropLifetimeWithoutGlass, int maxDropsAtOnce, Camera targetCamera, Material dropMaterial, float offGlassScaleMultiplier)
	{
		RenderTexture_0 = dropsTexture;
		Float_1 = rainDropsDelay;
		Vector2_0 = dropsScale;
		Float_2 = dropLifetime;
		Float_3 = dropLifetimeWithoutGlass;
		Int_0 = maxDropsAtOnce;
		Camera_0 = targetCamera;
		Transform_0 = Camera_0.transform;
		Material_0 = dropMaterial;
		Float_7 = offGlassScaleMultiplier;
		Queue_0.Clear();
		for (int i = 0; i < dropsAmount; i++)
		{
			GClass987 item = new GClass987(scaleCurve, method_2, Material_0, Camera_0.aspect);
			Queue_0.Enqueue(item);
		}
		method_0(1000f);
		Clear();
	}

	public void Update(float dt)
	{
		bool isCameraUnderRain = RainController.IsCameraUnderRain;
		if (Intensity > 0.15f && isCameraUnderRain)
		{
			float num = Mathf.Clamp01((Vector3.Dot(Transform_0.forward, Vector3.up) + 1f) * 0.5f * 1.2f);
			Float_6 += dt * UnityEngine.Random.Range(0.7f, 3f) * num;
			if (Float_6 > Float_1)
			{
				int num2 = Mathf.CeilToInt((float)UnityEngine.Random.Range(1, Int_0) * num * Intensity);
				for (int i = 0; i < num2; i++)
				{
					if (Queue_0.Count > 0 && num > 0.2f)
					{
						method_1(num);
					}
				}
				Float_6 = 0f;
			}
		}
		if (List_0.Count != 0)
		{
			method_0(dt);
		}
	}

	public void Clear()
	{
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(RenderTexture_0);
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		Graphics.SetRenderTarget(active);
	}

	public void method_0(float dt)
	{
		RenderTexture active = RenderTexture.active;
		Graphics.SetRenderTarget(RenderTexture_0);
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		foreach (GClass987 item in List_0)
		{
			item.UpdateDrop(dt);
			item.Draw();
		}
		RenderTexture.active = active;
		foreach (GClass987 item2 in List_1)
		{
			List_0.Remove(item2);
		}
		List_1.Clear();
	}

	public void method_1(float amount)
	{
		GClass987 gClass = Queue_0.Dequeue();
		Vector3 localPosition = new Vector3(UnityEngine.Random.Range(-1f, 1f), amount * UnityEngine.Random.Range(-2f, 0f) + 1f, 5f);
		if (!Bool_0)
		{
			gClass.Lifetime = Float_9 * UnityEngine.Random.Range(0.3f, 1f) * Mathf.Max(amount, 0.5f);
		}
		else
		{
			gClass.Lifetime = Float_9 * UnityEngine.Random.Range(0.3f, 1f) * Mathf.Max(1f - amount, 0.5f);
		}
		gClass.Scale = Vector2_0 * Float_8;
		gClass.Use(amount, Intensity, localPosition);
		List_0.Add(gClass);
	}

	public void method_2(GClass987 drop)
	{
		Queue_0.Enqueue(drop);
		List_1.Add(drop);
	}

	public void ChangeGlassState(bool hasGlasses)
	{
		Float_8 = (hasGlasses ? 1f : Float_7);
		Float_9 = (hasGlasses ? Float_2 : Float_3);
	}
}
