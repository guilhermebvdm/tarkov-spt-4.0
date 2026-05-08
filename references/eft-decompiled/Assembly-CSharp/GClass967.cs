using System;
using System.Collections.Generic;
using System.Threading;
using Comfort.Common;
using UnityEngine;

public class GClass967 : GClass965<GClass971, GClass964>
{
	public class Class607
	{
		public int updatedDuringFrame;

		public int requestedMipMapLevel = 100;

		public float pixelsHBuf;

		public float pixelsVBuf;

		public float closestDist;
	}

	public class Class608
	{
		public GClass967 tree;
	}

	public class Class609
	{
		public GClass2380 frustum = new GClass2380();

		public float maxScreenHScale;

		public float maxScreenVScale;

		public int cameraInstanceID;

		public int frameIndex;
	}

	[NonSerialized]
	public static Thread Thread_0;

	[NonSerialized]
	public static CancellationTokenSource CancellationTokenSource_0;

	[NonSerialized]
	public static CancellationToken CancellationToken_0;

	public static int ThreadSleepTime = 100;

	public static int ThreadMinSleepTime = 10;

	public static bool ThreadSkipJob = false;

	public static int MipOffset = -1;

	public static int DropResultsAfterFramesCount = 240;

	[NonSerialized]
	public static HashSet<GClass967> HashSet_0 = new HashSet<GClass967>();

	[NonSerialized]
	public static object Object_0 = new object();

	[NonSerialized]
	public Texture2D Texture2D_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public Dictionary<int, Class607> Dictionary_0 = new Dictionary<int, Class607>();

	[NonSerialized]
	public object Object_1 = new object();

	[NonSerialized]
	public static Class609 Class609_0 = new Class609();

	public Texture2D Texture => Texture2D_0;

	public void Dispose()
	{
		Monitor.Enter(Object_0);
		HashSet_0.Remove(this);
		Monitor.Exit(Object_0);
		if (HashSet_0.Count == 0)
		{
			if (CancellationTokenSource_0 != null)
			{
				CancellationTokenSource_0.Cancel();
			}
			if (Thread_0 != null)
			{
				Thread_0.Join();
			}
			Thread_0 = null;
			CancellationTokenSource_0.Dispose();
			CancellationTokenSource_0 = null;
		}
		if (Gclass962_0 != null)
		{
			(Gclass962_0 as GClass964).Dispose();
		}
	}

	public GClass967()
	{
		CancellationTokenSource_0 = new CancellationTokenSource();
		CancellationToken_0 = CancellationTokenSource_0.Token;
		Monitor.Enter(Object_0);
		HashSet_0.Add(this);
		Monitor.Exit(Object_0);
	}

	public override GClass962<GClass971> CreateQuadrant(GClass962<GClass971> parent, Rect bounds)
	{
		return new GClass964(parent as GClass964, bounds);
	}

	public void SetThreadParams(Camera camera, GClass2380 frustum, float maxScreenHScale, float maxScreenVScale)
	{
		if (Monitor.TryEnter(Object_1))
		{
			frustum.CopyTo(Class609_0.frustum);
			Class609_0.maxScreenHScale = maxScreenHScale;
			Class609_0.maxScreenVScale = maxScreenVScale;
			Class609_0.cameraInstanceID = camera.GetInstanceID();
			Class609_0.frameIndex = Time.frameCount;
			if (Thread_0 == null)
			{
				Class608 @class = new Class608();
				@class.tree = this;
				Thread_0 = new Thread(smethod_0);
				Thread_0.Start(@class);
			}
			Monitor.Exit(Object_1);
		}
	}

	public void UpdateTextureRequestedMipMapLevel()
	{
		int num = 0;
		if (Singleton<SharedGameSettingsClass>.Instantiated)
		{
			num = GraphicsSettingsClass.TextureQualityToMipBias(Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.TextureQuality);
		}
		int num2 = Int_0 - 1;
		int frameCount = Time.frameCount;
		foreach (KeyValuePair<int, Class607> item in Dictionary_0)
		{
			if (frameCount - item.Value.updatedDuringFrame <= DropResultsAfterFramesCount)
			{
				int num3 = Math.Min(item.Value.requestedMipMapLevel, Int_0 - 1);
				num3 = Math.Min(num3 + MipOffset + num, Int_0 - 1);
				num3 = Math.Max(num3, -1);
				num2 = Math.Min(num2, num3);
			}
		}
		Texture2D_0.requestedMipmapLevel = num2;
	}

	public void Insert(ref List<GClass971> infosList, Texture2D texture)
	{
		Rect bounds = default(Rect);
		bool flag = false;
		foreach (GClass971 infos in infosList)
		{
			Rect rect = new Rect
			{
				center = new Vector2(infos.pos.x, infos.pos.z),
				size = new Vector2(infos.maxSize, infos.maxSize)
			};
			if (flag)
			{
				bounds.min = Vector2.Min(rect.min, bounds.min);
				bounds.max = Vector2.Max(rect.max, bounds.max);
			}
			else
			{
				bounds = rect;
			}
		}
		base.Bounds = bounds;
		foreach (GClass971 infos2 in infosList)
		{
			Insert(infos2, new Rect
			{
				center = new Vector2(infos2.pos.x, infos2.pos.z),
				size = new Vector2(infos2.maxSize, infos2.maxSize)
			});
		}
		Texture2D_0 = texture;
		Int_0 = Texture2D_0.mipmapCount;
		Int_1 = Texture2D_0.width;
		Int_2 = Texture2D_0.height;
		String_0 = Texture2D_0.name;
		(Gclass962_0 as GClass964).ComputeBounds();
	}

	public (float, float, float) method_2(GClass2380 frustum)
	{
		float item = 0f;
		float item2 = 0f;
		float item3 = 0f;
		if (Gclass962_0 != null)
		{
			(item, item2, item3) = (Gclass962_0 as GClass964).GetMaxMipFactor(frustum);
		}
		return (item, item2, item3);
	}

	public static void smethod_0(object p)
	{
		GClass2380 gClass = new GClass2380();
		float num = 0f;
		float num2 = 0f;
		int updatedDuringFrame = 0;
		GClass967 tree = (p as Class608).tree;
		int key = 0;
		while (!CancellationToken_0.IsCancellationRequested)
		{
			if (!ThreadSkipJob)
			{
				bool flag = false;
				if (Monitor.TryEnter(tree.Object_1))
				{
					Class609_0.frustum.CopyTo(gClass);
					num = Class609_0.maxScreenHScale;
					num2 = Class609_0.maxScreenVScale;
					key = Class609_0.cameraInstanceID;
					updatedDuringFrame = Class609_0.frameIndex;
					flag = true;
					Monitor.Exit(tree.Object_1);
				}
				if (!flag)
				{
					continue;
				}
				Monitor.Enter(Object_0);
				float nearHeight = gClass.GetNearHeight();
				int count = HashSet_0.Count;
				if (count > 0)
				{
					int millisecondsTimeout = Math.Max(ThreadSleepTime / count, ThreadMinSleepTime);
					foreach (GClass967 item in HashSet_0)
					{
						float num3 = 0f;
						float num4 = 0f;
						float num5 = 0f;
						if (!item.Dictionary_0.ContainsKey(key))
						{
							item.Dictionary_0.Add(key, new Class607());
						}
						(num3, num4, num5) = item.method_2(gClass);
						if (num3 != 0f && num4 != 0f)
						{
							float num6 = num3 * num / nearHeight;
							float num7 = num4 * num2 / nearHeight;
							item.Dictionary_0[key].pixelsHBuf = num6;
							item.Dictionary_0[key].pixelsVBuf = num7;
							item.Dictionary_0[key].closestDist = num5;
							int val = 0;
							for (int i = 1; i < item.Int_0 && !((float)(item.Int_1 >> i) < num6); i++)
							{
								val = i;
							}
							int val2 = 0;
							for (int j = 1; j < item.Int_0 && !((float)(item.Int_2 >> j) < num7); j++)
							{
								val2 = j;
							}
							int requestedMipMapLevel = Math.Min(val, val2);
							item.Dictionary_0[key].requestedMipMapLevel = requestedMipMapLevel;
							item.Dictionary_0[key].updatedDuringFrame = updatedDuringFrame;
							Thread.Sleep(millisecondsTimeout);
						}
						else
						{
							item.Dictionary_0[key].requestedMipMapLevel = 100;
						}
					}
				}
				else
				{
					Thread.Sleep(ThreadSleepTime);
				}
				Monitor.Exit(Object_0);
			}
			else
			{
				Thread.Sleep(ThreadSleepTime);
			}
		}
	}
}
