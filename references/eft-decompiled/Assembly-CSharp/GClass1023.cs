using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MultiFlare;
using UnityEngine;

public class GClass1023
{
	[NonSerialized]
	public static GClass1023 Gclass1023_0;

	[NonSerialized]
	public GClass1030 Gclass1030_0;

	[NonSerialized]
	public GClass1029 Gclass1029_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public static GClass1023 Instance => Gclass1023_0 ?? (Gclass1023_0 = new GClass1023());

	public bool IsInitialized
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public bool IsRenderingEnabled
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public GInterface59 FlareRenderer => Gclass1029_0;

	public GInterface60 LightStorage => Gclass1030_0;

	public void Initialize()
	{
		Dispose();
		Gclass1030_0 = new GClass1030();
		IsInitialized = true;
	}

	public void SetupRenderer(FlareSettings settings)
	{
		if (!settings.IsPopulated)
		{
			return;
		}
		Gclass1029_0?.Dispose();
		Gclass1029_0 = new GClass1029(Gclass1030_0);
		Gclass1029_0.RegisterBatch(FlareType.Normal, new GClass1027(settings.Atlas, settings.NormalBatch, Gclass1030_0));
		Gclass1029_0.RegisterBatch(FlareType.Noise, new GClass1027(settings.Atlas, settings.ShitBatch, Gclass1030_0));
		Gclass1029_0.RegisterBatch(FlareType.FlashLight, new GClass1028(settings.Atlas, settings.FlareOverlapSettings, Gclass1030_0));
		foreach (var item in Gclass1030_0.GetAll())
		{
			Gclass1029_0.AddFlare(item.Item2, item.Item1);
		}
	}

	public void DisposeRenderer()
	{
		Gclass1029_0?.Dispose();
		Gclass1029_0 = null;
	}

	public void Dispose()
	{
		Gclass1029_0?.Dispose();
		Gclass1030_0?.Dispose();
		Gclass1029_0 = null;
		Gclass1030_0 = null;
		IsInitialized = false;
	}

	public void AddLight(FlareLight flareLight)
	{
		if ((bool)flareLight && !Gclass1030_0.Contains(flareLight) && flareLight.Flares != null)
		{
			GClass1029 gclass1029_ = Gclass1029_0;
			if (gclass1029_ != null && gclass1029_.IsRendering)
			{
				Gclass1029_0.CompleteRendering();
			}
			int lightIndex = Gclass1030_0.Add(flareLight);
			Gclass1029_0?.AddFlare(lightIndex, flareLight);
		}
	}

	public void RemoveLight(FlareLight flareLight)
	{
		if (Gclass1030_0.Contains(flareLight))
		{
			GClass1029 gclass1029_ = Gclass1029_0;
			if (gclass1029_ != null && gclass1029_.IsRendering)
			{
				Gclass1029_0.CompleteRendering();
			}
			Gclass1030_0.Remove(flareLight, out var removedIndex, out var replacedIndex);
			Gclass1029_0?.RemoveFlare(removedIndex, replacedIndex);
		}
	}

	public ref GStruct76 AccessLightData(FlareLight flareLight)
	{
		return ref Gclass1030_0.Get(flareLight);
	}

	public void AddCamera(GClass1025 camera)
	{
		if (Gclass1029_0 != null)
		{
			Gclass1029_0.AddCamera(camera);
		}
	}

	public void RemoveCamera(Camera camera)
	{
		if (Gclass1029_0 != null)
		{
			Gclass1029_0.RemoveCamera(camera);
		}
	}

	public void StartRendering()
	{
		if (IsRenderingEnabled)
		{
			Gclass1029_0?.StartRendering();
		}
	}

	public void CompleteRendering()
	{
		if (IsRenderingEnabled)
		{
			Gclass1029_0?.CompleteRendering();
		}
	}

	public void RenderInstantBatches()
	{
		if (IsRenderingEnabled)
		{
			Gclass1029_0?.RenderInstantBatches();
		}
	}

	[Conditional("DEBUG")]
	public void RenderNow()
	{
		if (Gclass1029_0 != null && !Application.isPlaying && IsRenderingEnabled)
		{
			Gclass1029_0.StartRendering();
			Gclass1029_0.CompleteRendering();
			Gclass1029_0.RenderInstantBatches();
		}
	}
}
