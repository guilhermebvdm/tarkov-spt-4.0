using System;
using System.Collections.Generic;
using UnityEngine;

public class GClass958
{
	[NonSerialized]
	public int Int_0 = 1;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public const int Int_2 = 64;

	[NonSerialized]
	public Vector2 Vector2_0;

	[NonSerialized]
	public Queue<GClass956> Queue_0 = new Queue<GClass956>();

	[NonSerialized]
	public Queue<GClass956> Queue_1 = new Queue<GClass956>();

	[NonSerialized]
	public RenderTexture RenderTexture_0;

	public GClass957 DropPlacer;

	public Action<RenderTexture> OnImageRendered;

	[NonSerialized]
	public float Float_0 = 1f;

	[NonSerialized]
	public float Float_1 = 1f;

	public int Int32_0
	{
		get
		{
			return Int_0;
		}
		set
		{
			Int_0 = Mathf.Min(value, 64);
		}
	}

	public GClass958(float aspect, int renderTextureDimension, Material dropMaterial, Vector2 dropLifetimeDistribution, float offGlassLifetimeMultiplier)
	{
		Vector2_0 = dropLifetimeDistribution;
		DropPlacer = new GClass957();
		RenderTexture_0 = new RenderTexture(Mathf.CeilToInt(Mathf.Pow(2f, renderTextureDimension)), Mathf.CeilToInt(Mathf.Pow(2f, renderTextureDimension)), 0)
		{
			name = "BloodDropsController _dropsTex"
		};
		Float_0 = offGlassLifetimeMultiplier;
		for (int i = 0; i < 64; i++)
		{
			Queue_1.Enqueue(new GClass956(dropMaterial, aspect));
		}
	}

	public void CaptureBloodShot(Vector3 direction)
	{
		foreach (GClass956 dropsPosition in DropPlacer.GetDropsPositions(direction, Queue_1))
		{
			Queue_0.Enqueue(dropsPosition);
		}
	}

	public void CaptureBloodShotBleeding(Vector3 direction, bool isArterial)
	{
		foreach (GClass956 item in DropPlacer.GetDropsPositionsBleeding(direction, Queue_1, isArterial))
		{
			Queue_0.Enqueue(item);
		}
	}

	public void Update()
	{
		if (Queue_0.Count != 0)
		{
			RenderTexture active = RenderTexture.active;
			Graphics.SetRenderTarget(RenderTexture_0);
			GL.Clear(clearDepth: true, clearColor: true, Color.black);
			int num = 1;
			if (Queue_0.Peek().DropNumber == 0 && Queue_0.Count >= Int32_0)
			{
				num = UnityEngine.Random.Range(1, Int32_0);
			}
			for (int i = 0; i < num; i++)
			{
				GClass956 gClass = Queue_0.Dequeue();
				gClass.SetLifetime(UnityEngine.Random.Range(Vector2_0.x, Vector2_0.y) * Float_1);
				gClass.Draw();
				Queue_1.Enqueue(gClass);
			}
			Graphics.SetRenderTarget(active);
			OnImageRendered?.Invoke(RenderTexture_0);
		}
	}

	public void ChangeGlassState(bool hasGlasses)
	{
		Float_1 = (hasGlasses ? 1f : Float_0);
	}

	public void Destroy()
	{
		if (RenderTexture_0 != null)
		{
			UnityEngine.Object.Destroy(RenderTexture_0);
		}
	}
}
