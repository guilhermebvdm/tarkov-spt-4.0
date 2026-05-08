using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MultiFlare;
using Unity.Jobs;
using UnityEngine;

public class GClass1029 : GInterface59
{
	[Serializable]
	[CompilerGenerated]
	public class Class716
	{
		public static readonly Class716 class716_0 = new Class716();

		public static Func<GClass1025, Camera> func_0;

		public Camera method_0(GClass1025 renderer)
		{
			return renderer.Camera;
		}
	}

	[CompilerGenerated]
	public class Class717
	{
		public Camera camera;

		public bool method_0(GClass1025 cameraRenderer)
		{
			return cameraRenderer.Camera == camera;
		}
	}

	[NonSerialized]
	public const string String_0 = "OVERDRAW_MODE";

	[NonSerialized]
	public HashSet<GClass1025> HashSet_0 = new HashSet<GClass1025>();

	[NonSerialized]
	public GClass1024 Gclass1024_0;

	[NonSerialized]
	public GClass1030 Gclass1030_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool IsRendering
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

	public bool IsDebugModeEnabled
	{
		get
		{
			return Shader.IsKeywordEnabled("OVERDRAW_MODE");
		}
		set
		{
			if (value)
			{
				Shader.EnableKeyword("OVERDRAW_MODE");
			}
			else
			{
				Shader.DisableKeyword("OVERDRAW_MODE");
			}
		}
	}

	public IEnumerable<Camera> GetCameras()
	{
		return HashSet_0.Select((GClass1025 renderer) => renderer.Camera);
	}

	public IEnumerable<GInterface58> GetBatches()
	{
		return Gclass1024_0;
	}

	public GClass1029(GClass1030 lightStorage)
	{
		Gclass1030_0 = lightStorage;
		Gclass1024_0 = new GClass1024();
	}

	public void RegisterBatch(FlareType flareType, GClass1027 batch)
	{
		Gclass1024_0.Register(flareType, batch);
	}

	public void AddFlare(int lightIndex, FlareLight flareLight)
	{
		for (int i = 0; i < flareLight.Flares.Count; i++)
		{
			MultiFlare.Flare flare = flareLight.Flares[i];
			Gclass1024_0[flare.FlareType].Add(lightIndex, flare);
		}
	}

	public void RemoveFlare(int lightIndex, int replacedIndex)
	{
		foreach (GClass1027 item in Gclass1024_0)
		{
			item.Remove(lightIndex, replacedIndex);
		}
	}

	public void Dispose()
	{
		foreach (GClass1025 item in HashSet_0)
		{
			item.Dispose();
		}
		foreach (GClass1027 item2 in Gclass1024_0)
		{
			item2.Dispose();
		}
		HashSet_0.Clear();
		Gclass1024_0.Clear();
	}

	public void AddCamera(GClass1025 camera)
	{
		HashSet_0.Add(camera);
	}

	public void RemoveCamera(Camera camera)
	{
		GClass1025 gClass = HashSet_0.FirstOrDefault((GClass1025 cameraRenderer) => cameraRenderer.Camera == camera);
		if (gClass != null)
		{
			gClass.Dispose();
			HashSet_0.Remove(gClass);
		}
	}

	public void StartRendering()
	{
		foreach (GClass1025 item in HashSet_0)
		{
			item.StartRendering();
		}
		JobHandle dependsOn = Gclass1030_0.UpdatePositions();
		for (int i = 0; i < Gclass1024_0.DefaultBatches.Count; i++)
		{
			Gclass1024_0.DefaultBatches[i].Schedule(dependsOn);
		}
		JobHandle.ScheduleBatchedJobs();
		IsRendering = true;
	}

	public void CompleteRendering()
	{
		for (int i = 0; i < Gclass1024_0.DefaultBatches.Count; i++)
		{
			Gclass1024_0.DefaultBatches[i].Complete();
		}
		method_0(Gclass1024_0.DefaultBatches);
		Gclass1030_0.DisposeIntermediateData();
		IsRendering = false;
	}

	public void RenderInstantBatches()
	{
		JobHandle dependsOn = Gclass1030_0.UpdatePositions();
		for (int i = 0; i < Gclass1024_0.InstantBatches.Count; i++)
		{
			GClass1027 gClass = Gclass1024_0.InstantBatches[i];
			gClass.Schedule(dependsOn);
			gClass.Complete();
		}
		method_0(Gclass1024_0.InstantBatches);
		Gclass1030_0.DisposeIntermediateData();
	}

	public void method_0(IReadOnlyList<GClass1027> batches)
	{
		for (int i = 0; i < batches.Count; i++)
		{
			foreach (GClass1025 item in HashSet_0)
			{
				item.ApplyRenderCommands(batches[i]);
			}
		}
	}
}
